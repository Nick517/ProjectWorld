using System;
using System.Collections.Generic;
using System.Linq;
using Editor.TerrainGenerationGraph.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public abstract class TggPort : Port
    {
        #region Fields

        protected readonly TerrainGenGraphView GraphView;

        public readonly TggNode ParentNode;
        public string ID;
        private readonly string _defaultName;

        #endregion

        #region Constructors

        protected TggPort(TerrainGenGraphView graphView, TggNode parentNode, string defaultName, Direction direction,
            Capacity capacity, Type type)
            : base(Orientation.Horizontal, direction, capacity, type)
        {
            GraphView = graphView;
            ParentNode = parentNode;
            ID = GraphUtil.NewID;
            _defaultName = defaultName;
            SetDimensions(DimensionsFromType(type));

            this.AddManipulator(new EdgeConnector<TggEdge>(new TggEdgeConnectorListener(graphView)));
        }

        #endregion

        #region Methods

        public void SetDimensions(int dimensions)
        {
            portType = PortTypes.ElementAt(dimensions - 1);
            portName = _defaultName != null ? $"{_defaultName}({dimensions})" : "";
            ConnectedPorts.OfType<InputPort>().ToList().ForEach(inputPort => inputPort.ParentNode.Update());
        }

        public void Disconnect()
        {
            ConnectedEdges.ForEach(edge => edge.Destroy());
        }

        public TggPort ConnectedPort => ConnectedPorts.First();

        public List<TggPort> ConnectedPorts =>
            ConnectedEdges
                .Select(edge => edge.PortOfType(OppositePortDirection)).ToList();

        protected List<TggPort> AllConnectedPorts =>
            AllConnectedEdges
                .Select(edge => edge.PortOfType(OppositePortDirection)).ToList();

        public List<TggEdge> ConnectedEdges =>
            GraphView.Edges
                .Where(edge => !edge.IsValueNodeEdge && edge.TggPorts.Contains(this))
                .ToList();

        public List<TggEdge> AllConnectedEdges =>
            GraphView.Edges
                .Where(edge => edge.TggPorts.Contains(this))
                .ToList();

        private Type OppositePortDirection => this is OutputPort ? typeof(InputPort) : typeof(OutputPort);

        public int Dimensions => DimensionsFromType(portType);

        public Vector2 Position => GraphView.contentViewContainer.WorldToLocal(GetGlobalCenter());

        public static int GetLowestDimension(List<TggPort> ports)
        {
            return ports.Count == 0 ? 1 : ports.Select(port => DimensionsFromType(port.portType)).Prepend(4).Min();
        }

        public static Type TypeFromDimensions(int dimensions)
        {
            return PortTypes.ElementAt(dimensions - 1);
        }

        private static int DimensionsFromType(Type type)
        {
            return PortTypes.IndexOf(type) + 1;
        }

        private static readonly List<Type> PortTypes = new()
        {
            typeof(float), typeof(Vector2), typeof(Vector3), typeof(Vector4)
        };

        #endregion
    }
}