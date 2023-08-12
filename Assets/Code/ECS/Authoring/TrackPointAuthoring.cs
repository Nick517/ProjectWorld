using Unity.Entities;
using UnityEngine;

namespace Terrain
{
    [AddComponentMenu("Custom Authoring/Track Point Authoring")]
    public class TrackPointAuthoring : MonoBehaviour { }

    public class TrackPointBaker : Baker<TrackPointAuthoring>
    {
        public override void Bake(TrackPointAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<TrackPointTagComponent>(entity);
            AddComponent<ChunkPositionComponent>(entity, new() { position = authoring.transform.position });
            AddComponent<LoadChunksPointTagComponent>(entity);
        }
    }
}
