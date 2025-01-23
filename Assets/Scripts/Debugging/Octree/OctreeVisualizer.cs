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

        public void OnDrawGizmos()
        {
            if (!OctreeA.IsCreated || !OctreeB.IsCreated) return;

            Gizmos.color = Color.white;
            
            OctreeA.Traverse(node =>
            {
                var size = GetSegSize(OctreeA.BaseNodeSize, node.Scale);
                var center = GetClosestSegCenter(node.Position, size);

                Gizmos.DrawWireCube(center, (float3)size);
                Handles.Label(center, node.Value.ToString());
            });

            Gizmos.color = Color.yellow;

            OctreeB.Traverse(node =>
            {
                var size = GetSegSize(OctreeA.BaseNodeSize, node.Scale);
                var center = GetClosestSegCenter(node.Position, size);

                Gizmos.DrawWireCube(center, (float3)size);
                Handles.Label(center, node.Value.ToString());
            });
        }
    }
}