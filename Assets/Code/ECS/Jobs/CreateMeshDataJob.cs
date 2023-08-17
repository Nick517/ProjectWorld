using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Terrain
{
    [BurstCompile]
    public partial struct CreateMeshDataJob : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
        [ReadOnly] public EntityTypeHandle entityTypeHandle;
        [ReadOnly] public ComponentTypeHandle<LocalTransform> localTransformTypeHandle;
        [ReadOnly] public ComponentTypeHandle<ChunkScaleComponent> chunkScaleComponentTypeHandle;
        [ReadOnly] public ChunkGenerationSettingsComponent chunkGenerationSettings;
        [ReadOnly] public WorldDataComponent worldData;
        [ReadOnly] public NativeArray<TerrainGenerationLayerBufferElement> terrainGenerationLayers;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);
            NativeArray<LocalTransform> localTransforms = chunk.GetNativeArray(ref localTransformTypeHandle);
            NativeArray<ChunkScaleComponent> chunkScaleComponents = chunk.GetNativeArray(ref chunkScaleComponentTypeHandle);

            ChunkEntityEnumerator enumerator = new(useEnabledMask, chunkEnabledMask, chunk.Count);
            while (enumerator.NextEntityIndex(out int i))
            {
                Entity entity = entities[i];
                float3 position = localTransforms[i].Position;
                float chunkScale = chunkScaleComponents[i].chunkScale;

                NativeList<float3> vertices = new(Allocator.Temp);
                NativeArray<float> cubeMap = new(TerrainGenerator.PopulateMap(chunkGenerationSettings, worldData, terrainGenerationLayers, position, chunkScale), Allocator.Temp);

                _ = entityCommandBuffer.AddBuffer<TriangleBufferElement>(i, entity);
                _ = entityCommandBuffer.AddBuffer<VerticeBufferElement>(i, entity);

                int cubeCount = chunkGenerationSettings.cubeCount;
                for (int x = 0; x < cubeCount; x++)
                {
                    for (int y = 0; y < cubeCount; y++)
                    {
                        for (int z = 0; z < cubeCount; z++)
                        {
                            MarchCube(i, entityCommandBuffer, chunkGenerationSettings, entity, chunkScale, cubeMap, new(x, y, z), ref vertices);
                        }
                    }
                }

                foreach (float3 vertice in vertices)
                {
                    entityCommandBuffer.AppendToBuffer<VerticeBufferElement>(i, entity, vertice);
                }

                entityCommandBuffer.RemoveComponent<CreateChunkMeshDataTagComponent>(i, entity);
                entityCommandBuffer.AddComponent<SetChunkMeshTagComponent>(i, entity);
            }
        }

        private static void MarchCube(int sortKey, EntityCommandBuffer.ParallelWriter entityCommandBuffer, ChunkGenerationSettingsComponent chunkLoaderSettings, Entity entity, float chunkScale, NativeArray<float> cubeMap, int3 position, ref NativeList<float3> vertices)
        {
            float cubeSize = ChunkOperations.GetCubeSize(chunkLoaderSettings, chunkScale);

            NativeArray<float> cube = new(8, Allocator.Temp);

            for (int i = 0; i < 8; i++)
            {
                cube[i] = SampleMap(chunkLoaderSettings, cubeMap, position + MarchingCubesTables.Corner(i));
            }

            int configIndex = GetCubeConfiguration(chunkLoaderSettings, cube);

            if (configIndex is 0 or 255) { return; }

            int edgeIndex = 0;

            for (int i = 0; i < 15; i++)
            {
                int indice = MarchingCubesTables.Triangle(configIndex, edgeIndex);

                if (indice == -1) { return; }

                float3 verticePosition = IndiceForVerticePosition(chunkLoaderSettings, position, ref cube, indice);
                int vertice = VerticeForIndice(verticePosition * cubeSize, ref vertices);

                entityCommandBuffer.AppendToBuffer<TriangleBufferElement>(sortKey, entity, vertice);

                edgeIndex++;
            }
        }

        private static float SampleMap(ChunkGenerationSettingsComponent chunkLoaderSettings, NativeArray<float> cubeMap, int3 point)
        {
            return TerrainGenerator.GetCube(cubeMap, chunkLoaderSettings.cubeCount, point);
        }

        private static int GetCubeConfiguration(ChunkGenerationSettingsComponent chunkLoaderSettings, NativeArray<float> cube)
        {
            int configurationIndex = 0;

            for (int i = 0; i < 8; i++)
            {
                if (cube[i] > chunkLoaderSettings.mapSurface)
                {
                    configurationIndex |= 1 << i;
                }
            }

            return configurationIndex;
        }

        private static int VerticeForIndice(float3 vertice, ref NativeList<float3> vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].Equals(vertice))
                {
                    return i;
                }
            }

            vertices.Add(vertice);

            return vertices.Length - 1;
        }

        private static float3 IndiceForVerticePosition(ChunkGenerationSettingsComponent chunkLoaderSettings, int3 position, ref NativeArray<float> cube, int indice)
        {
            float3 vertice1 = position + MarchingCubesTables.Corner(MarchingCubesTables.Edge(indice, 0));
            float3 vertice2 = position + MarchingCubesTables.Corner(MarchingCubesTables.Edge(indice, 1));

            float vertice1Sample = cube[MarchingCubesTables.Edge(indice, 0)];
            float vertice2Sample = cube[MarchingCubesTables.Edge(indice, 1)];

            float difference = vertice2Sample - vertice1Sample;
            difference = difference == 0 ? chunkLoaderSettings.mapSurface : (chunkLoaderSettings.mapSurface - vertice1Sample) / difference;

            float3 verticePosition = vertice1 + ((vertice2 - vertice1) * difference);

            return verticePosition;
        }
    }
}
