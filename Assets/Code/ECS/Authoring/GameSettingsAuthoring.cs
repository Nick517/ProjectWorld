using Unity.Entities;
using UnityEngine;

namespace Terrain
{
    [AddComponentMenu("Custom Authoring/Game Settings Authoring")]
    public class GameSettingsAuthoring : MonoBehaviour
    {
        public GameObject chunkPrefab;
        public TerrainGenerationSettings terrainGenerationSettings;
    }

    public class ChunkLoaderBaker : Baker<GameSettingsAuthoring>
    {
        public override void Bake(GameSettingsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<TerrainGenerationSettingsComponent>(entity, new()
            {
                chunkPrefab = GetEntity(authoring.chunkPrefab, TransformUsageFlags.Dynamic),
                baseCubeSize = authoring.terrainGenerationSettings.baseCubeSize,
                cubeCount = authoring.terrainGenerationSettings.cubeCount,
                mapSurface = authoring.terrainGenerationSettings.mapSurface,
                maxChunkScale = authoring.terrainGenerationSettings.maxChunkScale,
                megaChunks = authoring.terrainGenerationSettings.megaChunks,
                LOD = authoring.terrainGenerationSettings.LOD,
                reloadScale = authoring.terrainGenerationSettings.reloadScale
            });
        }
    }
}
