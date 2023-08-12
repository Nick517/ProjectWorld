using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Terrain
{
    [BurstCompile]
    public partial struct TrackPointSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TrackPointTagComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer entityCommandBuffer = new(Allocator.Temp);
            ChunkLoaderSettingsComponent chunkLoaderSettings = SystemAPI.GetSingleton<ChunkLoaderSettingsComponent>();

            foreach (TrackPointAspect trackPoint in SystemAPI.Query<TrackPointAspect>())
            {
                float3 trackPointChunkPosition = ChunkOperations.GetClosestChunkPosition(chunkLoaderSettings, new(trackPoint.Position, chunkLoaderSettings.reloadScale));

                if (!trackPointChunkPosition.Equals(trackPoint.ChunkPosition))
                {
                    trackPoint.UpdateChunkPosition(entityCommandBuffer, trackPointChunkPosition);
                }
            }

            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }
}
