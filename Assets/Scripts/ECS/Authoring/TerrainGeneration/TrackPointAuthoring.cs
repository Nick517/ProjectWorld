using ECS.Components.TerrainGeneration;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring.TerrainGeneration
{
    [AddComponentMenu("Custom Authoring/Track Point Authoring")]
    public class TrackPointAuthoring : MonoBehaviour
    {
        public int rendererMaxSegmentScale = 9;
        public int rendererMegaSegments = 12;
        public float rendererLOD = 0.15f;
        public int colliderMaxSegmentScale = 4;
        public int colliderMegaSegments = 4;
        public float colliderLOD = 0.4f;
    }

    public class TrackPointBaker : Baker<TrackPointAuthoring>
    {
        public override void Bake(TrackPointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new TrackPoint
            {
                RendererMaxSegmentScale = authoring.rendererMaxSegmentScale,
                RendererMegaSegments = authoring.rendererMegaSegments,
                RendererLOD = authoring.rendererLOD,
                ColliderMaxSegmentScale = authoring.colliderMaxSegmentScale,
                ColliderMegaSegments = authoring.colliderMegaSegments,
                ColliderLOD = authoring.colliderLOD,
                Update = true
            });
        }
    }
}