using Unity.Entities;
using UnityEngine;

namespace ECS.Components.TerrainGeneration
{
    public struct ChunkGenerationSettings : IComponentData
    {
        public Entity ChunkPrefab;
        public UnityObjectRef<Material> Material;
        public float BaseCubeSize;
        public int CubeCount;
        public float MapSurface;
        public int MaxChunkScale;
        public int MegaChunks;
        public float LOD;
        public float ReloadScale;
    }
}