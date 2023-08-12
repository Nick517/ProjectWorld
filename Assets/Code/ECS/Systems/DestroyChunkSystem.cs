using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Terrain
{
    [UpdateAfter(typeof(SetChunkMeshSystem))]
    [BurstCompile]
    public partial struct DestroyChunkSystem : ISystem
    {
        [BurstCompile]
        public void OnStart(ref SystemState state)
        {
            state.RequireForUpdate<DestroyChunkTagComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer entityCommandBuffer = new(Allocator.Temp);

            foreach (ChunkAspect chunkAspect in SystemAPI.Query<ChunkAspect>().WithAll<DestroyChunkTagComponent>())
            {
                entityCommandBuffer.DestroyEntity(chunkAspect.entity);
            }

            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }
}
