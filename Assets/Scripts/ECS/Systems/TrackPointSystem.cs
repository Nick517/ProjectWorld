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
            state.RequireForUpdate<ChunkGenerationSettings>();
            state.RequireForUpdate<TrackPointTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var settings = SystemAPI.GetSingleton<ChunkGenerationSettings>();

            foreach (var trackPoint in SystemAPI.Query<TrackPointAspect>())
            {
                var chunkPosition = ChunkOperations.GetClosestChunkPosition(settings,
                    new ChunkAspect.Data(trackPoint.Position, settings.ReloadScale));

                if (!chunkPosition.Equals(trackPoint.ChunkPosition))
                    trackPoint.UpdateChunkPosition(ecb, chunkPosition);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}