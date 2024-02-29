using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Editor.Terrain.Generation.Nodes
{
    public class TgNode : Node
    {
        #region Variables

        public string id;
        public TgGraphView graph;

        #endregion

        #region Methods

        public static TgNode Create(TgGraphView graph, Type nodeType)
        {
            var node = (TgNode)Activator.CreateInstance(nodeType);
            node.id = Guid.NewGuid().ToString();
            node.graph = graph;
            node.Initialize();
            graph.tgNodes.Add(node);

            return node;
        }

        public void Initialize()
        {
            graph.AddElement(this);
            SetUp();
            RefreshPorts();
            RefreshExpandedState();
        }

        protected virtual void SetUp()
        {
        }

        #endregion

        public virtual Dto ToDto()
        {
            return new Dto(this);
        }


        [Serializable]
        public class Dto
        {
            public string id;
            public SerializableVector2 position;

            public Dto(TgNode tgNode)
            {
                id = tgNode.id;
                position = new SerializableVector2(tgNode.GetPosition().position);
            }

            public virtual TgNode Deserialize(TgGraphView graph)
            {
                var node = Create(graph, typeof(TgNode));
                node.id = id;
                node.SetPosition(position.AsVector2());

                return node;
            }
        }

        #region Utility

        public void SetPosition(Vector2 position)
        {
            base.SetPosition(new Rect(position, Vector2.zero));
        }

        protected TgPort AddInputPort(string portName, Type type)
        {
            TgPort nodePort = new(this, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, type)
                { port = { portName = portName } };

            inputContainer.Add(nodePort.port);

            return nodePort;
        }

        protected TgPort AddOutputPort(string portName, Type type)
        {
            TgPort nodePort = new(this, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type)
                { port = { portName = portName } };

            outputContainer.Add(nodePort.port);

            return nodePort;
        }

        #endregion
    }
}