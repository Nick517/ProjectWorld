using Unity.Entities;
using Unity.Mathematics;

namespace PlayerInput
{
    public struct FlyCameraInputComponent : IComponentData
    {
        public float3 movement;
        public bool sprint;
    }
}
