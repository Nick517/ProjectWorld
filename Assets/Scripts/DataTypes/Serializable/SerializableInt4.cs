using System;
using Unity.Mathematics;

namespace DataTypes.Serializable
{
    [Serializable]
    public class SerializableInt4
    {
        public int x;
        public int y;
        public int z;
        public int w;

        public SerializableInt4()
        {
        }

        private SerializableInt4(int4 int4)
        {
            x = int4.x;
            y = int4.y;
            z = int4.z;
            w = int4.w;
        }

        public static implicit operator SerializableInt4(int4 int4)
        {
            return new SerializableInt4(int4);
        }

        public static implicit operator int4(SerializableInt4 serializableInt4)
        {
            return new int4(serializableInt4.x, serializableInt4.y, serializableInt4.z, serializableInt4.w);
        }
    }
}