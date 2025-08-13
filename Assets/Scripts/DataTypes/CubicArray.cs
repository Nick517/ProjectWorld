using System;
using Unity.Collections;
using Unity.Mathematics;

namespace DataTypes
{
    public struct CubicArray<T> : IDisposable where T : unmanaged
    {
        public NativeArray<T> Array;
        public readonly int SideLength;

        public CubicArray(int sideLength, Allocator allocator)
        {
            Array = new NativeArray<T>(sideLength * sideLength * sideLength, allocator);
            SideLength = sideLength;
        }

        public CubicArray(int sideLength, NativeArray<T> array)
        {
            Array = array;
            SideLength = sideLength;
        }

        public readonly T GetAt(int3 index3D)
        {
            return Array[GetFlatIndex(index3D)];
        }

        public void SetAt(int3 index3D, T value)
        {
            Array[GetFlatIndex(index3D)] = value;
        }

        public void Dispose()
        {
            Array.Dispose();
        }

        private readonly int GetFlatIndex(int3 index3D)
        {
            return index3D.x +
                   index3D.y * SideLength +
                   index3D.z * SideLength * SideLength;
        }
    }
}