using Unity.Entities;

namespace ECS.Components
{
    public struct CameraSettings : IComponentData
    {
        public float Sensitivity;
        public float MaxVerticalAngle;
    }
}