using Unity.Entities;
using Unity.Mathematics;
using static TerrainGenerationGraph.NodeOperations;

namespace TerrainGenerationGraph
{
    public struct TgTree
    {
        public BlobArray<Node> Nodes;
        public BlobArray<float4> ConstVals;
        public int CacheCount;

        public struct Node
        {
            public Operation Operation;
            public int4 Next;
        }
    }
}