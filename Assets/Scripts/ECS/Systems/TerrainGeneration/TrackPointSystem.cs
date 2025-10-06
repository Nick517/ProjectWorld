using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
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
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();

            foreach (var (point, transform, entity) in SystemAPI.Query<RefRW<RendererPoint>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                var position = transform.ValueRO.Position;

                if (PointWithinSeg(position, point.ValueRO.SegmentPosition, settings.BaseSegSize)) continue;

                point.ValueRW.SegmentPosition = GetClosestSegPos(position, GetSegSize(settings.BaseSegSize));
                ecb.AddComponent<UpdateRendererSegmentsTag>(entity);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}