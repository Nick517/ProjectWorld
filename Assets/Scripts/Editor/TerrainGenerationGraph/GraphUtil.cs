using System;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph
{
    public static class GraphUtil
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            Button button = new(onClick) { text = text };

            return button;
        }

        public static string NewID => Guid.NewGuid().ToString();
    }
}