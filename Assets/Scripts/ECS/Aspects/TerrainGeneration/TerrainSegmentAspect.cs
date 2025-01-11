using System;
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

        public static void Create(EntityCommandBuffer ecb, BaseSegmentSettings settings, SegData segData)
        {
            var entity = ecb.Instantiate(settings.RendererSegmentPrefab);
            ecb.SetComponent(entity, LocalTransform.FromPosition(segData.Position));
            ecb.AddComponent(entity, new SegmentScale { Scale = segData.Scale });
            ecb.AddComponent<CreateRendererMeshTag>(entity);
        }

        public struct SegData : IEquatable<SegData>
        {
            public float3 Position;
            public readonly int Scale;
            public Entity Entity;

            public SegData(float3 position, int scale = 0, Entity entity = default)
            {
                Position = position;
                Scale = scale;
                Entity = entity;
            }

            public bool Equals(SegData other)
            {
                return Position.Equals(other.Position) && Scale == other.Scale;
            }

            public override int GetHashCode()
            {
                return (int)math.hash(new float4(Position, Scale));
            }
        }
    }
}