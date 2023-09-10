using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Terrain.Graph
{
    public static class TerrainGraphElementUtility
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new(onClick)
            {
                text = text
            };

            return button;
        }

        public static void AddPort(this TerrainNode node, string portName, Type type, bool isInput = false)
        {
            if (isInput)
            {
                Port port = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type);
                port.portName = portName;
                node.outputContainer.Add(port);
            }
            else
            {
                Port port = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, type);
                port.portName = portName;
                node.inputContainer.Add(port);
            }
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
