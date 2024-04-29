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
        private EntityCommandBuffer _ecb;
        private ChunkGenerationSettings _settings;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChunkGenerationSettings>();
            state.RequireForUpdate<TrackPointTag>();
            state.RequireForUpdate<LoadChunksPointTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _ecb = new EntityCommandBuffer(Allocator.Temp);
            _settings = SystemAPI.GetSingleton<ChunkGenerationSettings>();

            // Gather Data of all existing Chunks not marked for destruction
            var existingChunkData = new NativeHashSet<ChunkAspect.Data>(2000, Allocator.Temp);
            foreach (var chunk in SystemAPI.Query<ChunkAspect>().WithAbsent<DestroyChunkTag>())
                _ = existingChunkData.Add(ChunkAspect.ChunkAspectToChunkData(chunk));

            // Create Data for all Chunks that need to be created around the TrackPoint
            var trackPointAspect =
                SystemAPI.GetAspect<TrackPointAspect>(SystemAPI.GetSingletonEntity<TrackPointTag>());
            var newChunkData = new NativeHashSet<ChunkAspect.Data>(200, Allocator.Temp);
            newChunkData.UnionWith(CreateNewChunkData(trackPointAspect.ChunkPosition));
            _ecb.RemoveComponent<LoadChunksPointTag>(trackPointAspect.Entity);

            // Gather Data from existingChunkData that are not in newChunkData
            var destroyChunkData = new NativeHashSet<ChunkAspect.Data>(200, Allocator.Temp);
            destroyChunkData.UnionWith(existingChunkData);
            destroyChunkData.ExceptWith(newChunkData);

            // Gather Data from newChunkData that are not in existingChunkData
            var createChunkData = new NativeHashSet<ChunkAspect.Data>(200, Allocator.Temp);
            createChunkData.UnionWith(newChunkData);
            createChunkData.ExceptWith(existingChunkData);

            // Mark existing Chunks whose data is in destroyChunkData for destruction
            foreach (var chunk in SystemAPI.Query<ChunkAspect>())
                if (destroyChunkData.Contains(ChunkAspect.ChunkAspectToChunkData(chunk)))
                    _ecb.AddComponent<DestroyChunkTag>(chunk.Entity);

            // Create Chunks from data in createChunkData
            foreach (var chunkData in createChunkData)
                ChunkAspect.CreateChunk(_ecb, _settings, chunkData);

            _ecb.Playback(state.EntityManager);
            _ecb.Dispose();
        }

        private readonly NativeHashSet<ChunkAspect.Data> CreateNewChunkData(float3 origin)
        {
            var maxChunkScale = _settings.MaxChunkScale;
            var megaChunks = _settings.MegaChunks;

            var subChunks = new NativeHashSet<ChunkAspect.Data>(10, Allocator.Temp);

            var pointPosition = ChunkOperations.GetClosestChunkPosition(_settings,
                new ChunkAspect.Data(origin, maxChunkScale - 1));
            var chunkSize = ChunkOperations.GetChunkSize(_settings, maxChunkScale);

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
            var subChunkSize = ChunkOperations.GetChunkSize(_settings, subChunkScale);
            var originPosition = ChunkOperations.GetClosestChunkPosition(_settings,
                new ChunkAspect.Data(origin, subChunkScale));

            for (var x = 0; x < 2; x++)
            for (var y = 0; y < 2; y++)
            for (var z = 0; z < 2; z++)
            {
                var subChunkPosition = new float3(x, y, z);
                subChunkPosition *= subChunkSize;
                subChunkPosition += data.Position;

                var subChunkData = new ChunkAspect.Data(subChunkPosition, subChunkScale);

                var originChunkCenter = ChunkOperations.GetClosestChunkCenter(_settings,
                    new ChunkAspect.Data(originPosition, subChunkScale));
                var subChunkCenter = ChunkOperations.GetClosestChunkCenter(_settings, subChunkData);
                var distance = math.distance(originChunkCenter, subChunkCenter);

                if (distance < subChunkSize.x * _settings.LOD && subChunkScale > 0)
                    subChunks.UnionWith(CreateSubChunkData(subChunkData, origin));
                else
                    _ = subChunks.Add(subChunkData);
            }

            return subChunks;
        }
    }
}