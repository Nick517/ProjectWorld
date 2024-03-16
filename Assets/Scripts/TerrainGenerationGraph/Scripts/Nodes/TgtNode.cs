using System;
using UnityEngine;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public abstract class TgtNode
    {
        public abstract Vector4 Traverse();
    }
}