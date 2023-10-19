using System;
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

            GUID = UnityEditor.GUID.Generate();

            graphView.AddElement(this);
            Draw();
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

                Debug.Log(value.ToString());
            });

            customDataContainer.Add(floatTextField);
            extensionContainer.Add(customDataContainer);

            RefreshExpandedState();
        }

        #region Save System
        public override SaveData GetSaveData()
        {
            return new FloatNodeSaveData(this);
        }

        public class FloatNodeSaveData : SaveData
        {
            public float value;

            public FloatNodeSaveData() : base() { }

            public FloatNodeSaveData(FloatNode floatNode) : base(floatNode)
            {
                value = floatNode.value;
            }

            public override void Load(TerrainGraphView graphView)
            {
                FloatNode floatNode = (FloatNode)Activator.CreateInstance(typeof(FloatNode));
                floatNode.value = value;
                floatNode.Initialize(graphView, new(positionX, positionY));
            }
        }
        #endregion
    }
}
