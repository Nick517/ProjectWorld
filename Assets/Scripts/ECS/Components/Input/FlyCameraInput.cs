using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.Input
{
    public struct FlyCameraInput : IComponentData
    {
        public float3 Movement;
        public bool Sprint;
    }
}