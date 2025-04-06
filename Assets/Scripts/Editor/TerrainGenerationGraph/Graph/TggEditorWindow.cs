using TerrainGenerationGraph;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph.Graph
{
    public class TggEditorWindow : EditorWindow
    {
        private TggGraphView _graphView;

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var tgGraph = EditorUtility.InstanceIDToObject(instanceID) as TgGraph;

            if (!tgGraph) return false;

            foreach (var window in Resources.FindObjectsOfTypeAll<TggEditorWindow>())
                if (window._graphView.Graph == tgGraph)
                {
                    window.Focus();
                    return true;
                }

            var newWindow = CreateWindow<TggEditorWindow>(tgGraph.name, typeof(SceneView));
            newWindow._graphView.Graph = tgGraph;
            newWindow._graphView.Load();

            return true;
        }

        private void OnEnable()
        {
            rootVisualElement.styleSheets.Add(
                EditorGUIUtility.Load("TerrainGenerationGraph/TgGraphVariables.uss") as StyleSheet);

            _graphView = new TggGraphView();
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);

            var toolbar = new Toolbar();
            toolbar.Add(new Button(_graphView.Save) { text = "Save" });
            rootVisualElement.Add(toolbar);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }
}