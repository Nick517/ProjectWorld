using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class TggEdge : Edge
    {
        #region Fields

        public TerrainGenGraphView GraphView;

        public readonly bool IsDvnEdge;

        public List<TggPort> TggPorts => new() { OutputPort, InputPort };

        #endregion

        public TggEdge()
        {
        }

        #region Constructors

        public TggEdge(TerrainGenGraphView graphView, Edge edge)
        {
            GraphView = graphView;
            output = edge.output;
            input = edge.input;

            graphView.AddElement(this);
            InputPort?.Update();
        }

        public TggEdge(TerrainGenGraphView graphView, ValueNode valueNode,
            InputPort parentingInputPort)
        {
            GraphView = graphView;
            output = valueNode.OutputPort;
            input = parentingInputPort;
            IsDvnEdge = true;

            graphView.AddElement(this);
        }

        private TggEdge(TerrainGenGraphView graphView, OutputPort outputPort, InputPort parentingInputPort,
            bool isDvnEdge = false)
        {
            GraphView = graphView;
            output = outputPort;
            input = parentingInputPort;
            IsDvnEdge = isDvnEdge;

            graphView.AddElement(this);
            parentingInputPort.Update();
        }

        #endregion

        #region Methods

        public void Destroy()
        {
            if (!IsDvnEdge)
            {
                GraphView.RemoveElement(this);
                InputPort?.Update();
            }
            else
            {
                InputPort.ParentTggNode.Destroy();
                GraphView.RemoveElement(this);
            }
        }

        private OutputPort OutputPort => output as OutputPort;

        public InputPort InputPort => input as InputPort;

        public TggPort PortOfType(Type type)
        {
            return type == typeof(OutputPort) ? OutputPort : InputPort;
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
            public string outputPortId;
            public string inputPortId;

            public Dto()
            {
            }

            public Dto(TggEdge tggEdge)
            {
                outputPortId = tggEdge.OutputPort.ID;
                inputPortId = tggEdge.InputPort.ID;
            }

            public virtual void Deserialize(TerrainGenGraphView graphView)
            {
                var outputPort = graphView.GetTggPort(outputPortId) as OutputPort;
                var inputPort = graphView.GetTggPort(inputPortId) as InputPort;
                _ = new TggEdge(graphView, outputPort, inputPort);
            }
        }

        #endregion
    }
}