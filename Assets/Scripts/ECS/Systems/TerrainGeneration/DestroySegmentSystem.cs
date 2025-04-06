using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(Renderer.SetRendererMeshSystem))]
    [BurstCompile]
    public partial struct DestroySegmentSystem : ISystem
    {
        private EntityQuery _segmentQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestroySegmentTag>();

            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<DestroySegmentTag>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            ecb.DestroyEntity(_segmentQuery, EntityQueryCaptureMode.AtPlayback);

            ecb.Playback(state.EntityManager);
        }
    }
}