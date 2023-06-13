using System;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public World world;

    public Transform trackPoint;

    public int maxLOD = 10;

    private float chunkSize;
    private Vector3 lastChunk;

    private void Start()
    {
        chunkSize = world.terrainBuilder.GetChunkSize();
    }

    private void Update()
    {
        Vector3Int currentChunk = ChunkOperations.GetCurrentChunk(trackPoint.position, chunkSize);

        if (currentChunk != lastChunk)
        {
            SpawnChunks(currentChunk);
            lastChunk = currentChunk;
        }
    }

    private void SpawnChunks(Vector3Int origin)
    {
        world.terrainBuilder.DeleteAllChunks();

        int LOD = 1;

        while (LOD < Math.Pow(2, maxLOD))
        {
            for (int x = -2; x < 2; x++)
            {
                for (int y = -2; y < 2; y++)
                {
                    for (int z = -2; z < 2; z++)
                    {
                        if (LOD == 1 || !(x >= -1 && x <= 0) || !(y >= -1 && y <= 0) || !(z >= -1 && z <= 0))
                        {
                            Vector3 position = new(x, y, z);
                            position *= chunkSize * LOD;
                            position += new Vector3(origin.x * chunkSize, origin.y * chunkSize, origin.z * chunkSize);

                            transform.position = position;

                            CreateChunk(LOD);
                        }
                    }
                }
            }

            LOD *= 2;
        }
    }

    private void CreateChunk(float cubeSize)
    {
        Vector3Int currentChunk = ChunkOperations.GetCurrentChunk(transform.position, chunkSize);
        world.terrainBuilder.CreateChunk(currentChunk, cubeSize);
    }
}
