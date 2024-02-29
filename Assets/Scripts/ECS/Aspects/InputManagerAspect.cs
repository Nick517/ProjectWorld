using Unity.Entities;
using Unity.Mathematics;

namespace PlayerInput
{
    public readonly partial struct InputManagerAspect : IAspect
    {
        public readonly Entity entity;

        private readonly RefRW<FlyCameraInputComponent> _flyCameraInput;
        private readonly RefRW<CameraInputComponent> _cameraControlInput;
        private readonly RefRO<CameraSettingsComponent> _cameraSettings;

        public float3 Movement
        {
            get => _flyCameraInput.ValueRO.movement;
            set => _flyCameraInput.ValueRW.movement = value;
        }

        public bool Sprint
        {
            get => _flyCameraInput.ValueRO.sprint;
            set => _flyCameraInput.ValueRW.sprint = value;
        }

        public float2 LookDelta
        {
            get => _cameraControlInput.ValueRO.lookDelta;
            set => _cameraControlInput.ValueRW.lookDelta = value;
        }
        public readonly float CameraSensitivity => _cameraSettings.ValueRO.cameraSensitivity;
    }
}
