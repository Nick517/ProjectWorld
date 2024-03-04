using System;
using System.Collections.Generic;
using TerrainGenerationGraph.Scripts.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public abstract class TggNode : Node
    {
        #region Fields

        private TerrainGenGraphView _graphView;

        protected string id;

        public virtual List<TggPort> TggPorts => new();

        #endregion

        #region Methods

        public static TggNode Create(TerrainGenGraphView graphView, Type nodeType)
        {
            var node = (TggNode)Activator.CreateInstance(nodeType);
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

        public abstract TgtNode ToTgtNode();

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

            protected Dto(TggNode tggNode)
            {
                id = tggNode.id;
                position = new SerializableVector2(tggNode.GetPosition());
            }

            public virtual TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var node = Create(graphView, typeof(TggNode));
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

        protected TggPort AddInputPort(string portName, Type type)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, type);
            port.portName = portName;
            inputContainer.Add(port);

            return new TggPort(_graphView, port);
        }

        protected TggPort AddOutputPort(string portName, Type type)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type);
            port.portName = portName;
            outputContainer.Add(port);

            return new TggPort(_graphView, port);
        }

        protected FloatField AddFloatField()
        {
            var floatField = new FloatField();
            extensionContainer.Add(floatField);

            return floatField;
        }

        #endregion
    }
}