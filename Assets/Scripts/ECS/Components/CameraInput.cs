using Unity.Entities;
using Unity.Mathematics;

namespace ECS.Components
{
    public struct CameraInput : IComponentData
    {
        public float2 LookDelta;
    }
}
