using System;
using Unity.Collections;
using Unity.Mathematics;
using static Utility.Collections.NativeArrayUtility.Cubic;

namespace DataTypes
{
    public struct CubicArray<T> : IDisposable where T : unmanaged
    {
        public NativeArray<T> Array;
        
        private readonly int _sideLength;
        
        public CubicArray(int sideLength, Allocator allocator)
        {
            Array = new NativeArray<T>(sideLength * sideLength * sideLength, allocator);
            _sideLength = sideLength;
        }

        public CubicArray(int sideLength, NativeArray<T> array)
        {
            Array = array;
            _sideLength = sideLength;
        }

        public T GetAt(int3 index3D)
        {
            return GetAt(index3D.x, index3D.y, index3D.z);
        }

        public T GetAt(int x, int y, int z)
        {
            return Array[GetFlatIndex(_sideLength, x, y, z)];
        }

        public void SetAt(int3 index3D, T value)
        {
            SetAt(index3D.x, index3D.y, index3D.z, value);
        }
        
        public void SetAt(int x, int y, int z, T value)
        {
            Array[GetFlatIndex(_sideLength, x, y, z)] = value;
        }
        
        public void Dispose()
        {
            Array.Dispose();
        }
    }
}