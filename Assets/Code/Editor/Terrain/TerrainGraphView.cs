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
    }
}
