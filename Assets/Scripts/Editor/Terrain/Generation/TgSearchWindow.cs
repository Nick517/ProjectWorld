using System;
using System.Collections.Generic;
using Editor.Terrain.Generation.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Editor.Terrain.Generation
{
    public class TgSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private TgGraphView _graphView;

        public void Initialize(TgGraphView graphView)
        {
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> entries = new()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node")),
                new SearchTreeEntry(new GUIContent("Test"))
                {
                    level = 1,
                    userData = typeof(TestNode)
                },
                new SearchTreeGroupEntry(new GUIContent("Input"), 1),
                new SearchTreeEntry(new GUIContent("Position"))
                {
                    level = 2,
                    userData = typeof(SampleNode)
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
                    userData = typeof(SampleNode)
                },
                new SearchTreeEntry(new GUIContent("Multiply"))
                {
                    level = 2,
                    userData = typeof(SampleNode)
                }
            };

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var nodeType = (Type)entry.userData;
            var tgNode = TgNode.Create(_graphView, nodeType);
            
            tgNode.SetPosition(context.screenMousePosition);

            return true;
        }
    }
}