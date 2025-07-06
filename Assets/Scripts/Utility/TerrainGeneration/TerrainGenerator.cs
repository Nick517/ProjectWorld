using DataTypes;
using ECS.Components.TerrainGeneration;
using Unity.Collections;
using Unity.Mathematics;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace Utility.TerrainGeneration
{
    public static class TerrainGenerator
    {
        public static CubicArray<float> PopulateMap(BaseSegmentSettings settings, TgTreeBlob tgTreeBlob,
            float3 segmentPosition, int segmentScale)
        {
            var cubeSize = GetCubeSize(settings.BaseCubeSize, segmentScale);
            var cubeCount = settings.CubeCount + 1;

            var map = new CubicArray<float>(cubeCount, Allocator.Temp);

            using var traversal = new TgTreeBlob.Traversal(tgTreeBlob);

            for (var x = 0; x < cubeCount; x++)
            for (var y = 0; y < cubeCount; y++)
            for (var z = 0; z < cubeCount; z++)
            {
                var index3D = new int3(x, y, z);
                var pos = index3D * (float3)cubeSize + segmentPosition;

                map.SetAt(index3D, traversal.Sample(pos));
            }

            return map;
        }
    }
}