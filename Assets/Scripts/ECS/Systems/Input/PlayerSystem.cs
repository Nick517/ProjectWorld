using ECS.BufferElements.TerrainGeneration;
using ECS.Components.Input;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace ECS.Systems.Input
{
    [UpdateAfter(typeof(InputManagerSystem))]
    [BurstCompile]
    public partial struct PlayerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TerrainModificationBufferElement>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<PlayerInput>();
            state.RequireForUpdate<PlayerSettings>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var playerSettings = SystemAPI.GetSingleton<PlayerSettings>();
            var playerInput = SystemAPI.GetSingleton<PlayerInput>();
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerSettings>();
            var transform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

            if (playerInput.AddTerrain || playerInput.RemoveTerrain)
            {
                var rayEnd = transform.Position + transform.Forward() * playerSettings.InteractionRange;
                var rayInput = new RaycastInput
                {
                    Start = transform.Position,
                    End = rayEnd,
                    Filter = CollisionFilter.Default
                };

                if (physicsWorld.CastRay(rayInput, out var hit))
                    ecb.AppendToBuffer(SystemAPI.GetSingletonEntity<TerrainModificationBufferElement>(),
                        new TerrainModificationBufferElement
                        {
                            Origin = hit.Position,
                            Range = playerSettings.ModificationRadius,
                            Addition = playerInput.AddTerrain
                        });
            }

            if (playerInput.ThrowObject)
            {
                var forceVector = transform.Forward() * playerSettings.ThrowForce;
                var throwEntity = ecb.Instantiate(playerSettings.ThrowObjectPrefab);
                ecb.SetComponent(throwEntity,
                    LocalTransform.FromPositionRotationScale(transform.Position, transform.Rotation, 0.5f));
                ecb.SetComponent(throwEntity, new PhysicsVelocity { Linear = forceVector });
            }

            ecb.Playback(state.EntityManager);
        }
    }
}