using PlayerInput;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace FlyCamera
{
    [BurstCompile]
    public partial struct FlyCameraMovementSystem : ISystem
    {
        private FlyCameraAspect _flyCameraAspect;
        private FlyCameraInputComponent _flyCameraInput;
        private float3 velocity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FlyCameraInputComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _flyCameraAspect = SystemAPI.GetAspect<FlyCameraAspect>(SystemAPI.GetSingletonEntity<FlyCameraSettingsComponent>());
            _flyCameraInput = SystemAPI.GetSingleton<FlyCameraInputComponent>();
            float deltaTime = SystemAPI.Time.DeltaTime;

            velocity += AccelerationVector;
            velocity = math.lerp(velocity, float3.zero, _flyCameraAspect.Damping * deltaTime);
            _flyCameraAspect.Position += _flyCameraAspect.LocalTransform.Right() * velocity.x * deltaTime;
            _flyCameraAspect.Position += _flyCameraAspect.LocalTransform.Up() * velocity.y * deltaTime;
            _flyCameraAspect.Position += _flyCameraAspect.LocalTransform.Forward() * velocity.z * deltaTime;
        }

        private readonly float3 AccelerationVector => _flyCameraInput.sprint
                ? _flyCameraInput.movement * _flyCameraAspect.Acceleration * _flyCameraAspect.SprintMultiplier
                : _flyCameraInput.movement * _flyCameraAspect.Acceleration;
    }
}
