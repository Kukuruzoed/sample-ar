#if UNITY_EDITOR
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#elif UNITY_2017_1_OR_NEWER
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

namespace UnityHcap.Scripts
{
    [ScriptedImporter(1, "hcap")]
    public class HcapImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            TextAsset subAsset = new TextAsset(System.IO.File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("text", subAsset);
            ctx.SetMainObject(subAsset);
        }
    }
}
#endif