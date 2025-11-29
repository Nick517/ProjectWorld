using ECS.BufferElements.TerrainGeneration.Renderer;
using ECS.Components.TerrainGeneration;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;
using MeshCollider = Unity.Physics.MeshCollider;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(CreateMeshSystem))]
    public partial struct SetMeshSystem : ISystem
    {
        private BufferLookup<VertexBufferElement> _vertexBufferLookup;
        private BufferLookup<TriangleBufferElement> _triangleBufferLookup;
        private EntityQuery _segmentQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<SetMeshTag>();

            _vertexBufferLookup = state.GetBufferLookup<VertexBufferElement>(true);
            _triangleBufferLookup = state.GetBufferLookup<TriangleBufferElement>(true);

            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SetMeshTag>()
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
                var info = state.EntityManager.GetComponentData<SegmentInfo>(entity);
                var vertexBuffer = _vertexBufferLookup[entity];
                var triangleBuffer = _triangleBufferLookup[entity];

                if (info.IsRenderer)
                {
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
                }
                else
                {
                    using var vertices = vertexBuffer.Reinterpret<Vector3>().AsNativeArray().Reinterpret<float3>();
                    var triangles = new NativeArray<int3>(triangleBuffer.Length / 3, Allocator.Temp);

                    for (var i = 0; i < triangles.Length; i++)
                        triangles[i] = new int3(
                            triangleBuffer[i * 3 + 0],
                            triangleBuffer[i * 3 + 1],
                            triangleBuffer[i * 3 + 2]);

                    var mesh = MeshCollider.Create(vertices, triangles, CollisionFilter.Default);

                    ecb.AddComponent(entity, new PhysicsCollider { Value = mesh });
                    ecb.AddSharedComponent(entity, new PhysicsWorldIndex());

                    triangles.Dispose();
                }

                ecb.RemoveComponent<VertexBufferElement>(entity);
                ecb.RemoveComponent<TriangleBufferElement>(entity);
                ecb.RemoveComponent<SetMeshTag>(entity);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}