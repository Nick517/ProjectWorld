using System;
using System.Collections.Generic;
using System.Linq;
using Editor.TerrainGenerationGraph.Nodes;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using Newtonsoft.Json;
using TerrainGenerationGraph.Scripts;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph
{
    public class TerrainGenGraphView : GraphView
    {
        
        #region Fields
        
        public TgGraph tgGraph;
        private TggSearchWindow _searchWindow;

        #endregion
        
        #region Constructors
        
        public TerrainGenGraphView()
        {
            AddGridBackground();
            AddStyles();
            AddManipulator();
            AddSearchWindow();

            TggNode.Create(this, typeof(SampleNode));
        }
        
        #endregion

        #region Methods
        
        private void AddGridBackground()
        {
            GridBackground gridBackground = new();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            var styleSheet = (StyleSheet)EditorGUIUtility.Load("TGGraph/TgGraphViewStyles.uss");
            styleSheets.Add(styleSheet);
        }

        private void AddManipulator()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var mousePosition = evt.localMousePosition;

            if (evt.target is GraphView)
                evt.menu.AppendAction("Create Node",
                    _ => { SearchWindow.Open(new SearchWindowContext(mousePosition), _searchWindow); });

            base.BuildContextualMenu(evt);
        }

        private void AddSearchWindow()
        {
            if (_searchWindow != null) return;
            _searchWindow = ScriptableObject.CreateInstance<TggSearchWindow>();
            _searchWindow.Initialize(this);
        }

        private void ClearGraph()
        {
            DeleteElements(graphElements.ToList());
        }
        
        #endregion

        #region Save System

        public void SerializeToTgGraph()
        {
            tgGraph.serializedTreeData = SerializeTree();
            tgGraph.serializedGraphData = SerializeGraph();
        }
        
        public string SerializeGraph()
        {
            return JsonConvert.SerializeObject(ToDto(), JsonSettings.Formatted);
        }

        public string SerializeTree()
        {
            var rootTgtNode = GetSampleNode().ToTgtNode();
            return JsonConvert.SerializeObject(rootTgtNode, JsonSettings.Formatted);
        }

        private Dto ToDto()
        {
            return new Dto(this);
        }

        [Serializable]
        public class Dto
        {
            public List<TggNode.Dto> tgNodeDtoList = new();
            public List<TggEdgeDto> tgEdgeDtoList;

            public Dto()
            {
            }

            public Dto(TerrainGenGraphView graph)
            {
                foreach (var tgNode in graph.TggNodes) tgNodeDtoList.Add(tgNode.ToDto());
                tgEdgeDtoList = graph.GetAllTggEdgeDto();
            }

            public void Deserialize(TerrainGenGraphView graph)
            {
                graph.ClearGraph();
                foreach (var dto in tgNodeDtoList) graph.TggNodes.Add(dto.Deserialize(graph));
                foreach (var dto in tgEdgeDtoList) dto.Deserialize(graph);
            }
        }

        #endregion

        #region Utility

        public List<TggNode> TggNodes => nodes.Select(GetTggNode).ToList();

        public List<TggPort> TggPorts
        {
            get
            {
                List<TggPort> tggPorts = new();
                foreach (var tggNode in TggNodes) tggPorts.AddRange(tggNode.TggPorts);
                return tggPorts;
            }
        }

        public List<TggEdgeDto> GetAllTggEdgeDto()
        {
            List<TggEdgeDto> tggEdges = new();

            foreach (var edge in edges) tggEdges.Add(new TggEdgeDto(this, edge));

            return tggEdges;
        }

        private TggNode GetTggNode(Node node)
        {
            return node as TggNode;
        }

        public TggPort GetTggPort(Port port)
        {
            foreach (var tggPort in TggPorts)
                if (tggPort.port == port)
                    return tggPort;

            return null;
        }

        public TggPort GetTggPort(string id)
        {
            foreach (var tggPort in TggPorts)
                if (tggPort.id == id)
                    return tggPort;

            return null;
        }

        private SampleNode GetSampleNode()
        {
            foreach (var tggNode in TggNodes)
                if (tggNode is SampleNode sampleNode)
                    return sampleNode;

            return null;
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new();

            // Checks if port is not startPort, if the port's node is not the same as startPort's, and if port does not go in the same direction as startPort
            ports.ForEach(port =>
            {
                // Checks if port is not startPort, if the port's node is not the same as startPort's, and if port does not go in the same direction as startPort
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        #endregion
    }
}