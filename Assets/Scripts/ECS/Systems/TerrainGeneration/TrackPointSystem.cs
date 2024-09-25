using ECS.Aspects.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using static Utility.TerrainGeneration.SegmentOperations;

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

            foreach (var point in SystemAPI.Query<RendererPointAspect>())
            {
                var segPos = GetClosestSegmentPosition(settings, point.Position, point.Settings.ReloadScale);

                if (!segPos.Equals(point.SegmentPosition)) point.UpdateSegmentPosition(ecb, segPos);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}