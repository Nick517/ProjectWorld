using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace FlyCamera
{
    public class FollowFlyCamera : MonoBehaviour
    {
        private EntityManager _entityManager;

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void Update()
        {
            NativeArray<Entity> entities = _entityManager.GetAllEntities(Allocator.Temp);

            foreach (Entity entity in entities.Where(entity => _entityManager.HasComponent<FlyCameraSettingsComponent>(entity)))
            {
                FlyCameraAspect flyCameraAspect = _entityManager.GetAspect<FlyCameraAspect>(entity);
                gameObject.transform.SetPositionAndRotation(flyCameraAspect.LocalTransform.Position, flyCameraAspect.LocalTransform.Rotation);
                break;
            }

            entities.Dispose();
        }
    }
}
