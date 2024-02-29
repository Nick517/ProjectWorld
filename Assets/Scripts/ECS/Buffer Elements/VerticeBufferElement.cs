using Unity.Entities;
using Unity.Mathematics;

namespace Terrain
{
    public struct VerticeBufferElement : IBufferElementData
    {
        public float3 value;

        public static implicit operator VerticeBufferElement(float3 value) => new() { value = value };

        public static implicit operator float3(VerticeBufferElement element) => element.value;
    }
}
