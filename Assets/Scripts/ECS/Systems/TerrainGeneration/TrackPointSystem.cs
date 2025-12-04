using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace ECS.Systems.TerrainGeneration
{
    [BurstCompile]
    public partial struct TrackPointSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<TrackPoint>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            var entity = SystemAPI.GetSingletonEntity<TrackPoint>();
            var point = SystemAPI.GetComponentRW<TrackPoint>(entity);
            var pos = SystemAPI.GetComponentRO<LocalTransform>(entity).ValueRO.Position;
            
            if (PointWithinSeg(pos, point.ValueRO.SegmentPosition, settings.BaseSegSize)) return;

            point.ValueRW.SegmentPosition = GetClosestSegPos(pos, settings.BaseSegSize);
            state.EntityManager.AddComponent<UpdateRendererSegmentsTag>(entity);
            state.EntityManager.AddComponent<UpdateColliderSegmentsTag>(entity);
        }
    }
}