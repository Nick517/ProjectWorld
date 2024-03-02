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

        private TgGraph _graph;

        [MenuItem("Assets/Create/Terrain Generation Graph", false, 10)]
        public static void CreateTgGraph()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string fullPath = Path.Combine(path, $"{DefaultName}.{Extension}");

            var tgGraph = new TgGraph { path = fullPath };
            
            SaveManager.Save(tgGraph);
            AssetDatabase.Refresh();
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var assetPath = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));

            if (assetPath.EndsWith(Extension))
            {
                var name = Path.GetFileNameWithoutExtension(assetPath);

                var window = GetWindow<TgEditorWindow>(name, typeof(SceneView));

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
            _graph = new TgGraph
            {
                name = "Terrain Generation"
            };

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

            toolbar.Add(saveButton);

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
    }
}