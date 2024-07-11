using System.Collections.Generic;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using TerrainGenerationGraph.Scripts;
using UnityEngine;
using UnityEngine.UIElements;
using static NodeOperations;
using static UnityEditor.EditorApplication;
using static UnityEngine.UIElements.FlexDirection;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public sealed class ValueNode : TggNode
    {
        #region Fields

        public OutputPort OutputPort;
        public InputPort ParentingInputPort;
        public int Dimensions;

        private FloatField _floatFieldX;
        private FloatField _floatFieldY;
        private FloatField _floatFieldZ;
        private FloatField _floatFieldW;

        protected override List<NodeType> NodeTypes => new() { NodeType.Value };

        #endregion

        #region Methods

        protected override void SetUp()
        {
            titleContainer.RemoveFromHierarchy();
            mainContainer.style.flexDirection = Row;

            _floatFieldX = CreateFloatField();
            _floatFieldY = CreateFloatField();
            _floatFieldZ = CreateFloatField();
            _floatFieldW = CreateFloatField();

            OutputPort = AddOutputPort(null);

            capabilities = 0;
        }

        public override void Update()
        {
            Dimensions = ParentingInputPort.Dimensions;

            mainContainer.Add(new Label("X"));
            mainContainer.Add(_floatFieldX);

            if (Dimensions > 1)
            {
                mainContainer.Add(new Label("Y"));
                mainContainer.Add(_floatFieldY);
            }

            if (Dimensions > 2)
            {
                mainContainer.Add(new Label("Z"));
                mainContainer.Add(_floatFieldZ);
            }

            if (Dimensions > 3)
            {
                mainContainer.Add(new Label("W"));
                mainContainer.Add(_floatFieldW);
            }

            OutputPort.SetDimensions(Dimensions);
            mainContainer.Add(OutputPort);

            _ = new TggEdge(GraphView, this, ParentingInputPort);

            ParentingTggNode.Add(this);

            update += Reposition;
        }

        public override void Destroy()
        {
            OutputPort.AllConnectedEdges.ForEach(tggEdge => GraphView.RemoveElement(tggEdge));
            GraphView.RemoveElement(this);
        }

        private const float OffsetX = -15;

        private void Reposition()
        {
            update -= Reposition;

            if (float.IsNaN(GetPosition().width))
            {
                update += Reposition;
                return;
            }

            var newPosition = ParentingInputPort.Position - ParentingTggNode.Position;
            var size = GetPosition().size;
            var offset = new Vector2(size.x, size.y / 2);
            offset.x -= OffsetX;
            newPosition -= offset;

            Position = newPosition;
        }

        private FloatField CreateFloatField()
        {
            var floatField = new FloatField();
            floatField.RegisterValueChangedCallback(_ => { ParentingInputPort.Value = Value; });

            return floatField;
        }

        public Vector4 Value
        {
            get
            {
                var x = _floatFieldX.value;
                var y = _floatFieldY.value;
                var z = _floatFieldZ.value;
                var w = _floatFieldW.value;

                return new Vector4(x, y, z, w);
            }
            set
            {
                _floatFieldX.value = value.x;
                _floatFieldY.value = value.y;
                _floatFieldZ.value = value.z;
                _floatFieldW.value = value.w;
            }
        }

        private TggNode ParentingTggNode => ParentingInputPort.ParentTggNode;

        #endregion

        #region Terrain Generation Tree

        public override TgtNodeDto GatherTgtNodeDto(InputPort inputPort = default)
        {
            return new TgtNodeDto(NodeType.Value, new TgtNodeDto[] { }, Value);
        }

        #endregion
    }
}