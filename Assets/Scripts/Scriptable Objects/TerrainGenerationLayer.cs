using UnityEngine;

namespace Terrain
{
    [CreateAssetMenu(fileName = "Terrain Generation Layer", menuName = "ScriptableObjects/Terrain Generation Layer", order = 3)]
    public class TerrainGenerationLayer : ScriptableObject
    {
        public float amplitude = 1.0f;
        public float frequency = 1.0f;
    }
}
