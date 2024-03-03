using System;
using System.Collections.Generic;
using Editor.Terrain.Generation.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Editor.Terrain.Generation
{
    public class TgSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private TerrainGenGraphView _graphView;

        public void Initialize(TerrainGenGraphView graphView)
        {
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> entries = new()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
                new SearchTreeGroupEntry(new GUIContent("Input"), 1),
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
                },
            };

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var nodeType = (Type)entry.userData;
            var tgNode = TggNode.Create(_graphView, nodeType);

            tgNode.SetPosition(context.screenMousePosition);

            return true;
        }
    }
}