using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Terrain
{
    public readonly partial struct ChunkAspect : IAspect
    {
        public readonly Entity entity;

        private readonly RefRO<LocalTransform> _localTransform;
        private readonly RefRO<ChunkScaleComponent> _chunkScale;
        public float3 Position => _localTransform.ValueRO.Position;

        public readonly float ChunkScale => _chunkScale.ValueRO.chunkScale;


        public struct ChunkData : IEquatable<ChunkData>
        {
            public float3 position;
            public float chunkScale;

            public ChunkData(float3 position, float chunkScale) : this()
            {
                this.position = position;
                this.chunkScale = chunkScale;
            }

            public bool Equals(ChunkData chunkData)
            {
                return position.Equals(chunkData.position) && chunkScale == chunkData.chunkScale;
            }

            public override readonly int GetHashCode()
            {
                int hash = 0;
                hash += (int)position.x;
                hash += (int)position.y * 10;
                hash += (int)position.z * 100;
                hash += (int)chunkScale * 1000;

                return hash;
            }
        }

        public static ChunkData ChunkAspectToChunkData(ChunkAspect chunkAspect)
        {
            return new(chunkAspect.Position, chunkAspect.ChunkScale);
        }

        public static void CreateChunk(EntityCommandBuffer entityCommandBuffer, ChunkGenerationSettingsComponent chunkGenerationSettings, ChunkData chunkData)
        {
            Entity chunkEntity = entityCommandBuffer.Instantiate(chunkGenerationSettings.chunkPrefab);
            entityCommandBuffer.SetComponent(chunkEntity, LocalTransform.FromPosition(chunkData.position));
            entityCommandBuffer.AddComponent(chunkEntity, new ChunkScaleComponent() { chunkScale = chunkData.chunkScale });
            entityCommandBuffer.AddComponent<CreateChunkMeshDataTagComponent>(chunkEntity);
        }
    }
}
