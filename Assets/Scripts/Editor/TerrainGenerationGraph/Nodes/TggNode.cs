using System;
using System.Collections.Generic;
using System.Linq;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using Serializable;
using TerrainGenerationGraph.Scripts.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public abstract class TggNode : Node
    {
        #region Fields

        protected TerrainGenerationGraphView GraphView;
        private readonly List<TggPort> _tggPorts = new();
        public string ID;

        #endregion

        #region Methods

        public static TggNode Create(TerrainGenerationGraphView graphView, Type nodeType)
        {
            var node = (TggNode)Activator.CreateInstance(nodeType);
            node.ID = GraphUtil.NewID;
            node.GraphView = graphView;
            node.Initialize();

            return node;
        }

        private void Initialize()
        {
            GraphView.AddElement(this);
            SetUp();
            RefreshPorts();
            RefreshExpandedState();
        }

        protected abstract void SetUp();

        public virtual void Update()
        {
            InputPorts.ForEach(inputPort => inputPort.UpdateDefaultValueNode());
        }

        public virtual void Destroy()
        {
            DisconnectAllPorts();

            InputPorts.ForEach(inputPort => inputPort.RemoveDefaultValueNode());

            GraphView.RemoveElement(this);
        }

        protected void SetAllPortDimensions(int dimensions)
        {
            _tggPorts.ForEach(tggPort => tggPort.SetDimensions(dimensions));
        }

        private void DisconnectAllPorts()
        {
            _tggPorts.ForEach(tggPort => tggPort.Disconnect());
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Delete", _ => Destroy(), DropdownMenuAction.AlwaysEnabled);
            base.BuildContextualMenu(evt);
        }

        #endregion

        #region Utility

        protected InputPort AddInputPort(string defaultName = "In", int defaultDimensions = 1)
        {
            var type = TggPort.TypeFromDimensions(defaultDimensions);
            var inputPort = new InputPort(GraphView, this, defaultName, type);

            inputContainer.Add(inputPort);
            _tggPorts.Add(inputPort);

            return inputPort;
        }

        protected OutputPort AddOutputPort(string defaultName = "Out", int defaultDimensions = 1)
        {
            var type = TggPort.TypeFromDimensions(defaultDimensions);
            var outputPort = new OutputPort(GraphView, this, defaultName, type);

            outputContainer.Add(outputPort);
            _tggPorts.Add(outputPort);

            return outputPort;
        }

        private List<InputPort> InputPorts => _tggPorts.OfType<InputPort>().ToList();

        protected List<TggPort> ConnectedOutputPorts =>
            InputPorts
                .SelectMany(inputPort => inputPort.ConnectedTggPorts)
                .ToList();

        public Vector2 Position
        {
            get => base.GetPosition().position;

            set => base.SetPosition(new Rect(value, Vector2.zero));
        }

        #endregion

        #region Terrain Generation Tree

        public abstract TgtNode ToTgtNode();

        #endregion

        #region Serialization

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
                id = tggNode.ID;
                position = new SerializableVector2(tggNode.Position);
            }

            public abstract TggNode Deserialize(TerrainGenerationGraphView graphView);

            public void DeserializeTo(TggNode tggNode)
            {
                tggNode.ID = id;
                tggNode.Position = position.Deserialize();
            }
        }

        #endregion
    }
}