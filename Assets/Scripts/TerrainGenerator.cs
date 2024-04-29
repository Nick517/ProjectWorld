using ECS.Components;
using Unity.Collections;
using Unity.Mathematics;

public static class TerrainGenerator
{
    public static NativeArray<float> PopulateMap(ChunkGenerationSettings settings, TerrainGenerationTree tgTree,
        float3 chunkPosition, float chunkScale)
    {
        var cubeSize = ChunkOperations.GetCubeSize(settings, chunkScale);
        var cubeCount = settings.CubeCount + 1;
        var offset = chunkPosition;

        var map = new NativeArray<float>((int)math.pow(cubeCount, 3), Allocator.Temp);

        for (var x = 0; x < cubeCount; x++)
        for (var y = 0; y < cubeCount; y++)
        for (var z = 0; z < cubeCount; z++)
        {
            var index3D = new int3(x, y, z);
            var position = index3D * cubeSize + offset;
            var index = GetFlatIndex(cubeCount, index3D);

            map[index] = tgTree.Traverse(position);
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