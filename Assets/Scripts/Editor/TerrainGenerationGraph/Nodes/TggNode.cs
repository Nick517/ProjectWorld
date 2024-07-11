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
        private readonly List<InputPort> _inputPorts = new();
        protected readonly List<OutputPort> OutputPorts = new();
        private string _id;
        protected bool SetPortsToLowest;

        #endregion

        #region Methods

        public static TggNode Create(TerrainGenGraphView graphView, Type nodeType)
        {
            var node = (TggNode)Activator.CreateInstance(nodeType);
            node.GraphView = graphView;
            node.Initialize();

            return node;
        }

        private void Initialize()
        {
            _id = GraphUtil.NewID;
            GraphView.AddElement(this);
            SetUp();
            RefreshPorts();
            RefreshExpandedState();
        }

        protected abstract void SetUp();

        public virtual void Update()
        {
            if (SetPortsToLowest) SetAllPortDimensions(TggPort.GetLowestDimension(ConnectedOutputPorts));

            _inputPorts.ForEach(inputPort => inputPort.UpdateValueNode());
        }

        public virtual void Destroy()
        {
            DisconnectAllPorts();
            _inputPorts.ForEach(inputPort => inputPort.RemoveValueNode());
            GraphView.RemoveElement(this);
        }

        private void SetAllPortDimensions(int dimensions)
        {
            TggPorts.ForEach(tggPort => tggPort.SetDimensions(dimensions));
        }

        private void DisconnectAllPorts()
        {
            TggPorts.ForEach(tggPort => tggPort.Disconnect());
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Delete", _ => Destroy(), DropdownMenuAction.AlwaysEnabled);
            base.BuildContextualMenu(evt);
        }

        #endregion

        #region Utility

        protected void AddInputPort(string defaultName = "In", int defaultDimensions = 1)
        {
            var type = TggPort.TypeFromDimensions(defaultDimensions);
            var inputPort = new InputPort(GraphView, this, defaultName, type);

            inputContainer.Add(inputPort);
            _inputPorts.Add(inputPort);
        }

        protected void AddOutputPort(NodeType nodeType, int defaultDimensions = 1)
        {
            AddOutputPort("Out", nodeType, defaultDimensions);
        }

        protected void AddOutputPort(string defaultName = "Out", NodeType nodeType = default,
            int defaultDimensions = 1)
        {
            var type = TggPort.TypeFromDimensions(defaultDimensions);
            var outputPort = new OutputPort(GraphView, this, defaultName, type, nodeType);

            outputContainer.Add(outputPort);
            OutputPorts.Add(outputPort);
        }

        private List<TggPort> TggPorts => _inputPorts.Concat<TggPort>(OutputPorts).ToList();

        private List<TggPort> ConnectedOutputPorts =>
            _inputPorts
                .SelectMany(inputPort => inputPort.ConnectedTggPorts)
                .ToList();

        public Vector2 Position
        {
            get => base.GetPosition().position;

            set => base.SetPosition(new Rect(value, Vector2.zero));
        }

        #endregion

        #region Terrain Generation Tree

        public virtual TgtNodeDto GatherTgtNodeDto(InputPort inputPort = default)
        {
            if (inputPort == default) return _inputPorts.First().NextTgtNodeDto();

            var outputPort = inputPort.ConnectedTggPort as OutputPort;

            if (outputPort == null || outputPort.NodeType == NodeType.Skip) return _inputPorts.First().NextTgtNodeDto();

            if (outputPort.TgtNodeDto != null)
            {
                outputPort.TgtNodeDto.cached = true;

                return outputPort.TgtNodeDto;
            }

            var nextNodes = _inputPorts.Select(port => port.NextTgtNodeDto()).ToArray();

            outputPort.TgtNodeDto = new TgtNodeDto(outputPort.NodeType, nextNodes);

            return outputPort.TgtNodeDto;
        }

        #endregion

        #region Serialization

        public Dto ToDto()
        {
            return new Dto(this);
        }

        [Serializable]
        public class Dto
        {
            public string typeName;
            public string id;
            public SerializableVector2 position;
            public List<InputPort.Dto> inputPortDtoList;
            public List<OutputPort.Dto> outputPortDtoList;

            public Dto()
            {
            }

            public Dto(TggNode tggNode)
            {
                typeName = tggNode.GetType().FullName;
                id = tggNode._id;
                position = new SerializableVector2(tggNode.Position);
                inputPortDtoList = tggNode._inputPorts.Select(inputPort => inputPort.ToDto()).ToList();
                outputPortDtoList = tggNode.OutputPorts.Select(outputPort => outputPort.ToDto()).ToList();
            }

            public TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var type = Type.GetType(typeName);

                var tggNode = Create(graphView, type);

                tggNode._id = id;
                tggNode.Position = position.Deserialize();

                for (var i = 0; i < tggNode._inputPorts.Count; i++)
                    inputPortDtoList[i].DeserializeTo(tggNode._inputPorts[i]);

                for (var i = 0; i < tggNode.OutputPorts.Count; i++)
                    outputPortDtoList[i].DeserializeTo(tggNode.OutputPorts[i]);

                return tggNode;
            }
        }

        #endregion
    }
}