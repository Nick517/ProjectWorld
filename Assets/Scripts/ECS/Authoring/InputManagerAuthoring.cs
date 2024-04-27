using ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    [AddComponentMenu("Custom Authoring/Input Manager Authoring")]
    public class InputManagerAuthoring : MonoBehaviour
    {
        public float cameraSensitivity = 10.0f;
        [Range(0.0f, 90.0f)] public float maxVerticalCameraAngle = 89.0f;
    }

    public class PlayerInputBaker : Baker<InputManagerAuthoring>
    {
        public override void Bake(InputManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<FlyCameraInputComponent>(entity);
            AddComponent<CameraInputComponent>(entity);
            AddComponent(entity, new CameraSettingsComponent
            {
                CameraSensitivity = authoring.cameraSensitivity,
                MaxVerticalCameraAngle = authoring.maxVerticalCameraAngle
            });
        }
    }
}