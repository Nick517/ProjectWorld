using System.Collections.Generic;
using System.Linq;
using Editor.TerrainGenerationGraph.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class TggEdgeConnectorListener : IEdgeConnectorListener
    {
        #region Fields

        private readonly TggGraphView _graphView;
        private readonly GraphViewChange _graphViewChange;
        private readonly List<Edge> _edgesToCreate = new();
        private readonly List<Edge> _edgesToDelete = new();

        #endregion

        #region Constructors

        public TggEdgeConnectorListener(TggGraphView graphView)
        {
            _graphView = graphView;
            _graphViewChange.edgesToCreate = _edgesToCreate;
        }

        #endregion

        #region Methods

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
            var tggEdge = edge as TggEdge;

            if (tggEdge == null) return;

            tggEdge.GraphView = _graphView;

            _edgesToCreate.Clear();
            _edgesToCreate.Add(tggEdge);
            _edgesToDelete.Clear();

            _edgesToDelete.AddRange(tggEdge.InputPort.ConnectedEdges.Where(connection => connection != tggEdge));


            if (_edgesToDelete.Count > 0) graphView.DeleteElements(_edgesToDelete);

            var edgesToCreate = _edgesToCreate;

            if (graphView.graphViewChanged != null)
                edgesToCreate = graphView.graphViewChanged(_graphViewChange).edgesToCreate;

            _edgesToDelete.OfType<TggEdge>().ToList().ForEach(tggEdgeToDelete => tggEdgeToDelete.Destroy());

            edgesToCreate.ForEach(edgeToCreate => { _ = new TggEdge(_graphView, edgeToCreate); });
        }

        #endregion
    }
}