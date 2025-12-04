using ECS.Components.Input;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Systems.Input
{
    [BurstCompile]
    public partial struct FlyCameraMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyCamera>();
            state.RequireForUpdate<FlyCameraInput>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var flyCam = SystemAPI.GetSingletonRW<FlyCamera>();
            var transform = SystemAPI.GetComponentRW<LocalTransform>(SystemAPI.GetSingletonEntity<FlyCamera>());
            var flyCamInput = SystemAPI.GetSingleton<FlyCameraInput>();
            var deltaTime = SystemAPI.Time.DeltaTime;
            var velocity = flyCam.ValueRO.Velocity;

            velocity += flyCamInput.Movement * flyCam.ValueRO.Acceleration *
                        (flyCamInput.Sprint ? flyCam.ValueRO.SprintMultiplier : 1);

            velocity = math.lerp(velocity, float3.zero, flyCam.ValueRO.Damping * deltaTime);
            transform.ValueRW.Position += transform.ValueRW.Right() * velocity.x * deltaTime;
            transform.ValueRW.Position += transform.ValueRW.Up() * velocity.y * deltaTime;
            transform.ValueRW.Position += transform.ValueRW.Forward() * velocity.z * deltaTime;
            
            flyCam.ValueRW.Velocity = velocity;
        }
    }
}