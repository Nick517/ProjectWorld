using ECS.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Systems
{
    [BurstCompile]
    public partial struct FirstPersonCameraSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CameraInputComponent>();
            state.RequireForUpdate<CameraSettingsComponent>();
            state.RequireForUpdate<FirstPersonCameraTagComponent>();
        }

        private quaternion _rotation;

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cameraEntity = SystemAPI.GetSingletonEntity<FirstPersonCameraTagComponent>();
            var localTransform = state.EntityManager.GetComponentData<LocalTransform>(cameraEntity);
            var cameraControlInput = SystemAPI.GetSingleton<CameraInputComponent>();
            var cameraSettings = SystemAPI.GetSingleton<CameraSettingsComponent>();
            var deltaTime = SystemAPI.Time.DeltaTime;

            _rotation.value.x += cameraControlInput.LookDelta.x * deltaTime;
            _rotation.value.y += cameraControlInput.LookDelta.y * deltaTime;
            _rotation.value.y = math.clamp(_rotation.value.y, -cameraSettings.MaxVerticalCameraAngle,
                cameraSettings.MaxVerticalCameraAngle);
            var xQuat = quaternion.AxisAngle(math.up(), math.radians(_rotation.value.x));
            var yQuat = quaternion.AxisAngle(math.left(), math.radians(_rotation.value.y));
            var newRotation = math.mul(xQuat, yQuat);

            state.EntityManager.SetComponentData<LocalTransform>(cameraEntity, new LocalTransform
            {
                Position = localTransform.Position,
                Rotation = newRotation,
                Scale = localTransform.Scale
            });
        }
    }
}