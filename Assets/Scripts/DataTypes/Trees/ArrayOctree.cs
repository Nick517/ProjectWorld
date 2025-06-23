using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Utility.Collections;

namespace DataTypes.Trees
{
    [BurstCompile]
    public struct ArrayOctree<T> : IDisposable where T : unmanaged, IEquatable<T>
    {
        private readonly int _size;
        private int Count { get; set; }
        public bool IsCreated { get; }
        private Octree<int> _octree;
        private NativeArray<T> _elements;

        public ArrayOctree(float baseNodeSize, int size, Allocator allocator)
        {
            _size = size;
            _octree = new Octree<int>(baseNodeSize, allocator, -1);
            _elements = new NativeArray<T>(_size, allocator);
            Count = 0;
            IsCreated = true;
        }

        [BurstCompile]
        public int PosToIndex(float3 position, int scale = 0)
        {
            var octreeIndex = _octree.PosToIndex(position, scale);
            var node = _octree.Nodes[octreeIndex];

            if (node.Value == -1)
            {
                node.Value = NextIndex();
                _octree.Nodes[octreeIndex] = node;
            }
            
            return node.Value;
        }

        [BurstCompile]
        public int GetIndexAtPos(float3 position, int scale = 0)
        {
            var index = _octree.PosToIndex(position, scale);
            
            if (index == -1) return -1;
            
            return _octree.Nodes[index].Value;
        }
        
        [BurstCompile]
        public NativeArray<T> GetArray(int index, Allocator allocator)
        {
            var offset = _size * index;
            var array = new NativeArray<T>(_size, allocator);

            for (var i = 0; i < _size; i++) array[i] = _elements[offset + i];

            return array;
        }

        [BurstCompile]
        public void SetArray(int index, NativeArray<T> array)
        {
            var offset = _size * index;

            for (var i = 0; i < _size; i++) _elements[offset + i] = array[i];
        }
        
        [BurstCompile]
        private int NextIndex()
        {
            var index = Count++;

            if (Count * _size > _elements.Length) _elements = _elements.SetSize(_elements.Length * 2, _octree.Allocator);

            return index;
        }

        [BurstCompile]
        public void Dispose()
        {
            if (!IsCreated) return;
            
            _octree.Dispose();
            _elements.Dispose();
        }
    }
}