using System.Collections.Generic;
using System.Linq;
using Editor.TerrainGenerationGraph.Nodes;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph.Graph
{
    public class TggGraphView : GraphView
    {
        public TgGraph Graph;
        private readonly TggSearchWindow _searchWindow;

        public TggGraphView()
        {
            _searchWindow = ScriptableObject.CreateInstance<TggSearchWindow>();
            _searchWindow.Init(this);

            styleSheets.Add(EditorGUIUtility.Load("TerrainGenerationGraph/TgGraphViewStyles.uss") as StyleSheet);

            var background = new GridBackground();
            background.StretchToParentSize();
            Insert(0, background);

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            graphViewChanged += evt =>
            {
                evt.elementsToRemove?.OfType<TggEdge>().ToList().ForEach(edge => edge.Destroy());
                return evt;
            };
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is GraphView)
                evt.menu.AppendAction("Create Node",
                    _ => { SearchWindow.Open(new SearchWindowContext(evt.localMousePosition), _searchWindow); });

            base.BuildContextualMenu(evt);
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

        private List<TggNode> Nodes => nodes.OfType<TggNode>().Where(node => node is not ConstNode).ToList();

        public List<TggEdge> Edges => AllEdges.Where(edge => !edge.IsConstantEdge).ToList();
        
        public List<TggEdge> AllEdges => edges.OfType<TggEdge>().ToList();

        public void Save()
        {
            Graph.nodes = Nodes.Select(node => node.Dto).ToList();
            Graph.edges = Edges.Select(edge => edge.Dto).ToList();
            EditorUtility.SetDirty(Graph);
        }

        public void Load()
        {
            foreach (var dto in Graph.nodes) _ = new TggNode(this, dto);

            foreach (var dto in Graph.edges)
                _ = new TggEdge(
                    this,
                    ports.OfType<OutputPort>().FirstOrDefault(port => port.ID == dto.outputPortId),
                    ports.OfType<InputPort>().FirstOrDefault(port => port.ID == dto.inputPortId)
                );

            foreach (var node in Nodes) node.Update();
        }
    }
}