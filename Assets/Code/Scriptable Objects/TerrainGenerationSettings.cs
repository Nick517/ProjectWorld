using UnityEngine;

namespace Terrain
{
    [CreateAssetMenu(fileName = "World Settings", menuName = "ScriptableObjects/Terrain Generation Settings", order = 1)]
    public class TerrainGenerationSettings : ScriptableObject
    {
        public float baseCubeSize = 1.0f;
        public int cubeCount = 8;
        [Range(0.0f, 1.0f)]
        public float mapSurface = 0.5f;
        public int maxChunkScale = 8;
        public int megaChunks = 2;
        public float LOD = 2.0f;
        public float reloadScale = 1.0f;
    }
}
