using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Chunk Generation Settings", menuName = "ScriptableObjects/Chunk Generation Settings",
        order = 1)]
    public class ChunkGenerationSettings : ScriptableObject
    {
        public float baseCubeSize = 1;
        public int cubeCount = 8;
        [Range(0, 1)] public float mapSurface = 0.5f;
        public int maxChunkScale = 8;
        public int megaChunks = 2;
        public float lod = 2;
        public float reloadScale = 1;
    }
}