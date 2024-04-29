using System;
using ECS.Components;
using Serializable;

namespace TerrainGenerationGraph.Scripts
{
    [Serializable]
    public class TgtNodeDto
    {
        public TgTreeData.NodeType nodeType;
        public SerializableInt4 nextIndexes = new(-1);

        public TgtNodeDto(TgTreeData.NodeType nodeType)
        {
            this.nodeType = nodeType;
        }
    }
}