using ECS.Components.TerrainGeneration;
using ECS.Components.TerrainGeneration.Renderer;
using ECS.Systems.TerrainGeneration.Renderer;
using TerrainGenerationGraph;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEditor;
using UnityEngine;
using static Utility.ECS.EntityConversion;
using static Utility.SpacialPartitioning.SegmentOperations;
using TerrainData = ECS.Components.TerrainGeneration.TerrainData;

namespace Debugging.TerrainGeneration
{
    [ExecuteAlways]
    public class TerrainTest : MonoBehaviour
    {
        private const string WorldName = "Test World";

        public GameObject rendererSegmentPrefab;
        public TgGraph tgGraph;
        public float baseCubeSize;
        public int cubeCount;
        [Range(0, 1)] public float mapSurface;

        public World World;

        private Entity _segmentEntity;
        private RenderMeshArray _renderMeshArray;

        private bool _initialized;

        private void OnEnable()
        {
            EditorApplication.update += EditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= EditorUpdate;
            Dispose();
        }

        public void Initialize()
        {
            if (_initialized) return;

            World = DefaultWorldInitialization.Initialize(WorldName, true);

            var simGroup = World.GetOrCreateSystemManaged<SimulationSystemGroup>();
            simGroup.AddSystemToUpdateList(World.GetOrCreateSystem<TerrainDataManagerSystem>());
            simGroup.AddSystemToUpdateList(World.GetOrCreateSystem<CreateRendererMeshSystem>());
            simGroup.AddSystemToUpdateList(World.GetOrCreateSystem<SetRendererMeshSystem>());

            var em = World.EntityManager;

            _segmentEntity = ConvertPrefab(rendererSegmentPrefab, em);

            em.AddComponentData(em.CreateEntity(), new BaseSegmentSettings
            {
                RendererSegmentPrefab = _segmentEntity,
                Material = rendererSegmentPrefab.GetComponent<MeshRenderer>().sharedMaterial,
                BaseCubeSize = baseCubeSize,
                CubeCount = cubeCount,
                MapSurface = mapSurface,
                MaxSegmentsPerFrame = 64
            });

            var treeBuilder = new TreeBuilder(tgGraph);
            var blobReference = treeBuilder.BuildBlob();
            em.AddComponentData(em.CreateEntity(), new TgTreeBlob { Blob = blobReference });

            _initialized = true;
        }

        public void CreateSegment(float3 position, int scale)
        {
            Initialize();

            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = World.EntityManager;
            var settings = entityManager.CreateEntityQuery(typeof(BaseSegmentSettings))
                .GetSingleton<BaseSegmentSettings>();
            var terrainData = entityManager.CreateEntityQuery(typeof(TerrainData)).GetSingletonRW<TerrainData>();

            if (terrainData.ValueRO.Segments.GetIndexAtPos(position, scale) != -1) return;
            
            var segPos = GetClosestSegPos(position, GetSegSize(settings.BaseSegSize, scale));
            var entity = ecb.Instantiate(_segmentEntity);

            ecb.AddComponent(entity, new SegmentInfo { Position = segPos, Scale = scale });
            ecb.AddComponent<CreateRendererMeshTag>(entity);
            terrainData.ValueRW.Segments.SetAtPos(entity, segPos, scale);

            ecb.Playback(World.EntityManager);
        }
        
        public void Dispose()
        {
            if (World is not { IsCreated: true }) return;

            World.Dispose();

            _initialized = false;
        }

        private void EditorUpdate()
        {
            if (!this) return;

            Initialize();

            World.Update();

            SceneView.RepaintAll();
        }

        private void OnValidate()
        {
            _initialized = false;
        }
    }
}