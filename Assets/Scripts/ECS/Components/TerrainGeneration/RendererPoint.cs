using Unity.Entities;

namespace ECS.Components.TerrainGeneration
{
    public struct RendererPoint : IComponentData
    {
        public int MaxSegmentScale;
        public int MegaSegments;
        public float LOD;
        public float ReloadScale;
    }
}