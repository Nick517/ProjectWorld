using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components
{
    public struct FlyCameraInputComponent : IComponentData
    {
        public float3 Movement;
        public bool Sprint;
    }
}