using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Terrain.Generation.Nodes
{
    public class TgPort
    {
        public string id;

        public TgNode parentNode;
        public readonly Port port;

        public TgPort(TgNode parentNode, Orientation orientation, Direction direction, Port.Capacity capacity,
            Type type)
        {
            id = Guid.NewGuid().ToString();
            this.parentNode = parentNode;
            port = parentNode.InstantiatePort(orientation, direction, capacity, type);
            parentNode.graph.tgPorts.Add(this);
            port.AddManipulator(new EdgeConnector<Edge>(new EdgeConnectorListener()));
        }
        
        public class EdgeConnectorListener : IEdgeConnectorListener
        {
            public void OnDropOutsidePort(Edge edge, Vector2 position)
            {
                Debug.Log("Edge dropped outside port");
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
                Debug.Log("Edge dropped");
            }
        }



        /*
        public void ConnectTo(string connectorPortId)
        {
            ConnectTo(parentNode.graph.GetTgPort(connectorPortId));
        }

        public void ConnectTo(TgPort connectorPort)
        {
            port.ConnectTo(connectorPort.port);
        }*/

        public TgPort ConnectedInputPort => parentNode.graph.GetTgPort(port.connections.Single().input);

        public List<TgPort> ConnectedOutputPorts => port.connections.Select(connection => connection.output)
            .Select(outputPort => parentNode.graph.GetTgPort(outputPort)).ToList();

        public Dto ToDto()
        {
            return new Dto(this);
        }

        [Serializable]
        public class Dto
        {
            public string id;

            public Dto(TgPort tgPort)
            {
                id = tgPort.id;
            }
        }
    }
}