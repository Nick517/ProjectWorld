using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes;
using TerrainGenerationGraph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static TerrainGenerationGraph.NodeDefinitions;

namespace Editor.TerrainGenerationGraph.Graph
{
    public class TggSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private TggGraphView _graphView;

        public void Init(TggGraphView graphView)
        {
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
                new SearchTreeGroupEntry(new GUIContent("Channel"), 1),
                new(new GUIContent("Split")) { level = 2, userData = Type.Split },
                new SearchTreeGroupEntry(new GUIContent("Input"), 1),
                new SearchTreeGroupEntry(new GUIContent("Basic"), 2),
                new(new GUIContent("Float")) { level = 3, userData = Type.Float },
                new(new GUIContent("Float 2")) { level = 3, userData = Type.Float2 },
                new(new GUIContent("Float 3")) { level = 3, userData = Type.Float3 },
                new(new GUIContent("Float 4")) { level = 3, userData = Type.Float3 },
                new SearchTreeGroupEntry(new GUIContent("Geometry"), 2),
                new(new GUIContent("Position")) { level = 3, userData = Type.Position },
                new SearchTreeGroupEntry(new GUIContent("Math"), 1),
                new SearchTreeGroupEntry(new GUIContent("Advanced"), 2),
                new(new GUIContent("Negate")) { level = 3, userData = Type.Negate },
                new SearchTreeGroupEntry(new GUIContent("Basic"), 2),
                new(new GUIContent("Add")) { level = 3, userData = Type.Add },
                new(new GUIContent("Subtract")) { level = 3, userData = Type.Subtract },
                new(new GUIContent("Multiply")) { level = 3, userData = Type.Multiply },
                new(new GUIContent("Divide")) { level = 3, userData = Type.Divide },
                new SearchTreeGroupEntry(new GUIContent("Procedural"), 1),
                new SearchTreeGroupEntry(new GUIContent("Noise"), 2),
                new(new GUIContent("Perlin 3D")) { level = 3, userData = Type.Perlin3D }
            };
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var node = new TggNode(_graphView,
                new TgGraph.Node
                {
                    type = (Type)entry.userData,
                    position = context.screenMousePosition
                });

            node.Update();

            return true;
        }
    }
}