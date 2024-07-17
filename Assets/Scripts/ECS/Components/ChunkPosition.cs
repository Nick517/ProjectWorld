using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components
{
    public struct ChunkPosition : IComponentData
    {
        public float3 Position;
    }
}
