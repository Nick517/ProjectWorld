using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.TerrainGeneration.Renderer
{
    public struct RendererPoint : IComponentData
    {
        public int MaxSegmentScale;
        public int MegaSegments;
        public float LOD;
        public float3 SegmentPosition;
        public bool Update;
    }
}