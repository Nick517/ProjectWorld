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
            var scaleMultiplier = (int)ScaleMultiplier(segmentScale);
            var cubeSize = GetCubeSize(settings.BaseCubeSize, segmentScale);
            
            var map = new CubicArray<float>(settings.CubeCount + 1, Allocator.Temp);
            using var traversal = new TgTreeBlob.Traversal(tgTreeBlob);
            
            if (scaleMultiplier > settings.CubeCount)
            {
                for (var x = 0; x < map.SideLength; x++)
                for (var y = 0; y < map.SideLength; y++)
                for (var z = 0; z < map.SideLength; z++)
                {
                    var xyz = new int3(x, y, z);
                    var pos = segmentPosition + xyz * (float3)cubeSize;

                    map.SetAt(xyz, traversal.Sample(pos));
                }

                return map;
            }
            
            var cubeIncrement = settings.CubeCount / scaleMultiplier;

            for (var a = 0; a <= scaleMultiplier; a++)
            for (var b = 0; b <= scaleMultiplier; b++)
            for (var c = 0; c <= scaleMultiplier; c++)
            {
                var abc = new int3(a, b, c);

                var start = abc * cubeIncrement;
                var end = math.min(start + cubeIncrement, map.SideLength);

                var secPos = segmentPosition + (float3)abc * settings.BaseSegSize;
                var index = maps.GetIndexAtPos(secPos);

                if (index != -1)
                {
                    using var existingMap =
                        new CubicArray<float>(settings.CubeCount, maps.GetArray(index, Allocator.Temp));

                    for (var x = start.x; x < end.x; x++)
                    for (var y = start.y; y < end.y; y++)
                    for (var z = start.z; z < end.z; z++)
                    {
                        var xyz = new int3(x, y, z);
                        var existingXyz = (xyz - start) * scaleMultiplier;

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
            float3 segmentPosition, int segmentScale = 0)
        {
            var segSize = GetCubeSize(settings.BaseCubeSize, segmentScale);
            
            var map = new CubicArray<float>(settings.CubeCount, Allocator.Temp);
            using var traversal = new TgTreeBlob.Traversal(tgTreeBlob);

            for (var x = 0; x < settings.CubeCount; x++)
            for (var y = 0; y < settings.CubeCount; y++)
            for (var z = 0; z < settings.CubeCount; z++)
            {
                var xyz = new int3(x, y, z);
                var pos = segmentPosition + xyz * (float3)segSize;

                map.SetAt(xyz, math.clamp(traversal.Sample(pos), 0, 1));
            }

            return map;
        }
    }
}