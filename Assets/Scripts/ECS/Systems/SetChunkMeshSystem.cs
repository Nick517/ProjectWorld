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
            state.RequireForUpdate<SetChunkMeshTagComponent>();

            _verticeBufferLookup = state.GetBufferLookup<VerticeBufferElement>(true);
            _triangleBufferLookup = state.GetBufferLookup<TriangleBufferElement>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _verticeBufferLookup.Update(ref state);
            _triangleBufferLookup.Update(ref state);

            var entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

            foreach (var chunk in SystemAPI.Query<ChunkAspect, RenderMeshArray>().WithAll<SetChunkMeshTagComponent>())
            {
                var chunkAspect = chunk.Item1;
                var renderMeshArray = chunk.Item2;

                var mesh = new Mesh
                {
                    vertices =
                        _verticeBufferLookup[chunkAspect.Entity].Reinterpret<Vector3>().AsNativeArray().ToArray(),
                    triangles = _triangleBufferLookup[chunkAspect.Entity].Reinterpret<int>().AsNativeArray().ToArray()
                };

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                entityCommandBuffer.SetSharedComponentManaged(chunkAspect.Entity,
                    new RenderMeshArray(renderMeshArray.Materials, new[] { mesh }));
                entityCommandBuffer.SetComponent(chunkAspect.Entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

                entityCommandBuffer.RemoveComponent<VerticeBufferElement>(chunkAspect.Entity);
                entityCommandBuffer.RemoveComponent<TriangleBufferElement>(chunkAspect.Entity);

                entityCommandBuffer.RemoveComponent<SetChunkMeshTagComponent>(chunkAspect.Entity);
            }

            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }
}