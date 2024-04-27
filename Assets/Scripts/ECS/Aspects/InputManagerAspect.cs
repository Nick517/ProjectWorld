using ECS.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Aspects
{
    public readonly partial struct InputManagerAspect : IAspect
    {
        private readonly RefRW<FlyCameraInputComponent> _flyCameraInput;
        private readonly RefRW<CameraInputComponent> _cameraInput;
        private readonly RefRO<CameraSettingsComponent> _cameraSettings;

        public float3 Movement
        {
            set => _flyCameraInput.ValueRW.Movement = value;
        }

        public bool Sprint
        {
            set => _flyCameraInput.ValueRW.Sprint = value;
        }

        public float2 LookDelta
        {
            set => _cameraInput.ValueRW.LookDelta = value;
        }

        public float CameraSensitivity => _cameraSettings.ValueRO.CameraSensitivity;
    }
}