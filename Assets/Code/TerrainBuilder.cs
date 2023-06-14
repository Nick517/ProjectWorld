using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainBuilder : MonoBehaviour
{
    public MarchingCubes marchingCubeChunkPrefab;

    public int cubeCount = 10;
    public float noiseScale = 0.15f;
    [Range(0, 1)]
    public float mapSurface = 0.5f;
    private readonly List<MarchingCubes> chunks = new();

    public void LoadChunks(List<MarchingCubes> newChunks)
    {
        List<MarchingCubes> chunksToDestroy = chunks.Except(newChunks).ToList();
        chunksToDestroy.ForEach(thing => Destroy(thing.gameObject));
        _ = chunks.RemoveAll(chunk => chunksToDestroy.Contains(chunk));

        List<MarchingCubes> chunksToCreate = newChunks.Except(chunks).ToList();
        chunks.AddRange(chunksToCreate);
        chunksToCreate.ForEach(chunk => UpdateChunk(chunk));
    }

    private void UpdateChunk(MarchingCubes chunk)
    {
        CubeMap cubeMap = new(cubeCount);
        cubeMap.map = TerrainGenerator.PopulateMap(cubeMap.map.GetLength(0), chunk.cubeSize, chunk.transform.position, noiseScale);
        chunk.cubeMap = cubeMap;
        chunk.CreateMeshData();
    }

    public float GetChunkSize()
    {
        return cubeCount * marchingCubeChunkPrefab.cubeSize;
    }
}
