using Unity.Entities;
using UnityEngine;

namespace ECS.Components.TerrainGeneration
{
    public struct TerrainSegmentGenerationSettings : IComponentData
    {
        public Entity TerrainSegmentPrefab;
        public UnityObjectRef<Material> Material;
        public float BaseCubeSize;
        public int CubeCount;
        public float MapSurface;
        public int MaxSegmentScale;
        public int MegaSegments;
        public float LOD;
        public float ReloadScale;
    }
}