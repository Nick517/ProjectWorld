using UnityEngine;

public static class TerrainGenerator
{
    public static float[,,] PopulateMap(int size, Vector3 offset, float noiseScale)
    {
        float[,,] map = new float[size, size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Vector3 position = new(x, y, z);
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

        Vector3 noisePosition = new(x, y, z);

        return Noise3D.Perlin(noisePosition);
    }
}
