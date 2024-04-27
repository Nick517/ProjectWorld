using ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Aspects
{
    public readonly partial struct TrackPointAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRO<LocalTransform> _localTransform;
        private readonly RefRW<ChunkPositionComponent> _chunkPosition;

        public float3 Position => _localTransform.ValueRO.Position;

        public float3 ChunkPosition
        {
            get => _chunkPosition.ValueRO.Position;
            private set => _chunkPosition.ValueRW.Position = value;
        }

        public void UpdateChunkPosition(EntityCommandBuffer entityCommandBuffer, float3 newChunkPosition)
        {
            ChunkPosition = newChunkPosition;
            entityCommandBuffer.AddComponent<LoadChunksPointTagComponent>(Entity);
        }
    }
}