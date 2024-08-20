using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(SetChunkMeshSystem))]
    [BurstCompile]
    public partial struct DestroyChunkSystem : ISystem
    {
        private EntityQuery _chunkQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestroyChunkTag>();

            _chunkQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<DestroyChunkTag>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            ecb.DestroyEntity(_chunkQuery, EntityQueryCaptureMode.AtRecord);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}