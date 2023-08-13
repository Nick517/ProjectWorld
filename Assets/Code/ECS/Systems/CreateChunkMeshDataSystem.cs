using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Terrain
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

            EntityCommandBuffer entityCommandBuffer = new(Allocator.TempJob);
            TerrainGenerationSettingsComponent terrainGenerationSettings = SystemAPI.GetSingleton<TerrainGenerationSettingsComponent>();
            WorldDataComponent worldData = SystemAPI.GetSingleton<WorldDataComponent>();

            JobHandle createMeshDataJobHandle = new CreateMeshDataJob
            {
                entityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                entityTypeHandle = _entityTypeHandle,
                localTransformTypeHandle = _localTransformTypeHandle,
                chunkScaleComponentTypeHandle = _chunkScaleComponentTypeHandle,
                terrainGenerationSettings = terrainGenerationSettings,
                worldData = worldData
            }.ScheduleParallel(_chunkQuery, state.Dependency);
            createMeshDataJobHandle.Complete();

            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }
}
