using Unity.Entities;

namespace ECS.Components.Input
{
    public struct CameraSettings : IComponentData
    {
        public float Sensitivity;
        public float MaxVerticalAngle;
    }
}