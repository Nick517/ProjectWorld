using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public abstract class TggPort : Port
    {
        #region Fields

        protected readonly TerrainGenGraphView GraphView;

        public readonly TggNode ParentTggNode;
        public string ID;
        private readonly string _defaultName;

        #endregion

        #region Constructors

        protected TggPort(TerrainGenGraphView graphView, TggNode parentTggNode, string defaultName,
            Direction direction, Capacity capacity, Type type)
            : base(Orientation.Horizontal, direction, capacity, type)
        {
            GraphView = graphView;
            ParentTggNode = parentTggNode;
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
            ConnectedTggPorts.OfType<InputPort>().ToList().ForEach(inputPort => inputPort.ParentTggNode.Update());
        }

        public void Disconnect()
        {
            ConnectedTggEdges.ForEach(tggEdge => tggEdge.Destroy());
        }

        public TggPort ConnectedTggPort => ConnectedTggPorts.First();

        public List<TggPort> ConnectedTggPorts =>
            ConnectedTggEdges
                .Select(tggEdge => tggEdge.PortOfType(OppositePortDirection)).ToList();

        protected List<TggPort> AllConnectedPorts =>
            AllConnectedEdges
                .Select(tggEdge => tggEdge.PortOfType(OppositePortDirection)).ToList();

        public List<TggEdge> ConnectedTggEdges =>
            GraphView.TggEdges
                .Where(tggEdge => !tggEdge.IsDvnEdge && tggEdge.TggPorts.Contains(this))
                .ToList();
        
        public List<TggEdge> AllConnectedEdges =>
            GraphView.TggEdges
                .Where(tggEdge => tggEdge.TggPorts.Contains(this))
                .ToList();

        private Type OppositePortDirection => this is OutputPort ? typeof(InputPort) : typeof(OutputPort);

        public int Dimensions => DimensionsFromType(portType);

        public Vector2 Position => GraphView.contentViewContainer.WorldToLocal(GetGlobalCenter());

        public static int GetLowestDimension(List<TggPort> tggPorts)
        {
            return tggPorts == null || tggPorts.Count == 0
                ? 1
                : tggPorts.Select(tggPort => DimensionsFromType(tggPort.portType)).Prepend(4).Min();
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