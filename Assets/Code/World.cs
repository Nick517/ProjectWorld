using UnityEngine;

public class World : MonoBehaviour
{
    public TerrainBuilder terrainBuilder;

    private void Start()
    {
        terrainBuilder.CreateChunk();
    }

    private void Update()
    {
        terrainBuilder.UpdateChunk();
    }
}
