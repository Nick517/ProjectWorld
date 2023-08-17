using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Terrain
{
    public static class TerrainGenerator
    {
        public static NativeArray<float> PopulateMap(ChunkGenerationSettingsComponent chunkLoaderSettings, WorldDataComponent worldData, NativeArray<TerrainGenerationLayerBufferElement> terrainGenerationLayers, float3 chunkPosition, float chunkScale)
        {
            float cubeSize = ChunkOperations.GetCubeSize(chunkLoaderSettings, chunkScale);
            int cubeCount = chunkLoaderSettings.cubeCount + 1;
            float3 offset = chunkPosition;

            NativeArray<float> map = new((int)math.pow(cubeCount, 3), Allocator.Temp);

            for (int x = 0; x < cubeCount; x++)
            {
                for (int y = 0; y < cubeCount; y++)
                {
                    for (int z = 0; z < cubeCount; z++)
                    {
                        int3 index3D = new(x, y, z);
                        float3 position = ((float3)index3D * cubeSize) + offset;

                        int index = GetFlatIndex(cubeCount, index3D);
                        map[index] = GetSample(worldData, terrainGenerationLayers, position);
                    }
                }
            }

            return map;
        }

        public static float GetCube(NativeArray<float> map, int cubeCount, int3 index3D)
        {
            return map[GetFlatIndex(cubeCount + 1, index3D)];
        }

        public static int GetFlatIndex(int cubeCount, int3 index3D)
        {
            return index3D.x + (index3D.y * cubeCount) + (index3D.z * cubeCount * cubeCount);
        }

        // Returns a value between 0 and 1
        private static float GetSample(WorldDataComponent worldData, NativeArray<TerrainGenerationLayerBufferElement> terrainGenerationLayers, float3 position)
        {
            float3 offset = new(worldData.seed, 0, worldData.seed * 2);
            position += offset;

            float height = 0;

            foreach (TerrainGenerationLayerBufferElement terrainGenerationLayer in terrainGenerationLayers)
            {
                height += NoiseLayer(position, terrainGenerationLayer);
            }

            return position.y > height ? 1 : 0;
        }

        private static float NoiseLayer(float3 position, TerrainGenerationLayerBufferElement terrainGenerationLayer)
        {
            float x = position.x * terrainGenerationLayer.frequency;
            float z = position.z * terrainGenerationLayer.frequency;

            return terrainGenerationLayer.amplitude * math.clamp(Mathf.PerlinNoise(x, z), 0.0f, 1.0f);
        }
    }
}
/*
    Terrain generator will add layers from bottom up. then, the z of the position will be checked if it overlaps the sample
 */