using System.Collections.Generic;
using UnityEngine;

public class TerrainBuilder : MonoBehaviour
{
    public MarchingCubes marchingCubeChunkPrefab;

    public int cubeCount = 10;
    public float noiseScale = 0.15f;
    [Range(0, 1)]
    public float mapSurface = 0.5f;
    private readonly List<MarchingCubes> chunks = new();

    public void CreateChunk(Vector3Int chunkPosition, float cubeSize)
    {
        MarchingCubes chunk = Instantiate(marchingCubeChunkPrefab, marchingCubeChunkPrefab.cubeSize * cubeCount * (Vector3)chunkPosition, Quaternion.identity);
        chunk.name = $"Chunk {chunkPosition}, size: {cubeSize}";
        chunk.cubeSize = cubeSize;
        chunk.mapSurface = mapSurface;
        chunk.chunkPosition = chunkPosition;
        chunks.Add(chunk);

        UpdateChunk(chunk, cubeSize);
    }

    public void UpdateChunk(MarchingCubes chunk, float cubeSize)
    {
        CubeMap cubeMap = new(cubeCount);
        cubeMap.map = TerrainGenerator.PopulateMap(cubeMap.map.GetLength(0), cubeSize, GetChunkSize() * chunk.chunkPosition, noiseScale);
        chunk.cubeMap = cubeMap;
        chunk.CreateMeshData();
    }

    public void DeleteAllChunks()
    {
        MarchingCubes[] chunkArray = chunks.ToArray();

        foreach (MarchingCubes chunk in chunkArray)
        {
            Destroy(chunk.gameObject);
        }

        chunks.Clear();
    }

    public float GetChunkSize()
    {
        return cubeCount * marchingCubeChunkPrefab.cubeSize;
    }
}
