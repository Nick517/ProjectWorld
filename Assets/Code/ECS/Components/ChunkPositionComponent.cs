using Unity.Entities;
using Unity.Mathematics;

namespace Terrain
{
    public struct ChunkPositionComponent : IComponentData
    {
        public float3 position;
    }
}
