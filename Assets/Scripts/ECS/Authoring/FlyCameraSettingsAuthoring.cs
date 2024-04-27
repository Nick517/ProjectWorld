using ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    [AddComponentMenu("Custom Authoring/Fly Camera Authoring")]
    public class FlyCameraSettingsAuthoring : MonoBehaviour
    {
        public float acceleration = 1.0f;
        public float sprintMultiplier = 4.0f;
        public float damping = 5.0f;
    }

    public class FlyCameraBaker : Baker<FlyCameraSettingsAuthoring>
    {
        public override void Bake(FlyCameraSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<FirstPersonCameraTagComponent>(entity);
            AddComponent(entity, new FlyCameraSettingsComponent
            {
                Acceleration = authoring.acceleration,
                SprintMultiplier = authoring.sprintMultiplier,
                Damping = authoring.damping
            });
        }
    }
}