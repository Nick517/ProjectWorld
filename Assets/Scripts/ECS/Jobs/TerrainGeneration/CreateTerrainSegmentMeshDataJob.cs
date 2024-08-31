using ECS.BufferElements.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Utility.TerrainGeneration.MarchingCubeTables;
using static Utility.TerrainGeneration.SegmentOperations;
using static Utility.TerrainGeneration.TerrainGenerator;

namespace ECS.Jobs.TerrainGeneration
{
    [BurstCompile]
    public struct CreateTerrainSegmentMeshDataJob : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public EntityTypeHandle EntityTypeHandle;
        [ReadOnly] public ComponentTypeHandle<LocalTransform> LocalTransformTypeHandle;
        [ReadOnly] public ComponentTypeHandle<TerrainSegmentScale> TerrainSegmentScaleTypeHandle;
        [ReadOnly] public TerrainSegmentGenerationSettings Settings;
        [ReadOnly] public TerrainGenerationTreeBlob TerrainGenerationTreeBlob;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            var entities = chunk.GetNativeArray(EntityTypeHandle);
            var localTransforms = chunk.GetNativeArray(ref LocalTransformTypeHandle);
            var segmentScales = chunk.GetNativeArray(ref TerrainSegmentScaleTypeHandle);

            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

            while (enumerator.NextEntityIndex(out var i))
            {
                var entity = entities[i];
                var pos = localTransforms[i].Position;
                var segScale = segmentScales[i].Scale;

                var verts = new NativeList<float3>(Allocator.Temp);
                var cubeMap = new NativeArray<float>(PopulateMap(Settings, TerrainGenerationTreeBlob, pos, segScale),
                    Allocator.Temp);

                Ecb.AddBuffer<TriangleBufferElement>(i, entity);
                Ecb.AddBuffer<VerticeBufferElement>(i, entity);

                for (var x = 0; x < Settings.CubeCount; x++)
                for (var y = 0; y < Settings.CubeCount; y++)
                for (var z = 0; z < Settings.CubeCount; z++)
                    MarchCube(i, Ecb, Settings, entity, segScale, cubeMap, new int3(x, y, z), ref verts);

                foreach (var vert in verts) Ecb.AppendToBuffer<VerticeBufferElement>(i, entity, vert);

                Ecb.RemoveComponent<CreateTerrainSegmentMeshDataTag>(i, entity);
                Ecb.AddComponent<SetTerrainSegmentMeshTag>(i, entity);
            }
        }

        private static void MarchCube(int sortKey, EntityCommandBuffer.ParallelWriter ecb,
            TerrainSegmentGenerationSettings settings, Entity entity, float segmentScale, NativeArray<float> cubeMap,
            int3 position, ref NativeList<float3> vertices)
        {
            var cubeSize = GetCubeSize(settings, segmentScale);

            var cube = new NativeArray<float>(8, Allocator.Temp);

            for (var i = 0; i < 8; i++)
                cube[i] = SampleMap(settings, cubeMap, position + Corner(i));

            var configIndex = GetCubeConfiguration(settings, cube);

            if (configIndex is 0 or 255) return;

            var edgeIndex = 0;

            for (var i = 0; i < 15; i++)
            {
                var indice = Triangle(configIndex, edgeIndex);

                if (indice == -1) return;

                var vertPos = IndiceForVerticePosition(settings, position, ref cube, indice);
                var vert = VerticeForIndice(vertPos * cubeSize, ref vertices);

                ecb.AppendToBuffer<TriangleBufferElement>(sortKey, entity, vert);

                edgeIndex++;
            }
        }

        private static float SampleMap(TerrainSegmentGenerationSettings settings, NativeArray<float> cubeMap,
            int3 point)
        {
            return GetCube(cubeMap, settings.CubeCount, point);
        }

        private static int GetCubeConfiguration(TerrainSegmentGenerationSettings settings, NativeArray<float> cube)
        {
            var configIndex = 0;

            for (var i = 0; i < 8; i++)
                if (cube[i] <= settings.MapSurface)
                    configIndex |= 1 << i;

            return configIndex;
        }

        private static int VerticeForIndice(float3 vertice, ref NativeList<float3> vertices)
        {
            for (var i = 0; i < vertices.Length; i++)
                if (vertices[i].Equals(vertice))
                    return i;

            vertices.Add(vertice);

            return vertices.Length - 1;
        }

        private static float3 IndiceForVerticePosition(TerrainSegmentGenerationSettings settings, int3 position,
            ref NativeArray<float> cube, int indice)
        {
            var vert1 = position + Corner(Edge(indice, 0));
            var vert2 = position + Corner(Edge(indice, 1));

            var sample1 = cube[Edge(indice, 0)];
            var sample2 = cube[Edge(indice, 1)];

            var dif = sample2 - sample1;

            dif = dif == 0 ? settings.MapSurface : (settings.MapSurface - sample1) / dif;

            return vert1 + (float3)(vert2 - vert1) * dif;
        }
    }
}