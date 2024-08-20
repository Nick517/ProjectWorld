using ECS.Components.TerrainGeneration;
using Unity.Mathematics;

namespace Utility.TerrainGeneration
{
    public static class ChunkOperations
    {
        public static float3 GetCubeSize(ChunkGenerationSettings settings, float chunkScale)
        {
            return math.pow(2, chunkScale) * settings.BaseCubeSize;
        }

        public static float3 GetChunkSize(ChunkGenerationSettings settings, float chunkScale)
        {
            return GetCubeSize(settings, chunkScale) * settings.CubeCount;
        }

        public static float3 GetClosestCubePosition(ChunkGenerationSettings settings, float3 position)
        {
            return math.floor(position / settings.BaseCubeSize) * settings.BaseCubeSize;
        }

        public static float3 GetClosestChunkPosition(ChunkGenerationSettings settings, float3 position,
            float chunkScale)
        {
            var chunkSize = GetChunkSize(settings, chunkScale);

            return math.floor(position / chunkSize) * chunkSize;
        }

        public static float3 GetClosestChunkCenter(ChunkGenerationSettings settings, float3 position, float chunkScale)
        {
            return position + GetChunkSize(settings, chunkScale) / 2;
        }
    }
}