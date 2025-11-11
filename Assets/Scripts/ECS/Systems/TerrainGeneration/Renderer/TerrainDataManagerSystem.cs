using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using static Utility.TerrainGeneration.SegmentTags;
using TerrainData = ECS.Components.TerrainGeneration.TerrainData;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(TrackPointSystem))]
    [BurstCompile]
    public partial struct TerrainDataManagerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<TerrainData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var terrainData = SystemAPI.GetSingletonRW<TerrainData>();

            foreach (var (seg, entity) in SystemAPI.Query<RefRO<InactiveSegment>>().WithEntityAccess())
            {
                terrainData.ValueRW.Segments.SetAtPos(Inactive, seg.ValueRO.Position, seg.ValueRO.Scale);
                ecb.RemoveComponent<InactiveSegment>(entity);
                ecb.AddComponent<DestroySegmentTag>(entity);
            }
            
            ecb.Playback(state.EntityManager);
        }
    }
}