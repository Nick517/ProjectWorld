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

            using var existingSegData = new NativeHashSet<SegData>(2000, Allocator.Temp);
            foreach (var seg in SystemAPI.Query<TerrainSegmentAspect>().WithAbsent<DestroySegmentTag>())
                existingSegData.Add(new SegData(seg.Position, seg.Scale, seg.Entity));

            var newSegData = new NativeHashSet<SegData>(1000, Allocator.Temp);

            foreach (var point in SystemAPI.Query<RendererPointAspect>().WithAll<UpdateRendererSegmentsTag>())
            {
                newSegData.UnionWith(CreateNewSegmentData(point));
                ecb.RemoveComponent<UpdateRendererSegmentsTag>(point.Entity);
            }

            var destroySegData = new NativeHashSet<SegData>(1000, Allocator.Temp);
            destroySegData.UnionWith(existingSegData);
            destroySegData.ExceptWith(newSegData);

            var createSegData = new NativeHashSet<SegData>(1000, Allocator.Temp);
            createSegData.UnionWith(newSegData);
            createSegData.ExceptWith(destroySegData);

            foreach (var seg in destroySegData) ecb.AddComponent<DestroySegmentTag>(seg.Entity);

            foreach (var seg in createSegData) Create(ecb, _settings, seg);

            ecb.Playback(state.EntityManager);
            newSegData.Dispose();
            destroySegData.Dispose();
            createSegData.Dispose();
        }

        private NativeHashSet<SegData> CreateNewSegmentData(RendererPointAspect point)
        {
            var scale = point.Settings.MaxSegmentScale;
            var segSize = GetSegSize(_settings.BaseSegSize, scale);
            var segPos = GetClosestSegPos(point.Position, segSize);
            var newSegData = new NativeHashSet<SegData>(1000, Allocator.Temp);

            for (var x = -1; x <= 1; x++)
            for (var y = -1; y <= 1; y++)
            for (var z = -1; z <= 1; z++)
            {
                var pos = segPos + (float3)segSize * new int3(x, y, z);

                newSegData.Add(new SegData(pos, scale));
            }

            return newSegData;
        }
    }
}