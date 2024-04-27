using ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    [AddComponentMenu("Custom Authoring/Track Point Authoring")]
    public class TrackPointAuthoring : MonoBehaviour
    {
    }

    public class TrackPointBaker : Baker<TrackPointAuthoring>
    {
        public override void Bake(TrackPointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<TrackPointTagComponent>(entity);
            AddComponent(entity, new ChunkPositionComponent { Position = authoring.transform.position });
            AddComponent<LoadChunksPointTagComponent>(entity);
        }
    }
}