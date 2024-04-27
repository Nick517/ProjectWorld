using ECS.Components;
using TerrainGenerationGraph.Scripts;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Authoring
{
    [AddComponentMenu("Custom Authoring/TgGraph Authoring")]
    public class TgGraphAuthoring : MonoBehaviour
    {
        public TgGraph tgGraph;
    }

    public class TgGraphBaker : Baker<TgGraphAuthoring>
    {
        public override void Bake(TgGraphAuthoring authoring)
        {
            var tgTree = authoring.tgGraph.DeserializeTree();

            var builder = new BlobBuilder(Allocator.Temp);
            ref var tgGraph = ref builder.ConstructRoot<TgGraphData>();

            var nodeArrayBuilder = builder.Allocate(ref tgGraph.Nodes, tgTree.nodes.Count);

            for (var i = 0; i < tgTree.nodes.Count; i++)
            {
                var dto = tgTree.nodes[i];
                ref var node = ref builder.ConstructRoot<TgGraphData.Node>();

                node.Type = dto.nodeType;
                node.Next = dto.nextIndexes.Deserialize();

                nodeArrayBuilder[i] = node;
            }

            var valueArrayBuilder = builder.Allocate(
                ref tgGraph.Values,
                tgTree.values.Count
            );

            for (var i = 0; i < tgTree.values.Count; i++)
                valueArrayBuilder[i] = new float4(tgTree.values[i].Deserialize());

            var blobReference = builder.CreateBlobAssetReference<TgGraphData>(Allocator.Persistent);

            builder.Dispose();

            AddBlobAsset(ref blobReference, out _);
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TgGraphComponent { Blob = blobReference });
        }
    }
}