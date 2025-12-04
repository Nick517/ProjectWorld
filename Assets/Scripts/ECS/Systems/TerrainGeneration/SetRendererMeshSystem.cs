using ECS.BufferElements.TerrainGeneration.Renderer;
using ECS.Components.TerrainGeneration;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace ECS.Systems.TerrainGeneration
{
    public partial struct SetRendererMeshSystem : ISystem
    {
        private BufferLookup<VertexBufferElement> _vertexBufferLookup;
        private BufferLookup<TriangleBufferElement> _triangleBufferLookup;
        private EntityQuery _segmentQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<SetRendererMeshTag>();

            _vertexBufferLookup = state.GetBufferLookup<VertexBufferElement>(true);
            _triangleBufferLookup = state.GetBufferLookup<TriangleBufferElement>(true);

            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SetRendererMeshTag>()
                .Build(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
            _vertexBufferLookup.Update(ref state);
            _triangleBufferLookup.Update(ref state);

            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var material = SystemAPI.GetSingleton<BaseSegmentSettings>().Material.Value;

            foreach (var entity in _segmentQuery.ToEntityArray(Allocator.Temp))
            {
                var info = SystemAPI.GetComponent<SegmentInfo>(entity);
                var vertexBuffer = _vertexBufferLookup[entity];
                var triangleBuffer = _triangleBufferLookup[entity];

                var mesh = new Mesh
                {
                    vertices = vertexBuffer.Reinterpret<Vector3>().AsNativeArray().ToArray(),
                    triangles = triangleBuffer.Reinterpret<int>().AsNativeArray().ToArray()
                };

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                ecb.AddSharedComponentManaged(entity,
                    new RenderMeshArray(new[] { material }, new[] { mesh }));

                ecb.AddComponent(entity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
                ecb.AddComponent(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });
                
                ecb.AddComponent(entity, LocalTransform.FromPosition(info.Position));

                ecb.RemoveComponent<VertexBufferElement>(entity);
                ecb.RemoveComponent<TriangleBufferElement>(entity);
                ecb.RemoveComponent<SetRendererMeshTag>(entity);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}