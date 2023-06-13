using UnityEngine;

public static class ChunkOperations
{
    public static Vector3Int GetCurrentChunk(Vector3 position, float chunkSize)
    {
        int chunkX = (int)(position.x / chunkSize);
        int chunkY = (int)(position.y / chunkSize);
        int chunkZ = (int)(position.z / chunkSize);

        Vector3Int chunk = new(chunkX, chunkY, chunkZ);

        return chunk;
    }
}
