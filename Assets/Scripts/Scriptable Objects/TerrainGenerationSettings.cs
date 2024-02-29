using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    [CreateAssetMenu(fileName = "Terrain Generation Settings", menuName = "ScriptableObjects/Terrain Generation Settings", order = 2)]
    public class TerrainGenerationSettings : ScriptableObject
    {
        public List<TerrainGenerationLayer> layers = new();
    }
}
