using Unity.Entities;

namespace ECS.Components.Input
{
    public struct PlayerSettings : IComponentData
    {
        public float InteractionRange;
        public Entity ThrowObjectPrefab;
        public float ThrowForce;
    }
}