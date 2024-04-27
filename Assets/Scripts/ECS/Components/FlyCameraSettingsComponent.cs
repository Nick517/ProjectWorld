using Unity.Entities;

namespace ECS.Components
{
    public struct FlyCameraSettingsComponent : IComponentData
    {
        public float Acceleration;
        public float SprintMultiplier;
        public float Damping;
    }
}