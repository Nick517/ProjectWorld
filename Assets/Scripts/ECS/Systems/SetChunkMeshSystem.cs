using ECS.Aspects;
using ECS.BufferElements;
using ECS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace ECS.Systems
{
    [UpdateAfter(typeof(CreateChunkMeshDataSystem))]
    [BurstCompile]
    public partial struct SetChunkMeshSystem : ISystem
    {
        private BufferLookup<VerticeBufferElement> _verticeBufferLookup;
        private BufferLookup<TriangleBufferElement> _triangleBufferLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SetChunkMeshTag>();

            _verticeBufferLookup = state.GetBufferLookup<VerticeBufferElement>(true);
            _triangleBufferLookup = state.GetBufferLookup<TriangleBufferElement>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _verticeBufferLookup.Update(ref state);
            _triangleBufferLookup.Update(ref state);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var chunkAspect in SystemAPI.Query<ChunkAspect, RenderMeshArray>().WithAll<SetChunkMeshTag>())
            {
                var chunk = chunkAspect.Item1;
                var renderMeshArray = chunkAspect.Item2;

                var mesh = new Mesh
                {
                    vertices =
                        _verticeBufferLookup[chunk.Entity].Reinterpret<Vector3>().AsNativeArray().ToArray(),
                    triangles = _triangleBufferLookup[chunk.Entity].Reinterpret<int>().AsNativeArray().ToArray()
                };

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                ecb.SetSharedComponentManaged(chunk.Entity,
                    new RenderMeshArray(renderMeshArray.Materials, new[] { mesh }));
                ecb.SetComponent(chunk.Entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

                ecb.RemoveComponent<VerticeBufferElement>(chunk.Entity);
                ecb.RemoveComponent<TriangleBufferElement>(chunk.Entity);

                ecb.RemoveComponent<SetChunkMeshTag>(chunk.Entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}