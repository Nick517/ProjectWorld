using UnityEngine;

public static class TerrainGenerator
{
    public static float[,,] PopulateMap(int cubeCount, float cubeSize, Vector3 offset, float noiseScale)
    {
        float[,,] map = new float[cubeCount, cubeCount, cubeCount];

        for (int x = 0; x < cubeCount; x++)
        {
            for (int y = 0; y < cubeCount; y++)
            {
                for (int z = 0; z < cubeCount; z++)
                {
                    Vector3 position = new(x, y, z);
                    position *= cubeSize;

                    map[x, y, z] = GetSample(position + offset, noiseScale);
                }
            }
        }

        return map;
    }

    public static float GetSample(Vector3 position, float noiseScale)
    {
        float x = noiseScale * position.x;
        float y = noiseScale * position.y;
        float z = noiseScale * position.z;

        //Vector3 noisePosition = new(x, y, z);
        //return Noise3D.Perlin(noisePosition);

        return y * Mathf.PerlinNoise(x, z);
    }
}
