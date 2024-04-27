using ECS.BufferElements;
using ECS.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Jobs
{
    [BurstCompile]
    public struct CreateMeshDataJob : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
        [ReadOnly] public EntityTypeHandle EntityTypeHandle;
        [ReadOnly] public ComponentTypeHandle<LocalTransform> LocalTransformTypeHandle;
        [ReadOnly] public ComponentTypeHandle<ChunkScaleComponent> ChunkScaleComponentTypeHandle;
        [ReadOnly] public ChunkGenerationSettingsComponent ChunkGenerationSettings;
        [ReadOnly] public TgGraphComponent TgGraph;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            var entities = chunk.GetNativeArray(EntityTypeHandle);
            var localTransforms = chunk.GetNativeArray(ref LocalTransformTypeHandle);
            var chunkScaleComponents = chunk.GetNativeArray(ref ChunkScaleComponentTypeHandle);

            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
            while (enumerator.NextEntityIndex(out var i))
            {
                var entity = entities[i];
                var position = localTransforms[i].Position;
                var chunkScale = chunkScaleComponents[i].ChunkScale;

                var vertices = new NativeList<float3>(Allocator.Temp);
                var cubeMap =
                    new NativeArray<float>(
                        TerrainGenerator.PopulateMap(ChunkGenerationSettings, TgGraph, position, chunkScale),
                        Allocator.Temp);

                _ = EntityCommandBuffer.AddBuffer<TriangleBufferElement>(i, entity);
                _ = EntityCommandBuffer.AddBuffer<VerticeBufferElement>(i, entity);

                var cubeCount = ChunkGenerationSettings.CubeCount;
                for (var x = 0; x < cubeCount; x++)
                for (var y = 0; y < cubeCount; y++)
                for (var z = 0; z < cubeCount; z++)
                    MarchCube(i, EntityCommandBuffer, ChunkGenerationSettings, entity, chunkScale, cubeMap,
                        new int3(x, y, z), ref vertices);

                foreach (var vertice in vertices)
                    EntityCommandBuffer.AppendToBuffer<VerticeBufferElement>(i, entity, vertice);

                EntityCommandBuffer.RemoveComponent<CreateChunkMeshDataTagComponent>(i, entity);
                EntityCommandBuffer.AddComponent<SetChunkMeshTagComponent>(i, entity);
            }
        }

        private static void MarchCube(int sortKey, EntityCommandBuffer.ParallelWriter entityCommandBuffer,
            ChunkGenerationSettingsComponent chunkLoaderSettings, Entity entity, float chunkScale,
            NativeArray<float> cubeMap, int3 position, ref NativeList<float3> vertices)
        {
            var cubeSize = ChunkOperations.GetCubeSize(chunkLoaderSettings, chunkScale);

            var cube = new NativeArray<float>(8, Allocator.Temp);

            for (var i = 0; i < 8; i++)
                cube[i] = SampleMap(chunkLoaderSettings, cubeMap, position + MarchingCubesTables.Corner(i));

            var configIndex = GetCubeConfiguration(chunkLoaderSettings, cube);

            if (configIndex is 0 or 255) return;

            var edgeIndex = 0;

            for (var i = 0; i < 15; i++)
            {
                var indice = MarchingCubesTables.Triangle(configIndex, edgeIndex);

                if (indice == -1) return;

                var verticePosition = IndiceForVerticePosition(chunkLoaderSettings, position, ref cube, indice);
                var vertice = VerticeForIndice(verticePosition * cubeSize, ref vertices);

                entityCommandBuffer.AppendToBuffer<TriangleBufferElement>(sortKey, entity, vertice);

                edgeIndex++;
            }
        }

        private static float SampleMap(ChunkGenerationSettingsComponent chunkLoaderSettings, NativeArray<float> cubeMap,
            int3 point)
        {
            return TerrainGenerator.GetCube(cubeMap, chunkLoaderSettings.CubeCount, point);
        }

        private static int GetCubeConfiguration(ChunkGenerationSettingsComponent chunkLoaderSettings,
            NativeArray<float> cube)
        {
            var configurationIndex = 0;

            for (var i = 0; i < 8; i++)
                if (cube[i] > chunkLoaderSettings.MapSurface)
                    configurationIndex |= 1 << i;

            return configurationIndex;
        }

        private static int VerticeForIndice(float3 vertice, ref NativeList<float3> vertices)
        {
            for (var i = 0; i < vertices.Length; i++)
                if (vertices[i].Equals(vertice))
                    return i;

            vertices.Add(vertice);

            return vertices.Length - 1;
        }

        private static float3 IndiceForVerticePosition(ChunkGenerationSettingsComponent chunkLoaderSettings,
            int3 position, ref NativeArray<float> cube, int indice)
        {
            var vertice1 = position + MarchingCubesTables.Corner(MarchingCubesTables.Edge(indice, 0));
            var vertice2 = position + MarchingCubesTables.Corner(MarchingCubesTables.Edge(indice, 1));

            var vertice1Sample = cube[MarchingCubesTables.Edge(indice, 0)];
            var vertice2Sample = cube[MarchingCubesTables.Edge(indice, 1)];

            var difference = vertice2Sample - vertice1Sample;
            difference = difference == 0
                ? chunkLoaderSettings.MapSurface
                : (chunkLoaderSettings.MapSurface - vertice1Sample) / difference;

            var verticePosition = vertice1 + (float3)(vertice2 - vertice1) * difference;

            return verticePosition;
        }
    }
}