using DataTypes.Trees;
using Unity.Burst;
using Unity.Collections;
using Unity.Logging;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Debugging.Octree
{
    [CustomEditor(typeof(OctreeVisualizer))]
    [BurstCompile]
    public class OctreeVisualizerEditor : Editor
    {
        private const float BaseNodeSize = 1;
        private const float HandleSize = 0.2f;

        private OctreeVisualizer _visualizer;
        private float3 _position;
        private FixedString32Bytes _value = "";
        private int _scale;

        private void OnEnable()
        {
            _visualizer = (OctreeVisualizer)target;
        }

        private void InitializeOctrees()
        {
            if (!_visualizer.OctreeA.IsCreated)
            {
                _visualizer.OctreeA = new Octree<FixedString32Bytes>(BaseNodeSize, Allocator.Persistent);
                _visualizer.OctreeB = new Octree<FixedString32Bytes>(BaseNodeSize, Allocator.Persistent);

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

        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
                CleanupOctrees();
        }

        public override void OnInspectorGUI()
        {
            InitializeOctrees();
            DrawDefaultInspector();

            ref var octreeA = ref _visualizer.OctreeA;
            ref var octreeB = ref _visualizer.OctreeB;

            var buttonWidth = EditorGUIUtility.currentViewWidth / 2 - 12;
            var options = new[] { GUILayout.Width(buttonWidth) };

            _value = EditorGUILayout.TextField("Value", _value.ToString());

            EditorGUI.BeginChangeCheck();
            _position = EditorGUILayout.Vector3Field("Position", _position);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

            _scale = EditorGUILayout.IntField("Scale", _scale);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set A", options)) octreeA.SetAtPos(_value, _position, _scale);
            if (GUILayout.Button("Set B", options)) octreeB.SetAtPos(_value, _position, _scale);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear A", options)) octreeA.Clear();
            if (GUILayout.Button("Clear B", options)) octreeB.Clear();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Print Octree A", options)) PrintOctreeState(octreeA, "Octree A");
            if (GUILayout.Button("Print Octree B", options)) PrintOctreeState(octreeB, "Octree B");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Print Node A", options)) PrintNodeState(octreeA, "Octree A");
            if (GUILayout.Button("Print Node B", options)) PrintNodeState(octreeB, "Octree B");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy A", options)) octreeA.Copy(octreeB);
            if (GUILayout.Button("Copy B", options)) octreeB.Copy(octreeA);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Union A", options)) octreeA.Union(octreeB);
            if (GUILayout.Button("Union B", options)) octreeB.Union(octreeA);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Except A", options)) octreeA.Except(octreeB);
            if (GUILayout.Button("Except B", options)) octreeB.Except(octreeA);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Intersect A", options)) octreeA.Intersect(octreeB);
            if (GUILayout.Button("Intersect B", options)) octreeB.Intersect(octreeA);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Subdivide A", options)) SubdivideNode(octreeA, "Octree A");
            if (GUILayout.Button("Subdivide B", options)) SubdivideNode(octreeB, "Octree B");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            _visualizer.drawA = GUILayout.Toggle(_visualizer.drawA, "Draw A");
            _visualizer.drawB = GUILayout.Toggle(_visualizer.drawB, "Draw B");
            EditorGUILayout.EndHorizontal();

            if (GUI.changed) SceneView.RepaintAll();
        }

        private void PrintOctreeState(in Octree<FixedString32Bytes> octree, string octreeName)
        {
            var result = $"{octreeName}: {octree.ToString()}\n";

            for (var i = 0; i < octree.Count; i++) result += $"{i}: {octree.Nodes[i].ToString()}\n";

            Log.Debug(result);
        }

        private void PrintNodeState(in Octree<FixedString32Bytes> octree, string octreeName)
        {
            var result = $"{octreeName}: ";
            var index = octree.GetIndexAtPos(_position, _scale);

            if (index == -1) result += NoNodeMessage();
            else result += $"{index}: {octree.Nodes[index].ToString()}\n";

            Log.Debug(result);
        }

        private void SubdivideNode(Octree<FixedString32Bytes> octree, string octreeName)
        {
            var index = octree.GetIndexAtPos(_position, _scale);

            if (index == -1) Log.Debug($"{octreeName}: {NoNodeMessage()}");
            else octree.Subdivide(index);
        }

        private string NoNodeMessage()
        {
            var posInfo = $"pos=({_position.x:F2}, {_position.y:F2}, {_position.z:F2})";
            var scaleInfo = $"scale={_scale}";
            return $"No node at {posInfo}, {scaleInfo}";
        }

        private void OnSceneGUI()
        {
            if (!_visualizer.OctreeA.IsCreated) return;

            Handles.color = Color.white;
            var handleSize = HandleUtility.GetHandleSize(_position) * HandleSize;

            EditorGUI.BeginChangeCheck();
            _position = Handles.FreeMoveHandle(_position, handleSize, float3.zero, Handles.SphereHandleCap);

            Repaint();
        }

        private void OnDestroy()
        {
            CleanupOctrees();
        }
    }
}