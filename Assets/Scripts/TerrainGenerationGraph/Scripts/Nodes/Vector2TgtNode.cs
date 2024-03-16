using System;
using UnityEngine;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class Vector2TgtNode : TgtNode
    {
        public TgtNode nextNodeX;
        public TgtNode nextNodeY;

        public override Vector4 Traverse()
        {
            return new Vector2
            {
                x = nextNodeX.Traverse().x,
                y = nextNodeY.Traverse().x
            };
        }
    }
}