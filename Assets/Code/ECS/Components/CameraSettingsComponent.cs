using Unity.Entities;

namespace PlayerInput
{
    public struct CameraSettingsComponent : IComponentData
    {
        public float cameraSensitivity;
        public float maxVerticalCameraAngle;
    }
}
