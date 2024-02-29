using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Terrain
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

            EntityCommandBuffer entityCommandBuffer = new(Allocator.Temp);

            foreach ((ChunkAspect, RenderMeshArray) chunk in SystemAPI.Query<ChunkAspect, RenderMeshArray>().WithAll<SetChunkMeshTagComponent>())
            {
                ChunkAspect chunkAspect = chunk.Item1;
                RenderMeshArray renderMeshArray = chunk.Item2;

                Mesh mesh = new()
                {
                    vertices = _verticeBufferLookup[chunkAspect.entity].Reinterpret<Vector3>().AsNativeArray().ToArray(),
                    triangles = _triangleBufferLookup[chunkAspect.entity].Reinterpret<int>().AsNativeArray().ToArray()
                };

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                entityCommandBuffer.SetSharedComponentManaged<RenderMeshArray>(chunkAspect.entity, new(renderMeshArray.Materials, new Mesh[] { mesh }));
                entityCommandBuffer.SetComponent<RenderBounds>(chunkAspect.entity, new() { Value = mesh.bounds.ToAABB() });

                entityCommandBuffer.RemoveComponent<VerticeBufferElement>(chunkAspect.entity);
                entityCommandBuffer.RemoveComponent<TriangleBufferElement>(chunkAspect.entity);

                entityCommandBuffer.RemoveComponent<SetChunkMeshTagComponent>(chunkAspect.entity);
            }

            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }
}
