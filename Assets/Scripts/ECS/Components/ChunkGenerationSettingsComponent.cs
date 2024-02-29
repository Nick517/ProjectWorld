using Unity.Entities;

namespace Terrain
{
    public struct ChunkGenerationSettingsComponent : IComponentData
    {
        public Entity chunkPrefab;
        public float baseCubeSize;
        public int cubeCount;
        public float mapSurface;
        public int maxChunkScale;
        public int megaChunks;
        public float LOD;
        public float reloadScale;
    }
}
