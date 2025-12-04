using DataTypes.Trees;
using ECS.BufferElements.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ECS.Systems.TerrainGeneration
{
    [BurstCompile]
    public partial struct TerrainDataInitializationSystem : ISystem
    {
        private bool _initialized;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BaseSegmentSettings>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_initialized) return;
            
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            var entity = state.EntityManager.CreateEntity();

            state.EntityManager.AddComponentData(entity, new TerrainData
            {
                Maps = new ArrayOctree<float>(settings.BaseCubeSize, settings.CubeCountTotal, Allocator.Persistent),
                RendererSegs = new Octree<Entity>(settings.BaseSegSize, Allocator.Persistent),
                ColliderSegs = new Octree<Entity>(settings.BaseSegSize, Allocator.Persistent)
            });

            state.EntityManager.AddBuffer<TerrainModificationBufferElement>(entity);
            state.EntityManager.AddBuffer<SegmentModifiedBufferElement>(entity);
            
            _initialized = true;
        }
    }
}