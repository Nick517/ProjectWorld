using ECS.Components.TerrainGeneration;
using TerrainGenerationGraph;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static ECS.Components.TerrainGeneration.TerrainGenTree;

namespace ECS.Authoring.TerrainGeneration
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
            ref var tgGraph = ref builder.ConstructRoot<TgTree>();

            var nodeArrayBuilder = builder.Allocate(ref tgGraph.Nodes, tgTree.nodes.Count);

            var cacheCount = 0;

            for (var i = 0; i < tgTree.nodes.Count; i++)
            {
                var dto = tgTree.nodes[i];
                ref var node = ref builder.ConstructRoot<TgTree.Node>();

                node.Type = dto.operation;
                node.CacheIndex = dto.cached ? cacheCount : -1;
                if (dto.cached) cacheCount++;
                node.Next = dto.nextIndex;

                nodeArrayBuilder[i] = node;
            }

            var valueArrayBuilder = builder.Allocate(ref tgGraph.Values, tgTree.values.Count);

            for (var i = 0; i < tgTree.values.Count; i++) valueArrayBuilder[i] = tgTree.values[i];

            var blobReference = builder.CreateBlobAssetReference<TgTree>(Allocator.Persistent);
            blobReference.Value.CacheCount = cacheCount;

            builder.Dispose();

            AddBlobAsset(ref blobReference, out _);
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new TerrainGenTree { Blob = blobReference });
        }
    }
}