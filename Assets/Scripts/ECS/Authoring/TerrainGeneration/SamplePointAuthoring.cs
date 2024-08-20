using ECS.Components.TerrainGeneration;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring.TerrainGeneration
{
    [AddComponentMenu("Custom Authoring/Sample Point Authoring")]
    public class SamplePointAuthoring : MonoBehaviour
    {
    }

    public class SamplePointBaker : Baker<SamplePointAuthoring>
    {
        public override void Bake(SamplePointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<SamplePointTag>(entity);
        }
    }
}