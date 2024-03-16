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
    public class TerrainGenerationGraphView : GraphView
    {
        #region Fields

        public TgGraph TgGraph;
        private TggSearchWindow _searchWindow;

        #endregion

        #region Constructors

        public TerrainGenerationGraphView()
        {
            AddGridBackground();
            AddStyles();
            AddManipulators();
            AddSearchWindow();
            graphViewChanged += OnGraphViewChanged;

            var sampleNode = TggNode.Create(this, typeof(SampleNode));
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
            evt.elementsToRemove?.OfType<TggEdge>().ToList().ForEach(tggEdge => tggEdge.Destroy());

            return evt;
        }

        private void ClearGraph()
        {
            DeleteElements(graphElements.ToList());
        }

        #endregion

        #region Utility

        private SampleNode RootNode => nodes.OfType<SampleNode>().FirstOrDefault();

        private List<TggNode> TggNodes => nodes.OfType<TggNode>().ToList();

        private List<TggPort> TggPorts => ports.OfType<TggPort>().ToList();

        public List<TggEdge> TggEdges => edges.OfType<TggEdge>().ToList();

        public TggPort GetTggPort(string id)
        {
            return TggPorts.FirstOrDefault(tggPort => tggPort.ID == id);
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

        #endregion

        #region Save System

        public void SerializeToTgGraph()
        {
            TgGraph.serializedTreeData = SerializeTree();
            TgGraph.serializedGraphData = SerializeGraph();
        }

        private string SerializeTree()
        {
            var rootTgtNode = RootNode.ToTgtNode();
            return JsonConvert.SerializeObject(rootTgtNode, JsonSettings.Formatted);
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
            public List<TggNode.Dto> TggNodeDtoList;
            public List<TggEdge.Dto> tggEdgeDtoList;

            public Dto()
            {
            }

            public Dto(TerrainGenerationGraphView graphView)
            {
                TggNodeDtoList = graphView.TggNodes.OfType<ITggNodeSerializable>()
                    .Select(tggNode => tggNode.ToDto()).ToList();
                tggEdgeDtoList = graphView.TggEdges.Where(tggEdge => !tggEdge.IsDvnEdge)
                    .Select(tggEdge => tggEdge.ToDto()).ToList();
            }

            public void Deserialize(TerrainGenerationGraphView graphView)
            {
                graphView.ClearGraph();
                TggNodeDtoList.ForEach(dto => dto.Deserialize(graphView));
                tggEdgeDtoList.ForEach(dto => dto.Deserialize(graphView));
                graphView.TggNodes.Where(tggNode => tggNode is not DefaultValueNode).ToList()
                    .ForEach(tggNode => tggNode.Update());
            }
        }

        #endregion
    }
}