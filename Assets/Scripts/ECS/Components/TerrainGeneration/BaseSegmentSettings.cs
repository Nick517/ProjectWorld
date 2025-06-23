using Unity.Entities;
using UnityEngine;

namespace ECS.Components.TerrainGeneration
{
    public struct BaseSegmentSettings : IComponentData
    {
        public Entity RendererSegmentPrefab;
        public UnityObjectRef<Material> Material;
        public float BaseCubeSize;
        public int CubeCount;
        public float MapSurface;
        public int MaxColliderScale;

        public float BaseSegSize => BaseCubeSize * CubeCount;
        public int CubeCountTotal => CubeCount * CubeCount * CubeCount;
    }
}