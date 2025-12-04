using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.TerrainGeneration
{
    public struct TrackPoint : IComponentData
    {
        public int RendererMaxSegmentScale;
        public int RendererMegaSegments;
        public float RendererLOD;
        public int ColliderMaxSegmentScale;
        public int ColliderMegaSegments;
        public float3 SegmentPosition;
    }
}