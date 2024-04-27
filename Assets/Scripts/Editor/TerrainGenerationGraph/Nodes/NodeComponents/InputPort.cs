using System;
using System.Linq;
using Serializable;
using TerrainGenerationGraph.Scripts;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class InputPort : TggPort
    {
        #region Fields

        public DefaultValueNode DefaultValueNode;
        public Vector4 DefaultValue = Vector4.zero;

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

        private void AddDefaultValueNode()
        {
            if (DefaultValueNode == null)
            {
                DefaultValueNode = (DefaultValueNode)TggNode.Create(GraphView, typeof(DefaultValueNode));
                DefaultValueNode.ParentingInputPort = this;
                DefaultValueNode.Update();
                DefaultValueNode.Value = DefaultValue;
            }
        }

        public void RemoveDefaultValueNode()
        {
            if (DefaultValueNode != null)
            {
                DefaultValue = DefaultValueNode.Value;
                DefaultValueNode?.Destroy();
                DefaultValueNode = null;
            }
        }

        public void UpdateDefaultValueNode()
        {
            if (DefaultValueNode != null)
            {
                if (ConnectedTggEdges.Any())
                {
                    RemoveDefaultValueNode();
                }
                else if (DefaultValueNode.Dimensions != Dimensions)
                {
                    RemoveDefaultValueNode();
                    AddDefaultValueNode();
                }
            }
            else if (!ConnectedTggEdges.Any())
            {
                AddDefaultValueNode();
            }
        }

        #endregion

        #region Terrain Generation Tree

        public TgGraph.TgTreeDto GetNextTgtNodeDto(TgGraph.TgTreeDto tgTreeDto)
        {
            return AllConnectedPorts.First().ParentTggNode.ToTgtNode(tgTreeDto);
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
                defaultValue = new SerializableVector4(inputPort.DefaultValue);
            }

            public void DeserializeTo(InputPort inputPort)
            {
                inputPort.ID = id;
                inputPort.DefaultValue = defaultValue.Deserialize();
            }
        }

        #endregion
    }
}