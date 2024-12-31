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

        public float BaseSegSize => BaseCubeSize * CubeCount;
    }
}