using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainGraphView : GraphView
{
    public TerrainGraphView()
    {
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        style.backgroundColor = new Color(0.125f, 0.125f, 0.125f, 1.0f);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        Vector2 mousePosition = evt.localMousePosition;

        if (evt.target is GraphView)
        {
            evt.menu.AppendAction("Create Node", (e) => { AddElement(CreateTerrainNode(viewTransform.matrix.inverse.MultiplyPoint(mousePosition))); });
            evt.menu.AppendSeparator();
        }

        base.BuildContextualMenu(evt);
    }

    private Port GeneratePort(TerrainNode node, Direction direction, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(float));
    }

    public TerrainNode CreateTerrainNode(Vector2 nodePosition)
    {
        TerrainNode node = new()
        {
            title = "Node",
            GUID = Guid.NewGuid().ToString(),
        };

        node.SetPosition(new Rect(nodePosition.x, nodePosition.y, 100, 150));

        return node;
    }
}
