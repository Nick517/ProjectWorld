using System;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Terrain.Graph
{
    public abstract class TerrainNode : Node
    {
        #region Variables
        public GraphView graphView;

        public string GUID;
        #endregion

        #region Methods
        public void Initialize(GraphView graphView, Vector2 position)
        {
            this.graphView = graphView;
            GUID = UnityEditor.GUID.Generate().ToString();
            SetPosition(position);
            Draw();
            graphView.AddElement(this);
        }

        public void Initialize(GraphView graphView, Vector2 position, string GUID)
        {
            this.graphView = graphView;
            this.GUID = GUID;
            SetPosition(position);
            Draw();
            graphView.AddElement(this);
        }

        public abstract void Draw();
        #endregion

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
                GUID = GUID.ToString();

                float2 position = terrainNode.GetPosition().position;
                positionX = position.x;
                positionY = position.y;
            }

            public abstract void Load(TerrainGraphView graphView);

            public abstract void LoadConnections(TerrainNode terrainNode, TerrainGraphView graphView);
        }
        #endregion

        #region Utility
        public void SetPosition(Vector2 position)
        {
            Vector2 graphPos = GraphUtil.GetGraphPosition(graphView, position);

            Rect nodePos = GetPosition();
            Rect newPos = new(graphPos.x, graphPos.y, nodePos.width, nodePos.height);

            base.SetPosition(newPos);
        }

        public void SetDimensions(float width, float height)
        {
            Rect nodePos = GetPosition();
            Rect newPos = new(nodePos.x, nodePos.y, width, height);

            SetPosition(newPos);
        }

        public NodePort AddInputPort(string portName, Type type)
        {
            NodePort port = new(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, type)
            {
                portName = portName
            };

            Add(port);

            return port;
        }

        public NodePort AddOutputPort(string portName, Type type)
        {
            NodePort port = new(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, type)
            {
                portName = portName
            };
            
            Add(port);

            return port;
        }
        #endregion
    }
}

/*
 * <<<FIX>>>
 * Cannot grab edges. May be caused from ports actually being NodePorts.
 */