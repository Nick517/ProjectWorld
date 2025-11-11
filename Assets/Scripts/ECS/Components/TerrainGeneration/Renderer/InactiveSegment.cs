using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.TerrainGeneration.Renderer
{
    public struct InactiveSegment : IComponentData
    {
        public float3 Position;
        public int Scale;
    }
}