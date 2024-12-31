using System.Text;
using DataTypes.Trees;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utility.Math;
using static Utility.TerrainGeneration.SegmentOperations;

namespace Debugging
{
    public class OctreeVisualizer : MonoBehaviour
    {
        public Octree<FixedString32Bytes> Octree;

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Handles.color = Color.white;

            for (var r = 0; r < 8; r++)
            {
                if (Octree.RootIndexes[r] == -1) continue;

                var scale = Octree.RootScales[r];
                var size = GetSegSize(Octree.BaseNodeSize, scale);
                var center = GetClosestSegCenter(r.ToBool3().ToSign() * (float3)Octree.BaseNodeSize / 2, size);

                Traverse(Octree.RootIndexes[r], center, size);
            }
        }

        private void Traverse(int index, float3 pos, float size)
        {
            if (index == -1) return;

            var node = Octree.Nodes[index];
            var center = GetClosestSegCenter(pos, size);

            Gizmos.DrawWireCube(center, (float3)size);
            Handles.Label(center, node.Value.ToString());

            if (!node.ChildIndexes.IsCreated) return;

            size /= 2;

            for (var c = 0; c < 8; c++)
                Traverse(node.ChildIndexes[c], pos + (float3)size * c.ToBool3().ToSign() / 2, size);
        }
    }

    [CustomEditor(typeof(OctreeVisualizer))]
    public class OctreeEditor : Editor
    {
        private float3 _position;
        private string _value = "";
        private int _scale;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var visualizer = (OctreeVisualizer)target;

            _value = EditorGUILayout.TextField("Value", _value);

            EditorGUI.BeginChangeCheck();
            _position = EditorGUILayout.Vector3Field("Position", _position);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

            _scale = EditorGUILayout.IntField("Scale", _scale);

            if (GUILayout.Button("Set"))
            {
                visualizer.Octree.SetValueAtPos(_value, _position, _scale);

                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Set to test values"))
            {
                visualizer.Octree.Dispose();
                visualizer.Octree = new Octree<FixedString32Bytes>(8, Allocator.Persistent);

                var ri = visualizer.Octree.AllocNode();
                visualizer.Octree.RootIndexes[7] = ri;
                visualizer.Octree.RootScales[7] = 1;

                var rn = visualizer.Octree.Nodes[ri];
                rn.Alloc(Allocator.Persistent);

                for (var c = 0; c < 8; c++) rn.ChildIndexes[c] = visualizer.Octree.AllocNode();

                visualizer.Octree.Nodes[ri] = rn;

                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Print"))
            {
                var builder = new StringBuilder($"Nodes in octree: {visualizer.Octree.Count}");
                builder.Append("\nRoot indexes: ");
                builder.AppendJoin(", ", visualizer.Octree.RootIndexes);
                builder.Append("\nRoot scales: ");
                builder.AppendJoin(", ", visualizer.Octree.RootScales);

                for (var n = 0; n < visualizer.Octree.Count; n++)
                {
                    var node = visualizer.Octree.Nodes[n];
                    var nodeBuilder = new StringBuilder($"\nNode {n}, Children: ");

                    if (node.ChildIndexes.IsCreated)
                        nodeBuilder.AppendJoin(", ", node.ChildIndexes);
                    else
                        nodeBuilder.Append("N/A");

                    builder.Append(nodeBuilder.ToString());
                }

                Debug.Log(builder.ToString());
            }

            if (!GUILayout.Button("Reset")) return;
            
            visualizer.Octree.Dispose();
            visualizer.Octree = new Octree<FixedString32Bytes>(8, Allocator.Persistent);
            _position = float3.zero;

            SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            Handles.color = Color.white;
            var handleSize = HandleUtility.GetHandleSize(_position) * 0.2f;

            EditorGUI.BeginChangeCheck();
            var newPosition = Handles.FreeMoveHandle(_position, handleSize, float3.zero, Handles.SphereHandleCap);
            if (!EditorGUI.EndChangeCheck()) return;

            _position = newPosition;

            Repaint();
        }
    }
}