using System;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    public bool smoothMesh = true;
    public bool smoothShaded = true;
    public float cubeSize = 1.0f;
    [Range(0.0f, 1.0f)]
    public float mapSurface = 0.5f;

    public float[,,] cubeMap;

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _triangles = new();

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    public void CreateMeshData()
    {
        if (cubeMap != null)
        {
            ClearMeshData();

            for (int x = 0; x < cubeMap.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < cubeMap.GetLength(1) - 1; y++)
                {
                    for (int z = 0; z < cubeMap.GetLength(2) - 1; z++)
                    {
                        MarchCube(new Vector3Int(x, y, z));
                    }
                }
            }

            BuildMesh();
        }
    }

    private void MarchCube(Vector3Int position)
    {
        float[] cube = new float[8];

        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleMap(position + MarchingCubesTables.Corner[i]);
        }

        int configIndex = GetCubeConfiguration(cube);

        if (configIndex is 0 or 255)
        {
            return;
        }

        int edgeIndex = 0;

        for (int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                int indice = MarchingCubesTables.Triangle[configIndex, edgeIndex];

                if (indice == -1)
                {
                    return;
                }

                Vector3 vert1 = position + MarchingCubesTables.Corner[MarchingCubesTables.Edge[indice, 0]];
                Vector3 vert2 = position + MarchingCubesTables.Corner[MarchingCubesTables.Edge[indice, 1]];

                Vector3 vertPosition;

                if (smoothMesh)
                {
                    float vert1Sample = cube[MarchingCubesTables.Edge[indice, 0]];
                    float vert2Sample = cube[MarchingCubesTables.Edge[indice, 1]];

                    float difference = vert2Sample - vert1Sample;

                    difference = difference == 0 ? mapSurface : (mapSurface - vert1Sample) / difference;

                    vertPosition = vert1 + ((vert2 - vert1) * difference);
                }
                else
                {
                    vertPosition = (vert1 + vert2) / 2f;
                }

                if (smoothShaded)
                {
                    _triangles.Add(VertForIndice(vertPosition * cubeSize));
                }
                else
                {
                    _vertices.Add(vertPosition * cubeSize);
                    _triangles.Add(_vertices.Count - 1);
                }

                edgeIndex++;
            }
        }
    }

    private int GetCubeConfiguration(float[] cube)
    {

        int configurationIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (cube[i] > mapSurface)
            {
                configurationIndex |= 1 << i;
            }
        }

        return configurationIndex;
    }

    private float SampleMap(Vector3Int point)
    {
        return cubeMap[point.x, point.y, point.z];
    }

    private int VertForIndice(Vector3 vert)
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            if (_vertices[i] == vert)
            {
                return i;
            }
        }

        _vertices.Add(vert);

        return _vertices.Count - 1;
    }

    private void ClearMeshData()
    {
        _vertices.Clear();
        _triangles.Clear();
    }

    private void BuildMesh()
    {
        Mesh mesh = new()
        {
            vertices = _vertices.ToArray(),
            triangles = _triangles.ToArray()
        };

        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        Vector3 debugCubeSize = new(cubeSize, cubeSize, cubeSize);
        debugCubeSize *= cubeMap.GetLength(0) - 1;
        Vector3 debugCubePosition = transform.position;
        debugCubePosition += debugCubeSize / 2;

        Gizmos.DrawWireCube(debugCubePosition, debugCubeSize);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 debugCubeSize = new(cubeSize, cubeSize, cubeSize);
        debugCubeSize *= cubeMap.GetLength(0) - 1;
        Vector3 debugCubePosition = transform.position;
        debugCubePosition += debugCubeSize / 2;

        Gizmos.DrawWireCube(debugCubePosition, debugCubeSize);
    }
}
