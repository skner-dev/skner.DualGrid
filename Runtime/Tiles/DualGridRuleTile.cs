using skner.DualGrid.Extensions;
using skner.DualGrid.Utils;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using static skner.DualGrid.DualGridRuleTile;

namespace skner.DualGrid
{
    /// <summary>
    /// The custom <see cref="RuleTile"/> used by the <see cref="DualGridTilemapModule"/> to generate tiles in the Render Tilemap.
    /// </summary>
    /// <remarks>
    /// Avoid using this tile in a palette, as any other data tile can be used.
    /// <para></para>
    /// This tile type will be used in all Render Tilemaps.
    /// </remarks>
    [Serializable]
    [CreateAssetMenu(fileName = "DualGridRuleTile", menuName = "Scriptable Objects/DualGridRuleTile")]
    public class DualGridRuleTile : RuleTile<DualGridNeighbor>
    {

        [SerializeField]
        [HideInInspector]
        private Texture2D _originalTexture;
        public Texture2D OriginalTexture { get => _originalTexture; internal set => _originalTexture = value; }

        private DualGridDataTile _dataTile;
        /// <summary>
        /// The Data Tile is a tile generated from this Dual Grid Rule Tile to populate the DataTilemap.
        /// </summary>
        public DualGridDataTile DataTile { get => _dataTile != null ? _dataTile : RefreshDataTile(); }

        private DualGridTilemapModule _dualGridTilemapModule;

        private Tilemap _dataTilemap;

        public class DualGridNeighbor
        {
            /// <summary>
            /// The Dual Grid Rule Tile will check if the contents of the data tile in that direction is filled.
            /// If not, the rule will fail.
            /// </summary>
            public const int Filled = 1;

            /// <summary>
            /// The Dual Grid Rule Tile will check if the contents of the data tile in that direction is not filled.
            /// If it is, the rule will fail.
            /// </summary>
            public const int NotFilled = 2;
        }

        /// <summary>
        /// Force sets the actual Data Tilemap before updating the tile, because Unity seems to move tiles between tilemaps sometimes.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="tilemap"></param>
        /// <param name="tileData"></param>
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            SetDataTilemap(tilemap);

            var iden = Matrix4x4.identity;

            tileData.sprite = m_DefaultSprite;
            tileData.gameObject = m_DefaultGameObject;
            tileData.colliderType = m_DefaultColliderType;
            tileData.flags = TileFlags.LockTransform;
            tileData.transform = iden;

            bool gameObjectShouldBeInRenderTilemap = _dualGridTilemapModule == null || _dualGridTilemapModule.GameObjectOrigin == GameObjectOrigin.RenderTilemap;
            Matrix4x4 transform = iden;
            foreach (TilingRule rule in m_TilingRules)
            {
                if (RuleMatches(rule, position, tilemap, ref transform))
                {
                    switch (rule.m_Output)
                    {
                        case TilingRuleOutput.OutputSprite.Single:
                        case TilingRuleOutput.OutputSprite.Animation:
                            tileData.sprite = rule.m_Sprites[0];
                            break;
                        case TilingRuleOutput.OutputSprite.Random:
                            int index = Mathf.Clamp(Mathf.FloorToInt(GetPerlinValue(position, rule.m_PerlinScale, 100000f) * rule.m_Sprites.Length), 0, rule.m_Sprites.Length - 1);
                            tileData.sprite = rule.m_Sprites[index];
                            if (rule.m_RandomTransform != TilingRuleOutput.Transform.Fixed)
                                transform = ApplyRandomTransform(rule.m_RandomTransform, transform, rule.m_PerlinScale, position);
                            break;
                    }
                    tileData.transform = transform;
                    tileData.gameObject = gameObjectShouldBeInRenderTilemap ? rule.m_GameObject : null;
                    break;
                }
            }
        }

        /// <summary>
        /// Refreshes the <see cref="DataTile"/> with this <see cref="DualGridRuleTile"/>'s configuration.
        /// </summary>
        /// <returns>The refreshed data tile.</returns>
        public virtual DualGridDataTile RefreshDataTile()
        {
            if (_dataTile == null) _dataTile = ScriptableObject.CreateInstance<DualGridDataTile>();

            _dataTile.name = this.name;
            _dataTile.colliderType = this.m_DefaultColliderType;
            _dataTile.gameObject = this.m_DefaultGameObject;

            return _dataTile;
        }

        /// <inheritdoc/>
        public override bool RuleMatches(TilingRule ruleToValidate, Vector3Int renderTilePosition, ITilemap tilemap, ref Matrix4x4 transform)
        {
            // Skip custom rule validation in cases where this DualGridRuleTile is not within a valid tilemap
            if (GetDataTilemap(tilemap) == null) return false;

            Vector3Int[] dataTilemapPositions = DualGridUtils.GetDataTilePositions(renderTilePosition);

            foreach (Vector3Int dataTilePosition in dataTilemapPositions)
            {
                if (!DoesRuleMatchWithDataTile(ruleToValidate, dataTilePosition, renderTilePosition))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the <paramref name="dataTilePosition"/> is filled in accordance with the defined <paramref name="rule"/>.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="dataTilePosition"></param>
        /// <param name="renderTilePosition"></param>
        /// <returns></returns>
        private bool DoesRuleMatchWithDataTile(TilingRule rule, Vector3Int dataTilePosition, Vector3Int renderTilePosition)
        {
            Vector3Int dataTileOffset = dataTilePosition - renderTilePosition;

            int neighborIndex = rule.GetNeighborIndex(dataTileOffset);
            if (neighborIndex == -1) return true; // If no neighbor is defined, it means it matches with anything.

            var neighborDataTile = _dataTilemap.GetEditorPreviewTile(dataTilePosition);
            if (neighborDataTile == null) neighborDataTile = _dataTilemap.GetTile(dataTilePosition);
            return RuleMatch(rule.m_Neighbors[neighborIndex], neighborDataTile);
        }

        /// <inheritdoc/>
        public override bool RuleMatch(int neighbor, TileBase other)
        {
            bool isEmptyPreviewTile = other is DualGridPreviewTile dualGridPreviewTile && dualGridPreviewTile.IsFilled == false;

            return neighbor switch
            {
                DualGridNeighbor.Filled => !isEmptyPreviewTile && other != null,
                DualGridNeighbor.NotFilled => isEmptyPreviewTile || other == null,
                _ => true,
            };
        }

        /// <summary>
        /// Getter for the data tilemap, which can attempt to set it from the <paramref name="tilemap"/> if the <see cref="_dataTilemap"/> field is <see langword="null"/>.
        /// <para></para>
        /// This is done because in key moments, the <see cref="StartUp"/> method has not yet been called, but the tile is being updated -> Unity messing this up and is not fixable externally.
        /// If the data tilemap would be null, the rule matching will not work properly.
        /// <para></para>
        /// See GitHub issue 5: https://github.com/skner-dev/DualGrid/issues/5.
        /// </summary>
        /// <param name="tilemap"></param>
        /// <returns></returns>
        private Tilemap GetDataTilemap(ITilemap tilemap)
        {
            if (_dualGridTilemapModule == null || _dualGridTilemapModule.DataTilemap == null)
            {
                SetDataTilemap(tilemap);
            }

            return _dataTilemap;
        }

        private void SetDataTilemap(ITilemap tilemap)
        {
            var originTilemap = tilemap.GetComponent<Tilemap>();

            _dualGridTilemapModule = originTilemap.GetComponentInParent<DualGridTilemapModule>();

            if (_dualGridTilemapModule != null)
            {
                _dataTilemap = _dualGridTilemapModule.DataTilemap;
            }
            else
            {
                // This situation can happen in two cases:
                // - When a DualGridRuleTile is used in a tile palette, which can be ignored
                // - When a DualGridRuleTile is used in a tilemap that does not have a DualGridTilemapModule, which is problematic
                // There is no definitive way to distinguish between these two scenarios, so a warning is thrown. (thanks Unity)

                //Debug.LogWarning($"DualGridRuleTile '{name}' detected outside of a {nameof(Tilemap)} that contains a {nameof(DualGridTilemapModule)}. " +
                //    $"If the tilemap is a tile palette, discard this warning, otherwise investigate it, as this tile won't work properly.", originTilemap);
            }
        }

    }
}
