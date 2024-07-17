using System;
using Unity.Mathematics;

namespace Serializable
{
    [Serializable]
    public class SerializableFloat4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public SerializableFloat4()
        {
        }

        private SerializableFloat4(float4 float4)
        {
            x = float4.x;
            y = float4.y;
            z = float4.z;
            w = float4.w;
        }

        public static implicit operator SerializableFloat4(float4 float4)
        {
            return new SerializableFloat4(float4);
        }

        public static implicit operator float4(SerializableFloat4 serializableFloat4)
        {
            return new float4(serializableFloat4.x, serializableFloat4.y, serializableFloat4.z, serializableFloat4.w);
        }
    }
}