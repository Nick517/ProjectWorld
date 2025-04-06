using System;
using Editor.TerrainGenerationGraph.Graph;
using TerrainGenerationGraph;
using UnityEditor.Experimental.GraphView;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class OutputPort : TggPort
    {
        public OutputPort(TggGraphView graphView, TggNode parentNode, string defaultName, Type type) :
            base(graphView, parentNode, defaultName, Direction.Output, Capacity.Multi, type)
        {
        }

        public TgGraph.Node.OutputPort Dto => new() { id = ID };
    }
}