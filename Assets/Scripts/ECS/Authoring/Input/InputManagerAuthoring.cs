using ECS.Components.Input;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring.Input
{
    [AddComponentMenu("Custom Authoring/Input Manager Authoring")]
    public class InputManagerAuthoring : MonoBehaviour
    {
        public float cameraSensitivity = 10;
        [Range(0, 90)] public float maxVerticalCameraAngle = 89;
    }

    public class PlayerInputBaker : Baker<InputManagerAuthoring>
    {
        public override void Bake(InputManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<FlyCameraInput>(entity);
            AddComponent<CameraInput>(entity);
            AddComponent<PlayerInput>(entity);
            AddComponent(entity, new CameraSettings
            {
                Sensitivity = authoring.cameraSensitivity,
                MaxVerticalAngle = authoring.maxVerticalCameraAngle
            });
        }
    }
}