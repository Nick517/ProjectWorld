using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

namespace Utility.SpacialPartitioning
{
    [BurstCompile]
    public static class OctantOperations
    {
        public static int GetOctant(float3 point)
        {
            return (point.x >= 0 ? 1 : 0) | (point.y >= 0 ? 2 : 0) | (point.z >= 0 ? 4 : 0);
        }

        public static readonly Dictionary<int, bool3> OctantToBool3 = new()
        {
            { 0, new bool3(false, false, false) },
            { 1, new bool3(true, false, false) },
            { 2, new bool3(false, true, false) },
            { 3, new bool3(true, true, false) },
            { 4, new bool3(false, false, true) },
            { 5, new bool3(true, false, true) },
            { 6, new bool3(false, true, true) },
            { 7, new bool3(true, true, true) }
        };

        public static readonly Dictionary<bool3, int> Bool3ToOctant = new()
        {
            { new bool3(false, false, false), 0 },
            { new bool3(true, false, false), 1 },
            { new bool3(false, true, false), 2 },
            { new bool3(true, true, false), 3 },
            { new bool3(false, false, true), 4 },
            { new bool3(true, false, true), 5 },
            { new bool3(false, true, true), 6 },
            { new bool3(true, true, true), 7 }
        };
    }
}