using System;
using UnityEngine;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class Vector3TgtNode : TgtNode
    {
        public TgtNode nextNodeX;
        public TgtNode nextNodeY;
        public TgtNode nextNodeZ;

        public override Vector4 Traverse()
        {
            return new Vector3
            {
                x = nextNodeX.Traverse().x,
                y = nextNodeY.Traverse().x,
                z = nextNodeZ.Traverse().x
            };
        }
    }
}