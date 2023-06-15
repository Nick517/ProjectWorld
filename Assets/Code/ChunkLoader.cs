using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public MarchingCubes marchingCubeChunkPrefab;

    public Transform trackPoint;

    public int chunkCubeCount = 10;
    public int maxChunkScale = 10;
    public bool trackSceneView = false;

    private float ChunkSize => chunkCubeCount * marchingCubeChunkPrefab.cubeSize;
    private Vector3 lastChunk = new(-1, 0, 0);

    private readonly List<MarchingCubes> chunks = new();

    private void Update()
    {
        Vector3 currentChunk = lastChunk;

        if (trackSceneView)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;

            if (sceneView != null)
            {
                currentChunk = GetCurrentChunk(sceneView.camera.transform.position);
            }
        }
        else
        {
            currentChunk = GetCurrentChunk(trackPoint.position);
        }

        if (currentChunk != lastChunk)
        {
            SpawnChunks(currentChunk);
            lastChunk = currentChunk;
        }
    }

    private void SpawnChunks(Vector3 origin)
    {
        List<MarchingCubes> chunks = new();

        int chunkScale = 1;

        while (chunkScale < Math.Pow(2, maxChunkScale))
        {
            for (int x = -2; x < 2; x++)
            {
                for (int y = -2; y < 2; y++)
                {
                    for (int z = -2; z < 2; z++)
                    {
                        if (chunkScale == 1 || !(x >= -1 && x <= 0) || !(y >= -1 && y <= 0) || !(z >= -1 && z <= 0))
                        {
                            Vector3 position = new(x, y, z);
                            position *= ChunkSize * chunkScale;
                            position += ChunkSize * origin;

                            chunks.Add(CreateChunk(position, chunkScale));
                        }
                    }
                }
            }

            chunkScale *= 2;
        }

        LoadChunks(chunks);
    }

    private MarchingCubes CreateChunk(Vector3 position, float cubeSize)
    {
        MarchingCubes chunk = Instantiate(marchingCubeChunkPrefab, position, Quaternion.identity, transform);
        chunk.name = $"Chunk {position}, size: {cubeSize}";
        chunk.cubeSize *= cubeSize;

        return chunk;
    }

    public void LoadChunks(List<MarchingCubes> newChunks)
    {
        List<MarchingCubes> chunksToDestroy = chunks.Except(newChunks).ToList();
        chunksToDestroy.ForEach(chunk => chunk.DestroyChunk());
        _ = chunks.RemoveAll(chunk => chunksToDestroy.Contains(chunk));

        List<MarchingCubes> chunksToCreate = newChunks.Except(chunks).ToList();
        chunks.AddRange(chunksToCreate);
        chunksToCreate.ForEach(chunk => UpdateChunk(chunk));
    }

    private void UpdateChunk(MarchingCubes chunk)
    {
        chunk.cubeMap = TerrainGenerator.PopulateMap(chunkCubeCount, chunk.cubeSize, chunk.transform.position);
        chunk.CreateMeshData();
    }

    public Vector3 GetCurrentChunk(Vector3 position)
    {
        int chunkX = (int)(position.x / ChunkSize);
        int chunkY = (int)(position.y / ChunkSize);
        int chunkZ = (int)(position.z / ChunkSize);

        return new(chunkX, chunkY, chunkZ);
    }
}
