using ECS.Aspects;
using ECS.Components;
using ECS.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ECS.Systems
{
    [UpdateAfter(typeof(ChunkLoaderSystem))]
    [BurstCompile]
    public partial struct CreateChunkMeshDataSystem : ISystem
    {
        private EntityQuery _chunkQuery;
        private EntityTypeHandle _entityTypeHandle;
        private ComponentTypeHandle<LocalTransform> _localTransformTypeHandle;
        private ComponentTypeHandle<ChunkScaleComponent> _chunkScaleComponentTypeHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TgGraphComponent>();
            state.RequireForUpdate<ChunkGenerationSettingsComponent>();
            state.RequireForUpdate<CreateChunkMeshDataTagComponent>();

            _chunkQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<ChunkAspect>()
                .WithAll<CreateChunkMeshDataTagComponent>()
                .Build(ref state);

            _entityTypeHandle = state.GetEntityTypeHandle();
            _localTransformTypeHandle = state.GetComponentTypeHandle<LocalTransform>();
            _chunkScaleComponentTypeHandle = state.GetComponentTypeHandle<ChunkScaleComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _entityTypeHandle.Update(ref state);
            _localTransformTypeHandle.Update(ref state);
            _chunkScaleComponentTypeHandle.Update(ref state);

            var entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var chunkGenerationSettings = SystemAPI.GetSingleton<ChunkGenerationSettingsComponent>();
            var tgGraph = SystemAPI.GetSingleton<TgGraphComponent>();

            var createMeshDataJobHandle = new CreateMeshDataJob
            {
                EntityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                EntityTypeHandle = _entityTypeHandle,
                LocalTransformTypeHandle = _localTransformTypeHandle,
                ChunkScaleComponentTypeHandle = _chunkScaleComponentTypeHandle,
                ChunkGenerationSettings = chunkGenerationSettings,
                TgGraph = tgGraph
            }.ScheduleParallel(_chunkQuery, state.Dependency);
            createMeshDataJobHandle.Complete();

            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }
}