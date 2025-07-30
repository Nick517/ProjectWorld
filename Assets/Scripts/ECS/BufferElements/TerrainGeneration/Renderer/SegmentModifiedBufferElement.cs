using Unity.Entities;
using Unity.Mathematics;

namespace ECS.BufferElements.TerrainGeneration.Renderer
{
    public struct SegmentModifiedBufferElement : IBufferElementData
    {
        private float3 _value;

        public static implicit operator SegmentModifiedBufferElement(float3 value)
        {
            return new SegmentModifiedBufferElement { _value = value };
        }

        public static implicit operator float3(SegmentModifiedBufferElement element)
        {
            return element._value;
        }
    }
}