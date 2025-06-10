using ECS.Components.Input;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using ISystem = Unity.Entities.ISystem;
using SystemAPI = Unity.Entities.SystemAPI;

namespace ECS.Systems.Input
{
    [UpdateAfter(typeof(InputManagerSystem))]
    [BurstCompile]
    public partial struct PlayerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerInput>();
            state.RequireForUpdate<PlayerSettings>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.GetSingleton<PlayerInput>().ThrowObject) return;

            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var settings = SystemAPI.GetSingleton<PlayerSettings>();
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerSettings>();
            var transform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var forceVector = transform.Forward() * settings.ThrowForce;

            var throwEntity = ecb.Instantiate(settings.ThrowObjectPrefab);
            ecb.SetComponent(throwEntity, LocalTransform.FromPositionRotation(transform.Position, transform.Rotation));
            ecb.SetComponent(throwEntity, new PhysicsVelocity { Linear = forceVector });

            ecb.Playback(state.EntityManager);
        }
    }
}