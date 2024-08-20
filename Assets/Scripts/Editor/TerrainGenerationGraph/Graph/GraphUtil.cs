using System;
using UnityEngine.UIElements;

namespace Editor.TerrainGenerationGraph.Graph
{
    public static class GraphUtil
    {
        public static Button CreateButton(string text, Action onClick = null)
        {
            return new Button(onClick) { text = text };
        }

        public static string NewID => Guid.NewGuid().ToString();
    }
}