using Unity.Entities;

namespace ECS.Components
{
    public struct CameraSettingsComponent : IComponentData
    {
        public float CameraSensitivity;
        public float MaxVerticalCameraAngle;
    }
}