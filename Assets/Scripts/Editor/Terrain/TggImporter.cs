using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Editor.Terrain
{
    [ScriptedImporter(1, "tgg")]
    public class TggImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var desc = new TextAsset("A visual graph for creating custom terrain generation.");

            var fileData = File.ReadAllBytes("Assets/Scripts/Editor/Terrain/Icons/TGG.png");
            var icon = new Texture2D(2, 2);
            icon.LoadImage(fileData);

            ctx.AddObjectToAsset("Terrain", desc, icon);
        }
    }
}