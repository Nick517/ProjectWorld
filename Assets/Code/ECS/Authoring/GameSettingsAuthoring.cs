using Unity.Entities;
using UnityEngine;

namespace Terrain
{
    [AddComponentMenu("Custom Authoring/Game Settings Authoring")]
    public class GameSettingsAuthoring : MonoBehaviour
    {
        public GameObject chunkPrefab;
        public WorldSettings worldSettings;
    }

    public class ChunkLoaderBaker : Baker<GameSettingsAuthoring>
    {
        public override void Bake(GameSettingsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ChunkLoaderSettingsComponent>(entity, new()
            {
                chunkPrefab = GetEntity(authoring.chunkPrefab, TransformUsageFlags.Dynamic),
                baseCubeSize = authoring.worldSettings.baseCubeSize,
                cubeCount = authoring.worldSettings.cubeCount,
                mapSurface = authoring.worldSettings.mapSurface,
                maxChunkScale = authoring.worldSettings.maxChunkScale,
                megaChunks = authoring.worldSettings.megaChunks,
                LOD = authoring.worldSettings.LOD,
                reloadScale = authoring.worldSettings.reloadScale
            });
        }
    }
}
