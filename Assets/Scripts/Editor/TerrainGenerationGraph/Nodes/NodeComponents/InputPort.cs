using System;
using System.Linq;
using DataTypes.Serializable;
using Editor.TerrainGenerationGraph.Graph;
using TerrainGenerationGraph;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using static Utility.TerrainGeneration.NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class InputPort : TggPort
    {
        #region Fields

        private ValueNode _valueNode;
        public float4 Value;

        #endregion

        #region Constructors

        public InputPort(TerrainGenGraphView graphView, TggNode parentNode, string defaultName, Type type) :
            base(graphView, parentNode, defaultName, Direction.Input, Capacity.Single, type)
        {
        }

        #endregion

        #region Methods

        public void Update()
        {
            ParentNode.Update();
        }

        public void UpdateValueNode()
        {
            if (_valueNode != null)
            {
                if (ConnectedEdges.Any())
                {
                    RemoveValueNode();
                }
                else if (_valueNode.Dimensions != Dimensions)
                {
                    RemoveValueNode();
                    AddValueNode();
                }
            }
            else if (!ConnectedEdges.Any())
            {
                AddValueNode();
            }
        }

        private void AddValueNode()
        {
            if (_valueNode == null)
            {
                _valueNode = new ValueNode(GraphView, this);
                _valueNode.Update();
                _valueNode.Value = Value;
            }
        }

        public void RemoveValueNode()
        {
            if (_valueNode != null)
            {
                Value = _valueNode.Value;
                _valueNode?.Destroy();
                _valueNode = null;
            }
        }

        #endregion

        #region Terrain Generation Tree

        public TreeNodeDto NextNodeDto()
        {
            if (_valueNode != null) return new TreeNodeDto(Operation.Value, new TreeNodeDto[] { }, Value);

            return AllConnectedPorts.First().ParentNode.GatherTgtNodeDto(this);
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
            public SerializableFloat4 defaultValue;

            public Dto()
            {
            }

            public Dto(InputPort inputPort)
            {
                id = inputPort.ID;
                defaultValue = inputPort.Value;
            }

            public void DeserializeTo(InputPort inputPort)
            {
                inputPort.ID = id;
                inputPort.Value = defaultValue;
            }
        }

        #endregion
    }
}