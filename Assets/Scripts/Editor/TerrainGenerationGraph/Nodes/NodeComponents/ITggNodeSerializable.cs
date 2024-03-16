namespace Editor.TerrainGenerationGraph.Nodes.NodeComponents
{
    public interface ITggNodeSerializable
    {
        public TggNode.Dto ToDto();
    }
}