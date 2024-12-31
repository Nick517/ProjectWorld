using ECS.Aspects.TerrainGeneration;
using ECS.BufferElements.TerrainGeneration.Renderer;
using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(CreateRendererMeshSystem))]
    [BurstCompile]
    public partial struct SetRendererMeshSystem : ISystem
    {
        private BufferLookup<VertexBufferElement> _vertexBufferLookup;
        private BufferLookup<TriangleBufferElement> _triangleBufferLookup;
        private EntityQuery _segmentQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<SetRendererMeshTag>();

            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<TerrainSegmentAspect>()
                .WithAll<SetRendererMeshTag>()
                .Build(ref state);

            _vertexBufferLookup = state.GetBufferLookup<VertexBufferElement>(true);
            _triangleBufferLookup = state.GetBufferLookup<TriangleBufferElement>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _vertexBufferLookup.Update(ref state);
            _triangleBufferLookup.Update(ref state);

            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var material = SystemAPI.GetSingleton<BaseSegmentSettings>().Material.Value;

            using var segments = _segmentQuery.ToEntityArray(Allocator.Temp);
            foreach (var seg in segments)
            {
                var mesh = new Mesh
                {
                    vertices = _vertexBufferLookup[seg].Reinterpret<Vector3>().AsNativeArray().ToArray(),
                    triangles = _triangleBufferLookup[seg].Reinterpret<int>().AsNativeArray().ToArray()
                };

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                ecb.SetSharedComponentManaged(seg, new RenderMeshArray(new[] { material }, new[] { mesh }));
                ecb.SetComponent(seg, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
                ecb.SetComponent(seg, new RenderBounds { Value = mesh.bounds.ToAABB() });

                ecb.RemoveComponent<VertexBufferElement>(seg);
                ecb.RemoveComponent<TriangleBufferElement>(seg);
                ecb.RemoveComponent<SetRendererMeshTag>(seg);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}