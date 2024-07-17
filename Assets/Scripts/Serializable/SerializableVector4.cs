using System;
using UnityEngine;

namespace Serializable
{
    [Serializable]
    public class SerializableVector4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public SerializableVector4()
        {
        }

        public SerializableVector4(Vector4 vector4)
        {
            x = vector4.x;
            y = vector4.y;
            z = vector4.z;
            w = vector4.w;
        }

        public SerializableVector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vector4 Deserialize()
        {
            return new Vector4(x, y, z, w);
        }
    }
}