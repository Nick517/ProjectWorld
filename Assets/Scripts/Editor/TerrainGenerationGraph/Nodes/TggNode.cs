using System;
using System.Collections.Generic;
using System.Linq;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using Serializable;
using TerrainGenerationGraph.Scripts;
using UnityEngine;
using UnityEngine.UIElements;
using static NodeOperations;
using Node = UnityEditor.Experimental.GraphView.Node;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public abstract class TggNode : Node
    {
        #region Fields
        
        protected TerrainGenGraphView GraphView;
        private readonly List<TggPort> _tggPorts = new();
        private string _id;

        protected abstract List<NodeType> NodeTypes { get; }

        #endregion

        #region Methods

        public static TggNode Create(TerrainGenGraphView graphView, Type nodeType)
        {
            var node = (TggNode)Activator.CreateInstance(nodeType);
            node._id = GraphUtil.NewID;
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
            InputPorts.ForEach(inputPort => inputPort.UpdateValueNode());
        }

        public virtual void Destroy()
        {
            DisconnectAllPorts();
            InputPorts.ForEach(inputPort => inputPort.RemoveValueNode());
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

        protected List<InputPort> InputPorts => _tggPorts.OfType<InputPort>().ToList();

        protected List<OutputPort> OutputPorts => _tggPorts.OfType<OutputPort>().ToList();

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

        private TgtNodeDto _savedDto;
        
        public virtual TgtNodeDto GatherDto(InputPort inputPort = default)
        {
            if (!NodeTypes.Any()) return InputPorts.First().NextTgtNodeDto();
            
            if (_savedDto != null)
            {
                _savedDto.cached = true;
                
                return _savedDto;
            }
            
            var index = OutputPorts.FindIndex(port => inputPort != null && inputPort.ConnectedTggPort == port);

            var nextNodes = InputPorts.Select(port => port.NextTgtNodeDto()).ToArray();

            _savedDto = new TgtNodeDto(NodeTypes.ElementAt(index), nextNodes);
            return _savedDto;
        }

        public virtual void ClearDto()
        {
            _savedDto = null;
        }

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
                id = tggNode._id;
                position = new SerializableVector2(tggNode.Position);
            }

            public abstract TggNode Deserialize(TerrainGenGraphView graphView);

            public void DeserializeTo(TggNode tggNode)
            {
                tggNode._id = id;
                tggNode.Position = position.Deserialize();
            }
        }

        #endregion
    }
}