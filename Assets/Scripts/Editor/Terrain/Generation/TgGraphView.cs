using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Terrain.Generation.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Terrain.Generation
{
    public class TgGraphView : GraphView
    {
        private TgSearchWindow _searchWindow;

        public readonly List<TgNode> tgNodes = new();
        public readonly List<TgPort> tgPorts = new();
        public readonly List<TgEdge> tgEdges = new();
        
        public TgGraphView()
        {
            AddGridBackground();
            AddStyles();
            AddManipulator();
            AddSearchWindow();

            graphViewChanged += OnGraphChange;

            var sampleNode = (SampleNode)TgNode.Create(this, typeof(SampleNode));
            sampleNode.SetPosition(Vector2.zero);
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            var styleSheet = (StyleSheet)EditorGUIUtility.Load("Terrain Generation/TGGraphViewStyles.uss");
            styleSheets.Add(styleSheet);
        }

        private void AddManipulator()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        private GraphViewChange OnGraphChange(GraphViewChange change)
        {
            if (change.edgesToCreate != null)
                foreach (var edge in change.edgesToCreate)
                {
                    var tgEdge = new TgEdge(this, edge);
                }

            if (change.elementsToRemove != null)
                foreach (var e in change.elementsToRemove)
                {
                    if (e is Edge edge)
                    {
                        Debug.Log("removed edge");
                        var tgEdge = GetTgEdge(edge);
                        tgEdges.Remove(tgEdge);
                    }
                    
                    if (e is Port port)
                    {
                        Debug.Log("removed port");
                        var tgPort = GetTgPort(port);
                        tgPorts.Remove(tgPort);
                    }
                    
                    if (e is Node node)
                    {
                        Debug.Log("removed node");
                        var tgNode = GetTgNode(node);
                        tgNodes.Remove(tgNode);
                    }
                }

            return change;
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

        public TgNode GetTgNode(Node node)
        {
            return tgNodes.FirstOrDefault(tgNode => tgNode == (TgNode)node);
        }

        public TgPort GetTgPort(string id)
        {
            return tgPorts.FirstOrDefault(tgPort => tgPort.id == id);
        }

        public TgPort GetTgPort(Port port)
        {
            return tgPorts.FirstOrDefault(tgPort => tgPort.port == port);
        }

        private TgEdge GetTgEdge(Edge edge)
        {
            return tgEdges.FirstOrDefault(tgEdge => tgEdge.edge == edge);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var mousePosition = evt.localMousePosition;

            if (evt.target is GraphView)
                evt.menu.AppendAction("Create Node",
                    e => { SearchWindow.Open(new SearchWindowContext(mousePosition), _searchWindow); });

            base.BuildContextualMenu(evt);
        }

        private void AddSearchWindow()
        {
            if (_searchWindow != null) return;
            _searchWindow = ScriptableObject.CreateInstance<TgSearchWindow>();
            _searchWindow.Initialize(this);
        }

        private void ClearGraph()
        {
            DeleteElements(graphElements.ToList());
            tgNodes.Clear();
            tgPorts.Clear();
        }

        #region Save System

        public Dto ToDto()
        {
            return new Dto(this);
        }

        [Serializable]
        public class Dto
        {
            public List<TgNode.Dto> tgNodeDtoList = new();
            public List<TgEdge.Dto> tgEdgeDtoList = new();

            public Dto(TgGraphView graphView)
            {
                foreach (var tgNode in graphView.tgNodes) tgNodeDtoList.Add(tgNode.ToDto());
                foreach (var tgEdge in graphView.tgEdges) tgEdgeDtoList.Add(tgEdge.ToDto());
            }

            public void Deserialize(TgGraphView graph)
            {
                graph.ClearGraph();
                foreach (var dto in tgNodeDtoList) graph.tgNodes.Add(dto.Deserialize(graph));
                foreach (var dto in tgEdgeDtoList) dto.Deserialize(graph);
            }
        }

        #endregion
    }
}