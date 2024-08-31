using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.TerrainGeneration
{
    public struct TerrainSegmentPosition : IComponentData
    {
        public float3 Position;
    }
}
