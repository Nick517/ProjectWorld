using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGenerationGraph.Scripts.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public abstract class TggNode : Node
    {
        #region Fields

        protected TerrainGenGraphView graphView;

        protected string id;

        public virtual List<TggPort> TggPorts => new();

        private List<TggPort> InputTggPorts => TggPorts.Where(t => t.IsInput).ToList();

        #endregion

        #region Methods

        public static TggNode Create(TerrainGenGraphView graphView, Type nodeType)
        {
            var node = (TggNode)Activator.CreateInstance(nodeType);
            node.id = GraphUtil.NewID;
            node.graphView = graphView;
            node.Initialize();
            return node;
        }

        private void Initialize()
        {
            graphView.AddElement(this);
            SetUp();
            RefreshPorts();
            RefreshExpandedState();
        }

        public void Delete()
        {
            DisconnectAllPorts();
            graphView.RemoveElement(this);
            graphView.OnChange();
        }

        private void DisconnectAllPorts()
        {
            foreach (var tggPort in TggPorts) tggPort.Disconnect();
        }

        public void AddDefaultValues()
        {
            foreach (var tggPort in InputTggPorts) tggPort.AddDefaultValue();
        }

        protected abstract void SetUp();

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Delete", _ => Delete(), DropdownMenuAction.AlwaysEnabled);
            base.BuildContextualMenu(evt);
        }

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
                position = new SerializableVector2(tggNode.Position);
            }

            public virtual TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var node = Create(graphView, typeof(TggNode));
                node.id = id;
                node.Position = position.Deserialize();

                return node;
            }
        }

        #endregion

        #region Utility

        public Vector2 Position
        {
            get => base.GetPosition().position;

            set => base.SetPosition(new Rect(value, Vector2.zero));
        }

        protected TggPort AddInputPort(string portName, Type type)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, type);
            port.portName = portName;
            inputContainer.Add(port);

            return new TggPort(graphView, port);
        }

        protected TggPort AddOutputPort(string portName, Type type)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type);
            port.portName = portName;
            outputContainer.Add(port);

            return new TggPort(graphView, port);
        }

        #endregion
    }
}