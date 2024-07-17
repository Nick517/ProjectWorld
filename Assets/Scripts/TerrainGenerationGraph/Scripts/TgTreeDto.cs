using System;
using System.Collections.Generic;
using System.Linq;
using Serializable;
using static NodeOperations;

namespace TerrainGenerationGraph.Scripts
{
    [Serializable]
    public class TgTreeDto
    {
        public List<TreeNodeDto> nodes = new();
        public List<SerializableFloat4> values = new();

        public TgTreeDto()
        {
        }

        public TgTreeDto(TreeNodeDto rootNode)
        {
            AddNode(rootNode);
        }

        public void AddNode(TreeNodeDto node)
        {
            if (nodes.Contains(node)) return;

            nodes.Add(node);

            if (node.operation == Operation.Value)
            {
                AddValue(node);

                return;
            }

            List<int> nextIndexList = new();

            foreach (var nextNode in node.nextNodes)
            {
                AddNode(nextNode);
                nextIndexList.Add(nodes.IndexOf(nextNode));
            }

            if (nextIndexList.Count > 0) node.nextIndex.x = nextIndexList.ElementAt(0);
            if (nextIndexList.Count > 1) node.nextIndex.y = nextIndexList.ElementAt(1);
            if (nextIndexList.Count > 2) node.nextIndex.z = nextIndexList.ElementAt(2);
            if (nextIndexList.Count > 3) node.nextIndex.w = nextIndexList.ElementAt(3);
        }

        public void AddValue(TreeNodeDto node)
        {
            values.Add(node.value);
            node.nextIndex.x = values.Count - 1;
        }
    }
}