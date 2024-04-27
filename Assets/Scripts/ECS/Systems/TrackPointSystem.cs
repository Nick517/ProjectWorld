using ECS.Aspects;
using ECS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Systems
{
    [BurstCompile]
    public partial struct TrackPointSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChunkGenerationSettingsComponent>();
            state.RequireForUpdate<TrackPointTagComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
            var chunkGenerationSettings = SystemAPI.GetSingleton<ChunkGenerationSettingsComponent>();

            foreach (var trackPoint in SystemAPI.Query<TrackPointAspect>())
            {
                var trackPointChunkPosition = ChunkOperations.GetClosestChunkPosition(chunkGenerationSettings,
                    new ChunkAspect.Data(trackPoint.Position, chunkGenerationSettings.ReloadScale));

                if (!trackPointChunkPosition.Equals(trackPoint.ChunkPosition))
                    trackPoint.UpdateChunkPosition(entityCommandBuffer, trackPointChunkPosition);
            }

            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }
}