using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using static Utility.TerrainGeneration.SegmentOperations;
using TrackPointAspect = ECS.Aspects.TerrainGeneration.TrackPointAspect;

namespace ECS.Systems.TerrainGeneration
{
    [BurstCompile]
    public partial struct TrackPointSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TerrainSegmentGenerationSettings>();
            state.RequireForUpdate<TrackPointTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var settings = SystemAPI.GetSingleton<TerrainSegmentGenerationSettings>();

            foreach (var point in SystemAPI.Query<TrackPointAspect>())
            {
                var segPos = GetClosestSegmentPosition(settings, point.Position, settings.ReloadScale);

                if (!segPos.Equals(point.SegmentPosition)) point.UpdateSegmentPosition(ecb, segPos);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}