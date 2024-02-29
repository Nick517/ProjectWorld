using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Terrain.Generation
{
    public class TgEditorWindow : EditorWindow
    {
        private TgGraphView _graphView;

        [MenuItem("Window/Terrain Generation")]
        public static void Open()
        {
            var window = GetWindow<TgEditorWindow>();
            window.titleContent = new GUIContent("Terrain Generation");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddStyles();
            AddToolbar();
        }

        private void AddGraphView()
        {
            _graphView = new TgGraphView
            {
                name = "Terrain Generation"
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void AddStyles()
        {
            var styleSheet = (StyleSheet)EditorGUIUtility.Load("Terrain Generation/TGVariables.uss");
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new();

            var saveButton = GraphUtil.CreateButton("Save As", Save);
            var loadButton = GraphUtil.CreateButton("Load", Load);

            toolbar.Add(saveButton);
            toolbar.Add(loadButton);

            rootVisualElement.Add(toolbar);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        private void Save()
        {
            var path = EditorUtility.SaveFilePanel("Save Graph As...", "C:", "Graph", "tg");

            SaveManager.Save(_graphView, path);
        }

        private void Load()
        {
            var path = EditorUtility.OpenFilePanel("Open Graph", "C:", "tg");

            SaveManager.Load(_graphView, path);
        }
    }
}