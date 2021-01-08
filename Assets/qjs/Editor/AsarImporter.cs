using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace qjs
{
    [ScriptedImporter(1, "asar")]
    public class AsarImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = ScriptableObject.CreateInstance<AsarAsset>();
            asset.bytes = File.ReadAllBytes(ctx.assetPath);
            ctx.AddObjectToAsset("main asset", asset);
        }
    }
}
