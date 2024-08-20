using Unity.Entities;
using Unity.Mathematics;

namespace ECS.BufferElements.TerrainGeneration
{
    public struct VerticeBufferElement : IBufferElementData
    {
        private float3 _value;

        public static implicit operator VerticeBufferElement(float3 value)
        {
            return new VerticeBufferElement { _value = value };
        }

        public static implicit operator float3(VerticeBufferElement element)
        {
            return element._value;
        }
    }
}