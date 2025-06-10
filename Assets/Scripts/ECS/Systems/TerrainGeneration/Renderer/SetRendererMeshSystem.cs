using ECS.Aspects.TerrainGeneration;
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

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<SetRendererMeshTag>();

            _vertexBufferLookup = state.GetBufferLookup<VertexBufferElement>(true);
            _triangleBufferLookup = state.GetBufferLookup<TriangleBufferElement>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _vertexBufferLookup.Update(ref state);
            _triangleBufferLookup.Update(ref state);

            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            var material = settings.Material.Value;

            foreach (var seg in SystemAPI.Query<TerrainSegmentAspect>().WithAll<SetRendererMeshTag>())
            {
                var entity = seg.Entity;

                var mesh = new Mesh
                {
                    vertices = _vertexBufferLookup[entity].Reinterpret<Vector3>().AsNativeArray().ToArray(),
                    triangles = _triangleBufferLookup[entity].Reinterpret<int>().AsNativeArray().ToArray()
                };

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                ecb.SetSharedComponentManaged(entity, new RenderMeshArray(new[] { material }, new[] { mesh }));
                ecb.SetComponent(entity, MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
                ecb.SetComponent(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

                if (seg.Scale <= settings.MaxColliderScale)
                {
                    var vertices = _vertexBufferLookup[entity].Reinterpret<float3>().AsNativeArray();
                    var triangles = new NativeArray<int3>(_triangleBufferLookup[entity].Length / 3, Allocator.Temp);

                    for (var i = 0; i < triangles.Length; i += 3)
                    {
                        var buffer = _triangleBufferLookup[entity];
                        triangles[i] = new int3(buffer[i], buffer[i + 1], buffer[i + 2]);
                    }

                    ecb.AddComponent(entity, new PhysicsCollider { Value = MeshCollider.Create(vertices, triangles) });
                }
                
                ecb.RemoveComponent<VertexBufferElement>(entity);
                ecb.RemoveComponent<TriangleBufferElement>(entity);
                ecb.RemoveComponent<SetRendererMeshTag>(entity);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}