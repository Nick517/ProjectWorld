using Unity.Entities;
using UnityEngine;

namespace Terrain
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
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ChunkGenerationSettingsComponent>(entity, new()
            {
                chunkPrefab = GetEntity(authoring.chunkPrefab, TransformUsageFlags.Dynamic),
                baseCubeSize = authoring.chunkGenerationSettings.baseCubeSize,
                cubeCount = authoring.chunkGenerationSettings.cubeCount,
                mapSurface = authoring.chunkGenerationSettings.mapSurface,
                maxChunkScale = authoring.chunkGenerationSettings.maxChunkScale,
                megaChunks = authoring.chunkGenerationSettings.megaChunks,
                LOD = authoring.chunkGenerationSettings.LOD,
                reloadScale = authoring.chunkGenerationSettings.reloadScale
            });
        }
    }
}
