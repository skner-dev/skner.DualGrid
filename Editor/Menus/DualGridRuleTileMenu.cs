using skner.DualGrid.Editor.Extensions;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace skner.DualGrid.Editor
{
    public static class DualGridRuleTileMenu
    {

        [MenuItem("Assets/Create/2D/Tiles/Dual Grid Rule Tile", false, 50)]
        private static void CreateDualGridRuleTile()
        {
            bool isSelectedObjectTexture2d = TryGetSelectedTexture2D(out Texture2D selectedTexture);

            DualGridRuleTile newRuleTile = ScriptableObject.CreateInstance<DualGridRuleTile>();

            if (isSelectedObjectTexture2d)
            {
                bool wasTextureApplied = newRuleTile.TryApplyTexture2D(selectedTexture);
                if (!wasTextureApplied) return;
            }

            string activeAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string assetName = isSelectedObjectTexture2d ? selectedTexture.name + "_DualGridRuleTile.asset" : "DualGridRuleTile.asset";
            string assetPath = Path.Combine(AssetDatabase.IsValidFolder(activeAssetPath) ? activeAssetPath : Path.GetDirectoryName(activeAssetPath), assetName);

            AssetDatabase.CreateAsset(newRuleTile, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = newRuleTile;
        }

        private static bool TryGetSelectedTexture2D(out Texture2D selectedTexture2d)
        {
            if (Selection.activeObject is Texture2D texture2d)
            {
                selectedTexture2d = texture2d;
                return true;
            }
            else
            {
                selectedTexture2d = null;
                return false;
            }
        }

    }
}
