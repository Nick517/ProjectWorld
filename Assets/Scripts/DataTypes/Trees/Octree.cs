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
        public readonly Allocator Allocator;
        public int Count { get; private set; }
        public bool IsCreated { get; private set; }

        private NativeArray<int> _rootIndexes;
        private readonly int _initialSize;
        private readonly T _defaultValue;

        public Octree(float baseNodeSize, Allocator allocator, T defaultValue = default)
        {
            Nodes = new NativeArray<Node>(DefaultInitialSize, allocator);
            BaseNodeSize = baseNodeSize;
            Allocator = allocator;
            Count = 0;
            IsCreated = true;
            _rootIndexes = new NativeArray<int>(8, allocator);
            _initialSize = DefaultInitialSize;

            for (var i = 0; i < 8; i++) _rootIndexes[i] = -1;
            _defaultValue = defaultValue;
        }

        private readonly Node RootNode(int octant)
        {
            return Nodes[_rootIndexes[octant]];
        }

        [BurstCompile]
        public int SetAtPos(T value, float3 position, int scale = 0)
        {
            var index = PosToIndex(position, scale);
            SetAtIndex(value, index);
            return index;
        }

        [BurstCompile]
        public T GetAtPos(float3 position, int scale = 0)
        {
            return GetAtIndex(PosToIndex(position, scale));
        }

        [BurstCompile]
        public void SetAtIndex(T value, int index)
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

                if (node.GetChild(oct) == -1) node.SetChild(oct, InitNode(pos, s, _defaultValue));

                Nodes[index] = node;
                index = node.GetChild(oct);
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
                if (node.GetChild(oct) == -1) return -1;

                index = node.GetChild(oct);
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
                {
                    var child = node.GetChild(i);
                    if (child != -1) stack.Add(child);
                }
            }
        }

        [BurstCompile]
        public void Copy(in Octree<T> other)
        {
            Clear();

            for (var oct = 0; oct < 8; oct++)
                if (other._rootIndexes[oct] != -1)
                    _rootIndexes[oct] = CopyByIndex(other._rootIndexes[oct], other);
        }

        [BurstCompile]
        private int CopyByIndex(int otherIndex, in Octree<T> other)
        {
            if (otherIndex == -1) return -1;

            var otherNode = other.Nodes[otherIndex];
            var newNode = new Node(otherNode.Position, otherNode.Scale, otherNode.Value);
            var newIndex = NextIndex();

            if (!otherNode.IsLeaf)
                for (var oct = 0; oct < 8; oct++)
                    newNode.SetChild(oct, CopyByIndex(otherNode.GetChild(oct), other));

            Nodes[newIndex] = newNode;

            return newIndex;
        }

        [BurstCompile]
        public void Union(in Octree<T> other)
        {
            var result = new Octree<T>(BaseNodeSize, Allocator);

            for (var oct = 0; oct < 8; oct++)
            {
                var thisIndex = _rootIndexes[oct];
                var otherIndex = other._rootIndexes[oct];

                if (thisIndex == -1 && otherIndex == -1) continue;

                var otherScale = otherIndex == -1 ? 0 : other.Nodes[otherIndex].Scale;

                if (otherIndex == -1)
                {
                    result._rootIndexes[oct] = result.CopyByIndex(thisIndex, this);
                }
                else if (thisIndex == -1)
                {
                    result._rootIndexes[oct] = result.CopyByIndex(otherIndex, other);
                }
                else if (Nodes[thisIndex].Scale < otherScale)
                {
                    while (Nodes[thisIndex].Scale < otherScale) thisIndex = UpscaleRoot(oct);

                    result._rootIndexes[oct] = UnionByIndex(thisIndex, otherIndex, other, ref result);
                }
                else if (Nodes[thisIndex].Scale > otherScale)
                {
                    result._rootIndexes[oct] = result.CopyByIndex(thisIndex, this);

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
            if (thisIndex == -1) return result.CopyByIndex(otherIndex, other);
            if (otherIndex == -1) return result.CopyByIndex(thisIndex, this);

            var thisNode = Nodes[thisIndex];
            var otherNode = other.Nodes[otherIndex];
            var resultIndex = copyThis ? thisIndex : result.NextIndex();
            var resultNode = new Node(thisNode.Position, thisNode.Scale,
                !IsDefault(thisNode) ? thisNode.Value : otherNode.Value);

            for (var oct = 0; oct < 8; oct++)
                resultNode.SetChild(oct, UnionByIndex(
                    thisNode.IsLeaf ? -1 : thisNode.GetChild(oct),
                    otherNode.IsLeaf ? -1 : otherNode.GetChild(oct),
                    other,
                    ref result));

            result.Nodes[resultIndex] = resultNode;

            return resultIndex;
        }

        [BurstCompile]
        public void Except(in Octree<T> other)
        {
            this = Except(this, other,
                Allocator,
                new DefaultComparison(),
                new DefaultCollisionAction()
            );
        }

        [BurstCompile]
        public void Except<TComparison>(in Octree<T> other, TComparison comparison)
            where TComparison : struct, IComparison
        {
            this = Except(this, other,
                Allocator,
                comparison,
                new DefaultCollisionAction()
            );
        }

        public static Octree<T> Except(in Octree<T> treeA, in Octree<T> treeB,
            Allocator allocator)
        {
            return Except(treeA, treeB,
                allocator,
                new DefaultComparison(),
                new DefaultCollisionAction()
            );
        }

        public static Octree<T> Except<TComparison>(in Octree<T> treeA, in Octree<T> treeB,
            Allocator allocator, TComparison comparison)
            where TComparison : struct, IComparison
        {
            return Except(treeA, treeB,
                allocator,
                comparison,
                new DefaultCollisionAction()
            );
        }

        [BurstCompile]
        public static Octree<T> Except<TComparison, TAction>(in Octree<T> treeA, in Octree<T> treeB,
            Allocator allocator, TComparison comparison, TAction action)
            where TComparison : struct, IComparison
            where TAction : struct, ICollisionAction
        {
            var treeR = new Octree<T>(treeA.BaseNodeSize, allocator);

            for (var oct = 0; oct < 8; oct++)
            {
                var indexA = treeA._rootIndexes[oct];
                var indexB = treeB._rootIndexes[oct];

                if (indexA == -1) continue;
                if (indexB == -1)
                {
                    treeR._rootIndexes[oct] = treeR.CopyByIndex(indexA, treeA);
                    continue;
                }

                var oppOct = 7 - oct;

                if (treeA.Nodes[indexA].Scale > treeB.Nodes[indexB].Scale)
                {
                    treeR._rootIndexes[oct] = ExceptLargerTree(
                        indexA, indexB, oppOct, treeB.Nodes[indexB].Scale,
                        treeA, treeB, ref treeR,
                        allocator, comparison, action);
                }
                else
                {
                    while (indexB != -1 && treeA.Nodes[indexA].Scale < treeB.Nodes[indexB].Scale)
                        indexB = treeB.Nodes[indexB].GetChild(oppOct);

                    treeR._rootIndexes[oct] = ExceptByIndex(
                        indexA, indexB,
                        treeA, treeB, ref treeR,
                        allocator, comparison, action);
                }
            }

            return treeR;
        }

        [BurstCompile]
        private static int ExceptLargerTree<TComparison, TAction>(int indexA, int indexB, int oppOct, int scale,
            in Octree<T> treeA, in Octree<T> treeB, ref Octree<T> treeR,
            Allocator allocator, TComparison comparison, TAction action)
            where TComparison : struct, IComparison
            where TAction : struct, ICollisionAction
        {
            if (indexA == -1 || indexB == -1) return -1;

            var nodeA = treeA.Nodes[indexA];

            if (nodeA.Scale == scale)
                return ExceptByIndex(
                    indexA, indexB,
                    treeA, treeB, ref treeR,
                    allocator, comparison, action);

            var keep = false;
            var childIndexes = new NativeArray<int>(8, allocator);

            for (var o = 0; o < 8; o++)
                if (o != oppOct)
                {
                    childIndexes[o] = treeR.CopyByIndex(nodeA.GetChild(o), treeA);
                    if (childIndexes[o] != -1) keep = true;
                }

            childIndexes[oppOct] = ExceptLargerTree(
                nodeA.GetChild(oppOct), indexB, oppOct, scale,
                treeA, treeB, ref treeR,
                allocator, comparison, action);

            if (!keep && childIndexes[oppOct] == -1)
            {
                childIndexes.Dispose();
                return -1;
            }

            var index = treeR.NextIndex();
            var node = new Node(nodeA.Position, nodeA.Scale, nodeA.Value);

            node.SetChildren(childIndexes);
            treeR.Nodes[index] = node;

            return index;
        }

        [BurstCompile]
        private static int ExceptByIndex<TComparison, TCollisionAction>(int indexA, int indexB,
            in Octree<T> treeA, in Octree<T> treeB, ref Octree<T> result,
            Allocator allocator, TComparison comparison, TCollisionAction action)
            where TComparison : struct, IComparison
            where TCollisionAction : struct, ICollisionAction
        {
            if (indexA == -1) return -1;
            if (indexB == -1) return result.CopyByIndex(indexA, treeA);

            var nodeA = treeA.Nodes[indexA];
            var nodeB = treeB.Nodes[indexB];
            var collides = !treeA.IsDefault(nodeA) && comparison.Evaluate(nodeA.Value, nodeB.Value);

            if (nodeA.IsLeaf)
                return !collides
                    ? result.InitNode(nodeA.Position, nodeA.Scale, action.Execute(nodeA.Value, nodeB.Value))
                    : -1;

            if (nodeB.IsLeaf) return !collides ? result.CopyByIndex(indexA, treeA) : -1;

            var keep = collides;
            var childIndexes = new NativeArray<int>(8, allocator);

            for (var oct = 0; oct < 8; oct++)
            {
                childIndexes[oct] = ExceptByIndex(
                    nodeA.GetChild(oct), nodeB.GetChild(oct),
                    treeA, treeB, ref result,
                    allocator, comparison, action);

                if (childIndexes[oct] != -1) keep = true;
            }

            if (!keep)
            {
                childIndexes.Dispose();
                return -1;
            }

            var index = result.NextIndex();
            var node = new Node(nodeA.Position, nodeA.Scale, treeA._defaultValue);
            node.SetChildren(childIndexes);

            if (collides) node.Value = action.Execute(node.Value, nodeB.Value);

            result.Nodes[index] = node;

            return index;
        }

        [BurstCompile]
        public void Intersect(in Octree<T> other)
        {
            this = Intersect(this, other,
                Allocator,
                new DefaultComparison(),
                new DefaultCollisionAction()
            );
        }

        [BurstCompile]
        public void Intersect<TComparison>(in Octree<T> other, TComparison comparison)
            where TComparison : struct, IComparison
        {
            this = Intersect(this, other,
                Allocator,
                comparison,
                new DefaultCollisionAction()
            );
        }

        public static Octree<T> Intersect(in Octree<T> treeA, in Octree<T> treeB,
            Allocator allocator)
        {
            return Intersect(treeA, treeB,
                allocator,
                new DefaultComparison(),
                new DefaultCollisionAction()
            );
        }

        public static Octree<T> Intersect<TComparison>(in Octree<T> treeA, in Octree<T> treeB,
            Allocator allocator, TComparison comparison)
            where TComparison : struct, IComparison
        {
            return Intersect(treeA, treeB,
                allocator,
                comparison,
                new DefaultCollisionAction()
            );
        }

        [BurstCompile]
        public static Octree<T> Intersect<TComparison, TCollisionAction>(in Octree<T> treeA, in Octree<T> treeB,
            Allocator allocator, TComparison comparison, TCollisionAction action)
            where TComparison : struct, IComparison
            where TCollisionAction : struct, ICollisionAction
        {
            var treeR = new Octree<T>(treeA.BaseNodeSize, allocator);

            for (var oct = 0; oct < 8; oct++)
            {
                var indexA = treeA._rootIndexes[oct];
                var indexB = treeB._rootIndexes[oct];

                if (indexA == -1 || indexB == -1) continue;

                var oppOct = 7 - oct;

                while (indexA != -1 && treeA.Nodes[indexA].Scale > treeB.Nodes[indexB].Scale)
                    indexA = treeA.Nodes[indexA].GetChild(oppOct);

                while (indexB != -1 && treeB.Nodes[indexB].Scale > treeA.Nodes[indexA].Scale)
                    indexB = treeB.Nodes[indexB].GetChild(oppOct);

                treeR._rootIndexes[oct] = IntersectByIndex(
                    indexA, indexB,
                    treeA, treeB, ref treeR,
                    allocator, comparison, action);
            }

            return treeR;
        }

        [BurstCompile]
        private static int IntersectByIndex<TComparison, TCollisionAction>(int indexA, int indexB,
            in Octree<T> treeA, in Octree<T> treeB, ref Octree<T> result,
            Allocator allocator, TComparison comparison, TCollisionAction action)
            where TComparison : struct, IComparison
            where TCollisionAction : struct, ICollisionAction
        {
            if (indexA == -1 || indexB == -1) return -1;

            var nodeA = treeA.Nodes[indexA];
            var nodeB = treeB.Nodes[indexB];
            var collides = !treeA.IsDefault(nodeA) && comparison.Evaluate(nodeA.Value, nodeB.Value);

            if (nodeA.IsLeaf || nodeB.IsLeaf)
                return collides
                    ? result.InitNode(nodeA.Position, nodeA.Scale, action.Execute(nodeA.Value, nodeB.Value))
                    : -1;

            var keep = collides;
            var childIndexes = new NativeArray<int>(8, allocator);

            for (var oct = 0; oct < 8; oct++)
            {
                childIndexes[oct] = IntersectByIndex(
                    nodeA.GetChild(oct), nodeB.GetChild(oct),
                    treeA, treeB, ref result,
                    allocator, comparison, action);

                if (childIndexes[oct] != -1) keep = true;
            }

            if (!keep)
            {
                childIndexes.Dispose();
                return -1;
            }

            var index = result.NextIndex();
            var node = new Node(nodeA.Position, nodeA.Scale, treeA._defaultValue);
            node.SetChildren(childIndexes);

            if (collides) node.Value = action.Execute(node.Value, nodeB.Value);

            result.Nodes[index] = node;

            return index;
        }

        [BurstCompile]
        public void Subdivide(int index)
        {
            if (index == -1) return;

            var node = Nodes[index];
            var childScale = node.Scale - 1;
            var childSize = GetSegSize(BaseNodeSize, childScale);

            for (var oct = 0; oct < 8; oct++)
            {
                var pos = node.Position + select(0, childSize, OctantToBool3(oct));
                node.SetChild(oct, InitNode(pos, childScale, _defaultValue));
            }

            Nodes[index] = node;
        }

        [BurstCompile]
        public void Clear()
        {
            Dispose();

            Nodes = new NativeArray<Node>(_initialSize, Allocator);
            _rootIndexes = new NativeArray<int>(8, Allocator);
            Count = 0;
            IsCreated = true;

            for (var i = 0; i < 8; i++) _rootIndexes[i] = -1;
        }

        [BurstCompile]
        public void Dispose()
        {
            Nodes.Dispose();
            _rootIndexes.Dispose();

            IsCreated = false;
        }

        [BurstCompile]
        private int NextIndex()
        {
            var index = Count++;

            if (Count > Nodes.Length) Nodes = Nodes.SetSize(Nodes.Length * 2, Allocator);

            return index;
        }

        [BurstCompile]
        public int InitNode(float3 position, int scale, T value)
        {
            var index = NextIndex();

            Nodes[index] = new Node(position, scale, value);

            return index;
        }

        [BurstCompile]
        private int InitRoot(int octant, int scale = 0)
        {
            var pos = select(-GetSegSize(BaseNodeSize, scale), 0, OctantToBool3(octant));
            var index = InitNode(pos, scale, _defaultValue);

            _rootIndexes[octant] = index;

            return index;
        }

        [BurstCompile]
        private int UpscaleRoot(int octant)
        {
            var index = NextIndex();
            var oldRoot = RootNode(octant);
            var newRoot = new Node(oldRoot.Position * 2, oldRoot.Scale + 1, _defaultValue);

            newRoot.SetChild(7 - octant, _rootIndexes[octant]);

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
        private readonly struct DefaultComparison : IComparison
        {
            [BurstCompile]
            public bool Evaluate(in T a, in T b)
            {
                return a.Equals(b);
            }
        }

        [BurstCompile]
        private readonly struct DefaultCollisionAction : ICollisionAction
        {
            [BurstCompile]
            public T Execute(in T a, in T b)
            {
                return a;
            }
        }

        [BurstCompile]
        private readonly bool IsDefault(Node node)
        {
            return node.Value.Equals(_defaultValue);
        }

        [BurstCompile]
        public struct Node
        {
            public T Value;
            public readonly float3 Position;
            public readonly int Scale;
            public bool IsLeaf { get; private set; }

            private int _child0, _child1, _child2, _child3, _child4, _child5, _child6, _child7;

            public Node(float3 position, int scale, T value)
            {
                Value = value;
                Position = position;
                Scale = scale;
                IsLeaf = true;
                _child0 = _child1 = _child2 = _child3 = _child4 = _child5 = _child6 = _child7 = -1;
            }

            [BurstCompile]
            public readonly int GetChild(int oct)
            {
                return oct switch
                {
                    0 => _child0,
                    1 => _child1,
                    2 => _child2,
                    3 => _child3,
                    4 => _child4,
                    5 => _child5,
                    6 => _child6,
                    7 => _child7,
                    _ => -1
                };
            }

            [BurstCompile]
            public void SetChild(int oct, int index)
            {
                IsLeaf = false;

                switch (oct)
                {
                    case 0: _child0 = index; break;
                    case 1: _child1 = index; break;
                    case 2: _child2 = index; break;
                    case 3: _child3 = index; break;
                    case 4: _child4 = index; break;
                    case 5: _child5 = index; break;
                    case 6: _child6 = index; break;
                    case 7: _child7 = index; break;
                }
            }

            [BurstCompile]
            public void SetChildren(NativeArray<int> children)
            {
                for (var i = 0; i < 8; i++) SetChild(i, children[i]);
            }

            [BurstCompile]
            public readonly override string ToString()
            {
                var posInfo = $"pos=({Position.x:F2}, {Position.y:F2}, {Position.z:F2})";
                var scaleInfo = $"scale={Scale}";
                var valueInfo = $"value={Value.ToString()}";
                var childInfo = IsLeaf ? "leaf" : "children=[";

                if (!IsLeaf)
                    for (var i = 0; i < 8; i++)
                    {
                        var c = GetChild(i);
                        childInfo += c != -1 ? c : "_";
                        childInfo += i != 7 ? "," : "]";
                    }

                return $"Node({posInfo}, {scaleInfo}, {valueInfo}, {childInfo})";
            }
        }
    }
}