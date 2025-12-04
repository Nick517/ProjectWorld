using DataTypes.Trees;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static Utility.SpacialPartitioning.SegmentOperations;
using static Utility.TerrainGeneration.SegmentTags;

namespace ECS.Systems.TerrainGeneration
{
    [BurstCompile]
    public partial struct ColliderManagerSystem : ISystem
    {
        private BaseSegmentSettings _settings;
        private RefRW<TerrainData> _terrainData;
        private CompareSegments _compareSegs;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<TrackPoint>();
            state.RequireForUpdate<TerrainData>();
            state.RequireForUpdate<UpdateColliderSegmentsTag>();

            _compareSegs = new CompareSegments();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            var pointEntity = SystemAPI.GetSingletonEntity<TrackPoint>();
            var point = SystemAPI.GetComponent<TrackPoint>(pointEntity);
            ecb.RemoveComponent<UpdateColliderSegmentsTag>(pointEntity);

            _settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            _terrainData = SystemAPI.GetSingletonRW<TerrainData>();

            var segsToCreate = Populate(point);

            using var segsToIgnore =
                Octree<Entity>.Intersect(_terrainData.ValueRO.ColliderSegs, segsToCreate, Allocator.Temp, _compareSegs);

            using var segsToDestroy =
                Octree<Entity>.Except(_terrainData.ValueRO.ColliderSegs, segsToIgnore, Allocator.Temp, _compareSegs);

            segsToCreate.Except(segsToIgnore, _compareSegs);

            for (var i = 0; i < segsToCreate.Count; i++)
            {
                var node = segsToCreate.Nodes[i];
                if (node.Value != PlaceHolder) continue;

                var entity = state.EntityManager.CreateEntity();

                ecb.AddComponent(entity, new SegmentInfo
                {
                    Position = node.Position,
                    Scale = node.Scale,
                    IsRenderer = false
                });

                ecb.AddComponent<CreateMeshTag>(entity);

                _terrainData.ValueRW.ColliderSegs.SetAtPos(entity, node.Position, node.Scale);
            }

            for (var i = 0; i < segsToDestroy.Count; i++)
            {
                var node = segsToDestroy.Nodes[i];
                if (node.Value == default) continue;

                var index = _terrainData.ValueRO.ColliderSegs.GetIndexAtPos(node.Position, node.Scale);
                if (index == -1) continue;

                if (_terrainData.ValueRO.ColliderSegs.GetAtIndex(index).IsTag()) continue;

                ecb.AddComponent<DestroySegmentTag>(node.Value);

                _terrainData.ValueRW.ColliderSegs.SetAtIndex(default, index);
            }

            segsToCreate.Dispose();

            ecb.Playback(state.EntityManager);
        }

        private Octree<Entity> Populate(TrackPoint point)
        {
            var segments = new Octree<Entity>(_settings.BaseSegSize, Allocator.Temp);
            var megaSegCount = point.ColliderMegaSegments;
            var scale = point.ColliderMaxSegmentScale;
            var size = GetSegSize(_settings.BaseSegSize, scale);
            var pointPos = GetClosestSegPos(point.SegmentPosition, size);

            for (var x = -megaSegCount; x <= megaSegCount; x++)
            for (var y = -megaSegCount; y <= megaSegCount; y++)
            for (var z = -megaSegCount; z <= megaSegCount; z++)
            {
                var segPos = pointPos + new float3(x, y, z) * size;
                if (IsActive(segPos, scale)) segments.SetAtPos(PlaceHolder, segPos, scale);
            }

            return segments;
        }

        private bool IsActive(float3 position, int scale)
        {
            return _terrainData.ValueRO.RendererSegs.GetAtPos(position, scale) != Inactive;
        }
    }
}