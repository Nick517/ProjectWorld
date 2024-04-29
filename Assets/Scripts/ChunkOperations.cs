using ECS.Aspects;
using ECS.Components;
using Unity.Mathematics;

public static class ChunkOperations
{
    public static float3 GetCubeSize(ChunkGenerationSettings settings, float chunkScale)
    {
        return math.pow(2, chunkScale) * settings.BaseCubeSize;
    }

    public static float3 GetChunkSize(ChunkGenerationSettings settings, float chunkScale)
    {
        return GetCubeSize(settings, chunkScale) * settings.CubeCount;
    }

    public static float3 GetClosestChunkPosition(ChunkGenerationSettings settings, ChunkAspect.Data data)
    {
        var chunkSize = GetChunkSize(settings, data.ChunkScale);

        return math.floor(data.Position / chunkSize) * chunkSize;
    }

    public static float3 GetClosestChunkCenter(ChunkGenerationSettings settings, ChunkAspect.Data data)
    {
        return data.Position + GetChunkSize(settings, data.ChunkScale) / 2;
    }
}