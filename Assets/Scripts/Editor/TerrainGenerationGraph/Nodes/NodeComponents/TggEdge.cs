using System;
using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Graph;
using TerrainGenerationGraph;
using UnityEditor.Experimental.GraphView;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class TggEdge : Edge
    {
        public TggGraphView GraphView;
        public readonly bool IsConstantEdge;

        public TggEdge()
        {
        }

        public TggEdge(TggGraphView graphView, Edge edge)
        {
            GraphView = graphView;
            output = edge.output;
            input = edge.input;

            graphView.AddElement(this);
            InputPort?.Update();
        }

        public TggEdge(TggGraphView graphView, ConstNode constNode, InputPort inputPort)
        {
            GraphView = graphView;
            output = constNode.OutputPort;
            input = inputPort;
            IsConstantEdge = true;

            graphView.AddElement(this);
        }

        public TggEdge(TggGraphView graphView, OutputPort outputPort, InputPort inputPort)
        {
            GraphView = graphView;
            output = outputPort;
            input = inputPort;

            graphView.AddElement(this);
            InputPort.Update();
        }

        public void Destroy()
        {
            if (!IsConstantEdge)
            {
                GraphView.RemoveElement(this);
                InputPort?.Update();
            }
            else
            {
                InputPort.ParentNode.Destroy();
                GraphView.RemoveElement(this);
            }
        }

        public List<TggPort> TggPorts => new() { OutputPort, InputPort };

        private OutputPort OutputPort => output as OutputPort;

        public InputPort InputPort => input as InputPort;

        public TggPort PortOfType(Type type)
        {
            return type == typeof(OutputPort) ? OutputPort : InputPort;
        }

        public TgGraph.Edge Dto => new() { inputPortId = InputPort.ID, outputPortId = OutputPort.ID };
    }
}