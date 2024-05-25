using ECS.Components;
using TerrainGenerationGraph.Scripts;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static ECS.Components.TerrainGenerationTree;
using static ECS.Components.TerrainGenerationTree.TgTree;
using static Unity.Collections.Allocator;

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

            var builder = new BlobBuilder(Temp);
            ref var tgGraph = ref builder.ConstructRoot<TgTree>();

            var nodeArrayBuilder = builder.Allocate(ref tgGraph.Nodes, tgTree.nodes.Count);

            var cacheCount = 0;

            for (var i = 0; i < tgTree.nodes.Count; i++)
            {
                var dto = tgTree.nodes[i];
                ref var node = ref builder.ConstructRoot<Node>();

                node.Type = dto.nodeType;
                node.CacheIndex = dto.cached ? cacheCount : -1;
                if (dto.cached) cacheCount++;
                node.Next = dto.nextIndex.Deserialize();

                nodeArrayBuilder[i] = node;
            }

            var valueArrayBuilder = builder.Allocate(ref tgGraph.Values, tgTree.values.Count);

            for (var i = 0; i < tgTree.values.Count; i++)
                valueArrayBuilder[i] = new float4(tgTree.values[i].Deserialize());

            var blobReference = builder.CreateBlobAssetReference<TgTree>(Persistent);
            blobReference.Value.CacheCount = cacheCount;

            builder.Dispose();

            AddBlobAsset(ref blobReference, out _);
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new TerrainGenerationTree { Blob = blobReference });
        }
    }
}