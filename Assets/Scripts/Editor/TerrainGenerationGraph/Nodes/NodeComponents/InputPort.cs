using System;
using System.Linq;
using Editor.TerrainGenerationGraph.Graph;
using TerrainGenerationGraph;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class InputPort : TggPort
    {
        public float4 ConstVal;
        private ConstNode _constNode;

        public InputPort(TggGraphView graphView, TggNode parentNode, string defaultName, Type type) :
            base(graphView, parentNode, defaultName, Direction.Input, Capacity.Single, type)
        {
        }

        public void Update()
        {
            ParentNode.Update();
        }

        public void UpdateConstNode()
        {
            if (_constNode != null)
            {
                if (ConnectedEdges.Any())
                {
                    RemoveConstNode();
                }
                else if (_constNode.Dimensions != Dimensions)
                {
                    RemoveConstNode();
                    AddConstNode();
                }
            }
            else if (!ConnectedEdges.Any())
            {
                AddConstNode();
            }
        }

        private void AddConstNode()
        {
            if (_constNode == null)
            {
                _constNode = new ConstNode(GraphView, this);
                _constNode.Update();
                _constNode.Value = ConstVal;
            }
        }

        public void RemoveConstNode()
        {
            if (_constNode != null)
            {
                ConstVal = _constNode.Value;
                _constNode?.Destroy();
                _constNode = null;
            }
        }

        public TgGraph.Node.InputPort Dto => new() { id = ID, constVal = ConstVal };
    }
}