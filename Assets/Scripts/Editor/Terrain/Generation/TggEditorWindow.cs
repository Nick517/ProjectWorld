using System.IO;
using Editor.Terrain.Generation.Nodes;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Terrain.Generation
{
    public class TggEditorWindow : EditorWindow
    {
        private const string DefaultName = "NewTGGraph";
        private const string Extension = "tgg";

        private TerrainGenGraphView _graph;

        [MenuItem("Assets/Create/Terrain Generation Graph", false, 10)]
        public static void CreateTgGraph()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var fullPath = Path.Combine(path, $"{DefaultName}.{Extension}");

            var tgGraph = new TerrainGenGraphView { path = fullPath };

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
                var windows = Resources.FindObjectsOfTypeAll<TggEditorWindow>();
                var name = Path.GetFileNameWithoutExtension(assetPath);

                foreach (var graph in windows)
                    if (graph._graph.path == assetPath)
                    {
                        graph.Focus();

                        return true;
                    }

                var window = CreateWindow<TggEditorWindow>(name, typeof(SceneView));
                window._graph.path = assetPath;
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
            _graph = new TerrainGenGraphView();

            _graph.StretchToParentSize();
            rootVisualElement.Add(_graph);
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
            var traverseButton = GraphUtil.CreateButton("Traverse", Traverse);

            toolbar.Add(saveButton);
            toolbar.Add(traverseButton);

            rootVisualElement.Add(toolbar);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graph);
        }

        private void Save()
        {
            SaveManager.Save(_graph);
        }

        private void Load()
        {
            SaveManager.Load(_graph);
        }

        private void Traverse()
        {
            var value = GetSampleNode().ToTgtNode().Traverse();

            Debug.Log($"Return of traversal: {value}");
        }

        private SampleNode GetSampleNode()
        {
            foreach (var tgNode in _graph.TggNodes)
                if (tgNode is SampleNode sampleNode)
                    return sampleNode;

            return null;
        }
    }
}