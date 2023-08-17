using Unity.Entities;

namespace Terrain
{
    public struct TerrainGenerationLayerBufferElement : IBufferElementData
    {
        public float amplitude;
        public float frequency;
    }
}
