using UnityEngine;

public static class TerrainGenerator
{
    public static float[,,] PopulateMap(int cubeCount, float cubeSize, Vector3 offset)
    {
        cubeCount++;
        float[,,] map = new float[cubeCount, cubeCount, cubeCount];

        for (int x = 0; x < cubeCount; x++)
        {
            for (int y = 0; y < cubeCount; y++)
            {
                for (int z = 0; z < cubeCount; z++)
                {
                    Vector3 position = new(x, y, z);
                    position *= cubeSize;

                    map[x, y, z] = GetSample(position + offset);
                }
            }
        }

        return map;
    }

    public static float GetSample(Vector3 position)
    {
        Vector3 offset = new(0, 800, 0);
        position += offset;

        float layer1 = NoiseLayer(position, 0.00001f) * 4;
        float layer2 = NoiseLayer(position, 0.0001f) * 16;
        float layer3 = NoiseLayer(position, 0.001f) * 64;

        return layer1 * layer2 * layer3;
    }

    private static float NoiseLayer(Vector3 position, float scale)
    {
        float x = scale * position.x;
        float y = scale * position.y;
        float z = scale * position.z;

        return y * Mathf.PerlinNoise(x, z);
    }
}
