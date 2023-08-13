using Unity.Entities;
using UnityEngine;

namespace Terrain
{
    [AddComponentMenu("Custom Authoring/World Data Authoring")]
    public class WorldDataAuthoring : MonoBehaviour
    {
        public int seed = -1;
    }

    public class WorldAuthoringBaker : Baker<WorldDataAuthoring>
    {
        public override void Bake(WorldDataAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            int seed = authoring.seed == -1 ? Random.Range(0, int.MaxValue/1000) : authoring.seed;

            AddComponent<WorldDataComponent>(entity, new() { seed = seed });
        }
    }
}
