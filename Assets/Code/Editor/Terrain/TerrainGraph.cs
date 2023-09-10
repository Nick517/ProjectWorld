using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Terrain.Graph
{
    public class TerrainGraph : EditorWindow
    {
        private const string defaultFileName = "TerrainGraph";
        private TerrainGraphView _graphView;

        [MenuItem("Window/Terrain Graph")]
        public static void Open()
        {
            TerrainGraph window = GetWindow<TerrainGraph>();
            window.titleContent = new GUIContent("Terrain Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();
        }

        private void AddGraphView()
        {
            _graphView = new TerrainGraphView()
            {
                name = "Terrain Graph"
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new();


            Button saveButton = TerrainGraphElementUtility.CreateButton("Save As");

            toolbar.Add(saveButton);

            rootVisualElement.Add(toolbar);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }
}
