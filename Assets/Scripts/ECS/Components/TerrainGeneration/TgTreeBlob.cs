using System;
using TerrainGenerationGraph;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static TerrainGenerationGraph.NodeOperations;

namespace ECS.Components.TerrainGeneration
{
    public struct TgTreeBlob : IComponentData
    {
        public BlobAssetReference<TgTree> Blob;

        public struct Traversal : IDisposable
        {
            public readonly BlobAssetReference<TgTree> Blob;
            public float3 Position;
            public NativeArray<float4> Cache;

            public Traversal(TgTreeBlob tgTreeBlob)
            {
                Blob = tgTreeBlob.Blob;
                Position = default;
                Cache = new NativeArray<float4>(Blob.Value.CacheCount, Allocator.Temp);
            }

            public float Sample(float3 position)
            {
                Position = position;

                return Traverse(Blob.Value.Nodes.Length - 1, this).x;
            }

            public void Dispose()
            {
                Cache.Dispose();
            }
        }

        private static float4 Traverse(int index, Traversal traversal)
        {
            if (index == -1) return default;

            var node = traversal.Blob.Value.Nodes[index];

            return node.Operation switch
            {
                Operation.Const => traversal.Blob.Value.ConstVals[node.Next.x],
                Operation.CacheGet => traversal.Cache[node.Next.x],
                Operation.CacheSet => traversal.Cache[node.Next.x] = Traverse(node.Next.y, traversal),
                Operation.Position => new float4(traversal.Position, 0),
                _ => GetOutput(
                    node.Operation, new float4x4(
                        Traverse(node.Next.x, traversal),
                        Traverse(node.Next.y, traversal),
                        Traverse(node.Next.z, traversal),
                        Traverse(node.Next.w, traversal)))
            };
        }
    }
}