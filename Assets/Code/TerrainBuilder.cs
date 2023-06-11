using System.Collections.Generic;
using UnityEngine;

public class TerrainBuilder : MonoBehaviour
{
    public MarchingCubes marchingCubeChunkPrefab;

    public int cubeCount = 10;
    public float noiseScale = 0.15f;
    [Range(0, 1)]
    public float mapSurface = 0.5f;

    public Dictionary<Vector3Int, MarchingCubes> chunks = new();

    public void CreateChunk(Vector3Int chunkPosition)
    {
        if (!chunks.ContainsKey(chunkPosition))
        {
            MarchingCubes chunk = Instantiate(marchingCubeChunkPrefab, cubeCount * marchingCubeChunkPrefab.cubeSize * (Vector3)chunkPosition, Quaternion.identity);
            chunk.name = $"Chunk {chunkPosition}";
            chunk.mapSurface = mapSurface;
            chunk.chunkPosition = chunkPosition;
            chunks.Add(chunkPosition, chunk);

            UpdateChunk(chunk);
        }
    }

    public void UpdateChunk(MarchingCubes chunk)
    {
        CubeMap cubeMap = new(cubeCount);
        cubeMap.map = TerrainGenerator.PopulateMap(cubeMap.map.GetLength(0), cubeCount * chunk.chunkPosition, noiseScale);
        chunk.cubeMap = cubeMap;
        chunk.CreateMeshData();
    }

    public float GetChunkSize()
    {
        return cubeCount * marchingCubeChunkPrefab.cubeSize;
    }
}
