using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Terrain.Generation
{
    public class TgEditorWindow : EditorWindow
    {
        private const string DefaultName = "NewTGGraph";
        private const string Extension = "tgg";

        private TgGraphView _graphView;

        [MenuItem("Assets/Create/Terrain Generation Graph", false, 10)]
        public static void CreateTgGraph()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var fullPath = Path.Combine(path, $"{DefaultName}.{Extension}");

            var tgGraph = new TgGraphView { path = fullPath };

            var fileData = File.ReadAllBytes("Assets/Scripts/Editor/Terrain/Icons/TGG.png");
            var icon = new Texture2D(2, 2);
            icon.LoadImage(fileData);

            ProjectWindowUtil.CreateAssetWithContent(fullPath, tgGraph.ToJson(), icon);

            AssetDatabase.Refresh();
        }


        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var assetPath = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));

            if (assetPath.EndsWith(Extension))
            {
                var windows = Resources.FindObjectsOfTypeAll<TgEditorWindow>();
                var name = Path.GetFileNameWithoutExtension(assetPath);

                foreach (var graph in windows)
                    if (graph._graphView.path == assetPath)
                    {
                        graph.Focus();

                        return true;
                    }

                var window = CreateWindow<TgEditorWindow>(name, typeof(SceneView));
                window._graphView.path = assetPath;
                window.Load();

                return true;
            }

            return false;
        }

        private void OnEnable()
        {
            AddGraphView();
            AddStyles();
            AddToolbar();
        }

        private void AddGraphView()
        {
            _graphView = new TgGraphView();

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

            var saveButton = GraphUtil.CreateButton("Save", Save);

            toolbar.Add(saveButton);

            rootVisualElement.Add(toolbar);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }

        private void Save()
        {
            SaveManager.Save(_graphView);
        }

        private void Load()
        {
            SaveManager.Load(_graphView);
        }
    }
}