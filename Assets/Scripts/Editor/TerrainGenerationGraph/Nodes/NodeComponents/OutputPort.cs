using System;
using UnityEditor.Experimental.GraphView;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class OutputPort : TggPort
    {
        #region Constructors

        public OutputPort(TerrainGenGraphView graphView, TggNode parentTggNode, string defaultName, Type type) :
            base(graphView, parentTggNode, defaultName, Direction.Output, Capacity.Multi, type)
        {
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