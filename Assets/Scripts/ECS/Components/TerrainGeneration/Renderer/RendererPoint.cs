using Unity.Entities;

namespace ECS.Components.TerrainGeneration.Renderer
{
    public struct RendererPoint : IComponentData
    {
        public int MaxSegmentScale;
        public int MegaSegments;
        public float LOD;
        public int ReloadScale;
    }
}