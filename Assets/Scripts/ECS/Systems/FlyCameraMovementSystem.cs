using ECS.Aspects;
using ECS.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Systems
{
    [BurstCompile]
    public partial struct FlyCameraMovementSystem : ISystem
    {
        private FlyCameraAspect _flyCameraAspect;
        private FlyCameraInputComponent _flyCameraInput;
        private float3 _velocity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyCameraSettingsComponent>();
            state.RequireForUpdate<FlyCameraInputComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _flyCameraAspect =
                SystemAPI.GetAspect<FlyCameraAspect>(SystemAPI.GetSingletonEntity<FlyCameraSettingsComponent>());
            _flyCameraInput = SystemAPI.GetSingleton<FlyCameraInputComponent>();
            var deltaTime = SystemAPI.Time.DeltaTime;

            _velocity += AccelerationVector;
            _velocity = math.lerp(_velocity, float3.zero, _flyCameraAspect.Damping * deltaTime);
            _flyCameraAspect.Position += _flyCameraAspect.LocalTransform.Right() * _velocity.x * deltaTime;
            _flyCameraAspect.Position += _flyCameraAspect.LocalTransform.Up() * _velocity.y * deltaTime;
            _flyCameraAspect.Position += _flyCameraAspect.LocalTransform.Forward() * _velocity.z * deltaTime;
        }

        private readonly float3 AccelerationVector => _flyCameraInput.Sprint
            ? _flyCameraInput.Movement * _flyCameraAspect.Acceleration * _flyCameraAspect.SprintMultiplier
            : _flyCameraInput.Movement * _flyCameraAspect.Acceleration;
    }
}