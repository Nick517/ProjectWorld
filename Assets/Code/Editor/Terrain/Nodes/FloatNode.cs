using System;
using UnityEngine.UIElements;

namespace Terrain.Graph
{
    public class FloatNode : TerrainNode
    {
        public float value = 0;

        public override void Draw()
        {
            title = "Float";
            SetDimensions(100, 150);

            /* OUTPUT CONTAINER */
            AddOutputPort("Out(1)", typeof(float));

            /* EXTENSIONS CONTAINER */
            VisualElement customDataContainer = new();
            TextField floatTextField = GraphUtil.CreateTextField(value.ToString(), null, callback =>
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

            public override void LoadConnections(TerrainNode terrainNode, TerrainGraphView graphView)
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}
