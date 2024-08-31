using ECS.Components.TerrainGeneration;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Aspects.TerrainGeneration
{
    public readonly partial struct TrackPointAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRO<LocalTransform> _localTransform;
        private readonly RefRW<TerrainSegmentPosition> _segmentPosition;

        public float3 Position => _localTransform.ValueRO.Position;

        public float3 SegmentPosition
        {
            get => _segmentPosition.ValueRO.Position;
            private set => _segmentPosition.ValueRW.Position = value;
        }

        public void UpdateSegmentPosition(EntityCommandBuffer ecb, float3 newSegmentPosition)
        {
            SegmentPosition = newSegmentPosition;
            ecb.AddComponent<LoadTerrainSegmentsPointTag>(Entity);
        }
    }
}