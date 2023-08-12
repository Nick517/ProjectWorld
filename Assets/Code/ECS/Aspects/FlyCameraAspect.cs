using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace FlyCamera
{
    public readonly partial struct FlyCameraAspect : IAspect
    {
        public readonly Entity entity;

        private readonly RefRW<LocalTransform> _localTransform;
        private readonly RefRO<FlyCameraSettingsComponent> _flyCameraSettings;

        public LocalTransform LocalTransform => _localTransform.ValueRO;

        public float3 Position
        {
            get => _localTransform.ValueRO.Position;
            set => _localTransform.ValueRW.Position = value;
        }

        public float Acceleration => _flyCameraSettings.ValueRO.acceleration;

        public float SprintMultiplier => _flyCameraSettings.ValueRO.sprintMultiplier;

        public float Damping => _flyCameraSettings.ValueRO.damping;
    }
}
