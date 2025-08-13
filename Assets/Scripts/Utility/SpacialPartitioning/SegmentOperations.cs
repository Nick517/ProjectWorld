using Unity.Burst;
using Unity.Mathematics;

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
            return math.floor(pos / segSize) * segSize;
        }

        public static float3 GetClosestSegCenter(float3 pos, float segSize)
        {
            return PosToCenter(GetClosestSegPos(pos, segSize), segSize);
        }

        public static float3 PosToCenter(float3 pos, float segSize)
        {
            return pos + segSize / 2;
        }

        public static bool PointWithinSeg(float3 point, float3 segPos, float segSize)
        {
            return math.all(point >= segPos) && math.all(point < segPos + segSize);
        }
        
        public static float ScaleMultiplier(int scale)
        {
            return math.pow(2, scale);
        }
    }
}