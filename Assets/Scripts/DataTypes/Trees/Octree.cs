using System;
using Unity.Burst;
using Unity.Collections;
using Utility.Collections;
using static Unity.Mathematics.math;
using static Utility.SpacialPartitioning.OctantOperations;
using static Utility.SpacialPartitioning.SegmentOperations;
using float3 = Unity.Mathematics.float3;

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

        private Node RootNode(int octant)
        {
            return Nodes[RootIndexes[octant]];
        }

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
        public T GetAtPos(float3 position, int scale = 0)
        {
            if (!IsCreated) throw new InvalidOperationException("Octree must be initialized before using GetAtPos.");

            return GetAtIndex(PosToIndex(position, scale));
        }

        [BurstCompile]
        public void SetAtIndex(T value, int index)
        {
            if (!IsCreated) throw new InvalidOperationException("Octree must be initialized before using SetAtIndex.");

            var node = Nodes[index];
            node.Value = value;
            Nodes[index] = node;
        }

        [BurstCompile]
        public T GetAtIndex(int index)
        {
            if (!IsCreated) throw new InvalidOperationException("Octree must be initialized before using GetAtIndex.");

            if (index < 0 || index > Count) throw new IndexOutOfRangeException("Index is out of range.");

            return Nodes[index].Value;
        }

        [BurstCompile]
        public int PosToIndex(float3 position, int scale = 0)
        {
            if (!IsCreated) throw new InvalidOperationException("Octree must be initialized before using PosToIndex.");

            var index = EncompassPoint(position);
            var node = Nodes[index];
            var pos = node.Position;
            var s = node.Scale;
            var size = GetSegSize(BaseNodeSize, s);

            while (s-- > scale)
            {
                node = Nodes[index];
                size /= 2;

                var o = position >= pos + size;
                var oct = Bool3ToOctant(o);

                if (node.IsLeaf) node.Alloc(_allocator);

                if (node.ChildIndexes[oct] == -1)
                {
                    var i = NextIndex();
                    var childPos = pos + select(0, size, o);
                    node.ChildIndexes[oct] = i;

                    Nodes[i] = new Node(childPos, s);
                }

                Nodes[index] = node;
                index = node.ChildIndexes[oct];
                pos += select(0, size, o);
            }

            return index;
        }

        [BurstCompile]
        public void Traverse(Action<Node> action)
        {
            if (!IsCreated) throw new InvalidOperationException("Octree must be initialized before using Traverse.");

            using var stack = new NativeList<int>(Count, Allocator.Temp);

            for (var i = 0; i < 8; i++)
                if (RootIndexes[i] != -1)
                    stack.Add(RootIndexes[i]);

            while (stack.Length > 0)
            {
                var stackIndex = stack.Length - 1;
                var node = Nodes[stack[stackIndex]];
                stack.RemoveAt(stackIndex);

                action(node);

                if (node.IsLeaf) continue;

                for (var i = 0; i < 8; i++)
                    if (node.ChildIndexes[i] != -1)
                        stack.Add(node.ChildIndexes[i]);
            }
        }

        [BurstCompile]
        public Octree<T> Intersect(Octree<T> other, Allocator allocator)
        {
            if (!IsCreated || !other.IsCreated)
                throw new InvalidOperationException("Both octrees must be initialized before intersection.");

            if (!BaseNodeSize.Equals(other.BaseNodeSize))
                throw new ArgumentException("Cannot intersect octrees with different base node sizes.");

            var result = new Octree<T>(BaseNodeSize, allocator);

            // Iterate over every octant.
            for (var oct = 0; oct < 8; oct++)
            {
                // Skip this octant if either root does not exist.
                if (RootIndexes[oct] == -1 || other.RootIndexes[oct] == -1) continue;

                // Do not keep the result node by default.
                var keep = false;
                var oppOct = 7 - oct;
                var thisRoot = RootNode(oct);
                var otherRoot = other.RootNode(oct);

                // Traverse the larger root, setting it to its child until it is the same scale as the smaller root.
                if (thisRoot.Scale >= otherRoot.Scale)
                    while (thisRoot.Scale > otherRoot.Scale)
                        thisRoot = Nodes[thisRoot.ChildIndexes[oppOct]];
                else
                    while (otherRoot.Scale > thisRoot.Scale)
                        otherRoot = other.Nodes[otherRoot.ChildIndexes[oppOct]];

                // Set the result root to have the same position and scale as the roots.
                var resultRoot = new Node(thisRoot.Position, thisRoot.Scale);

                // If the roots are not default and both roots have the same value, set the result root's value to theirs, and keep the root.
                if (!thisRoot.IsDefault & thisRoot.Value.Equals(otherRoot.Value))
                {
                    resultRoot.Value = thisRoot.Value;
                    keep = true;
                }

                // If both roots have children, continue.
                if (!thisRoot.IsLeaf && !otherRoot.IsLeaf)
                {
                    resultRoot.Alloc(_allocator);

                    // Iterate over every octant.
                    for (var i = 0; i < 8; i++)
                    {
                        var childIndex =
                            IntersectNode(thisRoot.ChildIndexes[i], otherRoot.ChildIndexes[i],
                                other, ref result, allocator);
                        resultRoot.ChildIndexes[i] = childIndex;

                        if (childIndex != -1) keep = true;
                    }
                }

                // Keep the root node.
                if (keep)
                {
                    var index = result.NextIndex();
                    result.Nodes[index] = resultRoot;
                    result.RootIndexes[oct] = index;
                }
                else
                {
                    resultRoot.Dispose();
                }
            }

            return result;
        }

        [BurstCompile]
        private int IntersectNode(int thisIndex, int otherIndex,
            Octree<T> other, ref Octree<T> result, Allocator allocator)
        {
            // Return -1 if either index is invalid.
            if (thisIndex == -1 || otherIndex == -1) return -1;

            // Do not keep the result node by default.
            var keep = false;

            var thisNode = Nodes[thisIndex];
            var otherNode = other.Nodes[otherIndex];
            var resultNode = new Node(thisNode.Position, thisNode.Scale);

            // If nodes are not default and both nodes have equal values, set the result node's value theirs, and keep the node.
            if (!thisNode.IsDefault && thisNode.Value.Equals(otherNode.Value))
            {
                resultNode.Value = thisNode.Value;
                keep = true;
            }

            // If both nodes have children, continue.
            if (!thisNode.IsLeaf && !otherNode.IsLeaf)
            {
                resultNode.Alloc(allocator);

                // Iterate over every octant, keeping the indexes of each child's result.
                for (var i = 0; i < 8; i++)
                {
                    var childIndex =
                        IntersectNode(thisNode.ChildIndexes[i], otherNode.ChildIndexes[i],
                            other, ref result, allocator);
                    resultNode.ChildIndexes[i] = childIndex;

                    if (childIndex != -1) keep = true;
                }
            }

            // If the result node is to be kept, get the next index and add the node to the result.
            if (keep)
            {
                var index = result.NextIndex();
                result.Nodes[index] = resultNode;

                return index;
            }

            resultNode.Dispose();

            // Return -1 if nodes do not intersect, and if no child nodes intersect.
            return -1;
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
        private int NextIndex()
        {
            var index = Count++;

            if (Count > Nodes.Length) Nodes = Nodes.SetSize(Nodes.Length * 2, _allocator);

            return index;
        }

        [BurstCompile]
        private int InitRoot(int octant, int scale = 0)
        {
            var i = NextIndex();
            var pos = select(-GetSegSize(BaseNodeSize, scale), 0, OctantToBool3(octant));

            Nodes[i] = new Node(pos, scale);
            RootIndexes[octant] = i;

            return i;
        }

        [BurstCompile]
        private void UpscaleRoot(int octant)
        {
            var i = NextIndex();
            var oldRoot = RootNode(octant);
            var newRoot = new Node(oldRoot.Position * 2, oldRoot.Scale + 1);

            newRoot.Alloc(_allocator);
            newRoot.ChildIndexes[7 - octant] = RootIndexes[octant];

            Nodes[i] = newRoot;
            RootIndexes[octant] = i;
        }

        [BurstCompile]
        private int EncompassPoint(float3 position)
        {
            var oct = GetOctant(position);

            if (RootIndexes[oct] == -1) InitRoot(oct, MinEncompassingScale(position));

            while (!PointWithinOctant(position, oct)) UpscaleRoot(oct);

            return RootIndexes[oct];
        }

        [BurstCompile]
        private bool PointWithinOctant(float3 position, int octant)
        {
            var root = RootNode(octant);

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
            public readonly float3 Position;
            public readonly int Scale;
            public NativeArray<int> ChildIndexes;
            
            public bool IsDefault => Value.Equals(default(T));

            public bool IsLeaf => !ChildIndexes.IsCreated;

            public Node(float3 position, int scale)
            {
                Value = default;
                Position = position;
                Scale = scale;
                ChildIndexes = default;
            }

            [BurstCompile]
            public void Alloc(Allocator allocator)
            {
                ChildIndexes = new NativeArray<int>(8, allocator);

                for (var i = 0; i < 8; i++) ChildIndexes[i] = -1;
            }

            [BurstCompile]
            public void Dispose()
            {
                if (!IsLeaf) ChildIndexes.Dispose();
            }
        }
    }
}