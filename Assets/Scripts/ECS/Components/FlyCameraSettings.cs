using Unity.Entities;

namespace ECS.Components
{
    public struct FlyCameraSettings : IComponentData
    {
        public float Acceleration;
        public float SprintMultiplier;
        public float Damping;
    }
}