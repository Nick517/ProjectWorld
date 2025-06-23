using ECS.Components.Input;
using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Logging;
using Unity.Physics;
using Unity.Transforms;
using Utility.SpacialPartitioning;
using Utility.TerrainGeneration;

namespace ECS.Systems.Input
{
    [UpdateAfter(typeof(InputManagerSystem))]
    [BurstCompile]
    public partial struct PlayerSystem : ISystem
    {
        private TgTreeBlob _tgTreeBlob;
        private BaseSegmentSettings _segmentSettings;
        private PlayerSettings _playerSettings;
        private bool _initialized;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TgTreeBlob>();
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<TerrainData>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<PlayerInput>();
            state.RequireForUpdate<PlayerSettings>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!_initialized)
            {
                _tgTreeBlob = SystemAPI.GetSingleton<TgTreeBlob>();
                _segmentSettings = SystemAPI.GetSingleton<BaseSegmentSettings>();
                _playerSettings = SystemAPI.GetSingleton<PlayerSettings>();
                _initialized = true;
            }

            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            var playerInput = SystemAPI.GetSingleton<PlayerInput>();
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerSettings>();
            var transform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;


            if (playerInput.RemoveTerrain)
            {
                var rayEnd = transform.Position + transform.Forward() * _playerSettings.InteractionRange;
                var rayInput = new RaycastInput
                {
                    Start = transform.Position,
                    End = rayEnd,
                    Filter = CollisionFilter.Default
                };

                if (physicsWorld.CastRay(rayInput, out var hit))
                {
                    var terrainData = SystemAPI.GetSingletonRW<TerrainData>();
                    var segPos = SegmentOperations.GetClosestSegPos(hit.Position, _segmentSettings.BaseSegSize);
                    var index = terrainData.ValueRW.Maps.GetIndexAtPos(segPos);

                    if (index == -1)
                    {
                        index = terrainData.ValueRW.Maps.PosToIndex(segPos);

                        var map = TerrainGenerator.PopulateMap(_segmentSettings, _tgTreeBlob, segPos, 0);
                        terrainData.ValueRW.Maps.SetArray(index, map);

                        Log.Debug($"Created seg at {segPos.ToString()}, Index: {index}");
                    }
                    else
                    {
                        Log.Debug($"Seg already exists at {segPos.ToString()}");
                    }
                }
            }

            if (playerInput.ThrowObject)
            {
                var forceVector = transform.Forward() * _playerSettings.ThrowForce;
                var throwEntity = ecb.Instantiate(_playerSettings.ThrowObjectPrefab);
                ecb.SetComponent(throwEntity,
                    LocalTransform.FromPositionRotation(transform.Position, transform.Rotation));
                ecb.SetComponent(throwEntity, new PhysicsVelocity { Linear = forceVector });
            }

            ecb.Playback(state.EntityManager);
        }
    }
}