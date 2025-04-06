using Unity.Mathematics;
using static TerrainGenerationGraph.NodeOperations.Operation;

namespace TerrainGenerationGraph
{
    public static class NodeOperations
    {
        public enum Operation
        {
            Const,
            CacheGet,
            CacheSet,
            SplitOutX,
            SplitOutY,
            SplitOutZ,
            SplitOutW,
            Float2,
            Float3,
            Float4,
            Position,
            Negate,
            Add,
            Subtract,
            Multiply,
            Divide,
            Perlin3D
        }

        public static float4 GetOutput(Operation operation, float4x4 values)
        {
            return operation switch
            {
                SplitOutX => new float4(values.c0.x, yzw: 0),
                SplitOutY => new float4(values.c0.y, yzw: 0),
                SplitOutZ => new float4(values.c0.z, yzw: 0),
                SplitOutW => new float4(values.c0.w, yzw: 0),
                Float2 => new float4(values.c0.x, values.c1.x, zw: 0),
                Float3 => new float4(values.c0.x, values.c1.x, values.c2.x, 0),
                Float4 => new float4(values.c0.x, values.c1.x, values.c2.x, values.c3.x),
                Negate => -values.c0,
                Add => values.c0 + values.c1,
                Subtract => values.c0 - values.c1,
                Multiply => values.c0 * values.c1,
                Divide => values.c0 / values.c1,
                Perlin3D => noise.cnoise(values.c0 / values.c1.x),
                _ => default
            };
        }
    }
}