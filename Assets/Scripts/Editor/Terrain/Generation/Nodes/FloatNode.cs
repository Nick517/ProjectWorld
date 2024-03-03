using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Editor.Terrain.Generation.Nodes
{
    public class FloatNode : TggNode
    {
        #region Fields

        private TggPort _outputPort;
        private FloatField _floatField;

        public override List<TggPort> TggPorts => new() { _outputPort };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            title = "Float";

            _outputPort = AddOutputPort("Out(1)", typeof(float));

            _floatField = new FloatField();
            extensionContainer.Add(_floatField);
        }
        
        #endregion
        
        #region Terrain Genereration Tree

        public override TgtNode ToTgtNode()
        {
            return new FloatTgtNode(this);
        }

        private class FloatTgtNode : TgtNode
        {
            private readonly float _value;

            public FloatTgtNode(FloatNode floatNode)
            {
                _value = floatNode._floatField.value;
            }

            public override float Traverse()
            {
                return _value;
            }
        }

        #endregion

        #region Serialization

        public override Dto ToDto()
        {
            return new FloatNodeDto(this);
        }

        [Serializable]
        public class FloatNodeDto : Dto
        {
            public string outputPortId;
            public float value;

            public FloatNodeDto()
            {
            }

            public FloatNodeDto(FloatNode floatNode) : base(floatNode)
            {
                outputPortId = floatNode._outputPort.id;
                value = floatNode._floatField.value;
            }

            public override TggNode Deserialize(TerrainGenGraphView graphView)
            {
                var floatNode = (FloatNode)Create(graphView, typeof(FloatNode));
                floatNode._outputPort.id = outputPortId;
                floatNode._floatField.value = value;
                floatNode.id = id;
                floatNode.SetPosition(position.Deserialize());

                return floatNode;
            }
        }

        #endregion
    }
}