using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.Input
{
    public struct FlyCamera : IComponentData
    {
        public float Acceleration;
        public float SprintMultiplier;
        public float Damping;
        public float3 Velocity;
    }
}