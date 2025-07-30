using Unity.Entities;
using Unity.Mathematics;

namespace ECS.BufferElements.TerrainGeneration
{
    public struct TerrainModificationBufferElement : IBufferElementData
    {
        public float3 Origin;
        public float Range;
        public bool Addition;
    }
}