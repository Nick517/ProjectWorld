using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PlayerInput
{
    [BurstCompile]
    public partial struct FirstPersonCameraSystem : ISystem
    {
        private quaternion _rotation;

        [BurstCompile]
        public void OnStart(ref SystemState state)
        {
            state.RequireForUpdate<CameraInputComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity cameraEntity = SystemAPI.GetSingletonEntity<FirstPersonCameraTagComponent>();
            LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(cameraEntity);
            CameraInputComponent cameraControlInput = SystemAPI.GetSingleton<CameraInputComponent>();
            CameraSettingsComponent cameraSettings = SystemAPI.GetSingleton<CameraSettingsComponent>();
            float _deltaTime = SystemAPI.Time.DeltaTime;

            _rotation.value.x += cameraControlInput.lookDelta.x * _deltaTime;
            _rotation.value.y += cameraControlInput.lookDelta.y * _deltaTime;
            _rotation.value.y = math.clamp(_rotation.value.y, -cameraSettings.maxVerticalCameraAngle, cameraSettings.maxVerticalCameraAngle);
            quaternion xQuat = quaternion.AxisAngle(math.up(), math.radians(_rotation.value.x));
            quaternion yQuat = quaternion.AxisAngle(math.left(), math.radians(_rotation.value.y));
            quaternion newRotation = math.mul(xQuat, yQuat);

            state.EntityManager.SetComponentData<LocalTransform>(cameraEntity, new()
            {
                Position = localTransform.Position,
                Rotation = newRotation,
                Scale = localTransform.Scale
            });
        }
    }
}
