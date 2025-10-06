using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using ECS.Jobs.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using TerrainData = ECS.Components.TerrainGeneration.TerrainData;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(RendererManagerSystem))]
    [BurstCompile]
    public partial struct CreateRendererMeshSystem : ISystem
    {
        private EntityQuery _segmentQuery;
        private EntityTypeHandle _entityTypeHandle;
        private ComponentTypeHandle<SegmentInfo> _segmentInfoTypeHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<TgTreeBlob>();
            state.RequireForUpdate<TerrainData>();
            state.RequireForUpdate<CreateRendererMeshTag>();

            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<CreateRendererMeshTag>()
                .Build(ref state);

            _entityTypeHandle = state.GetEntityTypeHandle();
            _segmentInfoTypeHandle = state.GetComponentTypeHandle<SegmentInfo>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _entityTypeHandle.Update(ref state);
            _segmentInfoTypeHandle.Update(ref state);

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            var tgGraph = SystemAPI.GetSingleton<TgTreeBlob>();
            var maps = SystemAPI.GetSingleton<TerrainData>().Maps;

            var jobHandle = new CreateRendererMeshJob
            {
                Ecb = ecb,
                EntityTypeHandle = _entityTypeHandle,
                SegmentInfoTypeHandle = _segmentInfoTypeHandle,
                Settings = settings,
                TgTreeBlob = tgGraph,
                Maps = maps
            }.ScheduleParallel(_segmentQuery, state.Dependency);

            state.Dependency = jobHandle;
        }
    }
}