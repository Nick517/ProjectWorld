using ECS.BufferElements.TerrainGeneration.Renderer;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using MeshCollider = Unity.Physics.MeshCollider;

namespace ECS.Systems.TerrainGeneration
{
    [BurstCompile]
    public partial struct SetColliderMeshSystem : ISystem
    {
        private BufferLookup<VertexBufferElement> _vertexBufferLookup;
        private BufferLookup<TriangleBufferElement> _triangleBufferLookup;
        private EntityQuery _segmentQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<SetColliderMeshTag>();

            _vertexBufferLookup = state.GetBufferLookup<VertexBufferElement>(true);
            _triangleBufferLookup = state.GetBufferLookup<TriangleBufferElement>(true);

            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SetColliderMeshTag>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _vertexBufferLookup.Update(ref state);
            _triangleBufferLookup.Update(ref state);

            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var entity in _segmentQuery.ToEntityArray(Allocator.Temp))
            {
                var info = SystemAPI.GetComponent<SegmentInfo>(entity);
                var vertexBuffer = _vertexBufferLookup[entity];
                var triangleBuffer = _triangleBufferLookup[entity];

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
                
                ecb.AddComponent(entity, LocalTransform.FromPosition(info.Position));

                ecb.RemoveComponent<VertexBufferElement>(entity);
                ecb.RemoveComponent<TriangleBufferElement>(entity);
                ecb.RemoveComponent<SetColliderMeshTag>(entity);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}