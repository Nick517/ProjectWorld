using System;
using UnityEngine;

public class ChunkLoaderMover : MonoBehaviour
{
    public int maxLOD = 10;

    private ChunkLoader chunkLoader;

    private float chunkSize;

    private void Start()
    {
        chunkLoader = GetComponent<ChunkLoader>();

        chunkSize = chunkLoader.world.terrainBuilder.GetChunkSize();

        SpawnChunks(chunkSize);
    }

    private void SpawnChunks(float chunkSize)
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

                            transform.position = chunkSize * LOD * position;

                            chunkLoader.CreateChunk(LOD);
                        }
                    }
                }
            }

            LOD *= 2;
        }
    }
}
