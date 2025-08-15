using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.TerrainGeneration
{
    public struct SegmentInfo : IComponentData
    {
        public float3 Position;
        public int Scale;
    }
}