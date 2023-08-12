using Unity.Entities;

namespace FlyCamera
{
    public struct FlyCameraSettingsComponent : IComponentData
    {
        public float acceleration;
        public float sprintMultiplier;
        public float damping;
    }
}
