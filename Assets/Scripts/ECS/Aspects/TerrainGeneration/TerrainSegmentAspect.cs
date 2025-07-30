using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Aspects.TerrainGeneration
{
    public readonly partial struct TerrainSegmentAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRO<LocalTransform> _localTransform;
        private readonly RefRO<SegmentScale> _segmentScale;

        public float3 Position => _localTransform.ValueRO.Position;

        public int Scale => _segmentScale.ValueRO.Scale;
    }
}