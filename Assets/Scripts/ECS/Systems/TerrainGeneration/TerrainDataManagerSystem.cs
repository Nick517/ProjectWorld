using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Systems.TerrainGeneration
{
    [BurstCompile]
    public partial struct TerrainDataManagerSystem : ISystem
    {
        private EntityQuery _segmentQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<TerrainData>();

            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SegmentInfo, InactiveSegmentTag>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var terrainData = SystemAPI.GetSingletonRW<TerrainData>();

            foreach (var entity in _segmentQuery.ToEntityArray(Allocator.Temp))
            {
                var seg = state.EntityManager.GetComponentData<SegmentInfo>(entity);

                terrainData.ValueRW.InactiveSegs.SetAtPos(true, seg.Position, seg.Scale);
                terrainData.ValueRW.RendererSegs.SetAtPos(default, seg.Position, seg.Scale);
                terrainData.ValueRW.ColliderSegs.SetAtPos(default, seg.Position, seg.Scale);

                ecb.RemoveComponent<InactiveSegmentTag>(entity);
                ecb.AddComponent<DestroySegmentTag>(entity);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}