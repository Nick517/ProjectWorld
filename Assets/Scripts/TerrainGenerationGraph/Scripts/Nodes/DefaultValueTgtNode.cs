using System;
using Serializable;
using UnityEngine;

namespace TerrainGenerationGraph.Scripts.Nodes
{
    [Serializable]
    public class DefaultValueTgtNode : TgtNode
    {
        public SerializableVector4 value;

        public override Vector4 Traverse()
        {
            return value.Deserialize();
        }
    }
}