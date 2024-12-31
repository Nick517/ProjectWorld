using System;
using UnityEngine;

namespace DataTypes.Serializable
{
    [Serializable]
    public class SerializableVector2
    {
        public float x;
        public float y;

        public SerializableVector2()
        {
        }

        private SerializableVector2(Vector2 vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }

        public static implicit operator SerializableVector2(Vector2 vector2)
        {
            return new SerializableVector2(vector2);
        }

        public static implicit operator Vector2(SerializableVector2 serializableVector2)
        {
            return new Vector2(serializableVector2.x, serializableVector2.y);
        }
    }
}