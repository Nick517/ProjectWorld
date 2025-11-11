using DataTypes.Trees;
using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static Utility.SpacialPartitioning.SegmentOperations;
using static Utility.TerrainGeneration.SegmentTags;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [UpdateAfter(typeof(TerrainDataManagerSystem))]
    [BurstCompile]
    public partial struct RendererManagerSystem : ISystem
    {
        private BaseSegmentSettings _settings;
        private RefRW<TerrainData> _terrainData;
        private SegmentComparison _comparison;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<RendererPoint>();
            state.RequireForUpdate<TerrainData>();

            _comparison = new SegmentComparison();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var pointEntity = SystemAPI.GetSingletonEntity<RendererPoint>();
            var point = SystemAPI.GetComponentRW<RendererPoint>(pointEntity);
            if (!point.ValueRO.Update) return;
            point.ValueRW.Update = false;
            
            _settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            _terrainData = SystemAPI.GetSingletonRW<TerrainData>();

            var pointPos = SystemAPI.GetComponentRO<LocalTransform>(pointEntity).ValueRO.Position;
            var segsToCreate = new Octree<Entity>(_settings.BaseSegSize, Allocator.Temp);
            Populate(point.ValueRO, pointPos, ref segsToCreate);

            using var segsToIgnore =
                Octree<Entity>.Intersect(_terrainData.ValueRO.Segments, segsToCreate, Allocator.Temp, _comparison);

            using var segsToDestroy =
                Octree<Entity>.Except(_terrainData.ValueRO.Segments, segsToIgnore, Allocator.Temp, _comparison);

            segsToCreate.Except(segsToIgnore, _comparison);

            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            for (var i = 0; i < segsToCreate.Count; i++)
            {
                var node = segsToCreate.Nodes[i];
                if (node.Value.IsValidSegment()) continue;

                var entity = state.EntityManager.Instantiate(_settings.RendererSegmentPrefab);

                ecb.AddComponent(entity, new SegmentInfo { Position = node.Position, Scale = node.Scale });
                ecb.AddComponent<CreateRendererMeshTag>(entity);

                _terrainData.ValueRW.Segments.SetAtPos(entity, node.Position, node.Scale);
            }

            for (var i = 0; i < segsToDestroy.Count; i++)
            {
                var node = segsToDestroy.Nodes[i];
                if (node.Value == default) continue;

                var index = _terrainData.ValueRO.Segments.GetIndexAtPos(node.Position, node.Scale);
                if (index == -1) continue;

                var seg = _terrainData.ValueRO.Segments.GetAtIndex(index);
                if (!seg.IsValidSegment()) continue;

                ecb.AddComponent<DestroySegmentTag>(seg);

                _terrainData.ValueRW.Segments.SetAtIndex(default, index);
            }

            segsToCreate.Dispose();
            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        private void Populate(RendererPoint point, float3 pointPos, ref Octree<Entity> segments)
        {
            var megaSegCount = point.MegaSegments;
            var scale = point.MaxSegmentScale;
            var size = GetSegSize(_settings.BaseSegSize, scale);
            pointPos = GetClosestSegPos(pointPos, scale);
            var pointCenter = PosToCenter(pointPos, _settings.BaseSegSize);

            for (var x = -megaSegCount; x <= megaSegCount; x++)
            for (var y = -megaSegCount; y <= megaSegCount; y++)
            for (var z = -megaSegCount; z <= megaSegCount; z++)
            {
                var segPos = pointPos + new float3(x, y, z) * size;
                var index = segments.SetAtPos(default, segPos, scale);

                segments.Subdivide(index);

                var node = segments.Nodes[index];

                for (var oct = 0; oct < 8; oct++)
                    PopulateRecursive(pointCenter, point.LOD, ref segments, node.GetChild(oct));
            }
        }

        [BurstCompile]
        private void PopulateRecursive(float3 pos, float lod, ref Octree<Entity> segments, int index)
        {
            if (index == -1) return;

            var node = segments.Nodes[index];
            var active = _terrainData.ValueRO.Segments.GetAtPos(node.Position, node.Scale) != Inactive;

            if (node.Scale == 0)
            {
                if (active) segments.SetAtIndex(PlaceHolder, index);
                return;
            }

            var size = GetSegSize(_settings.BaseSegSize, node.Scale);
            var center = PosToCenter(node.Position, size);
            var distance = math.distance(center, pos);

            if (size / distance <= lod)
            {
                if (active) segments.SetAtIndex(PlaceHolder, index);
                return;
            }

            segments.Subdivide(index);
            node = segments.Nodes[index];

            for (var oct = 0; oct < 8; oct++) PopulateRecursive(pos, lod, ref segments, node.GetChild(oct));
        }

        [BurstCompile]
        private readonly struct SegmentComparison : Octree<Entity>.IComparison
        {
            [BurstCompile]
            public bool Evaluate(in Entity a, in Entity b)
            {
                return a != default && b != default;
            }
        }
    }
}