using System.Linq;
using ECS.BufferElements.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using ChunkAspect = ECS.Aspects.TerrainGeneration.ChunkAspect;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(CreateChunkMeshDataSystem))]
    [BurstCompile]
    public partial struct SetChunkMeshSystem : ISystem
    {
        private BufferLookup<VerticeBufferElement> _verticeBufferLookup;
        private BufferLookup<TriangleBufferElement> _triangleBufferLookup;
        private EntityQuery _chunkQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChunkGenerationSettings>();
            state.RequireForUpdate<SetChunkMeshTag>();
            
            _chunkQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<ChunkAspect>()
                .WithAll<SetChunkMeshTag>()
                .Build(ref state);
            
            _verticeBufferLookup = state.GetBufferLookup<VerticeBufferElement>(true);
            _triangleBufferLookup = state.GetBufferLookup<TriangleBufferElement>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _verticeBufferLookup.Update(ref state);
            _triangleBufferLookup.Update(ref state);

            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var material = SystemAPI.GetSingleton<ChunkGenerationSettings>().Material.Value;

            using var chunks = _chunkQuery.ToEntityArray(Allocator.Temp);
            foreach (var chunk in chunks)
            {
                var mesh = new Mesh
                {
                    vertices = _verticeBufferLookup[chunk].Reinterpret<Vector3>().AsNativeArray().ToArray(),
                    triangles = _triangleBufferLookup[chunk].Reinterpret<int>().AsNativeArray().ToArray()
                };

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                ecb.SetSharedComponentManaged(chunk, new RenderMeshArray(new[] { material }, new[] { mesh }));
                ecb.SetComponent(chunk, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
                ecb.SetComponent(chunk, new RenderBounds { Value = mesh.bounds.ToAABB() });

                ecb.RemoveComponent<VerticeBufferElement>(chunk);
                ecb.RemoveComponent<TriangleBufferElement>(chunk);
                ecb.RemoveComponent<SetChunkMeshTag>(chunk);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}