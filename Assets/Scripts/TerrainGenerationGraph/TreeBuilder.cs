using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static TerrainGenerationGraph.NodeOperations;
using static TerrainGenerationGraph.TgTree;

namespace TerrainGenerationGraph
{
    [Serializable]
    public class TreeBuilder
    {
        public List<Node> Nodes = new();
        public List<float4> constVals = new();
        public int cacheCount;

        public TreeBuilder(TgGraph graph)
        {
            graph.BuildGraph();
            var traversal = BuildTree(graph.nodes[0].inputPorts[0].ConnectedPort);
            if (Nodes.Any()) return;

            Nodes.Add(new Node { Operation = Operation.Const, Next = new int4(0, yzw: -1) });
            constVals.Add(traversal.Value);
        }

        public BlobAssetReference<TgTree> BuildBlob()
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var tgTree = ref builder.ConstructRoot<TgTree>();

            var nodeArrayBuilder = builder.Allocate(ref tgTree.Nodes, Nodes.Count);
            var constValArrayBuilder = builder.Allocate(ref tgTree.ConstVals, constVals.Count);

            for (var i = 0; i < Nodes.Count; i++) nodeArrayBuilder[i] = Nodes[i];
            for (var i = 0; i < constVals.Count; i++) constValArrayBuilder[i] = constVals[i];

            var blobReference = builder.CreateBlobAssetReference<TgTree>(Allocator.Persistent);
            blobReference.Value.CacheCount = cacheCount;
            
            return blobReference;
        }

        private Traversal BuildTree(TgGraph.Node.OutputPort outputPort)
        {
            if (outputPort == null) return default;

            var inputPorts = outputPort.ParentNode.inputPorts;
            var traversals = inputPorts.Select(inputPort => inputPort.ConnectedPort != null
                ? BuildTree(inputPort.ConnectedPort)
                : new Traversal { IsConst = true, Value = inputPort.constVal }).ToList();

            var isConst = inputPorts.Any() && traversals.All(traversal => traversal.IsConst);
            var values = new float4x4();
            var indexes = new int4(-1);

            for (var i = 0; i < traversals.Count; i++)
            {
                var traversal = traversals[i];
                values[i] = traversal.Value;

                if (isConst || !traversal.IsConst)
                {
                    indexes[i] = traversal.Index;
                    continue;
                }

                indexes[i] = Nodes.Count;
                var item = new Node { Next = new int4(constVals.Count, yzw: -1) };
                Nodes.Add(item);
                constVals.Add(traversal.Value);
            }

            var index = -1;

            if (isConst)
                return new Traversal
                {
                    IsConst = true,
                    Value = GetOutput(outputPort.Operation, values),
                    Index = index
                };

            index = Nodes.Count;

            var node = new Node { Operation = outputPort.Operation, Next = indexes };
            Nodes.Add(node);

            switch (outputPort.CacheIndex)
            {
                case -3:
                    Nodes.Add(new Node
                    {
                        Operation = Operation.CacheSet,
                        Next = new int4(cacheCount, index++, zw: -1)
                    });
                    outputPort.CacheIndex = cacheCount++;
                    break;
                case >= 0:
                    Nodes.Add(new Node
                    {
                        Operation = Operation.CacheGet,
                        Next = new int4(outputPort.CacheIndex, yzw: -1)
                    });
                    node.Next.x = index++;
                    break;
            }

            return new Traversal
            {
                Value = GetOutput(outputPort.Operation, values),
                Index = index
            };
        }

        private struct Traversal
        {
            public bool IsConst;
            public float4 Value;
            public int Index;
        }
    }
}