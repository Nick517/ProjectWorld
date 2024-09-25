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
        private float3 Position => _localTransform.ValueRO.Position;

        private float Scale => _segmentScale.ValueRO.Scale;


        public struct Data : IEquatable<Data>
        {
            public float3 Position;
            public readonly float Scale;

            public Data(float3 position, float scale) : this()
            {
                Position = position;
                Scale = scale;
            }

            public bool Equals(Data data)
            {
                return math.all(Position == data.Position) && math.abs(Scale - data.Scale) == 0;
            }

            public readonly override int GetHashCode()
            {
                var hash = 0;
                hash += (int)Position.x;
                hash += (int)Position.y * 10;
                hash += (int)Position.z * 100;
                hash += (int)Scale * 1000;

                return hash;
            }
        }

        public static Data AspectToData(TerrainSegmentAspect terrainSegment)
        {
            return new Data(terrainSegment.Position, terrainSegment.Scale);
        }

        public static void Create(EntityCommandBuffer ecb, BaseSegmentSettings settings, Data data)
        {
            var segment = ecb.Instantiate(settings.RendererSegmentPrefab);
            ecb.SetComponent(segment, LocalTransform.FromPosition(data.Position));
            ecb.AddComponent(segment, new SegmentScale { Scale = data.Scale });
            ecb.AddComponent<CreateRendererMeshTag>(segment);
        }
    }
}