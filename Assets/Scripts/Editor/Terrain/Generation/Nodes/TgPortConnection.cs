using System;
using System.Collections;
using System.Collections.Generic;
using Editor.Terrain.Generation;
using Editor.Terrain.Generation.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TgPortConnection
{
    public TgPort inputPort;
    public TgPort outputPort;

    public TgPortConnection(TgPort inputPort, TgPort outputPort)
    {
        this.inputPort = inputPort;
        this.outputPort = outputPort;
    }

    public void MakeConnection()
    {
        inputPort.port.ConnectTo(outputPort.port);
    }

    public Dto ToDto()
    {
        return new Dto(this);
    }
    
    [Serializable]
    public class Dto
    {
        public string inputPortId;
        public string outputPortId;

        public Dto(TgPortConnection nodeConnection)
        {
            inputPortId = nodeConnection.inputPort.id;
            outputPortId = nodeConnection.outputPort.id;
        }

        public TgPortConnection Deserialize(TgGraphView graph)
        {
            TgPort inputPort = graph.GetTgPort(inputPortId);
            TgPort outputPort = graph.GetTgPort(outputPortId);

            var connection = new TgPortConnection(inputPort, outputPort);
            
            connection.MakeConnection();

            return connection;
        }
    }
}
