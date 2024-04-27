using System;
using ECS.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ECS.Aspects
{
    public readonly partial struct ChunkAspect : IAspect
    {
        public readonly Entity Entity;

        private readonly RefRO<LocalTransform> _localTransform;
        private readonly RefRO<ChunkScaleComponent> _chunkScale;
        private float3 Position => _localTransform.ValueRO.Position;

        private float ChunkScale => _chunkScale.ValueRO.ChunkScale;


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
                return Position.Equals(data.Position) && Mathf.Approximately(ChunkScale, data.ChunkScale);
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

        public static Data ChunkAspectToChunkData(ChunkAspect chunkAspect)
        {
            return new Data(chunkAspect.Position, chunkAspect.ChunkScale);
        }

        public static void CreateChunk(EntityCommandBuffer entityCommandBuffer,
            ChunkGenerationSettingsComponent chunkGenerationSettings, Data data)
        {
            var chunkEntity = entityCommandBuffer.Instantiate(chunkGenerationSettings.ChunkPrefab);
            entityCommandBuffer.SetComponent(chunkEntity, LocalTransform.FromPosition(data.Position));
            entityCommandBuffer.AddComponent(chunkEntity,
                new ChunkScaleComponent { ChunkScale = data.ChunkScale });
            entityCommandBuffer.AddComponent<CreateChunkMeshDataTagComponent>(chunkEntity);
        }
    }
}