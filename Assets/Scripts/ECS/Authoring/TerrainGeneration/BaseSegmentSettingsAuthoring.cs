using ECS.Components.TerrainGeneration;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring.TerrainGeneration
{
    [AddComponentMenu("Custom Authoring/Base Segment Settings Authoring")]
    public class BaseSegmentSettingsAuthoring : MonoBehaviour
    {
        public GameObject rendererSegmentPrefab;
        public Material terrainMaterial;
        public float baseCubeSize = 1;
        public int cubeCount = 8;
        [Range(0, 1)] public float mapSurface = 0.5f;
        public int maxSegmentsPerFrame = 64;
    }

    public class BaseSegmentSettingsBaker : Baker<BaseSegmentSettingsAuthoring>
    {
        public override void Bake(BaseSegmentSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BaseSegmentSettings
            {
                RendererSegmentPrefab = GetEntity(authoring.rendererSegmentPrefab, TransformUsageFlags.Dynamic),
                Material = authoring.terrainMaterial,
                BaseCubeSize = authoring.baseCubeSize,
                CubeCount = authoring.cubeCount,
                MapSurface = authoring.mapSurface,
                MaxSegmentsPerFrame = authoring.maxSegmentsPerFrame
            });
        }
    }
}