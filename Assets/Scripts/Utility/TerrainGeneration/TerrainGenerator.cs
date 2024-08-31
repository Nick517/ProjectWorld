using ECS.Components.TerrainGeneration;
using Unity.Collections;
using Unity.Mathematics;

namespace Utility.TerrainGeneration
{
    public static class TerrainGenerator
    {
        public static NativeArray<float> PopulateMap(TerrainSegmentGenerationSettings settings,
            TerrainGenerationTreeBlob tgTreeBlob,
            float3 segmentPosition, float segmentScale)
        {
            var cubeSize = SegmentOperations.GetCubeSize(settings, segmentScale);
            var cubeCount = settings.CubeCount + 1;
            var offset = segmentPosition;

            var map = new NativeArray<float>((int)math.pow(cubeCount, 3), Allocator.Temp);

            using var traversal = new TerrainGenerationTreeBlob.TerrainGenerationTree.Traversal(tgTreeBlob);

            for (var x = 0; x < cubeCount; x++)
            for (var y = 0; y < cubeCount; y++)
            for (var z = 0; z < cubeCount; z++)
            {
                var index3D = new int3(x, y, z);
                var pos = index3D * cubeSize + offset;
                var index = GetFlatIndex(cubeCount, index3D);

                map[index] = traversal.Sample(pos);
            }

            return map;
        }

        public static float GetCube(NativeArray<float> map, int cubeCount, int3 index3D)
        {
            return map[GetFlatIndex(cubeCount + 1, index3D)];
        }

        private static int GetFlatIndex(int cubeCount, int3 index3D)
        {
            return index3D.x +
                   index3D.y * cubeCount +
                   index3D.z * cubeCount * cubeCount;
        }
    }
}