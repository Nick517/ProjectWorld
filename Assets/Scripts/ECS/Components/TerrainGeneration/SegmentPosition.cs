using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.TerrainGeneration
{
    public struct SegmentPosition : IComponentData
    {
        public float3 Position;
    }
}