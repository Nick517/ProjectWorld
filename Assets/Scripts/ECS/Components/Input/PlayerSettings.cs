using Unity.Entities;

namespace ECS.Components.Input
{
    public struct PlayerSettings : IComponentData
    {
        public Entity ThrowObjectPrefab;
        public float ThrowForce;
    }
}