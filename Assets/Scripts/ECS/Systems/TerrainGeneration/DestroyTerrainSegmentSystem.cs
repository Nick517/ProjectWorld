using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(SetTerrainSegmentMeshSystem))]
    [BurstCompile]
    public partial struct DestroyTerrainSegmentSystem : ISystem
    {
        private EntityQuery _segmentQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestroyTerrainSegmentTag>();

            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<DestroyTerrainSegmentTag>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            ecb.DestroyEntity(_segmentQuery, EntityQueryCaptureMode.AtRecord);

            ecb.Playback(state.EntityManager);
        }
    }
}