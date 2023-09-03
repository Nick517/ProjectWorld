using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGraph : EditorWindow
{
    private TerrainGraphView _graphView;

    [MenuItem("Graph/Terrain Graph")]
    public static void OpenTerrainGraphWindow()
    {
        TerrainGraph window = GetWindow<TerrainGraph>();
        window.titleContent = new GUIContent("Terrain Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
    }

    private void ConstructGraphView()
    {
        _graphView = new TerrainGraphView()
        {
            name = "Terrain Graph"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
}
