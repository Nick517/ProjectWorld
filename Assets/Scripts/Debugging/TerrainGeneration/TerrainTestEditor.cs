using Unity.Burst;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using TerrainData = ECS.Components.TerrainGeneration.TerrainData;

namespace Debugging.TerrainGeneration
{
    [CustomEditor(typeof(TerrainTest))]
    [BurstCompile]
    public class TerrainTestEditor : Editor
    {
        private const float HandleSize = 0.2f;

        private static readonly Color HandleColor = Color.white;

        private TerrainTest _target;
        private float3 _position;
        private int _scale;
        private TerrainData _terrainData;

        private void OnEnable()
        {
            _target = (TerrainTest)target;
        }

        private void Initialize()
        {
            if (_target.World == null) _target.Initialize();
        }

        [BurstCompile]
        public override void OnInspectorGUI()
        {
            Initialize();

            DrawDefaultInspector();

            var buttonWidth = EditorGUIUtility.currentViewWidth / 2 - 12;
            var options = new[] { GUILayout.Width(buttonWidth) };

            EditorGUI.BeginChangeCheck();
            _position = EditorGUILayout.Vector3Field("Position", _position);
            _scale = EditorGUILayout.IntField("Scale", _scale);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Segment", options)) _target.CreateSegment(_position, _scale);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Clear", options)) _target.Dispose();
        }

        [BurstCompile]
        private void OnSceneGUI()
        {
            Handles.color = HandleColor;
            var handleSize = HandleUtility.GetHandleSize(_position) * HandleSize;

            EditorGUI.BeginChangeCheck();
            _position = Handles.FreeMoveHandle(_position, handleSize, float3.zero, Handles.SphereHandleCap);

            Repaint();
        }
    }
}