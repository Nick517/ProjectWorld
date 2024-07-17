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
        private FlyCameraAspect _flyCamera;
        private FlyCameraInput _flyCameraInput;
        private float3 _velocity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyCameraSettings>();
            state.RequireForUpdate<FlyCameraInput>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _flyCamera =
                SystemAPI.GetAspect<FlyCameraAspect>(SystemAPI.GetSingletonEntity<FlyCameraSettings>());
            _flyCameraInput = SystemAPI.GetSingleton<FlyCameraInput>();
            var deltaTime = SystemAPI.Time.DeltaTime;

            _velocity += AccelerationVector;
            _velocity = math.lerp(_velocity, float3.zero, _flyCamera.Damping * deltaTime);
            _flyCamera.Position += _flyCamera.LocalTransform.Right() * _velocity.x * deltaTime;
            _flyCamera.Position += _flyCamera.LocalTransform.Up() * _velocity.y * deltaTime;
            _flyCamera.Position += _flyCamera.LocalTransform.Forward() * _velocity.z * deltaTime;
        }

        private readonly float3 AccelerationVector => _flyCameraInput.Sprint
            ? _flyCameraInput.Movement * _flyCamera.Acceleration * _flyCamera.SprintMultiplier
            : _flyCameraInput.Movement * _flyCamera.Acceleration;
    }
}