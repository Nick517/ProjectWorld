using Unity.Entities;

namespace ECS.BufferElements
{
    public struct TriangleBufferElement : IBufferElementData
    {
        private int _value;

        public static implicit operator TriangleBufferElement(int value)
        {
            return new TriangleBufferElement { _value = value };
        }

        public static implicit operator int(TriangleBufferElement element)
        {
            return element._value;
        }
    }
}