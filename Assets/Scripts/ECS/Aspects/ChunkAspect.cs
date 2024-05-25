using System;
using ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Aspects
{
    public readonly partial struct ChunkAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRO<LocalTransform> _localTransform;
        private readonly RefRO<ChunkScale> _chunkScale;
        private float3 Position => _localTransform.ValueRO.Position;

        private float Scale => _chunkScale.ValueRO.Scale;


        public struct Data : IEquatable<Data>
        {
            public float3 Position;
            public readonly float ChunkScale;

            public Data(float3 position, float chunkScale) : this()
            {
                Position = position;
                ChunkScale = chunkScale;
            }

            public bool Equals(Data data)
            {
                return Position.Equals(data.Position) && Math.Abs(ChunkScale - data.ChunkScale) == 0;
            }

            public readonly override int GetHashCode()
            {
                var hash = 0;
                hash += (int)Position.x;
                hash += (int)Position.y * 10;
                hash += (int)Position.z * 100;
                hash += (int)ChunkScale * 1000;

                return hash;
            }
        }

        public static Data ChunkAspectToChunkData(ChunkAspect chunk)
        {
            return new Data(chunk.Position, chunk.Scale);
        }

        public static void CreateChunk(EntityCommandBuffer ecb, ChunkGenerationSettings settings, Data data)
        {
            var chunkEntity = ecb.Instantiate(settings.ChunkPrefab);
            ecb.SetComponent(chunkEntity, LocalTransform.FromPosition(data.Position));
            ecb.AddComponent(chunkEntity, new ChunkScale { Scale = data.ChunkScale });
            ecb.AddComponent<CreateChunkMeshDataTag>(chunkEntity);
        }
    }
}