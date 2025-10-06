using ECS.BufferElements.TerrainGeneration.Renderer;
using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;
using MeshCollider = Unity.Physics.MeshCollider;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(RendererManagerSystem))]
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
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            var material = settings.Material.Value;

            foreach (var entity in _segmentQuery.ToEntityArray(Allocator.Temp))
            {
                var vertexBuffer = _vertexBufferLookup[entity];
                var triangleBuffer = _triangleBufferLookup[entity];

                var mesh = new Mesh
                {
                    vertices = vertexBuffer.Reinterpret<Vector3>().AsNativeArray().ToArray(),
                    triangles = triangleBuffer.Reinterpret<int>().AsNativeArray().ToArray()
                };

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                ecb.AddSharedComponentManaged(entity, new RenderMeshArray(new[] { material }, new[] { mesh }));
                ecb.AddComponent(entity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
                ecb.AddComponent(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

                var vertices = vertexBuffer.Reinterpret<float3>().AsNativeArray();
                var triangles = new NativeArray<int3>(triangleBuffer.Length / 3, Allocator.Temp);

                for (var i = 0; i < triangles.Length; i += 3)
                    triangles[i] = new int3(
                        triangleBuffer[i],
                        triangleBuffer[i + 1],
                        triangleBuffer[i + 2]
                    );

                ecb.AddComponent(entity, new PhysicsCollider { Value = MeshCollider.Create(vertices, triangles) });
                ecb.RemoveComponent<VertexBufferElement>(entity);
                ecb.RemoveComponent<TriangleBufferElement>(entity);
                ecb.RemoveComponent<SetRendererMeshTag>(entity);

                triangles.Dispose();
            }

            ecb.Playback(state.EntityManager);
        }
    }
}