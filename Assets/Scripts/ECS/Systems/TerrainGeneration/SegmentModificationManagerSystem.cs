using DataTypes.Trees;
using ECS.BufferElements.TerrainGeneration.Renderer;
using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using ECS.Systems.TerrainGeneration.Renderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(RendererManagerSystem))]
    [BurstCompile]
    public partial struct SegmentModificationManagerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<TerrainData>();
            state.RequireForUpdate<SegmentModifiedBufferElement>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            var terrainData = SystemAPI.GetSingletonRW<TerrainData>();
            var segMods = SystemAPI.GetSingletonBuffer<SegmentModifiedBufferElement>();

            using var padded = new Octree<bool>(settings.BaseSegSize, Allocator.Temp);

            foreach (var mod in segMods)
                for (var x = -1; x <= 0; x++)
                for (var y = -1; y <= 0; y++)
                for (var z = -1; z <= 0; z++)
                    padded.SetAtPos(true, mod + new float3(x, y, z) * settings.BaseSegSize);
            
            foreach (var seg in padded.Nodes)
            {
                if (!seg.Value) continue;

                var index = terrainData.ValueRO.Segments.GetLeafAtPos(seg.Position);

                if (index == -1) continue;

                var node = terrainData.ValueRO.Segments.Nodes[index];

                if (node.Value != Placeholder && node.Value != default) ecb.AddComponent<DestroySegmentTag>(node.Value);

                var entity = state.EntityManager.Instantiate(settings.RendererSegmentPrefab);

                ecb.AddComponent(entity, new SegmentInfo { Position = node.Position, Scale = node.Scale });
                ecb.AddComponent<CreateRendererMeshTag>(entity);

                terrainData.ValueRW.Segments.SetAtPos(entity, node.Position, node.Scale);
            }

            segMods.Clear();

            ecb.Playback(state.EntityManager);
        }

        private static readonly Entity Placeholder = new() { Index = -1, Version = -1 };
    }
}