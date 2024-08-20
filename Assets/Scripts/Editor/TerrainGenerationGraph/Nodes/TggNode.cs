using System;
using System.Collections.Generic;
using System.Linq;
using Editor.TerrainGenerationGraph.Graph;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using Serializable;
using TerrainGenerationGraph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Utility.TerrainGeneration.NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class TggNode : Node
    {
        #region Fields

        public readonly string NodeType;
        protected TerrainGenGraphView GraphView;
        private readonly List<InputPort> _inputPorts = new();
        private readonly List<OutputPort> _outputPorts = new();
        private string _id;
        private readonly bool _setPortsToLowest;

        #endregion

        #region Methods

        protected TggNode()
        {
        }

        public TggNode(TerrainGenGraphView graphView, string nodeType)
        {
            GraphView = graphView;
            NodeType = nodeType;
            _id = GraphUtil.NewID;
            base.title = NodeType;

            switch (NodeType)
            {
                case "Sample":
                    AddInputPort();
                    capabilities &= ~Capabilities.Deletable;
                    break;

                case "Split":
                    AddInputPort();
                    AddOutputPort("X", Operation.SplitOutX);
                    AddOutputPort("Y", Operation.SplitOutY);
                    AddOutputPort("Z", Operation.SplitOutZ);
                    AddOutputPort("W", Operation.SplitOutW);
                    break;

                case "Float":
                    AddInputPort("X");
                    AddOutputPort();
                    break;

                case "Float 2":
                    AddInputPort("X");
                    AddInputPort("Y");
                    AddOutputPort(Operation.Float2, 2);
                    break;

                case "Float 3":
                    AddInputPort("X");
                    AddInputPort("Y");
                    AddInputPort("Z");
                    AddOutputPort(Operation.Float3, 3);
                    break;

                case "Float 4":
                    AddInputPort("X");
                    AddInputPort("Y");
                    AddInputPort("Z");
                    AddInputPort("W");
                    AddOutputPort(Operation.Float4, 4);
                    break;

                case "Position":
                    AddOutputPort(Operation.Position, 3);
                    break;

                case "Add":
                    AddInputPort("A");
                    AddInputPort("B");
                    AddOutputPort(Operation.Add);
                    _setPortsToLowest = true;
                    break;

                case "Subtract":
                    AddInputPort("A");
                    AddInputPort("B");
                    AddOutputPort(Operation.Subtract);
                    _setPortsToLowest = true;
                    break;

                case "Multiply":
                    AddInputPort("A");
                    AddInputPort("B");
                    AddOutputPort(Operation.Multiply);
                    _setPortsToLowest = true;
                    break;

                case "Divide":
                    AddInputPort("A");
                    AddInputPort("B");
                    AddOutputPort(Operation.Divide);
                    _setPortsToLowest = true;
                    break;

                case "Perlin 3D":
                    AddInputPort("Coord", 3);
                    AddInputPort("Scale");
                    AddOutputPort(Operation.Perlin3D);
                    break;
            }

            GraphView.AddElement(this);
            RefreshPorts();
            RefreshExpandedState();
        }

        public void Update()
        {
            if (_setPortsToLowest) SetAllPortDimensions(TggPort.GetLowestDimension(ConnectedOutputPorts));

            _inputPorts.ForEach(port => port.UpdateValueNode());
        }

        public void Destroy()
        {
            DisconnectAllPorts();
            _inputPorts.ForEach(port => port.RemoveValueNode());
            GraphView.RemoveElement(this);
        }

        private void SetAllPortDimensions(int dimensions)
        {
            Ports.ForEach(port => port.SetDimensions(dimensions));
        }

        private void DisconnectAllPorts()
        {
            Ports.ForEach(port => port.Disconnect());
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Delete", _ => Destroy(), DropdownMenuAction.AlwaysEnabled);
            base.BuildContextualMenu(evt);
        }

        #endregion

        #region Utility

        private void AddInputPort(string defaultName = "In", int defaultDimensions = 1)
        {
            var type = TggPort.TypeFromDimensions(defaultDimensions);
            var inputPort = new InputPort(GraphView, this, defaultName, type);

            inputContainer.Add(inputPort);
            _inputPorts.Add(inputPort);
        }

        private void AddOutputPort(Operation operation, int defaultDimensions = 1)
        {
            AddOutputPort("Out", operation, defaultDimensions);
        }

        private void AddOutputPort(string defaultName = "Out", Operation operation = default, int defaultDimensions = 1)
        {
            var type = TggPort.TypeFromDimensions(defaultDimensions);
            var outputPort = new OutputPort(GraphView, this, defaultName, type, operation);

            outputContainer.Add(outputPort);
            _outputPorts.Add(outputPort);
        }

        private List<TggPort> Ports => _inputPorts.Concat<TggPort>(_outputPorts).ToList();

        private List<TggPort> ConnectedOutputPorts => _inputPorts.SelectMany(port => port.ConnectedPorts).ToList();

        public Vector2 Position
        {
            get => base.GetPosition().position;

            set => base.SetPosition(new Rect(value, Vector2.zero));
        }

        #endregion

        #region Terrain Generation Tree

        public TreeNodeDto GatherTgtNodeDto(InputPort inputPort = null)
        {
            if (inputPort == null) return _inputPorts.First().NextNodeDto();

            var outputPort = inputPort.ConnectedPort as OutputPort;

            if (outputPort == null || outputPort.Operation == Operation.Skip) return _inputPorts.First().NextNodeDto();

            if (outputPort.TreeNodeDto != null)
            {
                outputPort.TreeNodeDto.cached = true;

                return outputPort.TreeNodeDto;
            }

            var nextNodes = _inputPorts.Select(port => port.NextNodeDto()).ToArray();

            outputPort.TreeNodeDto = new TreeNodeDto(outputPort.Operation, nextNodes);

            return outputPort.TreeNodeDto;
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
            public string nodeType;
            public string id;
            public SerializableVector2 position;
            public List<InputPort.Dto> inputPortDtoList;
            public List<OutputPort.Dto> outputPortDtoList;

            public Dto()
            {
            }

            public Dto(TggNode node)
            {
                nodeType = node.NodeType;
                id = node._id;
                position = node.Position;
                inputPortDtoList = node._inputPorts.Select(inputPort => inputPort.ToDto()).ToList();
                outputPortDtoList = node._outputPorts.Select(outputPort => outputPort.ToDto()).ToList();
            }

            public TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var tggNode = new TggNode(graphView, nodeType)
                {
                    _id = id,
                    Position = position
                };

                for (var i = 0; i < tggNode._inputPorts.Count; i++)
                    inputPortDtoList[i].DeserializeTo(tggNode._inputPorts[i]);

                for (var i = 0; i < tggNode._outputPorts.Count; i++)
                    outputPortDtoList[i].DeserializeTo(tggNode._outputPorts[i]);

                return tggNode;
            }
        }

        #endregion
    }
}