using System.Collections.Generic;
using System.IO;
using Terrain.Graph;
using Unity.Plastic.Newtonsoft.Json;

public static class SaveManager
{
    public static void Save(TerrainGraphView graphView, string path)
    {
        List<TerrainNode.SaveData> saveData = new();

        graphView.nodes.ToList().ForEach(node =>
        {
            TerrainNode terrainNode = (TerrainNode)node;
            saveData.Add(terrainNode.GetSaveData());
        });

        string json = JsonConvert.SerializeObject(saveData, SerializerSettings);

        File.WriteAllText(path, json);
    }

    public static void Load(TerrainGraphView graphView, string path)
    {
        graphView.ClearGraphView();

        string json = File.ReadAllText(path);

        List<TerrainNode.SaveData> saveDataList = JsonConvert.DeserializeObject<List<TerrainNode.SaveData>>(json, SerializerSettings);

        saveDataList.ForEach(saveData =>
        {
            saveData.Load(graphView);
        });
    }

    public static JsonSerializerSettings SerializerSettings => new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented
    };
}
