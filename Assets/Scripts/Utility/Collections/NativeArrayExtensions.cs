using Unity.Collections;
using Unity.Mathematics;

namespace Utility.Collections
{
    public static class NativeArrayExtensions
    {
        public static NativeArray<T> SetSize<T>(this NativeArray<T> nativeArray, int size, Allocator allocator)
            where T : struct
        {
            var newArray = new NativeArray<T>(size, allocator);
            var count = math.min(nativeArray.Length, size);

            for (var i = 0; i < count; i++) newArray[i] = nativeArray[i];

            nativeArray.Dispose();

            return newArray;
        }
    }
}