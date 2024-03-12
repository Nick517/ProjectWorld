using System.Collections.Generic;
using System.Linq;
using TerrainGenerationGraph.Scripts.Nodes;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class TggPort
    {
        private readonly TerrainGenGraphView _graphView;

        public string id;
        public readonly Port port;
        private DefaultValueNode _defaultValueNode;

        public bool IsInput => port.direction == Direction.Input;

        public bool HasConnection => port.connections.Any();

        private Vector2 Position => _graphView.contentViewContainer.WorldToLocal(port.GetGlobalCenter());

        public TggPort(TerrainGenGraphView graphView, Port port)
        {
            _graphView = graphView;
            this.port = port;
            id = GraphUtil.NewID;
        }

        private Edge ConnectTo(TggPort tggPort)
        {
            var edge = port.ConnectTo(tggPort.port);
            _graphView.AddElement(edge);
            return edge;
        }

        public void Disconnect()
        {
            List<Port> ports = new();

            foreach (var edge in port.connections)
            {
                ports.Add(edge.input);
                ports.Add(edge.output);
                _graphView.RemoveElement(edge);
            }

            ports.ForEach(p => p.DisconnectAll());
        }

        public void AddDefaultValue()
        {
            _defaultValueNode = (DefaultValueNode)TggNode.Create(_graphView, typeof(DefaultValueNode));
            ParentTggNode.Add(_defaultValueNode);
            var edge = ConnectTo(_defaultValueNode.outputPort);
            edge.capabilities = 0;

            EditorApplication.update += RepositionDefaultValue;
        }

        private void RepositionDefaultValue()
        {
            EditorApplication.update -= RepositionDefaultValue;

            var offsetX = -15;

            var newPos = Position - ParentTggNode.Position;
            var defaultValueSize = _defaultValueNode.GetPosition().size;
            var offset = new Vector2(defaultValueSize.x, defaultValueSize.y / 2);
            offset.x -= offsetX;
            newPos -= offset;

            _defaultValueNode.SetPosition(new Rect(newPos, Vector2.zero));
        }

        public TggPort ConnectedTggPort
        {
            get
            {
                var tggPorts = _graphView.TggPorts;

                foreach (var tggEdge in _graphView.GetAllTggEdgeDto())
                {
                    if (tggEdge.inputPortId == id)
                        foreach (var tggPort in tggPorts)
                            if (tggPort.id == tggEdge.outputPortId)
                                return tggPort;

                    if (tggEdge.outputPortId == id)
                        foreach (var tggPort in tggPorts)
                            if (tggPort.id == tggEdge.inputPortId)
                                return tggPort;
                }

                return null;
            }
        }

        private TggNode ParentTggNode
        {
            get
            {
                foreach (var tggNode in _graphView.TggNodes)
                    if (tggNode.TggPorts.Contains(this))
                        return tggNode;

                return null;
            }
        }

        public TggNode ConnectedTggNode => ConnectedTggPort?.ParentTggNode;

        public TgtNode ConnectedTgtNode => ConnectedTggNode?.ToTgtNode();
    }
}