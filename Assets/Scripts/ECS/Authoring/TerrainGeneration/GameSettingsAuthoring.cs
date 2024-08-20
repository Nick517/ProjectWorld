using ECS.Components.TerrainGeneration;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring.TerrainGeneration
{
    [AddComponentMenu("Custom Authoring/Game Settings Authoring")]
    public class GameSettingsAuthoring : MonoBehaviour
    {
        public GameObject chunkPrefab;
        public float baseCubeSize = 1;
        public int cubeCount = 8;
        [Range(0, 1)] public float mapSurface = 0.5f;
        public int maxChunkScale = 8;
        public int megaChunks = 2;
        public float lod = 2;
        public float reloadScale = 1;
    }

    public class ChunkLoaderBaker : Baker<GameSettingsAuthoring>
    {
        public override void Bake(GameSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ChunkGenerationSettings
            {
                ChunkPrefab = GetEntity(authoring.chunkPrefab, TransformUsageFlags.Dynamic),
                BaseCubeSize = authoring.baseCubeSize,
                CubeCount = authoring.cubeCount,
                MapSurface = authoring.mapSurface,
                MaxChunkScale = authoring.maxChunkScale,
                MegaChunks = authoring.megaChunks,
                LOD = authoring.lod,
                ReloadScale = authoring.reloadScale
            });
        }
    }
}