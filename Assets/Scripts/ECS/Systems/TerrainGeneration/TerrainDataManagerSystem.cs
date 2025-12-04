using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using static Utility.TerrainGeneration.SegmentTags;

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

                if (seg.IsRenderer) terrainData.ValueRW.RendererSegs.SetAtPos(Inactive, seg.Position, seg.Scale);
                else terrainData.ValueRW.ColliderSegs.SetAtPos(Inactive, seg.Position, seg.Scale);

                ecb.RemoveComponent<InactiveSegmentTag>(entity);
                ecb.AddComponent<DestroySegmentTag>(entity);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}