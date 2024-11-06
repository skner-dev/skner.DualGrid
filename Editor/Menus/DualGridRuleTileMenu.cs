using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                List<Sprite> sprites = GetSpritesFromTexture(selectedTexture);

                newRuleTile.m_DefaultSprite = sprites.FirstOrDefault();
                sprites.ForEach(sprite => AddTileRuleFromSprite(newRuleTile, sprite));

                bool isTextureSlicedIn16Pieces = sprites.Count == 16;

                if (isTextureSlicedIn16Pieces)
                {
                    bool shouldAutoSlice = EditorUtility.DisplayDialog("16x Sliced Texture Detected",
                        "The selected texture is sliced in 16 pieces. Perform automatic rule tiling?", "Yes", "No");

                    if(shouldAutoSlice) 
                        AutoDualGridRuleTileProvider.ApplyConfigurationPreset(ref newRuleTile);
                }
            }

            string activeAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string assetName = isSelectedObjectTexture2d ? selectedTexture.name + "_DualGridRuleTile.asset" : "DualGridRuleTile.asset";
            string assetPath = Path.Combine(AssetDatabase.IsValidFolder(activeAssetPath) ? activeAssetPath : Path.GetDirectoryName(activeAssetPath), assetName);

            AssetDatabase.CreateAsset(newRuleTile, AssetDatabase.GenerateUniqueAssetPath(assetPath));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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

        private static List<Sprite> GetSpritesFromTexture(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToList();
        }

        private static void AddTileRuleFromSprite(DualGridRuleTile tile, Sprite sprite)
        {
            tile.m_TilingRules.Add(new DualGridRuleTile.TilingRule() { m_Sprites = new Sprite[] { sprite } });
        }

    }
}
