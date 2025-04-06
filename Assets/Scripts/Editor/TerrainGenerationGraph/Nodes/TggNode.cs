using System.Collections.Generic;
using System.Linq;
using Editor.TerrainGenerationGraph.Graph;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static TerrainGenerationGraph.NodeDefinitions;
using InputPort = Editor.TerrainGenerationGraph.Nodes.NodeComponents.InputPort;
using OutputPort = Editor.TerrainGenerationGraph.Nodes.NodeComponents.OutputPort;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public class TggNode : Node
    {
        protected TggGraphView GraphView;
        private readonly Type _type;
        private readonly List<InputPort> _inputPorts = new();
        private readonly List<OutputPort> _outputPorts = new();
        private readonly bool _setPortsToLowest;

        protected TggNode()
        {
        }

        public TggNode(TggGraphView graphView, TgGraph.Node node)
        {
            GraphView = graphView;
            _type = node.type;
            base.title = node.Definition.Name;
            _setPortsToLowest = node.Definition.SetPortsToLowest;

            foreach (var inputPort in node.Definition.InputPorts) AddInputPort(inputPort);
            foreach (var outputPort in node.Definition.OutputPorts) AddOutputPort(outputPort);

            if (node.type == Type.Sample) capabilities &= ~Capabilities.Deletable;

            FromDto(node);

            GraphView.AddElement(this);
            RefreshPorts();
            RefreshExpandedState();
        }

        private void FromDto(TgGraph.Node dto)
        {
            Position = dto.position;

            for (var i = 0; i < dto.inputPorts.Count; i++)
            {
                _inputPorts[i].ID = dto.inputPorts[i].id;
                _inputPorts[i].ConstVal = dto.inputPorts[i].constVal;
            }

            for (var i = 0; i < dto.outputPorts.Count; i++) _outputPorts[i].ID = dto.outputPorts[i].id;
        }

        public void Update()
        {
            if (_setPortsToLowest) SetAllPortDimensions(TggPort.GetLowestDimension(ConnectedOutputPorts));

            _inputPorts.ForEach(port => port.UpdateConstNode());
        }

        public void Destroy()
        {
            DisconnectAllPorts();
            _inputPorts.ForEach(port => port.RemoveConstNode());
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

        private void AddInputPort(NodeDefinitions.InputPort def)
        {
            var type = TggPort.TypeFromDimensions(def.DefaultDimensions);
            var inputPort = new InputPort(GraphView, this, def.Name, type);

            _inputPorts.Add(inputPort);
            inputContainer.Add(inputPort);
        }

        private void AddOutputPort(NodeDefinitions.OutputPort def)
        {
            var type = TggPort.TypeFromDimensions(def.DefaultDimensions);
            var outputPort = new OutputPort(GraphView, this, def.Name, type);

            _outputPorts.Add(outputPort);
            outputContainer.Add(outputPort);
        }

        private List<TggPort> Ports => _inputPorts.Concat<TggPort>(_outputPorts).ToList();

        private List<TggPort> ConnectedOutputPorts => _inputPorts.SelectMany(port => port.ConnectedPorts).ToList();

        public Vector2 Position
        {
            get => base.GetPosition().position;

            private set => base.SetPosition(new Rect(value, Vector2.zero));
        }

        public TgGraph.Node Dto =>
            new()
            {
                type = _type,
                position = Position,
                inputPorts = _inputPorts.Select(port => port.Dto).ToList(),
                outputPorts = _outputPorts.Select(port => port.Dto).ToList()
            };
    }
}