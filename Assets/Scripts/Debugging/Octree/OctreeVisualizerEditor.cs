using System.Text;
using DataTypes.Trees;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace Debugging.Octree
{
    [CustomEditor(typeof(OctreeVisualizer))]
    [BurstCompile]
    public class OctreeVisualizerEditor : Editor
    {
        private const float HandleSize = 0.2f;

        private float _baseNodeSize = 8;
        private float3 _position;
        private FixedString32Bytes _value = "";
        private int _scale;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var visualizer = (OctreeVisualizer)target;

            _baseNodeSize = EditorGUILayout.FloatField("Base Node Size", _baseNodeSize);
            _value = EditorGUILayout.TextField("Value", _value.ToString());

            EditorGUI.BeginChangeCheck();
            _position = EditorGUILayout.Vector3Field("Position", _position);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

            _scale = EditorGUILayout.IntField("Scale", _scale);

            if (GUILayout.Button("Set"))
            {
                visualizer.Octree.SetAtPos(_value, _position, _scale);

                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Print"))
            {
                var builder = new StringBuilder($"Nodes in octree: {visualizer.Octree.Count}");
                builder.Append("\nRoot indexes: ");
                builder.AppendJoin(", ", visualizer.Octree.RootIndexes);

                for (var n = 0; n < visualizer.Octree.Count; n++)
                {
                    var node = visualizer.Octree.Nodes[n];
                    var p = node.Position;
                    var size = GetSegSize(_baseNodeSize, node.Scale);
                    var nodeBuilder =
                        new StringBuilder(
                            $"\nNode {n}, Position: ({p.x}, {p.y}, {p.z}), Scale: {node.Scale}, Size: {size}, Children: ");

                    if (node.ChildIndexes.IsCreated) nodeBuilder.AppendJoin(", ", node.ChildIndexes);
                    else nodeBuilder.Append("N/A");

                    nodeBuilder.Append($", Value: {node.Value}");
                    builder.Append(nodeBuilder.ToString());
                }

                Debug.Log(builder.ToString());
            }

            if (!GUILayout.Button("Reset")) return;

            visualizer.Octree.Dispose();
            visualizer.Octree = new Octree<FixedString32Bytes>(_baseNodeSize, Allocator.Persistent);
            _position = float3.zero;

            SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            Handles.color = Color.white;
            var handleSize = HandleUtility.GetHandleSize(_position) * HandleSize;

            EditorGUI.BeginChangeCheck();
            var newPosition = Handles.FreeMoveHandle(_position, handleSize, float3.zero, Handles.SphereHandleCap);
            if (!EditorGUI.EndChangeCheck()) return;

            _position = newPosition;

            Repaint();
        }
    }
}