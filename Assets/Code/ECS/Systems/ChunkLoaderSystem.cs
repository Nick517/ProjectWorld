using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Terrain
{
    [UpdateAfter(typeof(TrackPointSystem))]
    [BurstCompile]
    public partial struct ChunkLoaderSystem : ISystem
    {
        private EntityCommandBuffer _entityCommandBuffer;
        private ChunkLoaderSettingsComponent _chunkLoaderSettings;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LoadChunksPointTagComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _entityCommandBuffer = new(Allocator.Temp);
            _chunkLoaderSettings = SystemAPI.GetSingleton<ChunkLoaderSettingsComponent>();

            // Gather ChunkData of all existing Chunks not marked for destruction
            NativeHashSet<ChunkAspect.ChunkData> existingChunkData = new(2000, Allocator.Temp);
            foreach (ChunkAspect chunkAspect in SystemAPI.Query<ChunkAspect>().WithAbsent<DestroyChunkTagComponent>())
            {
                _ = existingChunkData.Add(ChunkAspect.ChunkAspectToChunkData(chunkAspect));
            }

            // Create ChunkData for all Chunks that need to be created around the TrackPoint
            TrackPointAspect trackPointAspect = SystemAPI.GetAspect<TrackPointAspect>(SystemAPI.GetSingletonEntity<TrackPointTagComponent>());
            NativeHashSet<ChunkAspect.ChunkData> newChunkData = new(200, Allocator.Temp);
            newChunkData.UnionWith(CreateNewChunkData(trackPointAspect.ChunkPosition));
            _entityCommandBuffer.RemoveComponent<LoadChunksPointTagComponent>(trackPointAspect.entity);

            // Gather ChunkData from existingChunkData that are not in newChunkData
            NativeHashSet<ChunkAspect.ChunkData> destroyChunkData = new(200, Allocator.Temp);
            destroyChunkData.UnionWith(existingChunkData);
            destroyChunkData.ExceptWith(newChunkData);

            // Gather ChunkData from newChunkData that are not in existingChunkData
            NativeHashSet<ChunkAspect.ChunkData> createChunkData = new(200, Allocator.Temp);
            createChunkData.UnionWith(newChunkData);
            createChunkData.ExceptWith(existingChunkData);

            // Mark existing Chunks whose data is in destroyChunkData for destruction
            foreach (ChunkAspect chunkAspect in SystemAPI.Query<ChunkAspect>())
            {
                if (destroyChunkData.Contains(ChunkAspect.ChunkAspectToChunkData(chunkAspect)))
                {
                    _entityCommandBuffer.AddComponent<DestroyChunkTagComponent>(chunkAspect.entity);
                }
            }

            // Create Chunks from data in createChunkData
            foreach (ChunkAspect.ChunkData chunkData in createChunkData)
            {
                ChunkAspect.CreateChunk(_entityCommandBuffer, _chunkLoaderSettings, chunkData);
            }

            _entityCommandBuffer.Playback(state.EntityManager);
            _entityCommandBuffer.Dispose();
        }

        private readonly NativeHashSet<ChunkAspect.ChunkData> CreateNewChunkData(float3 origin)
        {
            int maxChunkScale = _chunkLoaderSettings.maxChunkScale;
            int megaChunks = _chunkLoaderSettings.megaChunks;

            NativeHashSet<ChunkAspect.ChunkData> subChunks = new(10, Allocator.Temp);

            float3 pointPosition = ChunkOperations.GetClosestChunkPosition(_chunkLoaderSettings, new(origin, maxChunkScale - 1));
            float chunkSize = ChunkOperations.GetChunkSize(_chunkLoaderSettings, maxChunkScale);

            for (int x = -megaChunks; x < megaChunks; x++)
            {
                for (int y = -megaChunks; y < megaChunks; y++)
                {
                    for (int z = -megaChunks; z < megaChunks; z++)
                    {
                        float3 position = new(x, y, z);
                        position *= chunkSize;
                        position += pointPosition;

                        ChunkAspect.ChunkData subChunkData = new(position, maxChunkScale);
                        subChunks.UnionWith(CreateSubChunkData(subChunkData, origin));
                    }
                }
            }

            return subChunks;
        }

        private readonly NativeHashSet<ChunkAspect.ChunkData> CreateSubChunkData(ChunkAspect.ChunkData chunkData, float3 origin)
        {
            NativeHashSet<ChunkAspect.ChunkData> subChunks = new(10, Allocator.Temp);

            float subChunkScale = chunkData.chunkScale - 1;
            float subChunkSize = ChunkOperations.GetChunkSize(_chunkLoaderSettings, subChunkScale);
            float3 originPosition = ChunkOperations.GetClosestChunkPosition(_chunkLoaderSettings, new(origin, subChunkScale));

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        float3 subChunkPosition = new(x, y, z);
                        subChunkPosition *= subChunkSize;
                        subChunkPosition += chunkData.position;

                        ChunkAspect.ChunkData subChunkData = new(subChunkPosition, subChunkScale);

                        float3 originChunkCenter = ChunkOperations.GetClosestChunkCenter(_chunkLoaderSettings, new(originPosition, subChunkScale));
                        float3 subChunkCenter = ChunkOperations.GetClosestChunkCenter(_chunkLoaderSettings, subChunkData);
                        float distance = math.distance(originChunkCenter, subChunkCenter);

                        if (distance < subChunkSize * _chunkLoaderSettings.LOD && subChunkScale > 0)
                        {
                            subChunks.UnionWith(CreateSubChunkData(subChunkData, origin));
                        }
                        else
                        {
                            _ = subChunks.Add(subChunkData);
                        }
                    }
                }
            }

            return subChunks;
        }
    }
}
