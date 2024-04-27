using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components
{
    public struct TgGraphComponent : IComponentData
    {
        public BlobAssetReference<TgGraphData> Blob;

        public float Traverse(float3 position)
        {
            return Blob.Value.TraverseTree(position);
        }
    }

    public struct TgGraphData
    {
        public BlobArray<Node> Nodes;
        public BlobArray<float4> Values;

        public struct Node
        {
            public NodeType Type;
            public int4 Next;
        }

        public enum NodeType
        {
            Value,
            Float2,
            Float3,
            Float4,
            Add,
            Subtract,
            Multiply,
            Divide,
            Perlin4D,
            Position
        }


        public float TraverseTree(float3 position)
        {
            return Traverse(0, position).x;
        }

        private float4 Traverse(int index, float3 position)
        {
            if (index == -1) return float4.zero;

            var node = Nodes[index];

            switch (node.Type)
            {
                case NodeType.Value:
                    return Values[node.Next.x];

                case NodeType.Float2:
                    return new float4(
                        Traverse(node.Next.x, position).x,
                        Traverse(node.Next.y, position).x,
                        0,
                        0);

                case NodeType.Float3:
                    return new float4(
                        Traverse(node.Next.x, position).x,
                        Traverse(node.Next.y, position).x,
                        Traverse(node.Next.z, position).x,
                        0);

                case NodeType.Float4:
                    return new float4(
                        Traverse(node.Next.x, position).x,
                        Traverse(node.Next.y, position).x,
                        Traverse(node.Next.z, position).x,
                        Traverse(node.Next.w, position).x);

                case NodeType.Add:
                    return Traverse(node.Next.x, position) +
                           Traverse(node.Next.y, position);

                case NodeType.Subtract:
                    return Traverse(node.Next.x, position) -
                           Traverse(node.Next.y, position);

                case NodeType.Multiply:
                    return Traverse(node.Next.x, position) *
                           Traverse(node.Next.y, position);

                case NodeType.Divide:
                    return Traverse(node.Next.x, position) /
                           Traverse(node.Next.y, position);

                case NodeType.Perlin4D:
                    return noise.cnoise(1 / position);

                case NodeType.Position:
                    return new float4(position.x, position.y, position.z, 0);

                default:
                    return float4.zero;
            }
        }
    }
}