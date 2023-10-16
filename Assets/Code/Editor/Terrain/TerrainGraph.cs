using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            Button saveButton = TerrainGraphElementUtility.CreateButton("Save As", () => { Save(); });
            Button loadButton = TerrainGraphElementUtility.CreateButton("Load", () => { Load(); });

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
            string path = EditorUtility.SaveFilePanel("Save Graph As...", "C:", "Graph", "terraingraph");

            SaveManager.Save(_graphView, path);
        }

        private void Load()
        {
            string path = EditorUtility.OpenFilePanel("Open Graph", "C:", "terraingraph");

            List<object> loadedObjects = new();

            StreamReader streamReader = new(path);
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                object loadedObject = JsonUtility.FromJson<object>(line);
                loadedObjects.Add(loadedObject);
            }

            foreach (object loadedObject in loadedObjects)
            {
                if (loadedObject is TerrainNode)
                {
                    
                }
            }
        }
    }
}
