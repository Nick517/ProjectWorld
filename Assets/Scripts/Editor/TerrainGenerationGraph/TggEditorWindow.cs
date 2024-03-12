using Newtonsoft.Json;
using TerrainGenerationGraph.Scripts;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph
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

            if (tgGraph == null) return false;

            var windows = Resources.FindObjectsOfTypeAll<TggEditorWindow>();
            var name = tgGraph.name;

            foreach (var window in windows)
                if (window._graphView.tgGraph == tgGraph)
                {
                    window.Focus();

                    return true;
                }

            var newWindow = CreateWindow<TggEditorWindow>(name, typeof(SceneView));

            if (string.IsNullOrEmpty(tgGraph.serializedGraphData)) InitializeTgGraph(ref tgGraph);

            newWindow._graphView.tgGraph = tgGraph;

            newWindow.Load();

            return true;
        }

        private static void InitializeTgGraph(ref TgGraph tgGraph)
        {
            var tgGraphView = new TerrainGenGraphView { tgGraph = tgGraph };

            tgGraph.serializedTreeData = tgGraphView.SerializeTree();
            tgGraph.serializedGraphData = tgGraphView.SerializeGraph();
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
            var styleSheet = (StyleSheet)EditorGUIUtility.Load("TGGraph/TgGraphVariables.uss");
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
            var tgGraphJson = _graphView.tgGraph.serializedGraphData;
            var tgGraphViewDto =
                JsonConvert.DeserializeObject<TerrainGenGraphView.Dto>(tgGraphJson, JsonSettings.Formatted);
            tgGraphViewDto.Deserialize(_graphView);
        }

        #endregion
    }
}