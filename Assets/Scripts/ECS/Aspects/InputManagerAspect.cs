using ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;

namespace ECS.Aspects
{
    public readonly partial struct InputManagerAspect : IAspect
    {
        private readonly RefRW<FlyCameraInput> _flyCameraInput;
        private readonly RefRW<CameraInput> _cameraInput;
        private readonly RefRO<CameraSettings> _cameraSettings;

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

        public float Sensitivity => _cameraSettings.ValueRO.Sensitivity;
    }
}