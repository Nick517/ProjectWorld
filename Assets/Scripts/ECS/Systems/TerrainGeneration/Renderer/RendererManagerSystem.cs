using DataTypes.Trees;
using ECS.Aspects.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static ECS.Aspects.TerrainGeneration.TerrainSegmentAspect;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(TrackPointSystem))]
    [BurstCompile]
    public partial struct RendererManagerSystem : ISystem
    {
        private BaseSegmentSettings _settings;
        private Octree<Entity> _octree;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<RendererPoint>();
            state.RequireForUpdate<UpdateRendererSegmentsTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            _settings = SystemAPI.GetSingleton<BaseSegmentSettings>();

            if (!_octree.IsCreated) _octree = new Octree<Entity>(_settings.BaseSegSize, Allocator.Persistent);

            foreach (var point in SystemAPI.Query<RendererPointAspect>().WithAll<UpdateRendererSegmentsTag>())
            {
                var scale = point.Settings.MaxSegmentScale;
                var size = GetSegSize(_settings.BaseSegSize, scale);
                var pointPos = GetClosestSegPos(point.Position, size);

                for (var x = -1; x <= 1; x++)
                for (var y = -1; y <= 1; y++)
                for (var z = -1; z <= 1; z++)
                {
                    var segPos = pointPos + new float3(x, y, z) * size;
                    var index = _octree.PosToIndex(segPos, scale);

                    if (_octree.GetAtIndex(index) != default) continue;

                    var seg = CreateSegment(ecb, _settings, segPos, scale);
                    _octree.SetAtIndex(seg, index);
                }

                ecb.RemoveComponent<UpdateRendererSegmentsTag>(point.Entity);
            }

            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _octree.Dispose();
        }
    }
}