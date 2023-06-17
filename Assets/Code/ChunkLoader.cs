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
    private Vector3 _lastChunk = new(-1, 0, 0);

    private readonly List<Tuple<Vector3, float, MarchingCubes>> _chunks = new();

    private void Update()
    {
        Vector3 currentChunk = _lastChunk;

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

        if (currentChunk != _lastChunk)
        {
            SpawnChunks(currentChunk);
            _lastChunk = currentChunk;
        }
    }

    private void SpawnChunks(Vector3 origin)
    {
        List<Tuple<Vector3, float, MarchingCubes>> newChunks = new();

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

                            newChunks.Add(new(position, chunkScale, null));
                        }
                    }
                }
            }

            chunkScale *= 2;
        }

        LoadChunks(newChunks);
    }

    public void LoadChunks(List<Tuple<Vector3, float, MarchingCubes>> newChunks)
    {
        ChunkDataEqualityComparer comparer = new();

        List<Tuple<Vector3, float, MarchingCubes>> chunksToDestroy = _chunks.Except(newChunks, comparer).ToList();
        chunksToDestroy.ForEach(chunk => { if (chunk.Item3 != null) { Destroy(chunk.Item3.gameObject); } });
        _ = _chunks.RemoveAll(chunk => chunksToDestroy.Contains(chunk, comparer));

        List<Tuple<Vector3, float, MarchingCubes>> chunksToCreate = newChunks.Except(_chunks, comparer).ToList();
        List<Tuple<Vector3, float, MarchingCubes>> createdChunks = new();
        chunksToCreate.ForEach(chunk => { createdChunks.Add(new(chunk.Item1, chunk.Item2, CreateChunk(chunk.Item1, chunk.Item2))); });
        _chunks.AddRange(createdChunks);
    }

    private class ChunkDataEqualityComparer : IEqualityComparer<Tuple<Vector3, float, MarchingCubes>>
    {
        public bool Equals(Tuple<Vector3, float, MarchingCubes> chunk1, Tuple<Vector3, float, MarchingCubes> chunk2)
        {
            return chunk1.Item1.Equals(chunk2.Item1) && chunk1.Item2.Equals(chunk2.Item2);
        }

        public int GetHashCode(Tuple<Vector3, float, MarchingCubes> chunk)
        {
            return chunk.Item1.GetHashCode() ^ chunk.Item2.GetHashCode();
        }
    }

    private MarchingCubes CreateChunk(Vector3 position, float cubeSize)
    {
        MarchingCubes chunk = Instantiate(marchingCubeChunkPrefab, position, Quaternion.identity, transform);
        chunk.name = $"Chunk {position}, size: {cubeSize}";
        chunk.cubeSize *= cubeSize;
        chunk.cubeMap = TerrainGenerator.PopulateMap(chunkCubeCount, cubeSize, position);
        chunk.CreateMeshData();

        return chunk;
    }

    public Vector3 GetCurrentChunk(Vector3 position)
    {
        int chunkX = (int)(position.x / ChunkSize);
        int chunkY = (int)(position.y / ChunkSize);
        int chunkZ = (int)(position.z / ChunkSize);

        return new(chunkX, chunkY, chunkZ);
    }
}
