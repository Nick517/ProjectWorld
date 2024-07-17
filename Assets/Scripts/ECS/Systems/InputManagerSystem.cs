using ECS.Aspects;
using ECS.Components;
using Unity.Entities;
using UnityEngine;

namespace ECS.Systems
{
    public partial class InputManagerSystem : SystemBase
    {
        private PlayerControls _playerControls;

        protected override void OnCreate()
        {
            RequireForUpdate<CameraInput>();
            _playerControls = new PlayerControls();
        }

        protected override void OnStartRunning()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _playerControls.Enable();
        }

        protected override void OnUpdate()
        {
            var inputManager =
                SystemAPI.GetAspect<InputManagerAspect>(SystemAPI.GetSingletonEntity<CameraInput>());

            inputManager.Movement = _playerControls.FlyCamera.Movement.ReadValue<Vector3>();
            inputManager.Sprint = _playerControls.FlyCamera.Sprint.IsPressed();

            var lookDelta = _playerControls.Camera.Look.ReadValue<Vector2>();

            inputManager.LookDelta = lookDelta * inputManager.Sensitivity;
        }

        protected override void OnStopRunning()
        {
            _playerControls.Disable();
        }

        protected override void OnDestroy()
        {
            _playerControls.Dispose();
        }
    }
}