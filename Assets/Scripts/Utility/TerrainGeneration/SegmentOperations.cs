using ECS.Components.TerrainGeneration;
using Unity.Mathematics;

namespace Utility.TerrainGeneration
{
    public static class SegmentOperations
    {
        public static float3 GetCubeSize(TerrainSegmentGenerationSettings settings, float segmentScale)
        {
            return math.pow(2, segmentScale) * settings.BaseCubeSize;
        }

        public static float3 GetSegmentSize(TerrainSegmentGenerationSettings settings, float segmentScale)
        {
            return GetCubeSize(settings, segmentScale) * settings.CubeCount;
        }

        public static float3 GetClosestCubePosition(TerrainSegmentGenerationSettings settings, float3 position)
        {
            return math.floor(position / settings.BaseCubeSize) * settings.BaseCubeSize;
        }

        public static float3 GetClosestSegmentPosition(TerrainSegmentGenerationSettings settings, float3 position,
            float segmentScale)
        {
            var chunkSize = GetSegmentSize(settings, segmentScale);

            return math.floor(position / chunkSize) * chunkSize;
        }

        public static float3 GetClosestSegmentCenter(TerrainSegmentGenerationSettings settings, float3 position, float segmentScale)
        {
            return position + GetSegmentSize(settings, segmentScale) / 2;
        }
    }
}