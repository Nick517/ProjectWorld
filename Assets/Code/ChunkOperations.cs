using Unity.Mathematics;

namespace Terrain
{
    public static class ChunkOperations
    {
        public static float GetCubeSize(ChunkLoaderSettingsComponent chunkLoaderSettings, float chunkScale)
        {
            return math.pow(2, chunkScale) * chunkLoaderSettings.baseCubeSize;
        }

        public static float GetChunkSize(ChunkLoaderSettingsComponent chunkLoaderSettings, float chunkScale)
        {
            return GetCubeSize(chunkLoaderSettings, chunkScale) * chunkLoaderSettings.cubeCount;
        }

        public static float3 GetClosestChunkPosition(ChunkLoaderSettingsComponent chunkLoaderSettings, ChunkAspect.ChunkData chunkData)
        {
            float chunkSize = GetChunkSize(chunkLoaderSettings, chunkData.chunkScale);
            float3 position = chunkData.position;

            float x = math.floor(position.x / chunkSize);
            float y = math.floor(position.y / chunkSize);
            float z = math.floor(position.z / chunkSize);

            float3 chunkPosition = new(x, y, z);

            return chunkPosition * chunkSize;
        }

        public static float3 GetClosestChunkCenter(ChunkLoaderSettingsComponent chunkLoaderSettings, ChunkAspect.ChunkData chunkData)
        {
            float chunkSize = GetChunkSize(chunkLoaderSettings, chunkData.chunkScale);
            float3 chunkSize3D = new(chunkSize, chunkSize, chunkSize);

            float3 chunkCenter = chunkData.position + (chunkSize3D / 2);

            return chunkCenter;
        }
    }
}
