using PlayerInput;
using Unity.Entities;
using UnityEngine;

namespace FlyCamera
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
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<FirstPersonCameraTagComponent>(entity);
            AddComponent<FlyCameraSettingsComponent>(entity, new()
            {
                acceleration = authoring.acceleration,
                sprintMultiplier = authoring.sprintMultiplier,
                damping = authoring.damping
            });
        }
    }
}
