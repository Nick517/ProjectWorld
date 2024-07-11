using System;
using TerrainGenerationGraph.Scripts;
using UnityEditor.Experimental.GraphView;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class OutputPort : TggPort
    {
        public NodeOperations.NodeType NodeType;
        public TgtNodeDto TgtNodeDto;

        #region Constructors

        public OutputPort(TerrainGenGraphView graphView, TggNode parentTggNode, string defaultName, Type type,
            NodeOperations.NodeType nodeType) :
            base(graphView, parentTggNode, defaultName, Direction.Output, Capacity.Multi, type)
        {
            NodeType = nodeType;
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