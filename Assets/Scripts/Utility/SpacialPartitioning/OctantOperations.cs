using Unity.Burst;
using Unity.Mathematics;

namespace Utility.SpacialPartitioning
{
    [BurstCompile]
    public static class OctantOperations
    {
        public static int GetOctant(float3 point)
        {
            return (point.x >= 0 ? 1 : 0) |
                   (point.y >= 0 ? 2 : 0) |
                   (point.z >= 0 ? 4 : 0);
        }

        public static int Bool3ToOctant(bool3 bool3)
        {
            return (bool3.x ? 1 : 0) |
                   (bool3.y ? 2 : 0) |
                   (bool3.z ? 4 : 0);
        }

        public static bool3 OctantToBool3(int octant)
        {
            return OctantToBool3Array[octant];
        }

        private static readonly bool3[] OctantToBool3Array =
        {
            new(false, false, false),
            new(true, false, false),
            new(false, true, false),
            new(true, true, false),
            new(false, false, true),
            new(true, false, true),
            new(false, true, true),
            new(true, true, true)
        };
    }
}