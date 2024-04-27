using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components
{
    public struct ChunkPositionComponent : IComponentData
    {
        public float3 Position;
    }
}
