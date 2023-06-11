public class CubeMap
{
    public float[,,] map;

    public CubeMap(int size)
    {
        size++;
        map = new float[size, size, size];
    }
}
