using Unity.Entities;

namespace Terrain
{
    public struct TriangleBufferElement : IBufferElementData
    {
        public int value;

        public static implicit operator TriangleBufferElement(int value) => new() { value = value };

        public static implicit operator int(TriangleBufferElement element) => element.value;
    }
}
