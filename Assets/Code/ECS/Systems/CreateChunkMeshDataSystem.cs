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
        private BufferLookup<TerrainGenerationLayerBufferElement> _terrainGenerationLayerBufferLookup;
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

            _terrainGenerationLayerBufferLookup = state.GetBufferLookup<TerrainGenerationLayerBufferElement>();

            _entityTypeHandle = state.GetEntityTypeHandle();
            _localTransformTypeHandle = state.GetComponentTypeHandle<LocalTransform>();
            _chunkScaleComponentTypeHandle = state.GetComponentTypeHandle<ChunkScaleComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _terrainGenerationLayerBufferLookup.Update(ref state);
            _entityTypeHandle.Update(ref state);
            _localTransformTypeHandle.Update(ref state);
            _chunkScaleComponentTypeHandle.Update(ref state);

            EntityCommandBuffer entityCommandBuffer = new(Allocator.TempJob);
            ChunkGenerationSettingsComponent chunkGenerationSettings = SystemAPI.GetSingleton<ChunkGenerationSettingsComponent>();
            WorldDataComponent worldData = SystemAPI.GetSingleton<WorldDataComponent>();
            DynamicBuffer<TerrainGenerationLayerBufferElement> terrainGenerationLayers = _terrainGenerationLayerBufferLookup[SystemAPI.GetSingletonEntity<ChunkGenerationSettingsComponent>()];

            JobHandle createMeshDataJobHandle = new CreateMeshDataJob
            {
                entityCommandBuffer = entityCommandBuffer.AsParallelWriter(),
                entityTypeHandle = _entityTypeHandle,
                localTransformTypeHandle = _localTransformTypeHandle,
                chunkScaleComponentTypeHandle = _chunkScaleComponentTypeHandle,
                chunkGenerationSettings = chunkGenerationSettings,
                worldData = worldData,
                terrainGenerationLayers = terrainGenerationLayers.ToNativeArray(Allocator.TempJob)
            }.ScheduleParallel(_chunkQuery, state.Dependency);
            createMeshDataJobHandle.Complete();

            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }
}
