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
        private ComponentTypeHandle<ChunkScale> _chunkScaleTypeHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Components.TerrainGenerationTree>();
            state.RequireForUpdate<ChunkGenerationSettings>();
            state.RequireForUpdate<CreateChunkMeshDataTag>();

            _chunkQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<ChunkAspect>()
                .WithAll<CreateChunkMeshDataTag>()
                .Build(ref state);

            _entityTypeHandle = state.GetEntityTypeHandle();
            _localTransformTypeHandle = state.GetComponentTypeHandle<LocalTransform>();
            _chunkScaleTypeHandle = state.GetComponentTypeHandle<ChunkScale>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _entityTypeHandle.Update(ref state);
            _localTransformTypeHandle.Update(ref state);
            _chunkScaleTypeHandle.Update(ref state);

            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var settings = SystemAPI.GetSingleton<ChunkGenerationSettings>();
            var tgGraph = SystemAPI.GetSingleton<Components.TerrainGenerationTree>();

            var createMeshDataJobHandle = new CreateMeshDataJob
            {
                Ecb = ecb.AsParallelWriter(),
                EntityTypeHandle = _entityTypeHandle,
                LocalTransformTypeHandle = _localTransformTypeHandle,
                ChunkScaleTypeHandle = _chunkScaleTypeHandle,
                Settings = settings,
                TgTree = tgGraph
            }.ScheduleParallel(_chunkQuery, state.Dependency);
            
            createMeshDataJobHandle.Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}