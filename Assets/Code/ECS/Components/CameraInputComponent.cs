using Unity.Entities;
using Unity.Mathematics;

namespace PlayerInput
{
    public struct CameraInputComponent : IComponentData
    {
        public float2 lookDelta;
    }
}
