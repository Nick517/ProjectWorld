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
        public Octree<FixedString32Bytes> Octree;

        public void OnDrawGizmos()
        {
            if (!Octree.IsCreated) return;

            Gizmos.color = Color.white;
            Handles.color = Color.white;

            Octree.Traverse(node =>
            {
                var size = GetSegSize(Octree.BaseNodeSize, node.Scale);
                var center = GetClosestSegCenter(node.Position, size);

                Gizmos.DrawWireCube(center, (float3)size);
                Handles.Label(center, node.Value.ToString());
            });
        }
    }
}