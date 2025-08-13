using DataTypes.Trees;
using ECS.Aspects.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(TrackPointSystem))]
    [BurstCompile]
    public partial struct RendererManagerSystem : ISystem
    {
        private BaseSegmentSettings _settings;
        private Comparison _comparison;
        private bool _initialized;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<RendererPoint>();
            state.RequireForUpdate<UpdateRendererSegmentsTag>();
            state.RequireForUpdate<TerrainData>();

            _comparison = new Comparison();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!_initialized)
            {
                _settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
                _initialized = true;
            }

            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            var terrainData = SystemAPI.GetSingletonRW<TerrainData>();

            var segsToCreate = new Octree<Entity>(_settings.BaseSegSize, Allocator.Temp);
            foreach (var point in SystemAPI.Query<RendererPointAspect>())
            {
                Populate(point, ref segsToCreate);
                ecb.RemoveComponent<UpdateRendererSegmentsTag>(point.Entity);
            }

            using var segsToIgnore =
                Octree<Entity>.Intersect(terrainData.ValueRO.Segments, segsToCreate, Allocator.Temp, _comparison);
            
            using var segsToDestroy =
                Octree<Entity>.Except(terrainData.ValueRO.Segments, segsToIgnore, Allocator.Temp, _comparison);

            segsToCreate.Except(segsToIgnore, _comparison);

            for (var i = 0; i < segsToCreate.Count; i++)
            {
                var node = segsToCreate.Nodes[i];
                if (node.Value == Placeholder)
                {
                    var entity = state.EntityManager.Instantiate(_settings.RendererSegmentPrefab);

                    ecb.AddComponent(entity, LocalTransform.FromPosition(node.Position));
                    ecb.AddComponent(entity, new SegmentScale { Scale = node.Scale });
                    ecb.AddComponent<CreateRendererMeshTag>(entity);

                    terrainData.ValueRW.Segments.SetAtPos(entity, node.Position, node.Scale);
                }
            }

            for (var i = 0; i < segsToDestroy.Count; i++)
            {
                var node = segsToDestroy.Nodes[i];
                if (node.Value == default) continue;

                var index = terrainData.ValueRO.Segments.GetIndexAtPos(node.Position, node.Scale);
                if (index == -1) continue;

                ecb.AddComponent(node.Value, new DestroySegmentTag());
                
                terrainData.ValueRW.Segments.SetAtIndex(default, index);
            }

            segsToCreate.Dispose();
            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        private void Populate(RendererPointAspect point, ref Octree<Entity> octree)
        {
            var megaSegCount = point.Settings.MegaSegments;
            var scale = point.Settings.MaxSegmentScale;
            var size = GetSegSize(_settings.BaseSegSize, scale);
            var pointPos = GetClosestSegPos(point.SegmentPosition, scale);
            var pointCenter = PosToCenter(pointPos, _settings.BaseSegSize);

            for (var x = -megaSegCount; x <= megaSegCount; x++)
            for (var y = -megaSegCount; y <= megaSegCount; y++)
            for (var z = -megaSegCount; z <= megaSegCount; z++)
            {
                var segPos = pointPos + new float3(x, y, z) * size;
                var index = octree.SetAtPos(default, segPos, scale);

                octree.Subdivide(index);

                var node = octree.Nodes[index];

                for (var oct = 0; oct < 8; oct++)
                    PopulateRecursive(pointCenter, point.Settings.LOD, ref octree, node.GetChild(oct));
            }
        }

        [BurstCompile]
        private void PopulateRecursive(float3 point, float lod, ref Octree<Entity> octree, int index)
        {
            if (index == -1) return;

            var node = octree.Nodes[index];

            if (node.Scale == 0)
            {
                octree.SetAtIndex(Placeholder, index);
                return;
            }

            var size = GetSegSize(_settings.BaseSegSize, node.Scale);
            var nodeCenter = PosToCenter(node.Position, size);
            var distance = math.distance(nodeCenter, point);

            if (size / distance <= lod)
            {
                octree.SetAtIndex(Placeholder, index);
                return;
            }

            octree.Subdivide(index);
            node = octree.Nodes[index];

            for (var oct = 0; oct < 8; oct++) PopulateRecursive(point, lod, ref octree, node.GetChild(oct));
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