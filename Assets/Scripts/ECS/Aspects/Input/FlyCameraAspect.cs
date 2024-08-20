using ECS.Components.Input;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Aspects.Input
{
    public readonly partial struct FlyCameraAspect : IAspect
    {
        private readonly RefRW<LocalTransform> _localTransform;
        private readonly RefRO<FlyCameraSettings> _flyCameraSettings;

        public LocalTransform LocalTransform => _localTransform.ValueRO;

        public float3 Position
        {
            get => _localTransform.ValueRO.Position;
            set => _localTransform.ValueRW.Position = value;
        }

        public float Acceleration => _flyCameraSettings.ValueRO.Acceleration;

        public float SprintMultiplier => _flyCameraSettings.ValueRO.SprintMultiplier;

        public float Damping => _flyCameraSettings.ValueRO.Damping;
    }
}