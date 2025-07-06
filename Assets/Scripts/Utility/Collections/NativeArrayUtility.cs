using Unity.Mathematics;

namespace Utility.Collections
{
    public static class NativeArrayUtility
    {
        public static class Cubic
        {
            public static int GetFlatIndex(int sideLength, int3 index3D)
            {
                return GetFlatIndex(sideLength, index3D.x, index3D.y, index3D.z);
            }
            
            public static int GetFlatIndex(int sideLength, int x, int y, int z)
            {
                return x +
                       y * sideLength +
                       z * sideLength * sideLength;
            }
        }
    }
}