using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Terrain.Graph
{
    public class FloatNode : TerrainNode
    {
        public float value;

        public override void Initialize(TerrainGraphView graphView, Vector2 position)
        {
            title = "Float";

            Vector2 graphPosition = graphView.viewTransform.matrix.inverse.MultiplyPoint(position);
            base.SetPosition(new Rect(graphPosition.x, graphPosition.y, 100, 150));

            value = 0;

            graphView.AddElement(this);
        }

        public override void Draw()
        {
            /* OUTPUT CONTAINER */
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            outputPort.portName = "Float(1)";
            outputContainer.Add(outputPort);

            /* EXTENSIONS CONTAINER */
            VisualElement customDataContainer = new();
            TextField floatTextField = new()
            {
                value = value.ToString()
            };

            customDataContainer.Add(floatTextField);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }
    }
}
