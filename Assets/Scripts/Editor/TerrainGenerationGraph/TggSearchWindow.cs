using System;
using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Editor.TerrainGenerationGraph
{
    public class TggSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        #region Fields

        private TerrainGenGraphView _graphView;

        #endregion

        #region Methods

        public void Initialize(TerrainGenGraphView graphView)
        {
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> entries = new()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node")),

                new SearchTreeGroupEntry(new GUIContent("Channel"), 1),

                new SearchTreeEntry(new GUIContent("Split"))
                {
                    level = 2,
                    userData = typeof(SplitNode)
                },

                new SearchTreeGroupEntry(new GUIContent("Input"), 1),

                new SearchTreeGroupEntry(new GUIContent("Basic"), 2),

                new SearchTreeEntry(new GUIContent("Float"))
                {
                    level = 3,
                    userData = typeof(FloatNode)
                },
                new SearchTreeEntry(new GUIContent("Float 2"))
                {
                    level = 3,
                    userData = typeof(Float2Node)
                },
                new SearchTreeEntry(new GUIContent("Float 3"))
                {
                    level = 3,
                    userData = typeof(Float3Node)
                },
                new SearchTreeEntry(new GUIContent("Float 4"))
                {
                    level = 3,
                    userData = typeof(Float4Node)
                },

                new SearchTreeGroupEntry(new GUIContent("Geometry"), 2),

                new SearchTreeEntry(new GUIContent("Position"))
                {
                    level = 3,
                    userData = typeof(PositionNode)
                },

                new SearchTreeGroupEntry(new GUIContent("Math"), 1),

                new SearchTreeGroupEntry(new GUIContent("Basic"), 2),

                new SearchTreeEntry(new GUIContent("Add"))
                {
                    level = 3,
                    userData = typeof(AddNode)
                },

                new SearchTreeEntry(new GUIContent("Multiply"))
                {
                    level = 3,
                    userData = typeof(MultiplyNode)
                },

                new SearchTreeGroupEntry(new GUIContent("Procedural"), 1),

                new SearchTreeGroupEntry(new GUIContent("Noise"), 2),

                new SearchTreeEntry(new GUIContent("Perlin 3D"))
                {
                    level = 3,
                    userData = typeof(Perlin3DNode)
                }
            };

            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var nodeType = (Type)entry.userData;
            var tggNode = TggNode.Create(_graphView, nodeType);
            tggNode.Update();
            tggNode.Position = context.screenMousePosition;

            return true;
        }

        #endregion
    }
}