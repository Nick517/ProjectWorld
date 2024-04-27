using Unity.Entities;

namespace ECS.Components
{
    public struct ChunkGenerationSettingsComponent : IComponentData
    {
        public Entity ChunkPrefab;
        public float BaseCubeSize;
        public int CubeCount;
        public float MapSurface;
        public int MaxChunkScale;
        public int MegaChunks;
        public float LOD;
        public float ReloadScale;
    }
}