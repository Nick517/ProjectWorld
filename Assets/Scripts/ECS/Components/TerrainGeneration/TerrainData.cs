using DataTypes.Trees;
using Unity.Entities;

namespace ECS.Components.TerrainGeneration
{
    public struct TerrainData : IComponentData
    {
        public ArrayOctree<float> Maps;
        public Octree<Entity> Segments;
    }
}