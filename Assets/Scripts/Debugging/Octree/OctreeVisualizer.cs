using DataTypes.Trees;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static Utility.SpacialPartitioning.SegmentOperations;

namespace Debugging.Octree
{
    public class OctreeVisualizer : MonoBehaviour
    {
        public Octree<FixedString32Bytes> OctreeA;
        public Octree<FixedString32Bytes> OctreeB;

        [HideInInspector] public bool drawA = true;
        [HideInInspector] public bool drawB = true;
        
        public void OnDrawGizmos()
        {
            if (!OctreeA.IsCreated || !OctreeB.IsCreated) return;

            if (drawA) OctreeA.Traverse(new DrawOctree { Color = Color.white });
            if (drawB) OctreeB.Traverse(new DrawOctree { Color = Color.yellow });
        }

        private struct DrawOctree : Octree<FixedString32Bytes>.ITraverseAction
        {
            public Color Color;

            public void Execute(in Octree<FixedString32Bytes> octree, in Octree<FixedString32Bytes>.Node node)
            {
                var size = GetSegSize(octree.BaseNodeSize, node.Scale);
                var center = GetClosestSegCenter(node.Position, size);

                Gizmos.color = Color;
                Gizmos.DrawWireCube(center, (float3)size);
                Handles.Label(center, node.Value.ToString());
            }
        }
    }
}