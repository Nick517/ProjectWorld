using ECS.Aspects.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using static ECS.Aspects.TerrainGeneration.TerrainSegmentAspect;
using static Utility.TerrainGeneration.SegmentOperations;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(TrackPointSystem))]
    [BurstCompile]
    public partial struct TerrainSegmentManagerSystem : ISystem
    {
        private TerrainSegmentGenerationSettings _settings;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TerrainSegmentGenerationSettings>();
            state.RequireForUpdate<TrackPointTag>();
            state.RequireForUpdate<LoadTerrainSegmentsPointTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            _settings = SystemAPI.GetSingleton<TerrainSegmentGenerationSettings>();

            // Gather Data of all existing Segments not marked for destruction
            using var existingSegData = new NativeHashSet<Data>(2000, Allocator.Temp);
            foreach (var seg in SystemAPI.Query<TerrainSegmentAspect>().WithAbsent<DestroyTerrainSegmentTag>())
                existingSegData.Add(AspectToData(seg));

            // Create Data for all Segments that need to be created around the TrackPoint
            var point = SystemAPI.GetAspect<TrackPointAspect>(SystemAPI.GetSingletonEntity<TrackPointTag>());
            var newSegData = new NativeHashSet<Data>(200, Allocator.Temp);
            newSegData.UnionWith(CreateNewChunkData(point.SegmentPosition));
            ecb.RemoveComponent<LoadTerrainSegmentsPointTag>(point.Entity);

            // Gather Data from existingSegData that are not in newSegData
            var destroySegData = new NativeHashSet<Data>(200, Allocator.Temp);
            destroySegData.UnionWith(existingSegData);
            destroySegData.ExceptWith(newSegData);

            // Gather Data from newSegData that are not in existingSegData
            var createSegData = new NativeHashSet<Data>(200, Allocator.Temp);
            createSegData.UnionWith(newSegData);
            createSegData.ExceptWith(existingSegData);

            // Mark existing Segments whose data is in destroySegData for destruction
            foreach (var seg in SystemAPI.Query<TerrainSegmentAspect>())
                if (destroySegData.Contains(AspectToData(seg)))
                    ecb.AddComponent<DestroyTerrainSegmentTag>(seg.Entity);

            // Create Segments from data in createSegData
            foreach (var data in createSegData) Create(ecb, _settings, data);

            ecb.Playback(state.EntityManager);
            newSegData.Dispose();
            destroySegData.Dispose();
            createSegData.Dispose();
        }

        private readonly NativeHashSet<Data> CreateNewChunkData(float3 origin)
        {
            var maxSegScale = _settings.MaxSegmentScale;
            var megaSegs = _settings.MegaSegments;

            var subSegs = new NativeHashSet<Data>(8, Allocator.Temp);

            var pointPos = GetClosestSegmentPosition(_settings, origin, maxSegScale - 1);
            var segSize = GetSegmentSize(_settings, maxSegScale);

            for (var x = -megaSegs; x < megaSegs; x++)
            for (var y = -megaSegs; y < megaSegs; y++)
            for (var z = -megaSegs; z < megaSegs; z++)
            {
                var pos = new float3(x, y, z) * segSize + pointPos;

                var subData = new Data(pos, maxSegScale);
                subSegs.UnionWith(CreateSubSegmentData(subData, origin));
            }

            return subSegs;
        }

        private readonly NativeHashSet<Data> CreateSubSegmentData(Data data, float3 origin)
        {
            var subSegs = new NativeHashSet<Data>(8, Allocator.Temp);
            var subSegScale = data.Scale - 1;
            var subSegSize = GetSegmentSize(_settings, subSegScale);
            var originPos = GetClosestSegmentPosition(_settings, origin, subSegScale);

            for (var x = 0; x < 2; x++)
            for (var y = 0; y < 2; y++)
            for (var z = 0; z < 2; z++)
            {
                var subSegPos = new float3(x, y, z) * subSegSize + data.Position;
                var subSegData = new Data(subSegPos, subSegScale);
                var originSegCenter = GetClosestSegmentCenter(_settings, originPos, subSegScale);
                var subSegCenter = GetClosestSegmentCenter(_settings, subSegPos, subSegScale);
                var dist = math.distance(originSegCenter, subSegCenter);

                if (dist < subSegSize.x * _settings.LOD && subSegScale > 0)
                    subSegs.UnionWith(CreateSubSegmentData(subSegData, origin));
                else subSegs.Add(subSegData);
            }

            return subSegs;
        }
    }
}