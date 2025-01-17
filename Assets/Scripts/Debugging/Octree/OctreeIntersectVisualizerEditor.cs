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
    [CustomEditor(typeof(OctreeIntersectVisualizer))]
    [BurstCompile]
    public class OctreeIntersectVisualizerEditor : Editor
    {
        private const float HandleSize = 0.2f;
        private OctreeIntersectVisualizer _visualizer;

        private float _baseNodeSize = 8;
        private float3 _position;
        private FixedString32Bytes _value = "";
        private int _scale;

        private void OnEnable()
        {
            _visualizer = (OctreeIntersectVisualizer)target;
        }

        private void InitializeOctrees()
        {
            if (!_visualizer.OctreeA.IsCreated)
            {
                _visualizer.OctreeA = new Octree<FixedString32Bytes>(_baseNodeSize, Allocator.Persistent);
                _visualizer.OctreeB = new Octree<FixedString32Bytes>(_baseNodeSize, Allocator.Persistent);

                AssemblyReloadEvents.beforeAssemblyReload += CleanupOctrees;
                EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            }
        }

        private void CleanupOctrees()
        {
            _visualizer.OctreeA.Dispose();
            _visualizer.OctreeB.Dispose();

            AssemblyReloadEvents.beforeAssemblyReload -= CleanupOctrees;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        private void ReinitializeOctrees()
        {
            CleanupOctrees();
            InitializeOctrees();
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
                CleanupOctrees();
        }

        public override void OnInspectorGUI()
        {
            InitializeOctrees();

            DrawDefaultInspector();

            EditorGUI.BeginChangeCheck();
            var newBaseNodeSize = EditorGUILayout.FloatField("Base Node Size", _baseNodeSize);
            if (EditorGUI.EndChangeCheck() && !newBaseNodeSize.Equals(_baseNodeSize))
            {
                _baseNodeSize = newBaseNodeSize;
                ReinitializeOctrees();
            }

            _value = EditorGUILayout.TextField("Value", _value.ToString());

            EditorGUI.BeginChangeCheck();
            _position = EditorGUILayout.Vector3Field("Position", _position);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

            _scale = EditorGUILayout.IntField("Scale", _scale);

            using (new EditorGUI.DisabledScope(!_visualizer.OctreeA.IsCreated || !_visualizer.OctreeB.IsCreated))
            {
                if (GUILayout.Button("Set A"))
                {
                    _visualizer.OctreeA.SetAtPos(_value, _position, _scale);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Set B"))
                {
                    _visualizer.OctreeB.SetAtPos(_value, _position, _scale);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Intersect"))
                {
                    _visualizer.OctreeA = _visualizer.OctreeA.Intersect(_visualizer.OctreeB, Allocator.Persistent);
                    _visualizer.OctreeB.Clear();
                    SceneView.RepaintAll();
                }
                
                if (GUILayout.Button("Print")) PrintOctreeState();

                if (GUILayout.Button("Clear"))
                {
                    _visualizer.OctreeA.Clear();
                    _visualizer.OctreeB.Clear();
                    _position = float3.zero;
                    SceneView.RepaintAll();
                }
            }
        }

        private void PrintOctreeState()
        {
            var builder = new StringBuilder($"Nodes in Octree A: {_visualizer.OctreeA.Count}");
            builder.Append("\nRoot indexes: ");
            builder.AppendJoin(", ", _visualizer.OctreeA.RootIndexes);

            for (var n = 0; n < _visualizer.OctreeA.Count; n++)
            {
                var node = _visualizer.OctreeA.Nodes[n];
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

        private void OnSceneGUI()
        {
            if (!_visualizer.OctreeA.IsCreated) return;

            Handles.color = Color.white;
            var handleSize = HandleUtility.GetHandleSize(_position) * HandleSize;

            EditorGUI.BeginChangeCheck();
            var newPosition = Handles.FreeMoveHandle(_position, handleSize, float3.zero, Handles.SphereHandleCap);
            if (!EditorGUI.EndChangeCheck()) return;

            _position = newPosition;
            Repaint();
        }

        private void OnDestroy()
        {
            CleanupOctrees();
        }
    }
}