using DataTypes.Trees;
using Unity.Entities;

namespace Utility.TerrainGeneration
{
    public static class SegmentTags
    {
        public static readonly Entity PlaceHolder = new() { Index = -1 };

        public static readonly Entity Inactive = new() { Index = -2 };

        public static bool IsTag(this Entity entity)
        {
            return entity == PlaceHolder || entity == Inactive;
        }

        public readonly struct CompareSegments : Octree<Entity>.IComparison<Entity>
        {
            public bool Evaluate(in Entity a, in Entity b)
            {
                return a != default && b != default;
            }
        }
    }
}