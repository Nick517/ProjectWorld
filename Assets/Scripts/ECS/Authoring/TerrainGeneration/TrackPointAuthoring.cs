using ECS.Components.TerrainGeneration;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring.TerrainGeneration
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
            AddComponent<TrackPointTag>(entity);
            AddComponent(entity, new ChunkPosition { Position = authoring.transform.position });
            AddComponent<LoadChunksPointTag>(entity);
        }
    }
}