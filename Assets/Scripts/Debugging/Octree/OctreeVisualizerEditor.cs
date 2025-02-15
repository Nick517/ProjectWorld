using System.Text;
using DataTypes.Trees;
using Unity.Burst;
using Unity.Collections;
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

            _value = EditorGUILayout.TextField("Value", _value.ToString());

            EditorGUI.BeginChangeCheck();
            _position = EditorGUILayout.Vector3Field("Position", _position);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

            _scale = EditorGUILayout.IntField("Scale", _scale);

            using (new EditorGUI.DisabledScope(!_visualizer.OctreeA.IsCreated || !_visualizer.OctreeB.IsCreated))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set A")) _visualizer.OctreeA.SetAtPos(_value, _position, _scale);
                if (GUILayout.Button("Set B")) _visualizer.OctreeB.SetAtPos(_value, _position, _scale);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Union A")) _visualizer.OctreeA.Union(_visualizer.OctreeB);
                if (GUILayout.Button("Union B")) _visualizer.OctreeB.Union(_visualizer.OctreeA);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Except A")) _visualizer.OctreeA.Except(_visualizer.OctreeB);
                if (GUILayout.Button("Except B")) _visualizer.OctreeB.Except(_visualizer.OctreeA);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Intersect A")) _visualizer.OctreeA.Intersect(_visualizer.OctreeB);
                if (GUILayout.Button("Intersect B")) _visualizer.OctreeB.Intersect(_visualizer.OctreeA);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy A")) _visualizer.OctreeA.Copy(_visualizer.OctreeB);
                if (GUILayout.Button("Copy B")) _visualizer.OctreeB.Copy(_visualizer.OctreeA);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Print A")) PrintOctreeState(_visualizer.OctreeA, "Octree A");
                if (GUILayout.Button("Print B")) PrintOctreeState(_visualizer.OctreeB, "Octree B");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear A")) _visualizer.OctreeA.Clear();
                if (GUILayout.Button("Clear B")) _visualizer.OctreeB.Clear();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                _visualizer.drawA = GUILayout.Toggle(_visualizer.drawA, "Draw A");
                _visualizer.drawB = GUILayout.Toggle(_visualizer.drawB, "Draw B");
                EditorGUILayout.EndHorizontal();

                if (GUI.changed) SceneView.RepaintAll();
            }
        }

        private void PrintOctreeState(Octree<FixedString32Bytes> octree, string octreeName)
        {
            var builder = new StringBuilder($"{octreeName}: {octree.ToString()}");
            builder.AppendLine();

            for (var n = 0; n < octree.Count; n++) builder.AppendLine($"{n}: {octree.Nodes[n].ToString()}");

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