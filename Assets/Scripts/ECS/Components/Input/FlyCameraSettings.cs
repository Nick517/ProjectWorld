using Unity.Entities;

namespace ECS.Components.Input
{
    public struct FlyCameraSettings : IComponentData
    {
        public float Acceleration;
        public float SprintMultiplier;
        public float Damping;
    }
}