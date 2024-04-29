using Unity.Entities;
using UnityEngine;
using ChunkGenerationSettings = ECS.Components.ChunkGenerationSettings;

namespace ECS.Authoring
{
    [AddComponentMenu("Custom Authoring/Game Settings Authoring")]
    public class GameSettingsAuthoring : MonoBehaviour
    {
        public GameObject chunkPrefab;
        public ScriptableObjects.ChunkGenerationSettings settings;
    }

    public class ChunkLoaderBaker : Baker<GameSettingsAuthoring>
    {
        public override void Bake(GameSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ChunkGenerationSettings
            {
                ChunkPrefab = GetEntity(authoring.chunkPrefab, TransformUsageFlags.Dynamic),
                BaseCubeSize = authoring.settings.baseCubeSize,
                CubeCount = authoring.settings.cubeCount,
                MapSurface = authoring.settings.mapSurface,
                MaxChunkScale = authoring.settings.maxChunkScale,
                MegaChunks = authoring.settings.megaChunks,
                LOD = authoring.settings.lod,
                ReloadScale = authoring.settings.reloadScale
            });
        }
    }
}