using ECS.Aspects;
using ECS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Systems
{
    [UpdateAfter(typeof(TrackPointSystem))]
    [BurstCompile]
    public partial struct ChunkLoaderSystem : ISystem
    {
        private EntityCommandBuffer _entityCommandBuffer;
        private ChunkGenerationSettingsComponent _chunkGenerationSettings;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TrackPointTagComponent>();
            state.RequireForUpdate<ChunkGenerationSettingsComponent>();
            state.RequireForUpdate<LoadChunksPointTagComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
            _chunkGenerationSettings = SystemAPI.GetSingleton<ChunkGenerationSettingsComponent>();

            // Gather Data of all existing Chunks not marked for destruction
            var existingChunkData = new NativeHashSet<ChunkAspect.Data>(2000, Allocator.Temp);
            foreach (var chunkAspect in SystemAPI.Query<ChunkAspect>().WithAbsent<DestroyChunkTagComponent>())
                _ = existingChunkData.Add(ChunkAspect.ChunkAspectToChunkData(chunkAspect));

            // Create Data for all Chunks that need to be created around the TrackPoint
            var trackPointAspect =
                SystemAPI.GetAspect<TrackPointAspect>(SystemAPI.GetSingletonEntity<TrackPointTagComponent>());
            var newChunkData = new NativeHashSet<ChunkAspect.Data> (200, Allocator.Temp);
            newChunkData.UnionWith(CreateNewChunkData(trackPointAspect.ChunkPosition));
            _entityCommandBuffer.RemoveComponent<LoadChunksPointTagComponent>(trackPointAspect.Entity);

            // Gather Data from existingChunkData that are not in newChunkData
            var destroyChunkData = new NativeHashSet<ChunkAspect.Data>(200, Allocator.Temp);
            destroyChunkData.UnionWith(existingChunkData);
            destroyChunkData.ExceptWith(newChunkData);

            // Gather Data from newChunkData that are not in existingChunkData
            var createChunkData = new NativeHashSet<ChunkAspect.Data>(200, Allocator.Temp);
            createChunkData.UnionWith(newChunkData);
            createChunkData.ExceptWith(existingChunkData);

            // Mark existing Chunks whose data is in destroyChunkData for destruction
            foreach (var chunkAspect in SystemAPI.Query<ChunkAspect>())
                if (destroyChunkData.Contains(ChunkAspect.ChunkAspectToChunkData(chunkAspect)))
                    _entityCommandBuffer.AddComponent<DestroyChunkTagComponent>(chunkAspect.Entity);

            // Create Chunks from data in createChunkData
            foreach (var chunkData in createChunkData)
                ChunkAspect.CreateChunk(_entityCommandBuffer, _chunkGenerationSettings, chunkData);

            _entityCommandBuffer.Playback(state.EntityManager);
            _entityCommandBuffer.Dispose();
        }

        private readonly NativeHashSet<ChunkAspect.Data> CreateNewChunkData(float3 origin)
        {
            var maxChunkScale = _chunkGenerationSettings.MaxChunkScale;
            var megaChunks = _chunkGenerationSettings.MegaChunks;

            var subChunks = new NativeHashSet<ChunkAspect.Data>(10, Allocator.Temp);

            var pointPosition = ChunkOperations.GetClosestChunkPosition(_chunkGenerationSettings,
                new ChunkAspect.Data(origin, maxChunkScale - 1));
            var chunkSize = ChunkOperations.GetChunkSize(_chunkGenerationSettings, maxChunkScale);

            for (var x = -megaChunks; x < megaChunks; x++)
            for (var y = -megaChunks; y < megaChunks; y++)
            for (var z = -megaChunks; z < megaChunks; z++)
            {
                var position = new float3(x, y, z);
                position *= chunkSize;
                position += pointPosition;

                var subData = new ChunkAspect.Data(position, maxChunkScale);
                subChunks.UnionWith(CreateSubChunkData(subData, origin));
            }

            return subChunks;
        }

        private readonly NativeHashSet<ChunkAspect.Data> CreateSubChunkData(ChunkAspect.Data data,
            float3 origin)
        {
            var subChunks = new NativeHashSet<ChunkAspect.Data>(10, Allocator.Temp);

            var subChunkScale = data.ChunkScale - 1;
            var subChunkSize = ChunkOperations.GetChunkSize(_chunkGenerationSettings, subChunkScale);
            var originPosition = ChunkOperations.GetClosestChunkPosition(_chunkGenerationSettings,
                new ChunkAspect.Data(origin, subChunkScale));

            for (var x = 0; x < 2; x++)
            for (var y = 0; y < 2; y++)
            for (var z = 0; z < 2; z++)
            {
                var subChunkPosition = new float3(x, y, z);
                subChunkPosition *= subChunkSize;
                subChunkPosition += data.Position;

                var subChunkData = new ChunkAspect.Data(subChunkPosition, subChunkScale);

                var originChunkCenter = ChunkOperations.GetClosestChunkCenter(_chunkGenerationSettings,
                    new ChunkAspect.Data(originPosition, subChunkScale));
                var subChunkCenter = ChunkOperations.GetClosestChunkCenter(_chunkGenerationSettings, subChunkData);
                var distance = math.distance(originChunkCenter, subChunkCenter);

                if (distance < subChunkSize * _chunkGenerationSettings.LOD && subChunkScale > 0)
                    subChunks.UnionWith(CreateSubChunkData(subChunkData, origin));
                else
                    _ = subChunks.Add(subChunkData);
            }

            return subChunks;
        }
    }
}