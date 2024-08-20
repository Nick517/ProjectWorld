using ECS.Components.TerrainGeneration;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Aspects.TerrainGeneration
{
    public readonly partial struct TrackPointAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRO<LocalTransform> _localTransform;
        private readonly RefRW<ChunkPosition> _chunkPosition;

        public float3 Position => _localTransform.ValueRO.Position;

        public float3 ChunkPosition
        {
            get => _chunkPosition.ValueRO.Position;
            private set => _chunkPosition.ValueRW.Position = value;
        }

        public void UpdateChunkPosition(EntityCommandBuffer ecb, float3 newChunkPosition)
        {
            ChunkPosition = newChunkPosition;
            ecb.AddComponent<LoadChunksPointTag>(Entity);
        }
    }
}