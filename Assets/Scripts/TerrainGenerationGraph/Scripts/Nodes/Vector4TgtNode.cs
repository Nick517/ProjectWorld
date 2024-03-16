using System;
using UnityEngine;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class Vector4TgtNode : TgtNode
    {
        public TgtNode nextNodeX;
        public TgtNode nextNodeY;
        public TgtNode nextNodeZ;
        public TgtNode nextNodeW;

        public override Vector4 Traverse()
        {
            return new Vector4
            {
                x = nextNodeX.Traverse().x,
                y = nextNodeY.Traverse().x,
                z = nextNodeZ.Traverse().x,
                w = nextNodeW.Traverse().x
            };
        }
    }
}