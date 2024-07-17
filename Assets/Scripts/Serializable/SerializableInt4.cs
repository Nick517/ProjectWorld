using System;
using Unity.Mathematics;

namespace Serializable
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

        public SerializableInt4(int4 int4)
        {
            x = int4.x;
            y = int4.y;
            z = int4.z;
            w = int4.w;
        }

        public SerializableInt4(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public int4 Deserialize()
        {
            return new int4(x, y, z, w);
        }
    }
}