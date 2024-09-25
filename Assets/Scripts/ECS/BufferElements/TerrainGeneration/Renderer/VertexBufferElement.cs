using Unity.Entities;
using Unity.Mathematics;

namespace ECS.BufferElements.TerrainGeneration.Renderer
{
    public struct VertexBufferElement : IBufferElementData
    {
        private float3 _value;

        public static implicit operator VertexBufferElement(float3 value)
        {
            return new VertexBufferElement { _value = value };
        }

        public static implicit operator float3(VertexBufferElement element)
        {
            return element._value;
        }
    }
}