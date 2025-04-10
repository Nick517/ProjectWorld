using System;
using System.Linq;
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
    public struct Octree<T> : IDisposable where T : unmanaged, IEquatable<T>
    {
        private const int DefaultInitialSize = 2048;

        public NativeArray<Node> Nodes;
        public readonly float BaseNodeSize;
        public int Count { get; private set; }
        public bool IsCreated { get; private set; }

        private NativeArray<int> _rootIndexes;
        private readonly int _initialSize;
        private readonly Allocator _allocator;

        public Octree(float baseNodeSize, Allocator allocator) : this(baseNodeSize, DefaultInitialSize, allocator)
        {
        }

        private Octree(float baseNodeSize, int initialSize, Allocator allocator)
        {
            Nodes = new NativeArray<Node>(initialSize, allocator);
            BaseNodeSize = baseNodeSize;
            Count = 0;
            IsCreated = true;
            _rootIndexes = new NativeArray<int>(8, allocator);
            _initialSize = initialSize;
            _allocator = allocator;

            for (var i = 0; i < 8; i++) _rootIndexes[i] = -1;
        }

        private readonly Node RootNode(int octant)
        {
            return Nodes[_rootIndexes[octant]];
        }

        [BurstCompile]
        public void SetAtPos(T value, float3 position, int scale = 0)
        {
            SetAtIndex(value, PosToIndex(position, scale));
        }

        [BurstCompile]
        public T GetAtPos(float3 position, int scale = 0)
        {
            return GetAtIndex(PosToIndex(position, scale));
        }

        [BurstCompile]
        private void SetAtIndex(T value, int index)
        {
            var node = Nodes[index];
            node.Value = value;
            Nodes[index] = node;
        }

        [BurstCompile]
        private readonly T GetAtIndex(int index)
        {
            return Nodes[index].Value;
        }

        [BurstCompile]
        public int PosToIndex(float3 position, int scale = 0)
        {
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

                pos += select(0, size, o);

                if (node.IsLeaf) node.Alloc(_allocator);
                if (node.ChildIndexes[oct] == -1) node.ChildIndexes[oct] = InitNode(pos, s);

                Nodes[index] = node;
                index = node.ChildIndexes[oct];
            }

            return index;
        }

        [BurstCompile]
        public readonly int GetIndexAtPos(float3 position, int scale = 0)
        {
            var octant = GetOctant(position);
            var index = _rootIndexes[octant];

            if (index == -1) return -1;

            var node = Nodes[index];
            var pos = node.Position;
            var s = node.Scale;

            if (s < scale) return -1;

            var size = GetSegSize(BaseNodeSize, s);

            while (s-- > scale)
            {
                node = Nodes[index];
                size /= 2;

                var o = position >= pos + size;
                var oct = Bool3ToOctant(o);

                pos += select(0, size, o);

                if (node.IsLeaf) return -1;
                if (node.ChildIndexes[oct] == -1) return -1;

                index = node.ChildIndexes[oct];
            }

            return index;
        }

        [BurstCompile]
        public void Traverse<TAction>(in TAction action) where TAction : struct, ITraverseAction
        {
            for (var i = 0; i < 8; i++)
                if (_rootIndexes[i] != -1)
                    TraverseByIndex(_rootIndexes[i], action);
        }

        [BurstCompile]
        private void TraverseByIndex<TAction>(int index, TAction action) where TAction : struct, ITraverseAction
        {
            using var stack = new NativeList<int>(Allocator.Temp);
            stack.Add(index);

            while (stack.Length > 0)
            {
                var node = Nodes[stack[^1]];
                stack.RemoveAt(stack.Length - 1);

                action.Execute(this, node);

                if (node.IsLeaf) continue;

                for (var i = 0; i < 8; i++)
                    if (node.ChildIndexes[i] != -1)
                        stack.Add(node.ChildIndexes[i]);
            }
        }

        [BurstCompile]
        public void Copy(in Octree<T> other)
        {
            Clear();

            for (var oct = 0; oct < 8; oct++)
                if (other._rootIndexes[oct] != -1)
                    _rootIndexes[oct] = CopyByIndex(NextIndex(), other._rootIndexes[oct], other);
        }

        [BurstCompile]
        private int CopyByIndex(int thisIndex, int otherIndex, in Octree<T> other)
        {
            var otherNode = other.Nodes[otherIndex];
            var newNode = new Node(otherNode, true);

            if (!otherNode.IsLeaf)
            {
                newNode.Alloc(_allocator);

                for (var oct = 0; oct < 8; oct++)
                    if (otherNode.ChildIndexes[oct] != -1)
                        newNode.ChildIndexes[oct] = CopyByIndex(NextIndex(), otherNode.ChildIndexes[oct], other);
            }

            Nodes[thisIndex] = newNode;

            return thisIndex;
        }

        [BurstCompile]
        public void Union(in Octree<T> other)
        {
            var result = new Octree<T>(BaseNodeSize, _allocator);

            for (var oct = 0; oct < 8; oct++)
            {
                var thisIndex = _rootIndexes[oct];
                var otherIndex = other._rootIndexes[oct];
                var otherScale = otherIndex == -1 ? 0 : other.Nodes[otherIndex].Scale;

                if (thisIndex == -1 && otherIndex == -1) continue;

                if (otherIndex == -1)
                {
                    result._rootIndexes[oct] = result.CopyByIndex(result.NextIndex(), thisIndex, this);
                }
                else if (thisIndex == -1)
                {
                    result._rootIndexes[oct] = result.CopyByIndex(result.NextIndex(), otherIndex, other);
                }
                else if (Nodes[thisIndex].Scale < otherScale)
                {
                    while (Nodes[thisIndex].Scale < otherScale) thisIndex = UpscaleRoot(oct);

                    result._rootIndexes[oct] = UnionByIndex(thisIndex, otherIndex, other, ref result);
                }
                else if (Nodes[thisIndex].Scale > otherScale)
                {
                    result._rootIndexes[oct] = result.CopyByIndex(result.NextIndex(), thisIndex, this);

                    var resultIndex = result.PosToIndex(other.Nodes[otherIndex].Position, otherScale);
                    var newIndex = result.UnionByIndex(resultIndex, otherIndex, other, ref result, true);

                    result.Nodes[resultIndex] = result.Nodes[newIndex];
                }
                else
                {
                    result._rootIndexes[oct] = UnionByIndex(thisIndex, otherIndex, other, ref result);
                }
            }

            this = result;
        }

        [BurstCompile]
        private int UnionByIndex(int thisIndex, int otherIndex, in Octree<T> other, ref Octree<T> result,
            bool copyThis = false)
        {
            if (thisIndex == -1 && otherIndex == -1) return -1;
            if (thisIndex == -1) return result.CopyByIndex(result.NextIndex(), otherIndex, other);
            if (otherIndex == -1) return result.CopyByIndex(result.NextIndex(), thisIndex, this);

            var thisNode = Nodes[thisIndex];
            var otherNode = other.Nodes[otherIndex];
            var resultIndex = copyThis ? thisIndex : result.NextIndex();
            var resultNode = new Node(thisNode);

            resultNode.Value = !thisNode.IsDefault ? thisNode.Value : otherNode.Value;
            resultNode.Alloc(_allocator);

            for (var oct = 0; oct < 8; oct++)
                resultNode.ChildIndexes[oct] = UnionByIndex(
                    thisNode.IsLeaf ? -1 : thisNode.ChildIndexes[oct],
                    otherNode.IsLeaf ? -1 : otherNode.ChildIndexes[oct],
                    other,
                    ref result);

            result.Nodes[resultIndex] = resultNode;

            return resultIndex;
        }

        [BurstCompile]
        public void Except(in Octree<T> other)
        {
            Except(other,
                new DefaultComparison(),
                new DefaultCollisionAction()
            );
        }

        [BurstCompile]
        public void Except<TComparison>(in Octree<T> other, TComparison comparison)
            where TComparison : struct, IComparison
        {
            Except(other,
                comparison,
                new DefaultCollisionAction()
            );
        }

        [BurstCompile]
        public void Except<TComparison, TAction>(in Octree<T> other, TComparison comparison, TAction action)
            where TComparison : struct, IComparison
            where TAction : struct, ICollisionAction
        {
            var result = new Octree<T>(BaseNodeSize, _allocator);

            for (var oct = 0; oct < 8; oct++)
                result._rootIndexes[oct] =
                    ExceptByIndex(_rootIndexes[oct], other._rootIndexes[oct], other, ref result, comparison, action);

            this = result;
        }

        [BurstCompile]
        private int ExceptByIndex<TComparison, TCollisionAction>(int thisIndex, int otherIndex,
            in Octree<T> other, ref Octree<T> result,
            TComparison comparison,
            TCollisionAction action)
            where TComparison : struct, IComparison
            where TCollisionAction : struct, ICollisionAction
        {
            if (thisIndex == -1) return -1;
            if (otherIndex == -1) return result.CopyByIndex(result.NextIndex(), thisIndex, this);

            var thisNode = Nodes[thisIndex];
            var otherNode = other.Nodes[otherIndex];
            var keep = !thisNode.IsDefault && !comparison.Evaluate(thisNode.Value, otherNode.Value);

            if (thisNode.IsLeaf && otherNode.IsLeaf) return !keep ? -1 : result.InitNode(thisNode);

            var resultNode = new Node(thisNode);
            resultNode.Value = keep ? action.Execute(thisNode.Value, otherNode.Value) : default;

            resultNode.Alloc(_allocator);

            var hasChildren = false;

            for (var oct = 0; oct < 8; oct++)
            {
                var index = ExceptByIndex(
                    thisNode.IsLeaf ? -1 : thisNode.ChildIndexes[oct],
                    otherNode.IsLeaf ? -1 : otherNode.ChildIndexes[oct],
                    other,
                    ref result, comparison, action);

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
        public void Intersect(in Octree<T> other)
        {
            Intersect(other,
                new DefaultComparison(),
                new DefaultCollisionAction()
            );
        }

        [BurstCompile]
        public void Intersect<TComparison>(in Octree<T> other, TComparison comparison)
            where TComparison : struct, IComparison
        {
            Intersect(other,
                comparison,
                new DefaultCollisionAction()
            );
        }

        [BurstCompile]
        public void Intersect<TComparison, TCollisionAction>(in Octree<T> other,
            TComparison comparison,
            TCollisionAction action)
            where TComparison : struct, IComparison
            where TCollisionAction : struct, ICollisionAction
        {
            var result = new Octree<T>(BaseNodeSize, _allocator);

            for (var oct = 0; oct < 8; oct++)
            {
                if (_rootIndexes[oct] == -1 || other._rootIndexes[oct] == -1) continue;

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

                var resultRoot = new Node(thisRoot);

                if (!thisRoot.IsDefault && comparison.Evaluate(thisRoot.Value, otherRoot.Value))
                {
                    resultRoot.Value = action.Execute(thisRoot.Value, otherRoot.Value);
                    keep = true;
                }

                if (!thisRoot.IsLeaf && !otherRoot.IsLeaf)
                {
                    resultRoot.Alloc(_allocator);

                    for (var i = 0; i < 8; i++)
                    {
                        var childIndex = IntersectByIndex(
                            thisRoot.ChildIndexes[i],
                            otherRoot.ChildIndexes[i],
                            other,
                            ref result,
                            comparison,
                            action);

                        resultRoot.ChildIndexes[i] = childIndex;

                        if (childIndex != -1) keep = true;
                    }
                }

                if (keep)
                {
                    var index = result.NextIndex();

                    result.Nodes[index] = resultRoot;
                    result._rootIndexes[oct] = index;
                }
                else
                {
                    resultRoot.Dispose();
                }
            }

            this = result;
        }

        [BurstCompile]
        private int IntersectByIndex<TComparison, TCollisionAction>(int thisIndex, int otherIndex,
            in Octree<T> other, ref Octree<T> result,
            TComparison comparison,
            TCollisionAction action)
            where TComparison : struct, IComparison
            where TCollisionAction : struct, ICollisionAction
        {
            if (thisIndex == -1 || otherIndex == -1) return -1;

            var keep = false;
            var thisNode = Nodes[thisIndex];
            var otherNode = other.Nodes[otherIndex];
            var resultNode = new Node(thisNode);

            if (!thisNode.IsDefault && comparison.Evaluate(thisNode.Value, otherNode.Value))
            {
                resultNode.Value = action.Execute(thisNode.Value, otherNode.Value);
                keep = true;
            }

            if (!thisNode.IsLeaf && !otherNode.IsLeaf)
            {
                resultNode.Alloc(_allocator);

                for (var i = 0; i < 8; i++)
                {
                    var childIndex = IntersectByIndex(
                        thisNode.ChildIndexes[i],
                        otherNode.ChildIndexes[i],
                        other,
                        ref result,
                        comparison,
                        action);

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
        public void Clear()
        {
            Dispose();

            Nodes = new NativeArray<Node>(_initialSize, _allocator);
            _rootIndexes = new NativeArray<int>(8, _allocator);
            Count = 0;
            IsCreated = true;

            for (var i = 0; i < 8; i++) _rootIndexes[i] = -1;
        }

        [BurstCompile]
        public void Dispose()
        {
            for (var i = 0; i < Count; i++) Nodes[i].Dispose();

            Nodes.Dispose();
            _rootIndexes.Dispose();

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
        private int InitNode(Node node)
        {
            return InitNode(node.Position, node.Scale, node.Value);
        }

        [BurstCompile]
        private int InitNode(float3 position, int scale = 0, T value = default)
        {
            var index = NextIndex();

            Nodes[index] = new Node(position, scale, value);

            return index;
        }

        [BurstCompile]
        private int InitRoot(int octant, int scale = 0)
        {
            var pos = select(-GetSegSize(BaseNodeSize, scale), 0, OctantToBool3(octant));
            var index = InitNode(pos, scale);

            _rootIndexes[octant] = index;

            return index;
        }

        [BurstCompile]
        private int UpscaleRoot(int octant)
        {
            var index = NextIndex();
            var oldRoot = RootNode(octant);
            var newRoot = new Node(oldRoot.Position * 2, oldRoot.Scale + 1);

            newRoot.Alloc(_allocator);
            newRoot.ChildIndexes[7 - octant] = _rootIndexes[octant];

            Nodes[index] = newRoot;
            _rootIndexes[octant] = index;

            return index;
        }

        [BurstCompile]
        private int EncompassPoint(float3 position)
        {
            var oct = GetOctant(position);

            if (_rootIndexes[oct] == -1) InitRoot(oct, MinEncompassingScale(position));

            while (!PointWithinOctant(position, oct)) UpscaleRoot(oct);

            return _rootIndexes[oct];
        }

        [BurstCompile]
        private readonly bool PointWithinOctant(float3 position, int octant)
        {
            var root = RootNode(octant);

            return PointWithinSeg(position, root.Position, GetSegSize(BaseNodeSize, root.Scale));
        }

        [BurstCompile]
        private readonly int MinEncompassingScale(float3 point)
        {
            return point.Equals(default) ? 0 : (int)ceil(log2(cmax(abs(point)) / BaseNodeSize));
        }

        [BurstCompile]
        public readonly override string ToString()
        {
            var typeInfo = $"type={typeof(T)}";
            var sizeInfo = $"baseNodeSize={BaseNodeSize}";
            var countInfo = $"count={Count}";
            var rootInfo = $"roots=[{string.Join(",", _rootIndexes.Select(i => i == -1 ? "_" : i.ToString()))}]";

            return $"Octree({typeInfo}, {sizeInfo}, {countInfo}, {rootInfo})";
        }
        
        public interface ITraverseAction
        {
            public void Execute(in Octree<T> octree, in Node node);
        }

        public interface IComparison
        {
            public bool Evaluate(in T a, in T b);
        }

        public interface ICollisionAction
        {
            public T Execute(in T a, in T b);
        }

        [BurstCompile]
        private struct DefaultComparison : IComparison
        {
            [BurstCompile]
            public bool Evaluate(in T a, in T b)
            {
                return a.Equals(b);
            }
        }

        [BurstCompile]
        private struct DefaultCollisionAction : ICollisionAction
        {
            [BurstCompile]
            public T Execute(in T a, in T b)
            {
                return a;
            }
        }

        [BurstCompile]
        public struct Node : IDisposable
        {
            public T Value;
            public readonly float3 Position;
            public readonly int Scale;
            public NativeArray<int> ChildIndexes;

            public readonly bool IsDefault => Value.Equals(default);

            public readonly bool IsLeaf => !ChildIndexes.IsCreated;

            public Node(Node node, bool copyValue = false)
            {
                Value = copyValue ? node.Value : default;
                Position = node.Position;
                Scale = node.Scale;
                ChildIndexes = default;
            }

            public Node(float3 position, int scale, T value = default)
            {
                Value = value;
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

            [BurstCompile]
            public readonly override string ToString()
            {
                var posInfo = $"pos=({Position.x:F2}, {Position.y:F2}, {Position.z:F2})";
                var scaleInfo = $"scale={Scale}";
                var valueInfo = $"value={Value.ToString()}";
                var childInfo = IsLeaf
                    ? "leaf"
                    : $"children=[{string.Join(",", ChildIndexes.Select(i => i == -1 ? "_" : i.ToString()))}]";

                return $"Node({posInfo}, {scaleInfo}, {valueInfo}, {childInfo})";
            }
        }
    }
}