using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public World world;

    public Transform trackPoint;

    public int maxLOD = 10;

    private float chunkSize;
    private Vector3 lastChunk = new(-1, 0, 0);

    private void Start()
    {
        chunkSize = world.terrainBuilder.GetChunkSize();
    }

    private void Update()
    {
        Vector3 currentChunk = GetCurrentChunk(trackPoint.position, chunkSize);

        if (currentChunk != lastChunk)
        {
            SpawnChunks(currentChunk);
            lastChunk = currentChunk;
        }
    }

    private void SpawnChunks(Vector3 origin)
    {
        List<MarchingCubes> chunks = new();

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
                            position += chunkSize * origin;

                            chunks.Add(CreateChunk(position, LOD));
                        }
                    }
                }
            }

            LOD *= 2;
        }

        world.terrainBuilder.LoadChunks(chunks);
    }

    private MarchingCubes CreateChunk(Vector3 position, float cubeSize)
    {
        MarchingCubes chunk = Instantiate(world.terrainBuilder.marchingCubeChunkPrefab, position, Quaternion.identity);
        chunk.name = $"Chunk {position}, size: {cubeSize}";
        chunk.cubeSize *= cubeSize;

        return chunk;
    }

    public Vector3Int GetCurrentChunk(Vector3 position, float chunkSize)
    {
        int chunkX = (int)(position.x / chunkSize);
        int chunkY = (int)(position.y / chunkSize);
        int chunkZ = (int)(position.z / chunkSize);

        Vector3Int chunk = new(chunkX, chunkY, chunkZ);

        return chunk;
    }
}
