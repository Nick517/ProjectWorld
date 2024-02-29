using Unity.Mathematics;

namespace Terrain
{
    public static class ChunkOperations
    {
        public static float GetCubeSize(ChunkGenerationSettingsComponent chunkGenerationSettings, float chunkScale)
        {
            return math.pow(2, chunkScale) * chunkGenerationSettings.baseCubeSize;
        }

        public static float GetChunkSize(ChunkGenerationSettingsComponent chunkGenerationSettings, float chunkScale)
        {
            return GetCubeSize(chunkGenerationSettings, chunkScale) * chunkGenerationSettings.cubeCount;
        }

        public static float3 GetClosestChunkPosition(ChunkGenerationSettingsComponent chunkGenerationSettings, ChunkAspect.ChunkData chunkData)
        {
            float chunkSize = GetChunkSize(chunkGenerationSettings, chunkData.chunkScale);
            float3 position = chunkData.position;

            float x = math.floor(position.x / chunkSize);
            float y = math.floor(position.y / chunkSize);
            float z = math.floor(position.z / chunkSize);

            float3 chunkPosition = new(x, y, z);

            return chunkPosition * chunkSize;
        }

        public static float3 GetClosestChunkCenter(ChunkGenerationSettingsComponent chunkGenerationSettings, ChunkAspect.ChunkData chunkData)
        {
            float chunkSize = GetChunkSize(chunkGenerationSettings, chunkData.chunkScale);
            float3 chunkSize3D = new(chunkSize, chunkSize, chunkSize);

            float3 chunkCenter = chunkData.position + (chunkSize3D / 2);

            return chunkCenter;
        }
    }
}
