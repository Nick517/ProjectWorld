using UnityEngine;

public class ChunkLoaderMover : MonoBehaviour
{
    public int cubicChunks = 10;

    private ChunkLoader chunkLoader;

    private void Start()
    {
        chunkLoader = GetComponent<ChunkLoader>();

        float chunkSize = chunkLoader.world.terrainBuilder.GetChunkSize();

        MoveChunkLoader(chunkSize);
    }

    private void MoveChunkLoader(float chunkSize)
    {
        for (int x = 0; x < cubicChunks; x++)
        {
            for (int y = 0; y < cubicChunks; y++)
            {
                for (int z = 0; z < cubicChunks; z++)
                {
                    Vector3 position = new(x, y, z);

                    transform.position = position * chunkSize;

                    chunkLoader.CreateChunk();
                }
            }
        }
    }
}
