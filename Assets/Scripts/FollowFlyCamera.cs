using System.Linq;
using ECS.Aspects;
using ECS.Components;
using Unity.Entities;
using UnityEngine;

public class FollowFlyCamera : MonoBehaviour
{
    private EntityManager _entityManager;

    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Update()
    {
        var entities = _entityManager.GetAllEntities();

        foreach (var entity in entities.Where(entity => _entityManager.HasComponent<FlyCameraSettings>(entity)))
        {
            var flyCameraAspect = _entityManager.GetAspect<FlyCameraAspect>(entity);
            gameObject.transform.SetPositionAndRotation
                (flyCameraAspect.LocalTransform.Position, flyCameraAspect.LocalTransform.Rotation);
            break;
        }

        entities.Dispose();
    }
}