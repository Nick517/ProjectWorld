using ECS.Components.Input;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring.Input
{
    [AddComponentMenu("Custom Authoring/Player Authoring")]
    public class PlayerSettingsAuthoring : MonoBehaviour
    {
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
                ThrowObjectPrefab = GetEntity(authoring.throwObjectPrefab, TransformUsageFlags.Dynamic),
                ThrowForce = authoring.throwForce
            });
        }
    }
}