using UnityEngine;

public static class Noise3D
{
    public static float Perlin(Vector3 position)
    {
        float x = position.x;
        float y = position.y;
        float z = position.z;

        float xy = Mathf.PerlinNoise(x, y);
        float xz = Mathf.PerlinNoise(x, z);
        float yz = Mathf.PerlinNoise(y, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zx = Mathf.PerlinNoise(z, x);
        float zy = Mathf.PerlinNoise(z, y);

        return (xy + xz + yz + yx + zx + zy) / 6.0f;
    }
}
