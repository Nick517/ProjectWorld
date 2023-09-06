using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Terrain.Graph
{
    public class TerrainSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private TerrainGraphView _graphView;

        public void Initialize(TerrainGraphView graphView)
        {
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> entries = new()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Node")),
            new SearchTreeGroupEntry(new GUIContent("Basic"), 1),
            new SearchTreeEntry(new GUIContent("Node"))
            {
                level = 2
            }
        };

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            _graphView.AddElement(_graphView.CreateNode(_graphView.viewTransform.matrix.inverse.MultiplyPoint(context.screenMousePosition)));

            return true;
        }
    }
}
