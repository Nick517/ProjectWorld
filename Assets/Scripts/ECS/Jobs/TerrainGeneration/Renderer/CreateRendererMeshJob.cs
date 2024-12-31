using ECS.BufferElements.TerrainGeneration.Renderer;
using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Utility.TerrainGeneration.MarchingCubeTables;
using static Utility.TerrainGeneration.SegmentOperations;
using static Utility.TerrainGeneration.TerrainGenerator;

namespace ECS.Jobs.TerrainGeneration.Renderer
{
    [BurstCompile]
    public struct CreateRendererMeshJob : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public EntityTypeHandle EntityTypeHandle;
        [ReadOnly] public ComponentTypeHandle<LocalTransform> LocalTransformTypeHandle;
        [ReadOnly] public ComponentTypeHandle<SegmentScale> SegmentScaleTypeHandle;
        [ReadOnly] public BaseSegmentSettings Settings;
        [ReadOnly] public TerrainGenerationTreeBlob TerrainGenerationTreeBlob;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            var entities = chunk.GetNativeArray(EntityTypeHandle);
            var localTransforms = chunk.GetNativeArray(ref LocalTransformTypeHandle);
            var segmentScales = chunk.GetNativeArray(ref SegmentScaleTypeHandle);

            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

            while (enumerator.NextEntityIndex(out var i))
            {
                var entity = entities[i];
                var pos = localTransforms[i].Position;
                var scale = segmentScales[i].Scale;

                var vertexes = new NativeList<float3>(Allocator.Temp);
                var cubeMap = new NativeArray<float>(PopulateMap(Settings, TerrainGenerationTreeBlob, pos, scale),
                    Allocator.Temp);

                Ecb.AddBuffer<TriangleBufferElement>(i, entity);
                Ecb.AddBuffer<VertexBufferElement>(i, entity);

                for (var x = 0; x < Settings.CubeCount; x++)
                for (var y = 0; y < Settings.CubeCount; y++)
                for (var z = 0; z < Settings.CubeCount; z++)
                    MarchCube(i, Ecb, Settings, entity, scale, cubeMap, new int3(x, y, z), ref vertexes);

                foreach (var vert in vertexes) Ecb.AppendToBuffer<VertexBufferElement>(i, entity, vert);

                Ecb.RemoveComponent<CreateRendererMeshTag>(i, entity);
                Ecb.AddComponent<SetRendererMeshTag>(i, entity);
            }
        }

        private static void MarchCube(int sortKey, EntityCommandBuffer.ParallelWriter ecb, BaseSegmentSettings settings,
            Entity entity, int scale, NativeArray<float> cubeMap, int3 position, ref NativeList<float3> vertexes)
        {
            var cubeSize = GetCubeSize(settings.BaseCubeSize, scale);

            var cube = new NativeArray<float>(8, Allocator.Temp);

            for (var i = 0; i < 8; i++)
                cube[i] = SampleMap(settings, cubeMap, position + Corner(i));

            var config = GetCubeConfig(settings, cube);

            if (config is 0 or 255) return;

            var edgeIndex = 0;

            for (var i = 0; i < 15; i++)
            {
                var index = Triangle(config, edgeIndex);

                if (index == -1) return;

                var vertPos = IndexForVertexPosition(settings, position, ref cube, index);
                var vert = VertexForIndex(vertPos * cubeSize, ref vertexes);

                ecb.AppendToBuffer<TriangleBufferElement>(sortKey, entity, vert);

                edgeIndex++;
            }
        }

        private static float SampleMap(BaseSegmentSettings settings, NativeArray<float> cubeMap, int3 point)
        {
            return GetCube(cubeMap, settings.CubeCount, point);
        }

        private static int GetCubeConfig(BaseSegmentSettings settings, NativeArray<float> cube)
        {
            var configIndex = 0;

            for (var i = 0; i < 8; i++)
                if (cube[i] <= settings.MapSurface)
                    configIndex |= 1 << i;

            return configIndex;
        }

        private static int VertexForIndex(float3 vertex, ref NativeList<float3> vertexes)
        {
            for (var i = 0; i < vertexes.Length; i++)
                if (vertexes[i].Equals(vertex))
                    return i;

            vertexes.Add(vertex);

            return vertexes.Length - 1;
        }

        private static float3 IndexForVertexPosition(BaseSegmentSettings settings, int3 position,
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
    }
}