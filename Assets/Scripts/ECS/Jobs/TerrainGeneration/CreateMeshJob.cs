using System;
using DataTypes;
using DataTypes.Trees;
using ECS.BufferElements.TerrainGeneration.Renderer;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Utility.SpacialPartitioning.SegmentOperations;
using static Utility.TerrainGeneration.MarchingCubeTables;
using static Utility.TerrainGeneration.TerrainGenerator;

namespace ECS.Jobs.TerrainGeneration
{
    [BurstCompile]
    public struct CreateMeshJob : IJobChunk
    {
        [WriteOnly] public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public EntityTypeHandle EntityTypeHandle;
        [ReadOnly] public ComponentTypeHandle<SegmentInfo> SegmentInfoTypeHandle;
        [ReadOnly] public BaseSegmentSettings Settings;
        [ReadOnly] public ArrayOctree<float> Maps;
        [ReadOnly] public TgTreeBlob TgTreeBlob;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            var entities = chunk.GetNativeArray(EntityTypeHandle);
            var segmentInfos = chunk.GetNativeArray(ref SegmentInfoTypeHandle);

            var cube = new NativeArray<float>(8, Allocator.Temp);

            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
            var processed = 0;

            while (enumerator.NextEntityIndex(out var i))
            {
                if (processed >= Settings.MaxSegmentsPerFrame) break;

                var entity = entities[i];
                var info = segmentInfos[i];
                var pos = info.Position;
                var scale = info.Scale;
                var cubeSize = GetCubeSize(Settings.BaseCubeSize, scale);

                var cubeMap = PopulateMap(Settings, TgTreeBlob, Maps, pos, scale);
                var vertexes = new Vertices(Settings.CubeCount);

                Ecb.AddBuffer<TriangleBufferElement>(unfilteredChunkIndex, entity);
                Ecb.AddBuffer<VertexBufferElement>(unfilteredChunkIndex, entity);

                var active = false;

                for (var x = 0; x < Settings.CubeCount; x++)
                for (var y = 0; y < Settings.CubeCount; y++)
                for (var z = 0; z < Settings.CubeCount; z++)
                    if (MarchCube(unfilteredChunkIndex, Ecb, Settings, entity, cubeSize, cubeMap, new int3(x, y, z),
                            ref vertexes, ref cube))
                        active = true;

                foreach (var vert in vertexes.Positions)
                    Ecb.AppendToBuffer<VertexBufferElement>(unfilteredChunkIndex, entity, vert);

                if (active)
                {
                    if (info.IsRenderer) Ecb.AddComponent<SetRendererMeshTag>(unfilteredChunkIndex, entity);
                    else Ecb.AddComponent<SetColliderMeshTag>(unfilteredChunkIndex, entity);
                }
                else
                {
                    Ecb.AddComponent<InactiveSegmentTag>(unfilteredChunkIndex, entity);
                }

                Ecb.RemoveComponent<CreateMeshTag>(unfilteredChunkIndex, entity);

                vertexes.Dispose();

                processed++;
            }

            if (cube.IsCreated) cube.Dispose();
        }

        private static bool MarchCube(int sortKey, EntityCommandBuffer.ParallelWriter ecb, BaseSegmentSettings settings,
            Entity entity, float cubeSize, CubicArray<float> cubeMap, int3 position, ref Vertices vertices,
            ref NativeArray<float> cube)
        {
            for (var c = 0; c < 8; c++) cube[c] = cubeMap.GetAt(position + Corner(c));

            var config = GetCubeConfig(settings, cube);
            if (config is 0 or 255) return false;

            var edgeIndex = 0;

            for (var i = 0; i < 15; i++)
            {
                var index = Triangle(config, edgeIndex);
                if (index == -1) return true;

                var vertPos = IndexToVertexPosition(settings, position, ref cube, index);
                var vertIndex = vertices.GetOrCreateVertex(position, index, vertPos * cubeSize);

                ecb.AppendToBuffer<TriangleBufferElement>(sortKey, entity, vertIndex);

                edgeIndex++;
            }

            return true;
        }

        private static int GetCubeConfig(BaseSegmentSettings settings, NativeArray<float> cube)
        {
            var config = 0;

            for (var c = 0; c < 8; c++)
                if (cube[c] <= settings.MapSurface)
                    config |= 1 << c;

            return config;
        }

        private static float3 IndexToVertexPosition(BaseSegmentSettings settings, int3 position,
            ref NativeArray<float> cube, int index)
        {
            var vert1 = position + Corner(Edge(index, 0));
            var vert2 = position + Corner(Edge(index, 1));

            var sample1 = cube[Edge(index, 0)];
            var sample2 = cube[Edge(index, 1)];

            var dif = sample2 - sample1;
            dif = dif == 0 ? settings.MapSurface : (settings.MapSurface - sample1) / dif;

            return vert1 + (float3)(vert2 - vert1) * dif;
        }

        private struct Vertices : IDisposable
        {
            public NativeList<float3> Positions;

            private NativeArray<int> _vertIndices;
            private readonly int _vertLength;

            public Vertices(int cubeCount)
            {
                _vertLength = cubeCount + 1;
                Positions = new NativeList<float3>(Allocator.Temp);
                _vertIndices = new NativeArray<int>(_vertLength * _vertLength * _vertLength * 3, Allocator.Temp);
            }

            public int GetOrCreateVertex(int3 cubeIndex, int edgeIndex, float3 position)
            {
                var flatIndex = GetFlatIndex(cubeIndex, edgeIndex);

                var index = _vertIndices[flatIndex];
                if (index != 0) return index - 1;

                index = Positions.Length;
                Positions.Add(position);

                _vertIndices[flatIndex] = index + 1;
                return index;
            }

            private int GetFlatIndex(int3 index3D, int edgeIndex)
            {
                var dimension = EdgeToDimension[edgeIndex];
                index3D += EdgeOffset[edgeIndex];

                return index3D.x +
                       index3D.y * _vertLength +
                       index3D.z * _vertLength * _vertLength +
                       dimension * _vertLength * _vertLength * _vertLength;
            }

            private static readonly int[] EdgeToDimension = { 0, 1, 0, 1, 0, 1, 0, 1, 2, 2, 2, 2 };

            private static readonly int3[] EdgeOffset =
            {
                new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(0, 0, 0),
                new(0, 0, 1), new(1, 0, 1), new(0, 1, 1), new(0, 0, 1),
                new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)
            };

            public void Dispose()
            {
                if (_vertIndices.IsCreated) _vertIndices.Dispose();
                if (Positions.IsCreated) Positions.Dispose();
            }
        }
    }
}