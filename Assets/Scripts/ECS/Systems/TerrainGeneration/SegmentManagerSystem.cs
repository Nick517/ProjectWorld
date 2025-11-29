using DataTypes.Trees;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(TrackPointSystem))]
    [BurstCompile]
    public partial struct SegmentManagerSystem : ISystem
    {
        private BaseSegmentSettings _settings;
        private RefRW<TerrainData> _terrainData;
        private CompareSegments _compareSegs;
        private CheckInactive _checkInactive;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<TrackPoint>();
            state.RequireForUpdate<TerrainData>();

            _compareSegs = new CompareSegments();
            _checkInactive = new CheckInactive();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var point = SystemAPI.GetSingletonRW<TrackPoint>();
            if (!point.ValueRO.Update) return;
            point.ValueRW.Update = false;

            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            _settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            _terrainData = SystemAPI.GetSingletonRW<TerrainData>();

            UpdateSegments(ref state, ecb, point.ValueRO, ref _terrainData.ValueRW.RendererSegs, true);
            UpdateSegments(ref state, ecb, point.ValueRO, ref _terrainData.ValueRW.ColliderSegs, false);

            ecb.Playback(state.EntityManager);
        }

        private void UpdateSegments(ref SystemState state, EntityCommandBuffer ecb, TrackPoint point,
            ref Octree<Entity> segments, bool isRenderer)
        {
            var segsToCreate = new Octree<Entity>(_settings.BaseSegSize, Allocator.Temp);
            Populate(point, ref segsToCreate, isRenderer);
            segsToCreate.Except(_terrainData.ValueRO.InactiveSegs, _checkInactive);

            using var segsToIgnore =
                Octree<Entity>.Intersect(segments, segsToCreate, Allocator.Temp, _compareSegs);

            using var segsToDestroy =
                Octree<Entity>.Except(segments, segsToIgnore, Allocator.Temp, _compareSegs);

            segsToCreate.Except(segsToIgnore, _compareSegs);

            for (var i = 0; i < segsToCreate.Count; i++)
            {
                var node = segsToCreate.Nodes[i];
                if (node.Value != PlaceHolder) continue;

                var entity = isRenderer
                    ? state.EntityManager.Instantiate(_settings.RendererSegmentPrefab)
                    : state.EntityManager.CreateEntity();

                ecb.AddComponent(entity, new SegmentInfo
                {
                    Position = node.Position,
                    Scale = node.Scale,
                    IsRenderer = isRenderer
                });

                ecb.AddComponent<CreateMeshTag>(entity);
                ecb.AddComponent(entity, LocalTransform.FromPosition(node.Position));

                segments.SetAtPos(entity, node.Position, node.Scale);
            }

            for (var i = 0; i < segsToDestroy.Count; i++)
            {
                var node = segsToDestroy.Nodes[i];
                if (node.Value == default) continue;

                ecb.AddComponent<DestroySegmentTag>(node.Value);

                segments.SetAtPos(default, node.Position, node.Scale);
            }

            segsToCreate.Dispose();
        }

        private void Populate(TrackPoint point, ref Octree<Entity> segments, bool isRenderer)
        {
            var megaSegCount = isRenderer ? point.RendererMegaSegments : point.ColliderMegaSegments;
            var scale = isRenderer ? point.RendererMaxSegmentScale : point.ColliderMaxSegmentScale;
            var size = GetSegSize(_settings.BaseSegSize, scale);
            var pointPos = GetClosestSegPos(point.SegmentPosition, size);
            var lod = isRenderer ? point.RendererLOD : point.ColliderLOD;

            for (var x = -megaSegCount; x <= megaSegCount; x++)
            for (var y = -megaSegCount; y <= megaSegCount; y++)
            for (var z = -megaSegCount; z <= megaSegCount; z++)
            {
                var segPos = pointPos + new float3(x, y, z) * size;
                var index = segments.SetAtPos(default, segPos, scale);

                PopulateRecursive(point, ref segments, index, lod);
            }
        }

        private void PopulateRecursive(TrackPoint point, ref Octree<Entity> segments, int index, float lod)
        {
            var node = segments.Nodes[index];

            if (node.Scale == 0)
            {
                segments.SetAtIndex(PlaceHolder, index);
                return;
            }

            var size = GetSegSize(_settings.BaseSegSize, node.Scale);
            var center = PosToCenter(node.Position, size);
            var distance = math.distance(center, point.SegmentPosition);

            if (size / distance <= lod)
            {
                segments.SetAtIndex(PlaceHolder, index);
                return;
            }

            segments.Subdivide(index);
            node = segments.Nodes[index];

            for (var oct = 0; oct < 8; oct++) PopulateRecursive(point, ref segments, node.GetChild(oct), lod);
        }

        private static readonly Entity PlaceHolder = new() { Index = -1, Version = -1 };

        private readonly struct CompareSegments : Octree<Entity>.IComparison<Entity>
        {
            public bool Evaluate(in Entity a, in Entity b)
            {
                return a != default && b != default;
            }
        }

        private readonly struct CheckInactive : Octree<Entity>.IComparison<bool>
        {
            public bool Evaluate(in Entity a, in bool b)
            {
                return b;
            }
        }
    }
}