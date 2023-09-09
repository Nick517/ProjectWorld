using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Terrain.Graph
{
    public class AddNode : TerrainNode
    {
        public override void Initialize(TerrainGraphView graphView, Vector2 position)
        {
            title = "Add";

            Vector2 graphPosition = graphView.viewTransform.matrix.inverse.MultiplyPoint(position);
            base.SetPosition(new Rect(graphPosition.x, graphPosition.y, 100, 150));

            graphView.AddElement(this);
        }

        public override void Draw()
        {
            /* INPUT CONTAINER */
            Port inputPort1 = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort1.portName = "A(1)";
            inputContainer.Add(inputPort1);

            Port inputPort2 = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(float));
            inputPort2.portName = "B(1)";
            inputContainer.Add(inputPort2);

            /* OUTPUT CONTAINER */
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            outputPort.portName = "Float(1)";
            outputContainer.Add(outputPort);

            RefreshExpandedState();
        }
    }
}
