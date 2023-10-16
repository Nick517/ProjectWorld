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

        public abstract TerrainNodeSaveData GetSaveData();

        public class TerrainNodeSaveData
        {
            public string GUID;

            public string type;

            public float positionX;
            public float positionY;

            public TerrainNodeSaveData(TerrainNode terrainNode)
            {
                GUID = terrainNode.GUID.ToString();

                type = terrainNode.GetType().ToString();

                float2 position = terrainNode.GetPosition().position;
                positionX = position.x;
                positionY = position.y;
            }
        }
    }
}
