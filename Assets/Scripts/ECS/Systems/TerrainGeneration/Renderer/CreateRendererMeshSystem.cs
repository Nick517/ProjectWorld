using ECS.Aspects.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using ECS.Jobs.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(RendererManagerSystem))]
    [BurstCompile]
    public partial struct CreateRendererMeshSystem : ISystem
    {
        private EntityQuery _segmentQuery;
        private EntityTypeHandle _entityTypeHandle;
        private ComponentTypeHandle<LocalTransform> _localTransformTypeHandle;
        private ComponentTypeHandle<SegmentScale> _segmentScaleTypeHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<TgTreeBlob>();
            state.RequireForUpdate<CreateRendererMeshTag>();

            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<TerrainSegmentAspect>()
                .WithAll<CreateRendererMeshTag>()
                .Build(ref state);

            _entityTypeHandle = state.GetEntityTypeHandle();
            _localTransformTypeHandle = state.GetComponentTypeHandle<LocalTransform>(true);
            _segmentScaleTypeHandle = state.GetComponentTypeHandle<SegmentScale>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _entityTypeHandle.Update(ref state);
            _localTransformTypeHandle.Update(ref state);
            _segmentScaleTypeHandle.Update(ref state);

            using var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            var tgGraph = SystemAPI.GetSingleton<TgTreeBlob>();

            var jobHandle = new CreateRendererMeshJob
            {
                Ecb = ecb.AsParallelWriter(),
                EntityTypeHandle = _entityTypeHandle,
                LocalTransformTypeHandle = _localTransformTypeHandle,
                SegmentScaleTypeHandle = _segmentScaleTypeHandle,
                Settings = settings,
                TgTreeBlob = tgGraph
            }.ScheduleParallel(_segmentQuery, state.Dependency);

            state.Dependency = JobHandle.CombineDependencies(state.Dependency, jobHandle);
            state.Dependency.Complete();

            ecb.Playback(state.EntityManager);
        }
    }
}