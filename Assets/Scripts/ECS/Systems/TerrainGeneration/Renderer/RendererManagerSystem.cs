using DataTypes.Trees;
using ECS.Aspects.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(TrackPointSystem))]
    [BurstCompile]
    public partial struct RendererManagerSystem : ISystem
    {
        private BaseSegmentSettings _settings;
        private Comparison _comparison;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<RendererPoint>();
            state.RequireForUpdate<UpdateRendererSegmentsTag>();

            _comparison = new Comparison();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            _settings = SystemAPI.GetSingleton<BaseSegmentSettings>();

            using var existingSegs = new Octree<Entity>(_settings.BaseSegSize, Allocator.Temp);
            foreach (var seg in SystemAPI.Query<TerrainSegmentAspect>())
                existingSegs.SetAtPos(seg.Entity, seg.Position, seg.Scale);

            var segsToCreate = new Octree<Entity>(_settings.BaseSegSize, Allocator.Temp);
            foreach (var point in SystemAPI.Query<RendererPointAspect>())
            {
                Populate(point, ref segsToCreate);
                ecb.RemoveComponent<UpdateRendererSegmentsTag>(point.Entity);
            }

            using var segsToIgnore = Octree<Entity>.Intersect(existingSegs, segsToCreate, Allocator.Temp, _comparison);
            using var segsToDestroy = Octree<Entity>.Except(existingSegs, segsToIgnore, Allocator.Temp, _comparison);

            segsToCreate.Except(segsToIgnore, _comparison);

            for (var i = 0; i < segsToCreate.Count; i++)
            {
                var node = segsToCreate.Nodes[i];
                if (node.Value == Placeholder)
                    TerrainSegmentAspect.CreateSegment(ecb, _settings, node.Position, node.Scale);
            }

            for (var i = 0; i < segsToDestroy.Count; i++)
            {
                var node = segsToDestroy.Nodes[i];
                if (node.Value == default) continue;

                var index = existingSegs.GetIndexAtPos(node.Position, node.Scale);
                if (index == -1) continue;

                ecb.DestroyEntity(existingSegs.Nodes[index].Value);
            }

            segsToCreate.Dispose();
            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        private void Populate(RendererPointAspect point, ref Octree<Entity> octree)
        {
            var scale = 0;
            var size = GetSegSize(_settings.BaseSegSize, scale);
            var pointPos = GetClosestSegPos(point.Position, size);

            for (var x = -1; x <= 1; x++)
            for (var y = -1; y <= 1; y++)
            for (var z = -1; z <= 1; z++)
            {
                var segPos = pointPos + new float3(x, y, z) * size;
                octree.SetAtPos(Placeholder, segPos, scale);
            }
        }

        private static readonly Entity Placeholder = new() { Index = -1, Version = -1 };

        [BurstCompile]
        private readonly struct Comparison : Octree<Entity>.IComparison
        {
            [BurstCompile]
            public bool Evaluate(in Entity a, in Entity b)
            {
                return a != default && b != default;
            }
        }
    }
}