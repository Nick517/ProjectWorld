using System;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public World world;

    public Vector3Int origin = new(0, 0, 0);
    public int maxLOD = 10;

    private float chunkSize;

    private void Start()
    {
        chunkSize = world.terrainBuilder.GetChunkSize();

        SpawnChunks(origin);
    }

    private void SpawnChunks(Vector3Int origin)
    {
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
                            position += origin;

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
