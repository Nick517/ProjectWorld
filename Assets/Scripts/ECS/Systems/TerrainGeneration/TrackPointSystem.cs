using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace ECS.Systems.TerrainGeneration
{
    [BurstCompile]
    public partial struct TrackPointSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<RendererPoint>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            var entity = SystemAPI.GetSingletonEntity<RendererPoint>();
            var point = SystemAPI.GetComponentRW<RendererPoint>(entity);
            var pos = SystemAPI.GetComponentRO<LocalTransform>(entity).ValueRO.Position;
            
            if (PointWithinSeg(pos, point.ValueRO.SegmentPosition, settings.BaseSegSize)) return;

            point.ValueRW.SegmentPosition = GetClosestSegPos(pos, settings.BaseSegSize);
            point.ValueRW.Update = true;
        }
    }
}