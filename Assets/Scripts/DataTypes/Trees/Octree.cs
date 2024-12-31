using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Utility.Collections;
using Utility.Math;
using static Utility.TerrainGeneration.SegmentOperations;

namespace DataTypes.Trees
{
    [BurstCompile]
    public struct Octree<T> : IDisposable
    {
        public NativeArray<Node> Nodes;
        public NativeArray<int> RootIndexes;
        public NativeArray<int> RootScales;
        public readonly float BaseNodeSize;
        public int Count { get; private set; }
        public bool IsCreated { get; private set; }

        private readonly Allocator _allocator;

        public Octree(float baseNodeSize, Allocator allocator) : this(baseNodeSize, 128, allocator)
        {
        }

        private Octree(float baseNodeSize, int initialSize, Allocator allocator)
        {
            Nodes = new NativeArray<Node>(initialSize, allocator);
            RootIndexes = new NativeArray<int>(8, allocator);
            RootScales = new NativeArray<int>(8, allocator);
            BaseNodeSize = baseNodeSize;
            _allocator = allocator;
            Count = 0;
            IsCreated = true;

            for (var i = 0; i < RootIndexes.Length; i++) RootIndexes[i] = -1;
        }

        [BurstCompile]
        public T GetValueAtPos(float3 position, int scale = 0)
        {
            return Nodes[PosToIndex(position, scale)].Value;
        }

        [BurstCompile]
        public void SetValueAtPos(T value, float3 position, int scale = 0)
        {
            SetValueAtIndex(PosToIndex(position, scale), value);
        }

        [BurstCompile]
        public int PosToIndex(float3 position, int scale = 0)
        {
            var o = position >= 0;
            var r = o.ToIndex();
            var c = (!o).ToIndex();

            var size = GetSegSize(BaseNodeSize, RootScales[r]);
            var pos = GetClosestSegPos(o.ToSign() * (float3)BaseNodeSize / 2, size);

            if (RootIndexes[r] == -1) RootIndexes[r] = AllocNode();

            while (!PointWithinSeg(position, pos, size))
            {
                var i = AllocNode();
                var node = Nodes[i];
                node.Alloc(_allocator);
                node.ChildIndexes[c] = RootIndexes[r];
                
                Nodes[i] = node;
                RootIndexes[r] = i;
                RootScales[r]++;
                
                size *= 2;
                pos *= 2;
            }

            var index = RootIndexes[r];
            var s = RootScales[r];

            while (s-- > scale)
            {
                size /= 2;

                var b = position >= pos + size;
                var i = b.ToIndex();
                
                var node = Nodes[index];
                node.Alloc(_allocator);
                
                if (node.ChildIndexes[i] == -1) node.ChildIndexes[i] = AllocNode();

                Nodes[index] = node;
                index = node.ChildIndexes[i];
                pos += math.select(0, size, b);
            }

            return index;
        }

        [BurstCompile]
        public T GetValueAtIndex(int index)
        {
            return index >= 0 && index < Count ? Nodes[index].Value : default;
        }

        [BurstCompile]
        public void SetValueAtIndex(int index, T value)
        {
            var node = Nodes[index];
            node.Value = value;
            Nodes[index] = node;
        }

        [BurstCompile]
        public NativeList<int> GetChildIndexes(int index)
        {
            var indexes = new NativeList<int>(Allocator.Temp);
            var node = Nodes[index];
            
            indexes.Add(index);

            if (!node.ChildIndexes.IsCreated) return indexes;
            
            for (var c = 0; c < 8; c++) indexes.AddRange(GetChildIndexes(node.ChildIndexes[c]).AsArray());

            return indexes;
        }

        [BurstCompile]
        public NativeList<int> GetLeafIndexes(int index)
        {
            var indexes = new NativeList<int>(Allocator.Temp);
            var node = Nodes[index];

            if (!node.ChildIndexes.IsCreated)
            {
                indexes.Add(index);
                
                return indexes;
            }
            
            for (var c = 0; c < 8; c++) indexes.AddRange(GetLeafIndexes(node.ChildIndexes[c]).AsArray());
            
            return indexes;
        }

        [BurstCompile]
        public int AllocNode()
        {
            var index = Count++;

            if (Count > Nodes.Length) Nodes = Nodes.SetSize(Nodes.Length * 2, _allocator);

            Nodes[index] = new Node();

            return index;
        }

        [BurstCompile]
        public void Dispose()
        {
            if (!IsCreated) return;
            
            for (var i = 0; i < Count; i++) Nodes[i].Dispose();

            Nodes.Dispose();
            RootIndexes.Dispose();
            RootScales.Dispose();
            
            IsCreated = false;
        }

        [BurstCompile]
        public struct Node : IDisposable
        {
            public T Value;
            public NativeArray<int> ChildIndexes;

            [BurstCompile]
            public void Alloc(Allocator allocator)
            {
                if (ChildIndexes.IsCreated) return;

                ChildIndexes = new NativeArray<int>(8, allocator);

                for (var i = 0; i < 8; i++) ChildIndexes[i] = -1;
            }

            [BurstCompile]
            public void Dispose()
            {
                if (ChildIndexes.IsCreated) ChildIndexes.Dispose();
            }
        }
    }
}