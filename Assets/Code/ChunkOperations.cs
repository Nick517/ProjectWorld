using Unity.Mathematics;

namespace Terrain
{
    public static class ChunkOperations
    {
        public static float GetCubeSize(TerrainGenerationSettingsComponent terrainGenerationSettings, float chunkScale)
        {
            return math.pow(2, chunkScale) * terrainGenerationSettings.baseCubeSize;
        }

        public static float GetChunkSize(TerrainGenerationSettingsComponent terrainGenerationSettings, float chunkScale)
        {
            return GetCubeSize(terrainGenerationSettings, chunkScale) * terrainGenerationSettings.cubeCount;
        }

        public static float3 GetClosestChunkPosition(TerrainGenerationSettingsComponent terrainGenerationSettings, ChunkAspect.ChunkData chunkData)
        {
            float chunkSize = GetChunkSize(terrainGenerationSettings, chunkData.chunkScale);
            float3 position = chunkData.position;

            float x = math.floor(position.x / chunkSize);
            float y = math.floor(position.y / chunkSize);
            float z = math.floor(position.z / chunkSize);

            float3 chunkPosition = new(x, y, z);

            return chunkPosition * chunkSize;
        }

        public static float3 GetClosestChunkCenter(TerrainGenerationSettingsComponent terrainGenerationSettings, ChunkAspect.ChunkData chunkData)
        {
            float chunkSize = GetChunkSize(terrainGenerationSettings, chunkData.chunkScale);
            float3 chunkSize3D = new(chunkSize, chunkSize, chunkSize);

            float3 chunkCenter = chunkData.position + (chunkSize3D / 2);

            return chunkCenter;
        }
    }
}
