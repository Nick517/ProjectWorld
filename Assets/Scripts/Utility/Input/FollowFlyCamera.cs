using ECS.Components.Input;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Utility.Input
{
    public class FollowFlyCamera : MonoBehaviour
    {
        private EntityManager _entityManager;
        private EntityQuery _entityQuery;

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _entityQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<FlyCamera>());
        }

        private void Update()
        {
            var flyCamTransform = _entityManager.GetComponentData<LocalTransform>(_entityQuery.GetSingletonEntity());
            transform.SetPositionAndRotation(flyCamTransform.Position, flyCamTransform.Rotation);
        }
    }
}