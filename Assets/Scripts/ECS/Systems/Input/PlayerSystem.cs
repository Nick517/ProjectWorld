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
        private PlayerSettings _playerSettings;
        private bool _initialized;

        [BurstCompile]
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
            if (!_initialized)
            {
                _playerSettings = SystemAPI.GetSingleton<PlayerSettings>();
                _initialized = true;
            }

            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var playerInput = SystemAPI.GetSingleton<PlayerInput>();
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerSettings>();
            var transform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

            if (playerInput.AddTerrain || playerInput.RemoveTerrain)
            {
                var rayEnd = transform.Position + transform.Forward() * _playerSettings.InteractionRange;
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
                            Range = 0.5f,
                            Addition = playerInput.AddTerrain
                        });
            }

            if (playerInput.ThrowObject)
            {
                var forceVector = transform.Forward() * _playerSettings.ThrowForce;
                var throwEntity = ecb.Instantiate(_playerSettings.ThrowObjectPrefab);
                ecb.SetComponent(throwEntity,
                    LocalTransform.FromPositionRotationScale(transform.Position, transform.Rotation, 0.5f));
                ecb.SetComponent(throwEntity, new PhysicsVelocity { Linear = forceVector });
            }

            ecb.Playback(state.EntityManager);
        }
    }
}