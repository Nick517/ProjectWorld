using ECS.Components.Input;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring.Input
{
    [AddComponentMenu("Custom Authoring/Player Authoring")]
    public class PlayerSettingsAuthoring : MonoBehaviour
    {
        public float interactionRange = 50;
        public GameObject throwObjectPrefab;
        public float throwForce = 10;
    }

    public class PlayerSettingsBaker : Baker<PlayerSettingsAuthoring>
    {
        public override void Bake(PlayerSettingsAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerSettings
            {
                InteractionRange = authoring.interactionRange,
                ThrowObjectPrefab = GetEntity(authoring.throwObjectPrefab, TransformUsageFlags.Dynamic),
                ThrowForce = authoring.throwForce
            });
        }
    }
}