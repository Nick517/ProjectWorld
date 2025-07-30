using DataTypes;
using DataTypes.Trees;
using ECS.Components.TerrainGeneration;
using Unity.Collections;
using Unity.Mathematics;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace Utility.TerrainGeneration
{
    public static class TerrainGenerator
    {
        public static CubicArray<float> PopulateMap(BaseSegmentSettings settings, TgTreeBlob tgTreeBlob,
            ArrayOctree<float> maps, float3 segmentPosition, int segmentScale)
        {
            var cubeSize = GetCubeSize(settings.BaseCubeSize, segmentScale);
            var segSize = GetSegSize(settings.BaseSegSize, segmentScale);
            var cubeCount = settings.CubeCount + 1;

            var map = new CubicArray<float>(cubeCount, Allocator.Temp);
            using var traversal = new TgTreeBlob.Traversal(tgTreeBlob);

            for (var a = 0; a <= 1; a++)
            for (var b = 0; b <= 1; b++)
            for (var c = 0; c <= 1; c++)
            {
                var abc = new int3(a, b, c);
                var offset = segmentPosition + (float3)abc * segSize;
                var index = maps.GetIndexAtPos(offset, segmentScale);

                var start = abc * settings.CubeCount;
                var end = math.select(cubeCount, settings.CubeCount, abc == 0);

                if (index != -1)
                {
                    var existingMap = new CubicArray<float>(settings.CubeCount, maps.GetArray(index, Allocator.Temp));

                    for (var x = start.x; x < end.x; x++)
                    for (var y = start.y; y < end.y; y++)
                    for (var z = start.z; z < end.z; z++)
                    {
                        var xyz = new int3(x, y, z);
                        var existingXyz = xyz * (1 - abc);
                        
                        map.SetAt(xyz, existingMap.GetAt(existingXyz));
                    }
                }
                else
                {
                    for (var x = start.x; x < end.x; x++)
                    for (var y = start.y; y < end.y; y++)
                    for (var z = start.z; z < end.z; z++)
                    {
                        var xyz = new int3(x, y, z);
                        var pos = segmentPosition + xyz * (float3)cubeSize;

                        map.SetAt(xyz, traversal.Sample(pos));
                    }
                }
            }

            return map;
        }

        public static CubicArray<float> CreateMap(BaseSegmentSettings settings, TgTreeBlob tgTreeBlob,
            float3 segmentPosition)
        {
            var map = new CubicArray<float>(settings.CubeCount, Allocator.Temp);
            using var traversal = new TgTreeBlob.Traversal(tgTreeBlob);

            for (var x = 0; x < settings.CubeCount; x++)
            for (var y = 0; y < settings.CubeCount; y++)
            for (var z = 0; z < settings.CubeCount; z++)
            {
                var xyz = new int3(x, y, z);
                var pos = segmentPosition + xyz * (float3)settings.BaseCubeSize;

                map.SetAt(xyz, traversal.Sample(pos));
            }

            return map;
        }
    }
}