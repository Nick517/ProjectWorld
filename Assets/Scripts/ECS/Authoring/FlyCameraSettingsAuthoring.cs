using ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    [AddComponentMenu("Custom Authoring/Fly Camera Authoring")]
    public class FlyCameraSettingsAuthoring : MonoBehaviour
    {
        public float acceleration = 1;
        public float sprintMultiplier = 4;
        public float damping = 5;
    }

    public class FlyCameraBaker : Baker<FlyCameraSettingsAuthoring>
    {
        public override void Bake(FlyCameraSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<FirstPersonCameraTag>(entity);
            AddComponent(entity, new FlyCameraSettings
            {
                Acceleration = authoring.acceleration,
                SprintMultiplier = authoring.sprintMultiplier,
                Damping = authoring.damping
            });
        }
    }
}