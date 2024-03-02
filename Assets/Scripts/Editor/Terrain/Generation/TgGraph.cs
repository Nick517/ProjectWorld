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
    public class TgGraph : GraphView
    {
        public string path;
        private TgSearchWindow _searchWindow;

        private List<TgNode> TgNodes => nodes.Select(GetTgNode).ToList();

        private List<TgPort> TgPorts
        {
            get
            {
                List<TgPort> tgPorts = new();
                foreach (var tgNode in TgNodes) tgPorts.AddRange(tgNode.TgPorts);
                return tgPorts;
            }
        }
        
        private List<TgEdgeDto> GetAllTgEdgeDto()
        {
            List<TgEdgeDto> tgEdges = new();

            foreach (var edge in edges) tgEdges.Add(new TgEdgeDto(this, edge));

            return tgEdges;
        }

        private TgNode GetTgNode(Node node)
        {
            return node as TgNode;
        }

        public TgPort GetTgPort(Port port)
        {
            foreach (var tgPort in TgPorts)
                if (tgPort.port == port)
                    return tgPort;

            return null;
        }

        public TgPort GetTgPort(string id)
        {
            foreach (var tgPort in TgPorts)
                if (tgPort.id == id)
                    return tgPort;

            return null;
        }

        public TgGraph()
        {
            AddGridBackground();
            AddStyles();
            AddManipulator();
            AddSearchWindow();

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
            public List<TgEdgeDto> tgEdgeDtoList;

            public Dto()
            {
            }

            public Dto(TgGraph graph)
            {
                foreach (var tgNode in graph.TgNodes) tgNodeDtoList.Add(tgNode.ToDto());
                tgEdgeDtoList = graph.GetAllTgEdgeDto();
            }

            public void Deserialize(TgGraph graph)
            {
                graph.ClearGraph();
                foreach (var dto in tgNodeDtoList) graph.TgNodes.Add(dto.Deserialize(graph));
                foreach (var dto in tgEdgeDtoList) dto.Deserialize(graph);
            }
        }

        #endregion
    }
}