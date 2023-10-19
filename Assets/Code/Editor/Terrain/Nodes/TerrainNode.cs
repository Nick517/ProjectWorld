using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Terrain.Graph
{
    public abstract class TerrainNode : Node
    {
        public GUID GUID;

        public abstract void Initialize(TerrainGraphView graphView, Vector2 position);

        public abstract void Draw();

        #region Save System
        public abstract SaveData GetSaveData();

        public class SaveData
        {
            public string GUID;

            public float positionX;
            public float positionY;

            public SaveData() { }

            public SaveData(TerrainNode terrainNode)
            {
                GUID = terrainNode.GUID.ToString();

                float2 position = terrainNode.GetPosition().position;
                positionX = position.x;
                positionY = position.y;
            }

            public void Load(TerrainGraphView graphView)
            {
                TerrainNode node = (TerrainNode)Activator.CreateInstance(GetType().DeclaringType);
                node.Initialize(graphView, new(positionX, positionY));
            }
        }
        #endregion
    }
}
