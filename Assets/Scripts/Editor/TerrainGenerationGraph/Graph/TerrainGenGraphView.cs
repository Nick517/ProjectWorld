using System;
using System.Collections.Generic;
using System.Linq;
using Editor.TerrainGenerationGraph.Nodes;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using Newtonsoft.Json;
using TerrainGenerationGraph;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Utility.Serialization;

namespace Editor.TerrainGenerationGraph.Graph
{
    public class TerrainGenGraphView : GraphView
    {
        #region Fields

        public TgGraph TgGraph;
        private TggSearchWindow _searchWindow;

        #endregion

        #region Constructors

        public TerrainGenGraphView()
        {
            AddGridBackground();
            AddStyles();
            AddManipulators();
            AddSearchWindow();
            graphViewChanged += OnGraphViewChanged;

            var sampleNode = new TggNode(this, "Sample");
            sampleNode.Update();
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
            var styleSheet = (StyleSheet)EditorGUIUtility.Load("TerrainGenerationGraph/TgGraphViewStyles.uss");
            styleSheets.Add(styleSheet);
        }

        private void AddManipulators()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        private void AddSearchWindow()
        {
            if (_searchWindow != null) return;
            _searchWindow = ScriptableObject.CreateInstance<TggSearchWindow>();
            _searchWindow.Initialize(this);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var mousePosition = evt.localMousePosition;

            if (evt.target is GraphView)
                evt.menu.AppendAction("Create Node",
                    _ => { SearchWindow.Open(new SearchWindowContext(mousePosition), _searchWindow); });

            base.BuildContextualMenu(evt);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange evt)
        {
            evt.elementsToRemove?.OfType<TggEdge>().ToList().ForEach(edge => edge.Destroy());

            return evt;
        }

        private void ClearGraph()
        {
            DeleteElements(graphElements.ToList());
        }

        #endregion

        #region Utility

        private TggNode RootNode => Nodes.FirstOrDefault(tggNode => tggNode.NodeType == "Sample");

        private List<TggNode> Nodes => nodes.OfType<TggNode>().ToList();

        private List<TggPort> Ports => ports.OfType<TggPort>().ToList();

        public List<TggEdge> Edges => edges.OfType<TggEdge>().ToList();

        public TggPort GetTggPort(string id)
        {
            return Ports.FirstOrDefault(port => port.ID == id);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports
                .Where(port =>
                    startPort != port &&
                    startPort.node != port.node &&
                    startPort.direction != port.direction)
                .ToList();
        }

        private void ClearSavedDto()
        {
            Ports.OfType<OutputPort>().ToList().ForEach(outputPort => outputPort.TreeNodeDto = null);
        }

        #endregion

        #region Save System

        public void SerializeToTgGraph()
        {
            TgGraph.serializedTreeData = SerializeTree();
            TgGraph.serializedGraphData = SerializeGraph();
        }

        private string SerializeTree()
        {
            var rootNode = RootNode.GatherTgtNodeDto();
            ClearSavedDto();
            rootNode.Simplify();

            var tree = new TgTreeDto(rootNode);

            return JsonConvert.SerializeObject(tree, JsonSettings.Formatted);
        }

        private string SerializeGraph()
        {
            return JsonConvert.SerializeObject(ToDto(), JsonSettings.Formatted);
        }

        private Dto ToDto()
        {
            return new Dto(this);
        }

        [Serializable]
        public class Dto
        {
            public List<TggNode.Dto> nodeDtoList;
            public List<TggEdge.Dto> edgeDtoList;

            public Dto()
            {
            }

            public Dto(TerrainGenGraphView graphView)
            {
                nodeDtoList = graphView.Nodes.Where(node => node.NodeType != null).ToList()
                    .Select(node => node.ToDto()).ToList();

                edgeDtoList = graphView.Edges.Where(edge => !edge.IsValueNodeEdge)
                    .Select(edge => edge.ToDto()).ToList();
            }

            public void Deserialize(TerrainGenGraphView graphView)
            {
                graphView.ClearGraph();

                nodeDtoList.ForEach(dto => dto.Deserialize(graphView));

                edgeDtoList.ForEach(dto => dto.Deserialize(graphView));

                graphView.Nodes.ForEach(node => node.Update());
            }
        }

        #endregion
    }
}