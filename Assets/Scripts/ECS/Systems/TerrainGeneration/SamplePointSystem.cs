using ECS.Components.TerrainGeneration;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Utility.TerrainGeneration;

namespace ECS.Systems.TerrainGeneration
{
    [BurstCompile]
    public partial struct SamplePointSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TerrainGenerationTreeBlob>();
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<SamplePointTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var tgTree = SystemAPI.GetSingleton<TerrainGenerationTreeBlob>();
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();

            foreach (var point in SystemAPI.Query<RefRO<SamplePointTag>, RefRO<LocalTransform>>())
            {
                var pos = point.Item2.ValueRO.Position;
                var cubePos = SegmentOperations.GetClosestCubePosition(settings, pos);

                var traversal = new TerrainGenerationTreeBlob.TerrainGenerationTree.Traversal(tgTree);
                var sample = traversal.Sample(cubePos);
                traversal.Dispose();

                if (sample >= settings.MapSurface)
                    Debug.Log($"Collided at {cubePos.x}, {cubePos.y}, {cubePos.z} with sample of: {sample}");
            }
        }
    }
}