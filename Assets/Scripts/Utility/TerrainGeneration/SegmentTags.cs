using Unity.Entities;

namespace Utility.TerrainGeneration
{
    public static class SegmentTags
    {
        public static readonly Entity PlaceHolder = new() { Index = -1, Version = -1 };
        public static readonly Entity Inactive = new() { Index = -2, Version = -1 };

        public static bool IsValidSegment(this Entity segment)
        {
            return segment != PlaceHolder && segment != Inactive;
        }
    }
}