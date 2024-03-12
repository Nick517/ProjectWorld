using System;
using System.Collections.Generic;
using TerrainGenerationGraph.Scripts.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public class DefaultValueNode : TggNode
    {
        #region Fields

        public TggPort outputPort;
        private FloatField _floatField;

        public override List<TggPort> TggPorts => new() { outputPort };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            titleContainer.RemoveFromHierarchy();
            mainContainer.style.flexDirection = FlexDirection.Row;

            var label = new Label("X");
            mainContainer.Add(label);

            _floatField = new FloatField();
            mainContainer.Add(_floatField);

            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
            outputPort = new TggPort(graphView, port);

            port.portName = null;
            mainContainer.Add(port);

            capabilities = 0;
        }

        protected TggNode ParentTggNode => outputPort.ConnectedTggNode;

        public override TgtNode ToTgtNode()
        {
            return new TgtDefaultValue
            {
                value = _floatField.value
            };
        }

        #endregion

        #region Serialization

        public override Dto ToDto()
        {
            return new DefaultValueDto(this);
        }

        [Serializable]
        public class DefaultValueDto : Dto
        {
            public float value;
            public string outputPortId;

            public DefaultValueDto()
            {
            }

            public DefaultValueDto(DefaultValueNode defaultValueNode) : base(defaultValueNode)
            {
                value = defaultValueNode._floatField.value;
                outputPortId = defaultValueNode.outputPort.id;
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var defaultValue = (DefaultValueNode)Create(graphView, typeof(DefaultValueNode));
                defaultValue._floatField.value = value;
                defaultValue.outputPort.id = outputPortId;
                defaultValue.id = id;
                defaultValue.Position = position.Deserialize();
                
                return defaultValue;
            }
        }

        #endregion
    }
}