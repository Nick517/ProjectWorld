using Unity.Entities;
using UnityEngine;

namespace PlayerInput
{
    [AddComponentMenu("Custom Authoring/Input Manager Authoring")]
    public class InputManagerAuthoring : MonoBehaviour
    {
        public float cameraSensitivity = 10.0f;
        [Range(0.0f, 90.0f)]
        public float maxVerticalCameraAngle = 89.0f;
    }

    public class PlayerInputBaker : Baker<InputManagerAuthoring>
    {
        public override void Bake(InputManagerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<FlyCameraInputComponent>(entity);
            AddComponent<CameraInputComponent>(entity);
            AddComponent<CameraSettingsComponent>(entity, new()
            {
                cameraSensitivity = authoring.cameraSensitivity,
                maxVerticalCameraAngle = authoring.maxVerticalCameraAngle
            });
        }
    }
}
