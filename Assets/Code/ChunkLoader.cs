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

    private Vector3 _lastChunk = new(-1, 0, 0);

    public readonly List<Tuple<Vector3, float, MarchingCubes>> _chunks = new();
    private MegaChunk megaChunk;

    private void Update()
    {
        Vector3 currentChunk = _lastChunk;

        if (trackSceneView)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;

            if (sceneView != null)
            {
                currentChunk = GetClosestChunkPosition(sceneView.camera.transform.position, 1);
            }
        }
        else
        {
            currentChunk = GetClosestChunkPosition(trackPoint.position, 1);
        }

        if (currentChunk != _lastChunk)
        {
            SpawnChunks(currentChunk);
            _lastChunk = currentChunk;
        }
    }

    public void SpawnChunks(Vector3 origin)
    {
        megaChunk = new(this, GetClosestChunkPosition(origin, maxChunkScale), maxChunkScale);
        megaChunk.CreateSubChunks(origin);
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

    public MarchingCubes CreateChunk(Vector3 position, float chunkScale)
    {
        MarchingCubes chunk = Instantiate(marchingCubeChunkPrefab, position, Quaternion.identity, transform);
        chunk.name = $"Chunk {position}, scale: {chunkScale + 1}";
        chunk.cubeSize *= Mathf.Pow(2, chunkScale);
        chunk.cubeMap = TerrainGenerator.PopulateMap(chunkCubeCount, chunk.cubeSize, position);
        chunk.CreateMeshData();

        return chunk;
    }

    public Vector3 GetClosestChunkPosition(Vector3 position, float chunkScale)
    {
        float chunkSize = GetChunkSize(chunkScale);

        float x = (float)Math.Floor(position.x / chunkSize);
        float y = (float)Math.Floor(position.y / chunkSize);
        float z = (float)Math.Floor(position.z / chunkSize);

        Vector3 chunkPosition = new(x, y, z);

        return chunkPosition * chunkSize;
    }

    public float GetChunkSize(float chunkScale)
    {
        return (float)Math.Pow(2, chunkScale) * chunkCubeCount * marchingCubeChunkPrefab.cubeSize;
    }
}
