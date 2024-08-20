using System;
using Editor.TerrainGenerationGraph.Graph;
using TerrainGenerationGraph;
using UnityEditor.Experimental.GraphView;
using Utility.TerrainGeneration;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class OutputPort : TggPort
    {
        public readonly NodeOperations.Operation Operation;
        public TreeNodeDto TreeNodeDto;

        #region Constructors

        public OutputPort(TerrainGenGraphView graphView, TggNode parentNode, string defaultName, Type type,
            NodeOperations.Operation operation) :
            base(graphView, parentNode, defaultName, Direction.Output, Capacity.Multi, type)
        {
            Operation = operation;
        }

        #endregion

        #region Save System

        public Dto ToDto()
        {
            return new Dto(this);
        }

        [Serializable]
        public class Dto
        {
            public string id;

            public Dto()
            {
            }

            public Dto(OutputPort outputPort)
            {
                id = outputPort.ID;
            }

            public void DeserializeTo(OutputPort outputPort)
            {
                outputPort.ID = id;
            }
        }

        #endregion
    }
}