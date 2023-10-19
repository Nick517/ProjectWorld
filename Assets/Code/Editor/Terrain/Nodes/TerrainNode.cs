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

        public abstract class SaveData
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

            public abstract void Load(TerrainGraphView graphView);
        }
        #endregion
    }
}
