using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(SegmentManagerSystem))]
    [BurstCompile]
    public partial struct DestroySegmentSystem : ISystem
    {
        private EntityQuery _segmentQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestroySegmentTag>();

            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<DestroySegmentTag>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var entity in _segmentQuery.ToEntityArray(Allocator.Temp)) ecb.DestroyEntity(entity);

            ecb.Playback(state.EntityManager);
        }
    }
}