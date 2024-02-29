using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PlayerInput
{
    public partial class InputManagerSystem : SystemBase
    {
        private PlayerControls _playerControls;

        protected override void OnCreate()
        {
            _playerControls = new();
        }

        protected override void OnStartRunning()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _playerControls.Enable();
        }

        protected override void OnUpdate()
        {
            InputManagerAspect inputManager = SystemAPI.GetAspect<InputManagerAspect>(SystemAPI.GetSingletonEntity<CameraInputComponent>());

            inputManager.Movement = _playerControls.FlyCamera.Movement.ReadValue<Vector3>();

            inputManager.Sprint = _playerControls.FlyCamera.Sprint.IsPressed();

            float2 lookDelta = _playerControls.Camera.Look.ReadValue<Vector2>();
            inputManager.LookDelta = lookDelta * inputManager.CameraSensitivity;
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
