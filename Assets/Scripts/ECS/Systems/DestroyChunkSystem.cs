using ECS.Aspects;
using ECS.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Systems
{
    [UpdateAfter(typeof(SetChunkMeshSystem))]
    [BurstCompile]
    public partial struct DestroyChunkSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

            foreach (var chunkAspect in SystemAPI.Query<ChunkAspect>().WithAll<DestroyChunkTagComponent>())
                entityCommandBuffer.DestroyEntity(chunkAspect.Entity);

            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }
}