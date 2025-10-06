using DataTypes.Trees;
using ECS.BufferElements.TerrainGeneration;
using ECS.BufferElements.TerrainGeneration.Renderer;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using TerrainData = ECS.Components.TerrainGeneration.TerrainData;

namespace ECS.Systems.TerrainGeneration.Renderer
{
    [BurstCompile]
    public partial struct TerrainDataInitializerSystem : ISystem
    {
        private bool _initialized;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_initialized) return;

            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            var entity = ecb.CreateEntity();

            ecb.AddComponent(entity, new TerrainData
            {
                Maps = new ArrayOctree<float>(settings.BaseCubeSize, settings.CubeCountTotal, Allocator.Persistent),
                Segments = new Octree<Entity>(settings.BaseSegSize, Allocator.Persistent)
            });

            ecb.AddBuffer<TerrainModificationBufferElement>(entity);
            ecb.AddBuffer<SegmentModifiedBufferElement>(entity);

            ecb.Playback(state.EntityManager);
            _initialized = true;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _initialized = false;
            
            var entity = SystemAPI.GetSingletonEntity<TerrainData>();
            var terrainData = state.EntityManager.GetComponentData<TerrainData>(entity);

            if (terrainData.Maps.IsCreated) terrainData.Maps.Dispose();
            if (terrainData.Segments.IsCreated) terrainData.Segments.Dispose();
        }
    }
}