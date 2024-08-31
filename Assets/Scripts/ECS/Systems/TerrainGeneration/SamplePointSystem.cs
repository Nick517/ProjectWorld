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
            state.RequireForUpdate<TerrainGenTree>();
            state.RequireForUpdate<ChunkGenerationSettings>();
            state.RequireForUpdate<SamplePointTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var tgTree = SystemAPI.GetSingleton<TerrainGenTree>();
            var settings = SystemAPI.GetSingleton<ChunkGenerationSettings>();

            foreach (var point in SystemAPI.Query<RefRO<SamplePointTag>, RefRO<LocalTransform>>())
            {
                var position = point.Item2.ValueRO.Position;
                var cubePosition = ChunkOperations.GetClosestCubePosition(settings, position);

                var traversal = new TerrainGenTree.TgTree.Traversal(tgTree);
                var sample = traversal.Sample(cubePosition);
                traversal.Dispose();

                if (sample >= settings.MapSurface)
                    Debug.Log(
                        $"Collided at {cubePosition.x}, {cubePosition.y}, {cubePosition.z} with sample of: {sample}");
            }
        }
    }
}