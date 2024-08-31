using ECS.Aspects.TerrainGeneration;
using ECS.BufferElements.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(CreateTerrainSegmentMeshDataSystem))]
    [BurstCompile]
    public partial struct SetTerrainSegmentMeshSystem : ISystem
    {
        private BufferLookup<VerticeBufferElement> _verticeBufferLookup;
        private BufferLookup<TriangleBufferElement> _triangleBufferLookup;
        private EntityQuery _chunkQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TerrainSegmentGenerationSettings>();
            state.RequireForUpdate<SetTerrainSegmentMeshTag>();

            _chunkQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<TerrainSegmentAspect>()
                .WithAll<SetTerrainSegmentMeshTag>()
                .Build(ref state);

            _verticeBufferLookup = state.GetBufferLookup<VerticeBufferElement>(true);
            _triangleBufferLookup = state.GetBufferLookup<TriangleBufferElement>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _verticeBufferLookup.Update(ref state);
            _triangleBufferLookup.Update(ref state);

            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var material = SystemAPI.GetSingleton<TerrainSegmentGenerationSettings>().Material.Value;

            using var segments = _chunkQuery.ToEntityArray(Allocator.Temp);
            foreach (var seg in segments)
            {
                var mesh = new Mesh
                {
                    vertices = _verticeBufferLookup[seg].Reinterpret<Vector3>().AsNativeArray().ToArray(),
                    triangles = _triangleBufferLookup[seg].Reinterpret<int>().AsNativeArray().ToArray()
                };

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                ecb.SetSharedComponentManaged(seg, new RenderMeshArray(new[] { material }, new[] { mesh }));
                ecb.SetComponent(seg, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
                ecb.SetComponent(seg, new RenderBounds { Value = mesh.bounds.ToAABB() });

                ecb.RemoveComponent<VerticeBufferElement>(seg);
                ecb.RemoveComponent<TriangleBufferElement>(seg);
                ecb.RemoveComponent<SetTerrainSegmentMeshTag>(seg);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}