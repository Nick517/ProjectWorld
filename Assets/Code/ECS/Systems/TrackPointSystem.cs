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
            TerrainGenerationSettingsComponent terrainGenerationSettings = SystemAPI.GetSingleton<TerrainGenerationSettingsComponent>();

            foreach (TrackPointAspect trackPoint in SystemAPI.Query<TrackPointAspect>())
            {
                float3 trackPointChunkPosition = ChunkOperations.GetClosestChunkPosition(terrainGenerationSettings, new(trackPoint.Position, terrainGenerationSettings.reloadScale));

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
