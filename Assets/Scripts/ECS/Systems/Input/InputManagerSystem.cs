using ECS.Components.Input;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace ECS.Systems.Input
{
    [BurstCompile]
    public partial class InputManagerSystem : SystemBase
    {
        private PlayerControls _playerControls;

        [BurstCompile]
        protected override void OnCreate()
        {
            RequireForUpdate<CameraSettings>();
            RequireForUpdate<CameraInput>();
            RequireForUpdate<FlyCameraInput>();
            RequireForUpdate<PlayerInput>();

            _playerControls = new PlayerControls();
        }

        [BurstCompile]
        protected override void OnStartRunning()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _playerControls.Enable();
        }

        [BurstCompile]
        protected override void OnUpdate()
        {
            var cameraSettings = SystemAPI.GetSingleton<CameraSettings>();
            var cameraInput = SystemAPI.GetSingletonRW<CameraInput>();
            var flyCameraInput = SystemAPI.GetSingletonRW<FlyCameraInput>();
            var playerInput = SystemAPI.GetSingletonRW<PlayerInput>();

            flyCameraInput.ValueRW.Movement = _playerControls.FlyCamera.Movement.ReadValue<Vector3>();
            flyCameraInput.ValueRW.Sprint = _playerControls.FlyCamera.Sprint.IsPressed();
            playerInput.ValueRW.RemoveTerrain = _playerControls.Player.RemoveTerrain.WasPressedThisFrame();
            playerInput.ValueRW.AddTerrain = _playerControls.Player.AddTerrain.WasPressedThisFrame();
            playerInput.ValueRW.ThrowObject = _playerControls.Player.ThrowObject.WasPressedThisFrame();

            var lookDelta = _playerControls.Camera.Look.ReadValue<Vector2>();

            cameraInput.ValueRW.LookDelta = lookDelta * cameraSettings.Sensitivity;
        }

        [BurstCompile]
        protected override void OnStopRunning()
        {
            _playerControls.Disable();
        }

        [BurstCompile]
        protected override void OnDestroy()
        {
            _playerControls.Dispose();
        }
    }
}