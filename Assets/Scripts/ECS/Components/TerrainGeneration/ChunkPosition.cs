using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components.TerrainGeneration
{
    public struct ChunkPosition : IComponentData
    {
        public float3 Position;
    }
}
