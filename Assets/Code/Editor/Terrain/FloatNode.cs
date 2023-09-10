using UnityEngine;
using UnityEngine.UIElements;

namespace Terrain.Graph
{
    public class FloatNode : TerrainNode
    {
        public float value = 0;

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
            TerrainGraphElementUtility.AddPort(this, "Out(1)", typeof(float), true);

            /* EXTENSIONS CONTAINER */
            VisualElement customDataContainer = new();
            TextField floatTextField = TerrainGraphElementUtility.CreateTextField(value.ToString(), null, callback =>
            {
                if (float.TryParse(callback.newValue, out float v))
                {
                    value = v;
                }
            });

            customDataContainer.Add(floatTextField);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }
    }
}
