using System;
using System.Linq;
using Serializable;
using TerrainGenerationGraph.Scripts;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using static NodeOperations;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class InputPort : TggPort
    {
        #region Fields

        private ValueNode _valueNode;
        public float4 Value = float4.zero;

        #endregion

        #region Constructors

        public InputPort(TerrainGenGraphView graphView, TggNode parentTggNode, string defaultName, Type type) :
            base(graphView, parentTggNode, defaultName, Direction.Input, Capacity.Single, type)
        {
        }

        #endregion

        #region Methods

        public void Update()
        {
            ParentTggNode.Update();
        }

        public void UpdateValueNode()
        {
            if (_valueNode != null)
            {
                if (ConnectedTggEdges.Any())
                {
                    RemoveValueNode();
                }
                else if (_valueNode.Dimensions != Dimensions)
                {
                    RemoveValueNode();
                    AddValueNode();
                }
            }
            else if (!ConnectedTggEdges.Any())
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

        public TgtNodeDto NextTgtNodeDto()
        {
            if (_valueNode != null) return new TgtNodeDto(Operation.Value, new TgtNodeDto[] { }, Value);

            return AllConnectedPorts.First().ParentTggNode.GatherTgtNodeDto(this);
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
            public SerializableVector4 defaultValue;

            public Dto()
            {
            }

            public Dto(InputPort inputPort)
            {
                id = inputPort.ID;
                defaultValue = new SerializableVector4(inputPort.Value);
            }

            public void DeserializeTo(InputPort inputPort)
            {
                inputPort.ID = id;
                inputPort.Value = defaultValue.Deserialize();
            }
        }

        #endregion
    }
}