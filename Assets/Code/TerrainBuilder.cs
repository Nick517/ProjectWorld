using UnityEngine;

public class TerrainBuilder : MonoBehaviour
{
    public MarchingCubes marchingCubeChunkPrefab;

    public int chunkSize = 16;
    public float noiseScale = 0.15f;
    public Vector3Int offset = new(0, 0, 0);

    private CubeMap cubeMap;
    private MarchingCubes chunk;

    public void CreateChunk()
    {
        cubeMap = new(chunkSize);

        chunk = Instantiate(marchingCubeChunkPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        UpdateChunk();
    }

    public void UpdateChunk()
    {
        cubeMap.map = TerrainGenerator.PopulateMap(cubeMap.map.GetLength(0), offset, noiseScale);
        chunk.cubeMap = cubeMap;
        chunk.CreateMeshData();
    }
}
