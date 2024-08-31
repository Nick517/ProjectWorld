using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Utility.TerrainGeneration;
using TrackPointAspect = ECS.Aspects.TerrainGeneration.TrackPointAspect;

namespace ECS.Systems.TerrainGeneration
{
    [BurstCompile]
    public partial struct TrackPointSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChunkGenerationSettings>();
            state.RequireForUpdate<TrackPointTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var settings = SystemAPI.GetSingleton<ChunkGenerationSettings>();

            foreach (var trackPoint in SystemAPI.Query<TrackPointAspect>())
            {
                var chunkPosition =
                    ChunkOperations.GetClosestChunkPosition(settings, trackPoint.Position, settings.ReloadScale);

                if (!chunkPosition.Equals(trackPoint.ChunkPosition)) trackPoint.UpdateChunkPosition(ecb, chunkPosition);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}