using Unity.Burst;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Utility.SpacialPartitioning
{
    [BurstCompile]
    public static class SegmentOperations
    {
        public static float GetCubeSize(float baseCubeSize, int segScale = 0)
        {
            return baseCubeSize * ScaleMultiplier(segScale);
        }

        public static float GetSegSize(float baseSegSize, int segScale = 0)
        {
            return baseSegSize * ScaleMultiplier(segScale);
        }
        
        public static float3 GetClosestSegPos(float3 pos, float segSize)
        {
            return floor(pos / segSize) * segSize;
        }

        public static float3 GetClosestSegCenter(float3 pos, float segSize)
        {
            return GetClosestSegPos(pos, segSize) + segSize / 2;
        }

        public static bool PointWithinSeg(float3 point, float3 segPos, float segSize)
        {
            return all(point >= segPos) && all(point < segPos + segSize);
        }
        
        public static float ScaleMultiplier(int scale)
        {
            return pow(2, scale);
        }
    }
}