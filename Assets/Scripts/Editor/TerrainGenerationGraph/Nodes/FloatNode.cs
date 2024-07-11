namespace Editor.TerrainGenerationGraph.Nodes
{
    public class FloatNode : TggNode
    {
        protected override void SetUp()
        {
            title = "Float";

            AddInputPort("X");
            AddOutputPort();
        }
    }
}