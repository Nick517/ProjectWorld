using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components
{
    public struct CameraInputComponent : IComponentData
    {
        public float2 LookDelta;
    }
}
