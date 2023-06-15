using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    public bool smoothMesh = true;
    public bool smoothShaded = true;
    public float cubeSize = 1.0f;
    [Range(0.0f, 1.0f)]
    public float mapSurface = 0.5f;
    public float fadeDuration = 2.0f;
    public float fadeAtChunkScale = 4.0f;

    public float[,,] cubeMap;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();

    private bool fadingIn = false;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
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

        if (cubeSize >= Math.Pow(2, fadeAtChunkScale))
        {
            _ = StartCoroutine(FadeIn());
        }
    }

    public override bool Equals(object obj)
    {

        if (obj is null or not MarchingCubes)
        {
            return false;
        }

        MarchingCubes chunk = (MarchingCubes)obj;
        return transform.position == chunk.transform.position && cubeSize == chunk.cubeSize;
    }

    public override int GetHashCode()
    {
        return transform.position.GetHashCode() ^ cubeSize.GetHashCode();
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
                    triangles.Add(VertForIndice(vertPosition * cubeSize));
                }
                else
                {
                    vertices.Add(vertPosition * cubeSize);
                    triangles.Add(vertices.Count - 1);
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
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i] == vert)
            {
                return i;
            }
        }

        vertices.Add(vert);

        return vertices.Count - 1;
    }

    private void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
    }

    private void BuildMesh()
    {
        Mesh mesh = new()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void DestroyChunk()
    {
        if (cubeSize >= Math.Pow(2, fadeAtChunkScale))
        {
            _ = StartCoroutine(FadeOut());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator FadeIn()
    {
        fadingIn = true;

        Material material = GetComponent<Renderer>().material;

        float startTime = Time.time;
        float startAlpha = 0;
        float targetAlpha = 1;

        while (fadingIn && Time.time - startTime < fadeDuration)
        {
            float normalizedTime = (Time.time - startTime) / fadeDuration;

            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);

            material.SetFloat("_Alpha", newAlpha);

            yield return null;
        }

        material.SetFloat("_Alpha", targetAlpha);

        fadingIn = false;
        yield return null;
    }

    private IEnumerator FadeOut()
    {
        fadingIn = false;

        Material material = GetComponent<Renderer>().material;

        float startTime = Time.time;
        float startAlpha = material.GetFloat("_Alpha");
        float targetAlpha = 0;

        while (Time.time - startTime < fadeDuration)
        {
            float normalizedTime = (Time.time - startTime) / fadeDuration;

            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);

            material.SetFloat("_Alpha", newAlpha);

            yield return null;
        }

        material.SetFloat("_Alpha", targetAlpha);

        Destroy(gameObject);
        yield return null;
    }
}
