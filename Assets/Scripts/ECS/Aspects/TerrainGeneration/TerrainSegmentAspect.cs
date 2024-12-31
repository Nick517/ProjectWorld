using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Aspects.TerrainGeneration
{
    public readonly partial struct TerrainSegmentAspect : IAspect
    {
        private readonly Entity _entity;

        private readonly RefRO<LocalTransform> _localTransform;
        private readonly RefRO<SegmentScale> _segmentScale;

        public float3 Position => _localTransform.ValueRO.Position;

        public int Scale => _segmentScale.ValueRO.Scale;

        public static Entity Create(EntityCommandBuffer ecb, BaseSegmentSettings settings, float3 position,
            int scale = 0)
        {
            var seg = ecb.Instantiate(settings.RendererSegmentPrefab);
            ecb.SetComponent(seg, LocalTransform.FromPosition(position));
            ecb.AddComponent(seg, new SegmentScale { Scale = scale });
            ecb.AddComponent<CreateRendererMeshTag>(seg);

            return seg;
        }
    }
}