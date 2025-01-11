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
        private OctreeVisualizer _visualizer;

        private float _baseNodeSize = 8;
        private float3 _position;
        private FixedString32Bytes _value = "";
        private int _scale;

        private void OnEnable()
        {
            _visualizer = (OctreeVisualizer)target;
        }

        private void InitializeOctree()
        {
            if (!_visualizer.Octree.IsCreated)
            {
                _visualizer.Octree = new Octree<FixedString32Bytes>(_baseNodeSize, Allocator.Persistent);

                AssemblyReloadEvents.beforeAssemblyReload += CleanupOctree;
                EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            }
        }

        private void CleanupOctree()
        {
            _visualizer.Octree.Dispose();

            AssemblyReloadEvents.beforeAssemblyReload -= CleanupOctree;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        }

        private void ReinitializeOctree()
        {
            CleanupOctree();
            InitializeOctree();
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
                CleanupOctree();
        }

        public override void OnInspectorGUI()
        {
            InitializeOctree();

            DrawDefaultInspector();

            EditorGUI.BeginChangeCheck();
            var newBaseNodeSize = EditorGUILayout.FloatField("Base Node Size", _baseNodeSize);
            if (EditorGUI.EndChangeCheck() && !newBaseNodeSize.Equals(_baseNodeSize))
            {
                _baseNodeSize = newBaseNodeSize;
                ReinitializeOctree();
            }

            _value = EditorGUILayout.TextField("Value", _value.ToString());

            EditorGUI.BeginChangeCheck();
            _position = EditorGUILayout.Vector3Field("Position", _position);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

            _scale = EditorGUILayout.IntField("Scale", _scale);

            using (new EditorGUI.DisabledScope(!_visualizer.Octree.IsCreated))
            {
                if (GUILayout.Button("Set"))
                {
                    _visualizer.Octree.SetAtPos(_value, _position, _scale);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Print")) PrintOctreeState();

                if (GUILayout.Button("Clear"))
                {
                    _visualizer.Octree.Clear();
                    _position = float3.zero;
                    SceneView.RepaintAll();
                }
            }
        }

        private void PrintOctreeState()
        {
            var builder = new StringBuilder($"Nodes in octree: {_visualizer.Octree.Count}");
            builder.Append("\nRoot indexes: ");
            builder.AppendJoin(", ", _visualizer.Octree.RootIndexes);

            for (var n = 0; n < _visualizer.Octree.Count; n++)
            {
                var node = _visualizer.Octree.Nodes[n];
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
            if (!_visualizer.Octree.IsCreated) return;

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
            CleanupOctree();
        }
    }
}