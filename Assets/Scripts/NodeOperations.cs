using Unity.Mathematics;
using static NodeOperations.NodeType;

public static class NodeOperations
{
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
        Perlin3D,
        Position,
        Skip
    }

    public static float4 GetSample(NodeType type,
        float4 traversalX = default, float4 traversalY = default,
        float4 traversalZ = default, float4 traversalW = default)
    {
        return
            type switch
            {
                Float2 =>
                    new float4(
                        traversalX.x,
                        traversalY.x,
                        0,
                        0),

                Float3 =>
                    new float4(
                        traversalX.x,
                        traversalY.x,
                        traversalZ.x,
                        0),

                Float4 =>
                    new float4(
                        traversalX.x,
                        traversalY.x,
                        traversalZ.x,
                        traversalW.x),

                Add =>
                    traversalX +
                    traversalY,

                Subtract =>
                    traversalX -
                    traversalY,

                Multiply =>
                    traversalX *
                    traversalY,

                Divide =>
                    traversalX /
                    traversalY,

                Perlin3D =>
                    noise.cnoise(
                        traversalX /
                        traversalY.x),

                _ => default
            };
    }
}