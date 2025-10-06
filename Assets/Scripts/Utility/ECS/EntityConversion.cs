using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Utility.ECS
{
    public static class EntityConversion
    {
        public static Entity ConvertPrefab(GameObject gameObject, EntityManager entityManager)
        {
            var entity = entityManager.CreateEntity();
            var transform = gameObject.transform;

            var lossyScale = (float3)transform.lossyScale;
            var uniformScale = lossyScale.x;

            var pos = transform.localPosition;
            var rot = transform.localRotation;

            entityManager.AddComponentData(entity, LocalTransform.FromPositionRotationScale(pos, rot, uniformScale));

            if (!math.all(lossyScale == new float3(uniformScale)))
                entityManager.AddComponentData(entity, new PostTransformMatrix { Value = float4x4.Scale(lossyScale) });

            var meshFilter = gameObject.GetComponent<MeshFilter>();
            var meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (meshFilter == null || meshRenderer == null) return entity;

            var mesh = meshFilter.sharedMesh;
            var mat = meshRenderer.sharedMaterial;

            var desc = new RenderMeshDescription(
                meshRenderer.shadowCastingMode,
                meshRenderer.receiveShadows,
                meshRenderer.motionVectorGenerationMode,
                meshRenderer.sortingLayerID,
                meshRenderer.renderingLayerMask,
                meshRenderer.lightProbeUsage,
                meshRenderer.staticShadowCaster);

            var renderMeshArray = new RenderMeshArray(new[] { mat }, new[] { mesh });
            var info = MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0);
            
            RenderMeshUtility.AddComponents(entity, entityManager, desc, renderMeshArray, info);

            var aabb = new AABB { Center = mesh.bounds.center, Extents = mesh.bounds.extents };
            entityManager.AddComponentData(entity, new RenderBounds { Value = aabb });
            entityManager.AddComponentData(entity, new WorldRenderBounds { Value = aabb });

            return entity;
        }
    }
}