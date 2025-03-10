using Newtonsoft.Json;
using TerrainGenerationGraph;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utility.Serialization;

namespace Editor.TerrainGenerationGraph.Graph
{
    public class TggEditorWindow : EditorWindow
    {
        #region Fields

        private TerrainGenGraphView _graphView;

        #endregion

        #region Methods

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var tgGraph = EditorUtility.InstanceIDToObject(instanceID) as TgGraph;

            // Does not do anything if the clicked object is not a TerrainGenTree
            if (tgGraph == null) return false;

            var windows = Resources.FindObjectsOfTypeAll<TggEditorWindow>();
            var name = tgGraph.name;

            // If an editor window with a reference to the TerrainGenTree is already open, focuses on that window
            foreach (var window in windows)
                if (window._graphView.TgGraph == tgGraph)
                {
                    window.Focus();

                    return true;
                }

            // Creates a new editor window with the clicked TerrainGenTree
            var newWindow = CreateWindow<TggEditorWindow>(name, typeof(SceneView));

            // If the opened TerrainGenTree does not have any saved data, add default data
            if (string.IsNullOrEmpty(tgGraph.serializedGraphData)) InitializeTgGraph(ref tgGraph);

            newWindow._graphView.TgGraph = tgGraph;
            newWindow.Load();

            return true;
        }

        private static void InitializeTgGraph(ref TgGraph tgGraph)
        {
            var tgGraphView = new TerrainGenGraphView { TgGraph = tgGraph };

            tgGraphView.SerializeToTgGraph();
        }

        private void OnEnable()
        {
            AddGraphView();
            AddStyles();
            AddToolbar();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        private void AddGraphView()
        {
            _graphView = new TerrainGenGraphView();

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void AddStyles()
        {
            var styleSheet = (StyleSheet)EditorGUIUtility.Load("TerrainGenerationGraph/TgGraphVariables.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new();

            var saveButton = GraphUtil.CreateButton("Save", Save);

            toolbar.Add(saveButton);

            rootVisualElement.Add(toolbar);
        }

        private void Save()
        {
            _graphView.SerializeToTgGraph();
        }

        private void Load()
        {
            var tgGraphJson = _graphView.TgGraph.serializedGraphData;
            var tgGraphViewDto =
                JsonConvert.DeserializeObject<TerrainGenGraphView.Dto>(tgGraphJson, JsonSettings.Formatted);
            tgGraphViewDto.Deserialize(_graphView);
        }

        #endregion
    }
}