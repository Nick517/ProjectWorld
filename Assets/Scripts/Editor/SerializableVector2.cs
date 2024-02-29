using System;
using UnityEngine;

namespace Editor
{
    [Serializable]
    public class SerializableVector2
    {
        public float x;
        public float y;

        public SerializableVector2()
        {
        }

        public SerializableVector2(Vector2 vector2)
        {
            x = vector2.x;
            y = vector2.y;
        }

        public SerializableVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 AsVector2()
        {
            return new Vector2(x, y);
        }

        public void WriteTo(Vector2 vector2)
        {
            vector2.x = x;
            vector2.y = y;
        }
    }
}