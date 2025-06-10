using ECS.Aspects.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(SetRendererMeshSystem))]
    [BurstCompile]
    public partial struct DestroySegmentSystem : ISystem
    {
        private EntityQuery _segmentQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestroySegmentTag>();
            
            _segmentQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAspect<TerrainSegmentAspect>()
                .WithAll<DestroySegmentTag>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var seg in _segmentQuery.ToEntityArray(Allocator.Temp)) ecb.DestroyEntity(seg);

            ecb.Playback(state.EntityManager);
        }
    }
}