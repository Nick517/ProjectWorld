using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public MarchingCubes marchingCubeChunkPrefab;

    public Transform trackPoint;

    public int chunkCubeCount = 8;
    public int maxChunkScale = 8;
    public float LOD = 2.0f;
    public int megaChunks = 2;
    public bool trackSceneView = false;

    private Vector3 _lastChunk = new(-1, 0, 0);

    private readonly List<Tuple<Vector3, float, MarchingCubes>> _chunks = new();

    private void Update()
    {
        Vector3 currentChunk = _lastChunk;
        Vector3 trackPointPosition = currentChunk;

        if (trackSceneView)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;

            if (sceneView != null)
            {
                trackPointPosition = sceneView.camera.transform.position;
            }
        }
        else
        {
            trackPointPosition = trackPoint.position;
        }

        currentChunk = GetClosestChunkPosition(trackPointPosition, 1);

        if (currentChunk != _lastChunk)
        {
            SpawnChunks(currentChunk);
            _lastChunk = currentChunk;
        }
    }

    private List<Tuple<Vector3, float, MarchingCubes>> CreateChunkData(Vector3 point)
    {
        List<Tuple<Vector3, float, MarchingCubes>> subChunks = new();

        Vector3 pointPosition = GetClosestChunkPosition(point, maxChunkScale);

        for (int x = -megaChunks; x < megaChunks; x++)
        {
            for (int y = -megaChunks; y < megaChunks; y++)
            {
                for (int z = -megaChunks; z < megaChunks; z++)
                {
                    Vector3 position = new(x, y, z);
                    position *= GetChunkSize(maxChunkScale);
                    position += pointPosition;

                    subChunks.AddRange(CreateSubChunkData(position, maxChunkScale, point));
                }
            }
        }

        return subChunks;
    }

    private List<Tuple<Vector3, float, MarchingCubes>> CreateSubChunkData(Vector3 position, float scale, Vector3 point)
    {
        List<Tuple<Vector3, float, MarchingCubes>> subChunks = new();

        float subChunkScale = scale - 1;
        float subChunkSize = GetChunkSize(subChunkScale);
        Vector3 pointPosition = GetClosestChunkPosition(point, subChunkScale);

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    Vector3 subChunkPosition = new(x, y, z);
                    subChunkPosition *= subChunkSize;
                    subChunkPosition += position;

                    float distance = Vector3.Distance(GetClosestChunkCenter(pointPosition, subChunkScale), GetClosestChunkCenter(subChunkPosition, subChunkScale));

                    if (distance < subChunkSize * LOD && subChunkScale > 0)
                    {
                        subChunks.AddRange(CreateSubChunkData(subChunkPosition, subChunkScale, point));
                    }
                    else
                    {
                        subChunks.Add(new(subChunkPosition, subChunkScale, null));
                    }
                }
            }
        }

        return subChunks;
    }

    private void SpawnChunks(Vector3 origin)
    {
        LoadChunks(CreateChunkData(origin));
    }

    private void LoadChunks(List<Tuple<Vector3, float, MarchingCubes>> newChunks)
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

    private MarchingCubes CreateChunk(Vector3 position, float chunkScale)
    {
        MarchingCubes chunk = Instantiate(marchingCubeChunkPrefab, position, Quaternion.identity, transform);
        chunk.name = $"Chunk {position}, scale: {chunkScale + 1}";
        chunk.cubeSize *= Mathf.Pow(2, chunkScale);
        chunk.cubeMap = TerrainGenerator.PopulateMap(chunkCubeCount, chunk.cubeSize, position);
        chunk.CreateMeshData();

        return chunk;
    }

    private Vector3 GetClosestChunkPosition(Vector3 position, float chunkScale)
    {
        float chunkSize = GetChunkSize(chunkScale);

        float x = (float)Math.Floor(position.x / chunkSize);
        float y = (float)Math.Floor(position.y / chunkSize);
        float z = (float)Math.Floor(position.z / chunkSize);

        Vector3 chunkPosition = new(x, y, z);

        return chunkPosition * chunkSize;
    }

    private Vector3 GetClosestChunkCenter(Vector3 position, float chunkScale)
    {
        Vector3 chunkScaleVector = new(chunkScale, chunkScale, chunkScale);
        position += chunkScaleVector / 2;
        return position;
    }

    private float GetChunkSize(float chunkScale)
    {
        return (float)Math.Pow(2, chunkScale) * chunkCubeCount * marchingCubeChunkPrefab.cubeSize;
    }
}
