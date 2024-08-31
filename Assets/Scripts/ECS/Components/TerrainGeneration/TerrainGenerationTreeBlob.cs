using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Utility.TerrainGeneration.NodeOperations;
using static Utility.TerrainGeneration.NodeOperations.Operation;
using static Unity.Collections.Allocator;

namespace ECS.Components.TerrainGeneration
{
    public struct TerrainGenerationTreeBlob : IComponentData
    {
        public BlobAssetReference<TerrainGenerationTree> Blob;

        public struct TerrainGenerationTree
        {
            public BlobArray<Node> Nodes;
            public BlobArray<float4> Values;
            public int CacheCount;

            public struct Node
            {
                public Operation Type;
                public int CacheIndex;
                public int4 Next;
            }

            public struct Traversal : IDisposable
            {
                public readonly BlobAssetReference<TerrainGenerationTree> Blob;
                public float4 Position;
                public NativeArray<float4> Cache;
                public NativeArray<bool> Cached;

                public Traversal(TerrainGenerationTreeBlob terrainGenerationTreeBlob)
                {
                    Blob = terrainGenerationTreeBlob.Blob;
                    Position = default;
                    Cache = new NativeArray<float4>(Blob.Value.CacheCount, Temp);
                    Cached = new NativeArray<bool>(Cache.Length, Temp);
                }

                public float Sample(float3 position)
                {
                    Position = new float4(position.x, position.y, position.z, 0);

                    for (var x = 0; x < Cached.Length; x++) Cached[x] = false;

                    var traversal = Traverse(0, this).x;

                    ResetCache();

                    return traversal;
                }

                private void ResetCache()
                {
                    for (var i = 0; i < Cached.Length; i++) Cached[i] = false;
                }

                public void Dispose()
                {
                    Cache.Dispose();
                    Cached.Dispose();
                }
            }

            private static float4 Traverse(int index, Traversal traversal)
            {
                if (index == -1) return default;

                var node = traversal.Blob.Value.Nodes[index];

                if (node.CacheIndex != -1 && traversal.Cached[node.CacheIndex]) return traversal.Cache[node.CacheIndex];

                var sample =
                    node.Type switch
                    {
                        Value =>
                            traversal.Blob.Value.Values[node.Next.x],

                        Position =>
                            new float4(
                                traversal.Position.x,
                                traversal.Position.y,
                                traversal.Position.z,
                                0),

                        _ => GetOutput(
                            node.Type,
                            Traverse(node.Next.x, traversal),
                            Traverse(node.Next.y, traversal),
                            Traverse(node.Next.z, traversal),
                            Traverse(node.Next.w, traversal))
                    };

                if (node.CacheIndex != -1)
                {
                    traversal.Cached[node.CacheIndex] = true;
                    traversal.Cache[node.CacheIndex] = sample;
                }

                return sample;
            }
        }
    }
}