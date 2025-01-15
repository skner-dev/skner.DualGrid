using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace skner.DualGrid.Editor.Extensions
{
    public static class DualGridRuleTileExtensions
    {

        /// <summary>
        /// Applies the provided <paramref name="texture"/> to the <paramref name="dualGridRuleTile"/>.
        /// <para></para>
        /// If the texture is split in 16x sprites, an automatic rule tiling prompt will follow.
        /// <para></para>
        /// Otherwise, the texture is incompatible and will not be applied, displaying a warning popup.
        /// </summary>
        /// <param name="dualGridRuleTile"></param>
        /// <param name="texture"></param>
        /// <param name="ignoreAutoSlicePrompt"></param>
        /// <returns><see langword="true"/> if the texture was applied, <see langword="false"/> otherwise.</returns>
        public static bool TryApplyTexture2D(this DualGridRuleTile dualGridRuleTile, Texture2D texture, bool ignoreAutoSlicePrompt = false)
        {
            List<Sprite> sprites = texture.GetSplitSpritesFromTexture().OrderBy(sprite =>
            {
                var exception = new InvalidOperationException($"Cannot perform automatic tiling because sprite name '{sprite.name}' is not standardized. It must end with a '_' and a number. Example: 'tile_9'");

                var spriteNumberString = sprite.name.Split("_").LastOrDefault() ?? throw exception;
                bool wasParseSuccessful = int.TryParse(spriteNumberString, out int spriteNumber);

                if (wasParseSuccessful) return spriteNumber;
                else throw exception;
            }).ToList();

            bool isTextureSlicedIn16Pieces = sprites.Count == 16;

            if (isTextureSlicedIn16Pieces)
            {
                bool shouldAutoSlice = ignoreAutoSlicePrompt || EditorUtility.DisplayDialog("16x Sliced Texture Detected",
                    "The selected texture is sliced in 16 pieces. Perform automatic rule tiling?", "Yes", "No");

                dualGridRuleTile.OriginalTexture = texture;
                ApplySprites(ref dualGridRuleTile, sprites);

                if (shouldAutoSlice)
                    AutoDualGridRuleTileProvider.ApplyConfigurationPreset(ref dualGridRuleTile);

                return true;
            }
            else
            {
                EditorUtility.DisplayDialog($"{dualGridRuleTile.name} - Incompatible Texture Detected", "The selected texture is not sliced in 16 pieces.\nTexture will not be applied.", "Ok");
                return false;
            }
        }

        private static void ApplySprites(ref DualGridRuleTile dualGridRuleTile, List<Sprite> sprites)
        {
            dualGridRuleTile.m_DefaultSprite = sprites.FirstOrDefault();
            dualGridRuleTile.m_TilingRules.Clear();

            foreach (Sprite sprite in sprites)
            {
                AddNewTilingRuleFromSprite(ref dualGridRuleTile, sprite);
            }
        }

        private static void AddNewTilingRuleFromSprite(ref DualGridRuleTile tile, Sprite sprite)
        {
            tile.m_TilingRules.Add(new DualGridRuleTile.TilingRule() { m_Sprites = new Sprite[] { sprite }, m_ColliderType = UnityEngine.Tilemaps.Tile.ColliderType.None });
        }

        /// <summary>
        /// Returns a sorted list of <see cref="Sprite"/>s from a provided <paramref name="texture"/>.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static List<Sprite> GetSplitSpritesFromTexture(this Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToList();
        }

    }
}
