using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring.TerrainGeneration.Renderer
{
    [AddComponentMenu("Custom Authoring/Renderer Point Authoring")]
    public class RendererPointAuthoring : MonoBehaviour
    {
        public int maxSegmentScale = 8;
        public int megaSegments = 2;
        public float lod = 1.5f;
        public int reloadScale = 1;
    }

    public class RendererPointBaker : Baker<RendererPointAuthoring>
    {
        public override void Bake(RendererPointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RendererPoint
            {
                MaxSegmentScale = authoring.maxSegmentScale,
                MegaSegments = authoring.megaSegments,
                LOD = authoring.lod,
                ReloadScale = authoring.reloadScale,
            });
            AddComponent(entity, new SegmentPosition { Position = authoring.transform.position });
            AddComponent<UpdateRendererSegmentsTag>(entity);
        }
    }
}