using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Terrain.Graph
{
    public class TerrainGraphView : GraphView
    {
        private TerrainSearchWindow _searchWindow;

        public TerrainGraphView()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            AddSearchWindow();

            style.backgroundColor = new Color(0.125f, 0.125f, 0.125f, 1.0f);
            SampleNode sampleNode = new();
            sampleNode.Initialize(this, new(0, 0));
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new();

            ports.ForEach(port =>
            {
                // Checks if port is not startPort, if the port's node is not the same as startPort's, and if port does not go in the same direction as startPort
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        public NodePort GetPortFromGUID(string GUID)
        {
            foreach (NodePort nodePort in from Port port in ports
                                     let nodePort = (NodePort)port
                                     where nodePort.GetEdgeGUID() == GUID
                                     select nodePort)
            {
                return nodePort;
            }

            return null;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            Vector2 mousePosition = evt.localMousePosition;

            if (evt.target is GraphView)
            {
                evt.menu.AppendAction("Create Node", (e) => { _ = SearchWindow.Open(new SearchWindowContext(mousePosition), _searchWindow); });
            }

            base.BuildContextualMenu(evt);
        }

        private void AddSearchWindow()
        {
            if (_searchWindow == null)
            {
                _searchWindow = ScriptableObject.CreateInstance<TerrainSearchWindow>();

                _searchWindow.Initialize(this);
            }
        }

        public void ClearGraphView()
        {
            DeleteElements(graphElements.ToList());
        }
    }
}
