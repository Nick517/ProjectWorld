using System;
using System.Collections.Generic;
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

            for (var i = 0; i < 8; i++) RootIndexes[i] = -1;
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

            for (var i = 0; i < 8; i++)
                if (RootIndexes[i] != -1)
                    TraverseFromIndex(RootIndexes[i], action);
        }

        [BurstCompile]
        private void TraverseFromIndex(int index, Action<Node> action)
        {
            if (!IsCreated)
                throw new InvalidOperationException("Octree must be initialized before using TraverseFromIndex.");

            if (index < 0 || index > Count) throw new IndexOutOfRangeException("Index is out of range.");

            using var stack = new NativeList<int>(Allocator.Temp);

            stack.Add(index);

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
        public Octree<T> Union(Octree<T> other, Allocator allocator)
        {
            if (!IsCreated || !other.IsCreated)
                throw new InvalidOperationException("Both octrees must be initialized before unionising.");

            if (!BaseNodeSize.Equals(other.BaseNodeSize))
                throw new ArgumentException("Cannot union octrees with different base node sizes.");

            var result = new Octree<T>(BaseNodeSize, allocator);

            for (var oct = 0; oct < 8; oct++)
            {
                var thisIndex = RootIndexes[oct];
                var otherIndex = other.RootIndexes[oct];

                if (thisIndex != -1 && otherIndex != -1)
                {
                    while (RootNode(oct).Scale < other.RootNode(oct).Scale) thisIndex = UpscaleRoot(oct);

                    while (other.RootNode(oct).Scale < RootNode(oct).Scale) otherIndex = other.UpscaleRoot(oct);

                    result.RootIndexes[oct] = UnionNodes(thisIndex, otherIndex, other, ref result, allocator);

                    continue;
                }

                if (thisIndex != -1) CopyBranchTo(thisIndex, ref result, allocator);

                else if (otherIndex != -1) other.CopyBranchTo(otherIndex, ref result, allocator);
            }

            return result;
        }

        [BurstCompile]
        private int UnionNodes(int thisIndex, int otherIndex,
            Octree<T> other, ref Octree<T> result, Allocator allocator)
        {
            if (thisIndex == -1 && otherIndex == -1) return -1;

            if (thisIndex == -1) return other.CopyBranchToIndex(otherIndex, result.NextIndex(), ref result, allocator);

            if (otherIndex == -1) return CopyBranchToIndex(thisIndex, result.NextIndex(), ref result, allocator);

            var thisNode = Nodes[thisIndex];
            var otherNode = other.Nodes[otherIndex];
            var resultIndex = result.NextIndex();
            var resultNode = new Node(thisNode.Position, thisNode.Scale);

            resultNode.Value = !thisNode.IsDefault ? thisNode.Value : otherNode.Value;

            if (thisNode.IsLeaf && otherNode.IsLeaf)
            {
                result.Nodes[resultIndex] = resultNode;

                return resultIndex;
            }

            resultNode.Alloc(allocator);

            for (var oct = 0; oct < 8; oct++)
                resultNode.ChildIndexes[oct] =
                    UnionNodes(thisNode.ChildIndexes[oct], otherNode.ChildIndexes[oct], other, ref result, allocator);

            result.Nodes[resultIndex] = resultNode;

            return resultIndex;
        }

        [BurstCompile]
        public Octree<T> Except(Octree<T> other, Allocator allocator)
        {
            if (!IsCreated || !other.IsCreated)
                throw new InvalidOperationException("Both octrees must be initialized before exception.");

            if (!BaseNodeSize.Equals(other.BaseNodeSize))
                throw new ArgumentException("Cannot except octrees with different base node sizes.");

            var result = new Octree<T>(BaseNodeSize, allocator);

            for (var oct = 0; oct < 8; oct++)
                result.RootIndexes[oct] =
                    ExceptNodes(RootIndexes[oct], other.RootIndexes[oct], other, ref result, allocator);

            return result;
        }

        [BurstCompile]
        private int ExceptNodes(int thisIndex, int otherIndex,
            Octree<T> other, ref Octree<T> result, Allocator allocator)
        {
            if (thisIndex == -1) return -1;

            if (otherIndex == -1) return CopyBranchToIndex(thisIndex, result.NextIndex(), ref result, allocator);

            var thisNode = Nodes[thisIndex];
            var otherNode = other.Nodes[otherIndex];
            var keep = !thisNode.IsDefault && !thisNode.Value.Equals(otherNode.Value);
            
            var thisIsLeaf = thisNode.IsLeaf;
            var otherIsLeaf = otherNode.IsLeaf;

            if (thisIsLeaf && otherIsLeaf)
                return !keep ? -1 : result.CloneNode(thisNode);

            var resultNode = new Node(thisNode.Position, thisNode.Scale);
            if (keep) resultNode.Value = thisNode.Value;

            resultNode.Alloc(allocator);
            var hasChildren = false;

            for (var oct = 0; oct < 8; oct++)
            {
                var thisChild = thisIsLeaf ? -1 : thisNode.ChildIndexes[oct];
                var otherChild = otherIsLeaf ? -1 : otherNode.ChildIndexes[oct];
                var index = ExceptNodes(thisChild, otherChild, other, ref result, allocator);

                resultNode.ChildIndexes[oct] = index;

                if (index != -1) hasChildren = true;
            }

            if (keep || hasChildren)
            {
                var index = result.NextIndex();
                
                result.Nodes[index] = resultNode;
                
                return index;
            }

            resultNode.Dispose();
            
            return -1;
        }

        [BurstCompile]
        public Octree<T> Intersect(Octree<T> other, Allocator allocator)
        {
            if (!IsCreated || !other.IsCreated)
                throw new InvalidOperationException("Both octrees must be initialized before intersection.");

            if (!BaseNodeSize.Equals(other.BaseNodeSize))
                throw new ArgumentException("Cannot intersect octrees with different base node sizes.");

            var result = new Octree<T>(BaseNodeSize, allocator);

            for (var oct = 0; oct < 8; oct++)
            {
                if (RootIndexes[oct] == -1 || other.RootIndexes[oct] == -1) continue;

                var keep = false;
                var oppOct = 7 - oct;
                var thisRoot = RootNode(oct);
                var otherRoot = other.RootNode(oct);

                while (thisRoot.Scale != otherRoot.Scale)
                    if (thisRoot.Scale > otherRoot.Scale)
                    {
                        var childIndex = thisRoot.ChildIndexes[oppOct];

                        if (childIndex == -1) break;

                        thisRoot = Nodes[childIndex];
                    }
                    else
                    {
                        var childIndex = otherRoot.ChildIndexes[oppOct];

                        if (childIndex == -1) break;

                        otherRoot = other.Nodes[childIndex];
                    }

                var resultRoot = new Node(thisRoot.Position, thisRoot.Scale);

                if (!thisRoot.IsDefault && thisRoot.Value.Equals(otherRoot.Value))
                {
                    resultRoot.Value = thisRoot.Value;
                    keep = true;
                }

                if (!thisRoot.IsLeaf && !otherRoot.IsLeaf)
                {
                    resultRoot.Alloc(_allocator);

                    for (var i = 0; i < 8; i++)
                    {
                        var childIndex =
                            IntersectNodes(thisRoot.ChildIndexes[i], otherRoot.ChildIndexes[i],
                                other, ref result, allocator);
                        resultRoot.ChildIndexes[i] = childIndex;

                        if (childIndex != -1) keep = true;
                    }
                }

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
        private int IntersectNodes(int thisIndex, int otherIndex,
            Octree<T> other, ref Octree<T> result, Allocator allocator)
        {
            if (thisIndex == -1 || otherIndex == -1) return -1;

            var keep = false;
            var thisNode = Nodes[thisIndex];
            var otherNode = other.Nodes[otherIndex];
            var resultNode = new Node(thisNode.Position, thisNode.Scale);

            if (!thisNode.IsDefault && thisNode.Value.Equals(otherNode.Value))
            {
                resultNode.Value = thisNode.Value;
                keep = true;
            }

            if (!thisNode.IsLeaf && !otherNode.IsLeaf)
            {
                resultNode.Alloc(allocator);

                for (var i = 0; i < 8; i++)
                {
                    var childIndex =
                        IntersectNodes(thisNode.ChildIndexes[i], otherNode.ChildIndexes[i],
                            other, ref result, allocator);

                    resultNode.ChildIndexes[i] = childIndex;

                    if (childIndex != -1) keep = true;
                }
            }

            if (keep)
            {
                var index = result.NextIndex();
                result.Nodes[index] = resultNode;

                return index;
            }

            resultNode.Dispose();

            return -1;
        }

        [BurstCompile]
        public void Copy(ref Octree<T> octree)
        {
            for (var oct = 0; oct < 8; oct++)
                if (octree.RootIndexes[oct] != -1)
                    octree.CopyBranchTo(octree.RootIndexes[oct],ref this, _allocator);
        }

        [BurstCompile]
        public void CopyBranchTo(int startIndex, ref Octree<T> other, Allocator allocator)
        {
            var rootIndex = NodeIndexToRootIndex(startIndex);

            var otherIndex = rootIndex != -1 ? other.InitRoot(rootIndex, RootNode(rootIndex).Scale) : other.NextIndex();

            CopyBranchToIndex(startIndex, otherIndex, ref other, allocator);
        }

        [BurstCompile]
        private int CopyBranchToIndex(int startIndex, int otherStartIndex, ref Octree<T> other, Allocator allocator)
        {
            var startNode = Nodes[startIndex];
            var copiedNode = new Node(startNode.Position, startNode.Scale);
            copiedNode.Value = startNode.Value;
            copiedNode.Alloc(allocator);

            if (!startNode.IsLeaf)
                for (var oct = 0; oct < 8; oct++)
                    if (startNode.ChildIndexes[oct] != -1)
                        copiedNode.ChildIndexes[oct] =
                            CopyBranchToIndex(startNode.ChildIndexes[oct], other.NextIndex(), ref other, allocator);

            other.Nodes[otherStartIndex] = copiedNode;

            return otherStartIndex;
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
        private int InitNode(float3 position, int scale = 0, T value = default)
        {
            var index = NextIndex();
            
            Nodes[index] = new Node(position, scale) {Value = value};

            return index;
        }

        private int CloneNode(Node node)
        {
            return InitNode(node.Position, node.Scale, node.Value);
        }

        [BurstCompile]
        private int InitRoot(int octant, int scale = 0)
        {
            var pos = select(-GetSegSize(BaseNodeSize, scale), 0, OctantToBool3(octant));

            var index = InitNode(pos, scale);
            RootIndexes[octant] = index;

            return index;
        }

        [BurstCompile]
        private int UpscaleRoot(int octant)
        {
            var index = NextIndex();
            var oldRoot = RootNode(octant);
            var newRoot = new Node(oldRoot.Position * 2, oldRoot.Scale + 1);

            newRoot.Alloc(_allocator);
            newRoot.ChildIndexes[7 - octant] = RootIndexes[octant];

            Nodes[index] = newRoot;
            RootIndexes[octant] = index;

            return index;
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
            return point.Equals(default) ? 0 : (int)ceil(log2(cmax(abs(point)) / BaseNodeSize));
        }

        [BurstCompile]
        private int NodeIndexToRootIndex(int index)
        {
            for (var oct = 0; oct < 8; oct++)
                if (RootIndexes[oct] == index)
                    return oct;

            return -1;
        }

        [BurstCompile]
        public struct Node : IDisposable
        {
            public T Value;
            public readonly float3 Position;
            public readonly int Scale;
            public NativeArray<int> ChildIndexes;

            public bool IsDefault => EqualityComparer<T>.Default.Equals(Value, default);

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
                ChildIndexes.Dispose();

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