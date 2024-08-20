using Unity.Mathematics;
using static Utility.TerrainGeneration.NodeOperations.Operation;

namespace Utility.TerrainGeneration
{
    public static class NodeOperations
    {
        public enum Operation
        {
            Skip,
            Value,
            SplitOutX,
            SplitOutY,
            SplitOutZ,
            SplitOutW,
            Float2,
            Float3,
            Float4,
            Position,
            Add,
            Subtract,
            Multiply,
            Divide,
            Perlin3D
        }

        public static float4 GetOutput(Operation type, float4 inputX, float4 inputY, float4 inputZ, float4 inputW)
        {
            return type switch
            {
                SplitOutX => new float4(inputX.x, 0, 0, 0),

                SplitOutY => new float4(inputX.y, 0, 0, 0),

                SplitOutZ => new float4(inputX.z, 0, 0, 0),

                SplitOutW => new float4(inputX.w, 0, 0, 0),

                Float2 => new float4(inputX.x, inputY.x, 0, 0),

                Float3 => new float4(inputX.x, inputY.x, inputZ.x, 0),

                Float4 => new float4(inputX.x, inputY.x, inputZ.x, inputW.x),

                Add => inputX + inputY,

                Subtract => inputX - inputY,

                Multiply => inputX * inputY,

                Divide => inputX / inputY,

                Perlin3D => noise.cnoise(inputX / inputY.x),

                _ => default
            };
        }
    }
}