using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Editor.Terrain.Generation.Nodes
{
    public abstract class TgNode : Node
    {
        #region Fields

        private TgGraphView _graphView;

        protected string id;

        public virtual List<TgPort> TgPorts => new();

        #endregion

        #region Methods

        public static TgNode Create(TgGraphView graphView, Type nodeType)
        {
            var node = (TgNode)Activator.CreateInstance(nodeType);
            node.id = GraphUtil.NewID;
            node._graphView = graphView;
            node.Initialize();

            return node;
        }

        private void Initialize()
        {
            _graphView.AddElement(this);
            SetUp();
            RefreshPorts();
            RefreshExpandedState();
        }

        protected abstract void SetUp();

        #endregion

        #region Serialization
        
        public abstract Dto ToDto();
        
        [Serializable]
        public abstract class Dto
        {
            public string id;
            public SerializableVector2 position;

            protected Dto()
            {
            }

            protected Dto(TgNode tgNode)
            {
                id = tgNode.id;
                position = new SerializableVector2(tgNode.GetPosition());
            }

            public virtual TgNode Deserialize(TgGraphView graphView)
            {
                var node = Create(graphView, typeof(TgNode));
                node.id = id;
                node.SetPosition(position.Deserialize());

                return node;
            }
        }
        
        #endregion

        #region Utility

        private new Vector2 GetPosition()
        {
            return base.GetPosition().position;
        }

        public void SetPosition(Vector2 position)
        {
            base.SetPosition(new Rect(position, Vector2.zero));
        }

        protected TgPort AddInputPort(string portName, Type type)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, type);
            port.portName = portName;
            inputContainer.Add(port);

            return new TgPort(port);
        }

        protected TgPort AddOutputPort(string portName, Type type)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type);
            port.portName = portName;
            outputContainer.Add(port);

            return new TgPort(port);
        }

        #endregion
    }
}