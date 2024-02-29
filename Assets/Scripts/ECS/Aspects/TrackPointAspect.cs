using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Terrain
{
    public readonly partial struct TrackPointAspect : IAspect
    {
        public readonly Entity entity;

        private readonly RefRO<LocalTransform> _localTransform;
        private readonly RefRW<ChunkPositionComponent> _chunkPosition;

        public float3 Position => _localTransform.ValueRO.Position;

        public float3 ChunkPosition
        {
            get => _chunkPosition.ValueRO.position;
            set => _chunkPosition.ValueRW.position = value;
        }

        public void UpdateChunkPosition(EntityCommandBuffer entityCommandBuffer, float3 newChunkPosition)
        {
            ChunkPosition = newChunkPosition;
            entityCommandBuffer.AddComponent<LoadChunksPointTagComponent>(entity);
        }
    }
}
