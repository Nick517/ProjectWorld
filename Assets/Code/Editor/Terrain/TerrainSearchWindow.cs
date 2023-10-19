using System;
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
            new SearchTreeGroupEntry(new GUIContent("Input"), 1),
            new SearchTreeEntry(new GUIContent("Position"))
            {
                level = 2,
                userData = typeof(PositionNode)
            },
            new SearchTreeEntry(new GUIContent("Float"))
            {
                level = 2,
                userData = typeof(FloatNode)
            },
            new SearchTreeGroupEntry(new GUIContent("Math"), 1),
            new SearchTreeEntry(new GUIContent("Add"))
            {
                level = 2,
                userData = typeof(AddNode)
            }
        };

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            Type nodeType = (Type)entry.userData;
            TerrainNode node = (TerrainNode)Activator.CreateInstance(nodeType);
            node.Initialize(_graphView, context.screenMousePosition);

            return true;
        }
    }
}
