using DataTypes;
using ECS.BufferElements.TerrainGeneration;
using ECS.Components.TerrainGeneration;
using ECS.Systems.Input;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Utility.SpacialPartitioning;
using Utility.TerrainGeneration;

namespace ECS.Systems.TerrainGeneration
{
    [UpdateAfter(typeof(PlayerSystem))]
    [BurstCompile]
    public partial struct TerrainModificationManagerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SegmentModifiedBufferElement>();
            state.RequireForUpdate<TgTreeBlob>();
            state.RequireForUpdate<BaseSegmentSettings>();
            state.RequireForUpdate<TerrainModificationBufferElement>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var tgTreeBlob = SystemAPI.GetSingleton<TgTreeBlob>();
            var settings = SystemAPI.GetSingleton<BaseSegmentSettings>();
            var terrainData = SystemAPI.GetSingletonRW<TerrainData>();
            var modifications = SystemAPI.GetSingletonBuffer<TerrainModificationBufferElement>();
            var bufferEntity = SystemAPI.GetSingletonEntity<SegmentModifiedBufferElement>();
            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var mod in modifications)
            {
                var segMin = SegmentOperations.GetClosestSegPos(mod.Origin - mod.Range, settings.BaseSegSize);
                var segMax = SegmentOperations.GetClosestSegPos(mod.Origin + mod.Range, settings.BaseSegSize);
                var range = (int3)((segMax - segMin) / settings.BaseSegSize);

                for (var a = 0; a <= range.x; a++)
                for (var b = 0; b <= range.y; b++)
                for (var c = 0; c <= range.z; c++)
                {
                    var abc = new float3(a, b, c);
                    var segPos = segMin + abc * settings.BaseSegSize;
                    var index = terrainData.ValueRO.Maps.GetIndexAtPos(segPos);

                    var map = index == -1
                        ? TerrainGenerator.CreateMap(settings, tgTreeBlob, segPos)
                        : new CubicArray<float>(settings.CubeCount,
                            terrainData.ValueRO.Maps.GetArray(index, Allocator.Temp));

                    index = index == -1
                        ? terrainData.ValueRW.Maps.PosToIndex(segPos)
                        : terrainData.ValueRW.Maps.GetIndexAtPos(segPos);

                    for (var x = 0; x < settings.CubeCount; x++)
                    for (var y = 0; y < settings.CubeCount; y++)
                    for (var z = 0; z < settings.CubeCount; z++)
                    {
                        var xyz = new int3(x, y, z);
                        var pos = segPos + (float3)xyz * settings.BaseCubeSize;
                        var dist = math.distance(pos, mod.Origin);
                        var val = map.GetAt(xyz);

                        if (mod.Addition) val += mod.Range - dist;
                        else val -= mod.Range - dist;

                        val = math.clamp(val, 0, 1);

                        if (dist <= mod.Range) map.SetAt(xyz, val);
                    }

                    terrainData.ValueRW.Maps.SetArray(index, map.Array);

                    ecb.AppendToBuffer<SegmentModifiedBufferElement>(bufferEntity, segPos);
                }
            }

            modifications.Clear();

            ecb.Playback(state.EntityManager);
        }
    }
}