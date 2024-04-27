using ECS.Aspects;
using ECS.Components;
using Unity.Mathematics;

public static class ChunkOperations
{
    public static float GetCubeSize(ChunkGenerationSettingsComponent chunkGenerationSettings, float chunkScale)
    {
        return math.pow(2, chunkScale) * chunkGenerationSettings.BaseCubeSize;
    }

    public static float GetChunkSize(ChunkGenerationSettingsComponent chunkGenerationSettings, float chunkScale)
    {
        return GetCubeSize(chunkGenerationSettings, chunkScale) * chunkGenerationSettings.CubeCount;
    }

    public static float3 GetClosestChunkPosition(ChunkGenerationSettingsComponent chunkGenerationSettings,
        ChunkAspect.Data data)
    {
        var chunkSize = GetChunkSize(chunkGenerationSettings, data.ChunkScale);
        var position = data.Position;

        var x = math.floor(position.x / chunkSize);
        var y = math.floor(position.y / chunkSize);
        var z = math.floor(position.z / chunkSize);

        var chunkPosition = new float3(x, y, z);

        return chunkPosition * chunkSize;
    }

    public static float3 GetClosestChunkCenter(ChunkGenerationSettingsComponent chunkGenerationSettings,
        ChunkAspect.Data data)
    {
        var chunkSize = GetChunkSize(chunkGenerationSettings, data.ChunkScale);
        var chunkSize3D = new float3(chunkSize, chunkSize, chunkSize);

        var chunkCenter = data.Position + chunkSize3D / 2;

        return chunkCenter;
    }
}