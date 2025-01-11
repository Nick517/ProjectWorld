using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Utility.Collections;
using static Unity.Mathematics.math;
using static Utility.SpacialPartitioning.OctantOperations;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace DataTypes.Trees
{
    [BurstCompile]
    public struct Octree<T> : IDisposable
    {
        private const int DefaultInitialSize = 128;

        public NativeArray<Node> Nodes;
        public NativeArray<int> RootIndexes;
        public readonly float BaseNodeSize;
        public int Count { get; private set; }
        public bool IsCreated { get; private set; }

        private readonly int _initialSize;
        private readonly Allocator _allocator;

        public Octree(float baseNodeSize, Allocator allocator) : this(baseNodeSize, DefaultInitialSize, allocator)
        {
        }

        public Octree(float baseNodeSize, int initialSize, Allocator allocator)
        {
            Nodes = new NativeArray<Node>(initialSize, allocator);
            RootIndexes = new NativeArray<int>(8, allocator);
            BaseNodeSize = baseNodeSize;
            _initialSize = initialSize;
            _allocator = allocator;
            Count = 0;
            IsCreated = true;

            for (var i = 0; i < RootIndexes.Length; i++) RootIndexes[i] = -1;
        }

        [BurstCompile]
        public void SetAtPos(T value, float3 position, int scale = 0)
        {
            if (!IsCreated) throw new InvalidOperationException("Octree must be initialized before using SetAtPos.");

            SetAtIndex(value, PosToIndex(position, scale));
        }

        [BurstCompile]
        private void SetAtIndex(T value, int index)
        {
            var node = Nodes[index];
            node.Value = value;
            Nodes[index] = node;
        }

        [BurstCompile]
        private int PosToIndex(float3 position, int scale = 0)
        {
            var octant = GetOctant(position);

            if (RootIndexes[octant] == -1) InitRoot(octant, MinEncompassingScale(position));

            while (!PointWithinOctant(position, octant)) UpscaleRoot(octant);

            var index = RootIndexes[octant];
            var node = Nodes[index];
            var pos = node.Position;
            var s = node.Scale;
            var size = GetSegSize(BaseNodeSize, s);

            while (s-- > scale)
            {
                node = Nodes[index];
                size /= 2;
                
                var o = position >= pos + size;
                var c = Bool3ToOctant[o];

                if (!node.ChildIndexes.IsCreated) node.Alloc(_allocator);

                if (node.ChildIndexes[c] == -1)
                {
                    var i = NextIndex();
                    node.ChildIndexes[c] = i;

                    Nodes[i] = new Node
                    {
                        Position = pos + select(0, size, o),
                        Scale = s
                    };
                }

                Nodes[index] = node;

                index = node.ChildIndexes[c];
                pos += select(0, size, o);
            }

            return index;
        }

        [BurstCompile]
        public void Traverse(Action<Node> action)
        {
            using var stack = new NativeList<int>(Allocator.Temp);

            for (var octant = 0; octant < 8; octant++)
                if (RootIndexes[octant] != -1)
                    stack.Add(RootIndexes[octant]);

            while (stack.Length > 0)
            {
                var stackIndex = stack.Length - 1;
                var nodeIndex = stack[stackIndex];
                stack.RemoveAt(stackIndex);

                var node = Nodes[nodeIndex];
                action(node);

                if (!node.ChildIndexes.IsCreated) continue;

                for (var i = 0; i < 8; i++)
                    if (node.ChildIndexes[i] != -1)
                        stack.Add(node.ChildIndexes[i]);
            }
        }

        [BurstCompile]
        private int NextIndex()
        {
            var index = Count++;

            if (Count > Nodes.Length) Nodes = Nodes.SetSize(Nodes.Length * 2, _allocator);

            return index;
        }

        [BurstCompile]
        private void InitRoot(int octant, int scale = 0)
        {
            var i = NextIndex();

            Nodes[i] = new Node
            {
                Position = select(-GetSegSize(BaseNodeSize, scale), 0, OctantToBool3[octant]),
                Scale = scale
            };

            RootIndexes[octant] = i;
        }

        [BurstCompile]
        private void UpscaleRoot(int octant)
        {
            var i = NextIndex();

            var root = new Node
            {
                Position = Nodes[RootIndexes[octant]].Position * 2,
                Scale = Nodes[RootIndexes[octant]].Scale + 1
            };

            root.Alloc(_allocator);
            root.ChildIndexes[7 - octant] = RootIndexes[octant];

            Nodes[i] = root;
            RootIndexes[octant] = i;
        }

        [BurstCompile]
        public void Clear()
        {
            if (!IsCreated)
                throw new InvalidOperationException("Octree has not been created or has already been disposed.");

            Dispose();

            Nodes = new NativeArray<Node>(_initialSize, _allocator);
            RootIndexes = new NativeArray<int>(8, _allocator);
            Count = 0;
            IsCreated = true;

            for (var i = 0; i < 8; i++) RootIndexes[i] = -1;
        }


        [BurstCompile]
        public void Dispose()
        {
            if (!IsCreated)
                throw new InvalidOperationException("Octree has already been disposed or was never initialized.");

            for (var i = 0; i < Count; i++) Nodes[i].Dispose();

            Nodes.Dispose();
            RootIndexes.Dispose();

            IsCreated = false;
        }


        [BurstCompile]
        private bool PointWithinOctant(float3 position, int octant)
        {
            var root = Nodes[RootIndexes[octant]];

            return PointWithinSeg(position, root.Position, GetSegSize(BaseNodeSize, root.Scale));
        }

        [BurstCompile]
        private int MinEncompassingScale(float3 point)
        {
            if (point.Equals(default)) return 0;

            return (int)ceil(log2(cmax(abs(point)) / BaseNodeSize));
        }

        [BurstCompile]
        public struct Node : IDisposable
        {
            public T Value;
            public float3 Position;
            public int Scale;
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