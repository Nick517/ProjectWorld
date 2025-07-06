using Unity.Collections;
using Unity.Mathematics;

namespace Utility.Collections
{
    public static class NativeArrayExtensions
    {
        public static NativeArray<T> SetSize<T>(this NativeArray<T> array, int size, Allocator allocator)
            where T : struct
        {
            var newArray = new NativeArray<T>(size, allocator);
            var count = math.min(array.Length, size);

            for (var i = 0; i < count; i++) newArray[i] = array[i];

            array.Dispose();

            return newArray;
        }
    }
}