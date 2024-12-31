using DataTypes.Trees;
using ECS.Aspects.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using static ECS.Aspects.TerrainGeneration.TerrainSegmentAspect;
using static Utility.TerrainGeneration.SegmentOperations;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(TrackPointSystem))]
    [BurstCompile]
    public partial struct RendererManagerSystem : ISystem
    {
        private Octree<Entity> _tree;
        private NativeList<int> _existingSegIndexes;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<RendererPoint>();
            state.RequireForUpdate<UpdateRendererSegmentsTag>();

            _existingSegIndexes = new NativeList<int>(Allocator.Persistent);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();

            if (!_tree.IsCreated) _tree = new Octree<Entity>(settings.BaseSegSize, Allocator.Persistent);

            foreach (var point in SystemAPI.Query<RendererPointAspect>().WithAll<UpdateRendererSegmentsTag>())
            {
                var scale = point.Settings.MaxSegmentScale;
                var segSize = GetSegSize(settings.BaseSegSize, scale);
                var segPos = GetClosestSegPos(point.Position, segSize);
                var index = _tree.PosToIndex(segPos, scale);

                if (_tree.GetValueAtIndex(index) == default)
                {
                    _tree.SetValueAtIndex(index, Create(ecb, settings, segPos, scale));
                    _existingSegIndexes.Add(index);
                }

                ecb.RemoveComponent<UpdateRendererSegmentsTag>(point.Entity);
            }

            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _tree.Dispose();
            _existingSegIndexes.Dispose();
        }
    }
}