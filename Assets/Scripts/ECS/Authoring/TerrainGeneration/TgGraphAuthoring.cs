using ECS.Components.TerrainGeneration;
using TerrainGenerationGraph;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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
            var treeBuilder = new TreeBuilder(authoring.tgGraph);

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var tgTree = ref builder.ConstructRoot<TgTree>();

            var nodeArrayBuilder = builder.Allocate(ref tgTree.Nodes, treeBuilder.Nodes.Count);
            var constValArrayBuilder = builder.Allocate(ref tgTree.ConstVals, treeBuilder.constVals.Count);

            for (var i = 0; i < treeBuilder.Nodes.Count; i++) nodeArrayBuilder[i] = treeBuilder.Nodes[i];
            for (var i = 0; i < treeBuilder.constVals.Count; i++) constValArrayBuilder[i] = treeBuilder.constVals[i];

            var blobReference = builder.CreateBlobAssetReference<TgTree>(Allocator.Persistent);
            blobReference.Value.CacheCount = treeBuilder.cacheCount;

            AddBlobAsset(ref blobReference, out _);

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TgTreeBlob { Blob = blobReference });
        }
    }
}