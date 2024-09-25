using ECS.Components.TerrainGeneration;
using Unity.Mathematics;

namespace Utility.TerrainGeneration
{
    public static class SegmentOperations
    {
        public static float3 GetCubeSize(BaseSegmentSettings settings, float segmentScale)
        {
            return math.pow(2, segmentScale) * settings.BaseCubeSize;
        }

        public static float3 GetSegmentSize(BaseSegmentSettings settings, float segmentScale)
        {
            return GetCubeSize(settings, segmentScale) * settings.CubeCount;
        }

        public static float3 GetClosestCubePosition(BaseSegmentSettings settings, float3 position)
        {
            return math.floor(position / settings.BaseCubeSize) * settings.BaseCubeSize;
        }

        public static float3 GetClosestSegmentPosition(BaseSegmentSettings settings, float3 position,
            float segmentScale)
        {
            var chunkSize = GetSegmentSize(settings, segmentScale);

            return math.floor(position / chunkSize) * chunkSize;
        }

        public static float3 GetClosestSegmentCenter(BaseSegmentSettings settings, float3 segmentPosition,
            float segmentScale)
        {
            return segmentPosition + GetSegmentSize(settings, segmentScale) / 2;
        }
    }
}