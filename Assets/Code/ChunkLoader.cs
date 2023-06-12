using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public World world;

    public void CreateChunk(float cubeSize)
    {
        Vector3Int currentChunk = GetCurrentChunk();
        world.terrainBuilder.CreateChunk(currentChunk, cubeSize);
    }

    public Vector3Int GetCurrentChunk()
    {
        float chunkSize = world.terrainBuilder.GetChunkSize();

        int chunkX = (int)(transform.position.x / chunkSize);
        int chunkY = (int)(transform.position.y / chunkSize);
        int chunkZ = (int)(transform.position.z / chunkSize);

        Vector3Int chunk = new(chunkX, chunkY, chunkZ);

        return chunk;
    }
}
