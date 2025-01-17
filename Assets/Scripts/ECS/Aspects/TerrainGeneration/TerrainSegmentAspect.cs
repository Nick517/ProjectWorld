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

        public static Entity CreateSegment(EntityCommandBuffer ecb, BaseSegmentSettings settings, float3 position, int scale)
        {
            var entity = ecb.Instantiate(settings.RendererSegmentPrefab);
            
            ecb.SetComponent(entity, LocalTransform.FromPosition(position));
            ecb.AddComponent(entity, new SegmentScale { Scale = scale });
            ecb.AddComponent<CreateRendererMeshTag>(entity);
            
            return entity;
        }
    }
}