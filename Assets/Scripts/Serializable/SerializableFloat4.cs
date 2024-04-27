using System;
using Unity.Mathematics;
using UnityEngine;

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

        public SerializableFloat4(Vector4 vector4)
        {
            x = vector4.x;
            y = vector4.y;
            z = vector4.z;
            w = vector4.w;
        }

        public SerializableFloat4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float4 Deserialize()
        {
            return new float4(x, y, z, w);
        }
    }
}