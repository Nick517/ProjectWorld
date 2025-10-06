using ECS.Components.TerrainGeneration;
using TerrainGenerationGraph;
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
            var blobReference = treeBuilder.BuildBlob();

            AddBlobAsset(ref blobReference, out _);

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TgTreeBlob { Blob = blobReference });
        }
    }
}