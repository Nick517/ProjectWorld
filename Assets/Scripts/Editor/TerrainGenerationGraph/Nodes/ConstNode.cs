using Editor.TerrainGenerationGraph.Graph;
using Editor.TerrainGenerationGraph.Nodes.NodeComponents;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.EditorApplication;
using static UnityEngine.UIElements.FlexDirection;

namespace Editor.TerrainGenerationGraph.Nodes
{
    public sealed class ConstNode : TggNode
    {
        private const float OffsetX = -15;

        public readonly OutputPort OutputPort;
        public int Dimensions;

        private readonly FloatField _floatFieldX;
        private readonly FloatField _floatFieldY;
        private readonly FloatField _floatFieldZ;
        private readonly FloatField _floatFieldW;
        private readonly InputPort _parentingInputPort;

        public ConstNode(TggGraphView graphView, InputPort parentingInputPort)
        {
            GraphView = graphView;
            _parentingInputPort = parentingInputPort;

            titleContainer.RemoveFromHierarchy();
            mainContainer.style.flexDirection = Row;
            _floatFieldX = CreateFloatField();
            _floatFieldY = CreateFloatField();
            _floatFieldZ = CreateFloatField();
            _floatFieldW = CreateFloatField();
            OutputPort = new OutputPort(GraphView, this, null, typeof(float));
            capabilities = 0;
        }

        public new void Update()
        {
            Dimensions = _parentingInputPort.Dimensions;

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

            _ = new TggEdge(GraphView, this, _parentingInputPort);

            ParentingNode.Add(this);

            update += Reposition;
        }

        public new void Destroy()
        {
            OutputPort.AllConnectedEdges.ForEach(edge => GraphView.RemoveElement(edge));

            GraphView.RemoveElement(this);
        }

        private void Reposition()
        {
            update -= Reposition;

            if (float.IsNaN(GetPosition().width))
            {
                update += Reposition;
                return;
            }

            var newPos = _parentingInputPort.Position - ParentingNode.Position;
            var size = GetPosition().size;
            var offset = new Vector2(size.x, size.y / 2);
            offset.x -= OffsetX;
            newPos -= offset;

            SetPosition(new Rect(newPos, Vector2.zero));
        }

        private FloatField CreateFloatField()
        {
            var floatField = new FloatField();
            floatField.RegisterValueChangedCallback(_ => { _parentingInputPort.ConstVal = Value; });
            return floatField;
        }

        public float4 Value
        {
            get => new(_floatFieldX.value, _floatFieldY.value, _floatFieldZ.value, _floatFieldW.value);

            set
            {
                _floatFieldX.value = value.x;
                _floatFieldY.value = value.y;
                _floatFieldZ.value = value.z;
                _floatFieldW.value = value.w;
            }
        }

        private TggNode ParentingNode => _parentingInputPort.ParentNode;
    }
}