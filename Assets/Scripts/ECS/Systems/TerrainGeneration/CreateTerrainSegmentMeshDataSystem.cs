using ECS.Aspects.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using ECS.Jobs.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(TerrainSegmentManagerSystem))]
    [BurstCompile]
    public partial struct CreateTerrainSegmentMeshDataSystem : ISystem
    {
        private EntityQuery _terrainSegmentQuery;
        private EntityTypeHandle _entityTypeHandle;
        private ComponentTypeHandle<LocalTransform> _localTransformTypeHandle;
        private ComponentTypeHandle<TerrainSegmentScale> _terrainSegmentScaleTypeHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TerrainSegmentGenerationSettings>();
            state.RequireForUpdate<TerrainGenerationTreeBlob>();
            state.RequireForUpdate<CreateTerrainSegmentMeshDataTag>();

            _terrainSegmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<TerrainSegmentAspect>()
                .WithAll<CreateTerrainSegmentMeshDataTag>()
                .Build(ref state);

            _entityTypeHandle = state.GetEntityTypeHandle();
            _localTransformTypeHandle = state.GetComponentTypeHandle<LocalTransform>(true);
            _terrainSegmentScaleTypeHandle = state.GetComponentTypeHandle<TerrainSegmentScale>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _entityTypeHandle.Update(ref state);
            _localTransformTypeHandle.Update(ref state);
            _terrainSegmentScaleTypeHandle.Update(ref state);

            using var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var settings = SystemAPI.GetSingleton<TerrainSegmentGenerationSettings>();
            var tgGraph = SystemAPI.GetSingleton<TerrainGenerationTreeBlob>();

            var jobHandle = new CreateTerrainSegmentMeshDataJob
            {
                Ecb = ecb.AsParallelWriter(),
                EntityTypeHandle = _entityTypeHandle,
                LocalTransformTypeHandle = _localTransformTypeHandle,
                TerrainSegmentScaleTypeHandle = _terrainSegmentScaleTypeHandle,
                Settings = settings,
                TerrainGenerationTreeBlob = tgGraph
            }.ScheduleParallel(_terrainSegmentQuery, state.Dependency);

            state.Dependency = JobHandle.CombineDependencies(state.Dependency, jobHandle);
            state.Dependency.Complete();

            ecb.Playback(state.EntityManager);
        }
    }
}