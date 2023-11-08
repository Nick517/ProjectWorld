using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Terrain.Graph
{
    public static class GraphUtil
    {
        public static Vector2 GetGraphPosition(GraphView graphView, Vector2 position)
        {
            return graphView.viewTransform.matrix.inverse.MultiplyPoint(position);
        }

        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new(onClick)
            {
                text = text
            };

            return button;
        }

        public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null)
        {
            TextField textField = new()
            {
                value = value,
                label = label
            };

            if (onValueChanged != null)
            {
                _ = textField.RegisterValueChangedCallback(onValueChanged);
            }

            return textField;
        }
    }
}
