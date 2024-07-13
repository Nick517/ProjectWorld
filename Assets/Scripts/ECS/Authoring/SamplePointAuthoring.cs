using ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
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