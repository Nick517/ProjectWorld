using DataTypes.Trees;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static Utility.SpacialPartitioning.SegmentOperations;
using TerrainData = ECS.Components.TerrainGeneration.TerrainData;

namespace Debugging.TerrainGeneration
{
    public class DrawSegmentOutline : MonoBehaviour
    {
        public bool drawRenderer = true;
        public uint minScale;
        public uint maxScale = 9;

        public Color[] colors =
        {
            Color.black,
            Color.brown,
            Color.red,
            Color.orange,
            Color.yellow,
            Color.green,
            Color.blue,
            Color.violet,
            Color.gray,
            Color.white
        };

        public void OnDrawGizmos()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = entityManager.CreateEntityQuery(typeof(TerrainData));
            if (!query.TryGetSingleton<TerrainData>(out var terrainData)) return;

            var octree = drawRenderer ? terrainData.RendererSegs : terrainData.ColliderSegs;

            octree.Traverse(new DrawSegments
            {
                MinScale = minScale,
                MaxScale = maxScale,
                Colors = colors
            });
        }

        private struct DrawSegments : Octree<Entity>.ITraverseAction
        {
            public uint MinScale;
            public uint MaxScale;
            public Color[] Colors;

            public void Execute(in Octree<Entity> octree, in Octree<Entity>.Node node)
            {
                if (node.Value == default) return;
                if (node.Scale < MinScale || node.Scale > MaxScale) return;

                var size = GetSegSize(octree.BaseNodeSize, node.Scale);
                var center = GetClosestSegCenter(node.Position, size);

                Gizmos.color = Colors[node.Scale];
                Gizmos.DrawWireCube(center, (float3)size);
            }
        }
    }
}