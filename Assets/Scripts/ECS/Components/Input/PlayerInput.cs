using Unity.Entities;

namespace ECS.Components.Input
{
    public struct PlayerInput : IComponentData
    {
        public bool RemoveTerrain;
        public bool AddTerrain;
        public bool ThrowObject;
    }
}