using System.Linq;
using DataTypes.Trees;
using ECS.BufferElements.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(TerrainModificationManagerSystem))]
    public partial struct SegmentModificationManagerSystem : ISystem
    {
        private BaseSegmentSettings _settings;
        private RefRW<TerrainData> _terrainData;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<SegmentModifiedBufferElement>();
        }

        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            _settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            _terrainData = SystemAPI.GetSingletonRW<TerrainData>();
            var segMods = SystemAPI.GetSingletonBuffer<SegmentModifiedBufferElement>();

            using var padded = new Octree<bool>(_settings.BaseSegSize, Allocator.Temp);

            foreach (var mod in segMods)
                for (var x = -1; x <= 0; x++)
                for (var y = -1; y <= 0; y++)
                for (var z = -1; z <= 0; z++)
                    padded.SetAtPos(true, mod + new float3(x, y, z) * _settings.BaseSegSize);

            UpdateSegments(ref state, ecb, ref _terrainData.ValueRW.RendererSegs, padded, true);
            UpdateSegments(ref state, ecb, ref _terrainData.ValueRW.ColliderSegs, padded, false);

            segMods.Clear();

            ecb.Playback(state.EntityManager);
        }

        private void UpdateSegments(ref SystemState state, EntityCommandBuffer ecb, ref Octree<Entity> segments,
            in Octree<bool> padded, bool isRenderer)
        {
            foreach (var node in padded.Nodes.Where(node => node.Value))
            {
                var index = segments.GetLeafAtPos(node.Position);
                if (index == -1) continue;

                var seg = segments.GetAtIndex(index);

                if (seg != default) ecb.AddComponent<DestroySegmentTag>(seg);

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

                segments.SetAtIndex(entity, index);
            }
        }
    }
}