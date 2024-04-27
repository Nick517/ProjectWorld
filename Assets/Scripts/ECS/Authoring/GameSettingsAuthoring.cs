using ECS.Components;
using ScriptableObjects;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    [AddComponentMenu("Custom Authoring/Game Settings Authoring")]
    public class GameSettingsAuthoring : MonoBehaviour
    {
        public GameObject chunkPrefab;
        public ChunkGenerationSettings chunkGenerationSettings;
    }

    public class ChunkLoaderBaker : Baker<GameSettingsAuthoring>
    {
        public override void Bake(GameSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ChunkGenerationSettingsComponent
            {
                ChunkPrefab = GetEntity(authoring.chunkPrefab, TransformUsageFlags.Dynamic),
                BaseCubeSize = authoring.chunkGenerationSettings.baseCubeSize,
                CubeCount = authoring.chunkGenerationSettings.cubeCount,
                MapSurface = authoring.chunkGenerationSettings.mapSurface,
                MaxChunkScale = authoring.chunkGenerationSettings.maxChunkScale,
                MegaChunks = authoring.chunkGenerationSettings.megaChunks,
                LOD = authoring.chunkGenerationSettings.lod,
                ReloadScale = authoring.chunkGenerationSettings.reloadScale
            });
        }
    }
}