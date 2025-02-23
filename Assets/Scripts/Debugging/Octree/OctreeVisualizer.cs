using DataTypes.Trees;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace Debugging.Octree
{
    [BurstCompile]
    public class OctreeVisualizer : MonoBehaviour
    {
        public Octree<FixedString32Bytes> OctreeA;
        public Octree<FixedString32Bytes> OctreeB;

        [HideInInspector] public bool drawA = true;
        [HideInInspector] public bool drawB = true;

        [BurstCompile]
        public void OnDrawGizmos()
        {
            if (!OctreeA.IsCreated || !OctreeB.IsCreated) return;

            if (drawA) OctreeA.Traverse(new DrawOctreeAction { Color = Color.white });

            if (drawB) OctreeB.Traverse(new DrawOctreeAction { Color = Color.yellow });
        }

        [BurstCompile]

        private struct DrawOctreeAction : Octree<FixedString32Bytes>.ITraverseAction
        {
            public Color Color;

            [BurstCompile]
            public void Execute(in Octree<FixedString32Bytes> octree, in Octree<FixedString32Bytes>.Node node)
            {
                Gizmos.color = Color;

                var size = GetSegSize(octree.BaseNodeSize, node.Scale);
                var center = GetClosestSegCenter(node.Position, size);

                Gizmos.DrawWireCube(center, (float3)size);
                Handles.Label(center, node.Value.ToString());
            }
        }
    }
}