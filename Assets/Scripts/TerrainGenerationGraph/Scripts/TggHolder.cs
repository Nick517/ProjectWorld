using UnityEngine;

namespace TerrainGenerationGraph.Scripts
{
    public class TggHolder : MonoBehaviour
    {
        public TgGraph tgGraph;

        private void Start()
        {
            tgGraph.Initialize();
            Traverse();
        }

        private void Traverse()
        {
            var value = tgGraph.GetSample();
            Debug.Log($"Return of traversal: {value}");
        }
    }
}