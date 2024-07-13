using ECS.BufferElements;
using ECS.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ECS.Jobs
{
    [BurstCompile]
    public struct CreateMeshDataJob : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public EntityTypeHandle EntityTypeHandle;
        [ReadOnly] public ComponentTypeHandle<LocalTransform> LocalTransformTypeHandle;
        [ReadOnly] public ComponentTypeHandle<ChunkScale> ChunkScaleTypeHandle;
        [ReadOnly] public ChunkGenerationSettings Settings;
        [ReadOnly] public TerrainGenerationTree TgTree;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            var entities = chunk.GetNativeArray(EntityTypeHandle);
            var localTransforms = chunk.GetNativeArray(ref LocalTransformTypeHandle);
            var chunkScales = chunk.GetNativeArray(ref ChunkScaleTypeHandle);

            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

            while (enumerator.NextEntityIndex(out var i))
            {
                var entity = entities[i];
                var position = localTransforms[i].Position;
                var chunkScale = chunkScales[i].Scale;

                var vertices = new NativeList<float3>(Allocator.Temp);
                var cubeMap =
                    new NativeArray<float>(
                        TerrainGenerator.PopulateMap(Settings, TgTree, position, chunkScale),
                        Allocator.Temp);

                _ = Ecb.AddBuffer<TriangleBufferElement>(i, entity);
                _ = Ecb.AddBuffer<VerticeBufferElement>(i, entity);

                for (var x = 0; x < Settings.CubeCount; x++)
                for (var y = 0; y < Settings.CubeCount; y++)
                for (var z = 0; z < Settings.CubeCount; z++)
                    MarchCube(i, Ecb, Settings, entity, chunkScale, cubeMap, new int3(x, y, z), ref vertices);

                foreach (var vertice in vertices) Ecb.AppendToBuffer<VerticeBufferElement>(i, entity, vertice);

                Ecb.RemoveComponent<CreateChunkMeshDataTag>(i, entity);
                Ecb.AddComponent<SetChunkMeshTag>(i, entity);
            }
        }

        private static void MarchCube(int sortKey, EntityCommandBuffer.ParallelWriter ecb,
            ChunkGenerationSettings settings, Entity entity, float chunkScale, NativeArray<float> cubeMap,
            int3 position, ref NativeList<float3> vertices)
        {
            var cubeSize = ChunkOperations.GetCubeSize(settings, chunkScale);

            var cube = new NativeArray<float>(8, Allocator.Temp);

            for (var i = 0; i < 8; i++)
                cube[i] = SampleMap(settings, cubeMap, position + MarchingCubeTables.Corner(i));

            var configIndex = GetCubeConfiguration(settings, cube);

            if (configIndex is 0 or 255) return;

            var edgeIndex = 0;

            for (var i = 0; i < 15; i++)
            {
                var indice = MarchingCubeTables.Triangle(configIndex, edgeIndex);

                if (indice == -1) return;

                var verticePosition = IndiceForVerticePosition(settings, position, ref cube, indice);
                var vertice = VerticeForIndice(verticePosition * cubeSize, ref vertices);

                ecb.AppendToBuffer<TriangleBufferElement>(sortKey, entity, vertice);

                edgeIndex++;
            }
        }

        private static float SampleMap(ChunkGenerationSettings settings, NativeArray<float> cubeMap, int3 point)
        {
            return TerrainGenerator.GetCube(cubeMap, settings.CubeCount, point);
        }

        private static int GetCubeConfiguration(ChunkGenerationSettings settings, NativeArray<float> cube)
        {
            var configIndex = 0;

            for (var i = 0; i < 8; i++)
                if (cube[i] <= settings.MapSurface)
                    configIndex |= 1 << i;

            return configIndex;
        }

        private static int VerticeForIndice(float3 vertice, ref NativeList<float3> vertices)
        {
            for (var i = 0; i < vertices.Length; i++)
                if (vertices[i].Equals(vertice))
                    return i;

            vertices.Add(vertice);

            return vertices.Length - 1;
        }

        private static float3 IndiceForVerticePosition(ChunkGenerationSettings settings, int3 position,
            ref NativeArray<float> cube, int indice)
        {
            var vertice1 = position + MarchingCubeTables.Corner(MarchingCubeTables.Edge(indice, 0));
            var vertice2 = position + MarchingCubeTables.Corner(MarchingCubeTables.Edge(indice, 1));

            var vertice1Sample = cube[MarchingCubeTables.Edge(indice, 0)];
            var vertice2Sample = cube[MarchingCubeTables.Edge(indice, 1)];

            var difference = vertice2Sample - vertice1Sample;

            difference = difference == 0 ? settings.MapSurface : (settings.MapSurface - vertice1Sample) / difference;

            var verticePosition = vertice1 + (float3)(vertice2 - vertice1) * difference;

            return verticePosition;
        }
    }
}