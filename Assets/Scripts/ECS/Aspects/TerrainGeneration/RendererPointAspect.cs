using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace ECS.Aspects.TerrainGeneration
{
    public readonly partial struct RendererPointAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRO<RendererPoint> _rendererPoint;
        private readonly RefRO<LocalTransform> _localTransform;
        private readonly RefRW<SegmentPosition> _segmentPosition;

        public RendererPoint Settings => _rendererPoint.ValueRO;

        public float3 Position => _localTransform.ValueRO.Position;

        public float3 SegmentPosition
        {
            get => _segmentPosition.ValueRO.Position;
            private set => _segmentPosition.ValueRW.Position = value;
        }

        public void UpdateSegmentPosition(EntityCommandBuffer ecb, BaseSegmentSettings settings)
        {
            SegmentPosition = GetClosestSegPos(Position, GetSegSize(settings.BaseSegSize, Settings.ReloadScale));
            ecb.AddComponent<UpdateRendererSegmentsTag>(Entity);
        }
    }
}