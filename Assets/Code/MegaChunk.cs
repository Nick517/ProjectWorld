using UnityEngine;

public class MegaChunk
{
    private readonly ChunkLoader _chunkLoader;
    private Vector3 _position;
    private readonly int _scale;


    public MegaChunk(ChunkLoader chunkLoader, Vector3 position, int scale)
    {
        _chunkLoader = chunkLoader;
        _position = position;
        _scale = scale;
    }

    public void CreateSubChunks(Vector3 point)
    {

        int subChunkScale = _scale - 1;
        float subChunkSize = _chunkLoader.GetChunkSize(subChunkScale);
        Vector3 pointPosition = _chunkLoader.GetClosestChunkPosition(point, subChunkScale);

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    Vector3 subChunkPosition = new(x, y, z);
                    subChunkPosition *= subChunkSize;
                    subChunkPosition += _position;

                    if (pointPosition == subChunkPosition && subChunkScale > 0)
                    {
                        MegaChunk subChunk = new(_chunkLoader, subChunkPosition, subChunkScale);
                        subChunk.CreateSubChunks(point);
                    }
                    else
                    {
                        _ = _chunkLoader.CreateChunk(subChunkPosition, subChunkScale);
                    }
                }
            }
        }
    }
}
