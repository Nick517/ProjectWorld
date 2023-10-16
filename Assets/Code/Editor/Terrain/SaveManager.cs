using System.Collections.Generic;
using System.IO;
using Terrain.Graph;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public static class SaveManager
{
    public static void Save(TerrainGraphView graphView, string path)
    {
        List<TerrainNode.TerrainNodeSaveData> saveData = new();

        graphView.nodes.ToList().ForEach(node =>
        {
            TerrainNode terrainNode = (TerrainNode)node;
            saveData.Add(terrainNode.GetSaveData());
        });

        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);

        File.WriteAllText(path, json);
    }
}
