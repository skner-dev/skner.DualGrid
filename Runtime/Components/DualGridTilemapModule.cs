using skner.DualGrid.Extensions;
using skner.DualGrid.Utils;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid
{
    [RequireComponent(typeof(Tilemap))]
    [DisallowMultipleComponent]
    /// <summary>
    /// A module to be added that converts a <see cref="Tilemap"/> into a Dual Grid Tilemap.
    /// </summary>
    /// <remarks>
    /// This module only guarantees the continuous update of the render tilemap, for data tilemap changes.
    /// <para></para>
    /// Editor logic is kept in the associated editor script.
    /// </remarks>
    public class DualGridTilemapModule : MonoBehaviour
    {

        [Tooltip("The DualGridRuleTile that will be applied in the Render Tilemap")]
        public DualGridRuleTile Tile;

        private Tilemap _dataTilemap;
        public Tilemap DataTilemap
        {
            get
            {
                if (_dataTilemap == null) _dataTilemap = GetComponent<Tilemap>();
                return _dataTilemap;
            }
        }

        private Tilemap _renderTilemap;
        public Tilemap RenderTilemap
        {
            get
            {
                if (_renderTilemap == null) _renderTilemap = transform.GetComponentInImmediateChildren<Tilemap>();
                return _renderTilemap;
            }
        }

        private void Awake()
        {
            if (_dataTilemap == null) _dataTilemap = GetComponent<Tilemap>();
            if (_renderTilemap == null) _renderTilemap = transform.GetComponentInImmediateChildren<Tilemap>();
        }

        private void OnEnable()
        {
            Tilemap.tilemapTileChanged += HandleTilemapChange;
        }

        private void OnDisable()
        {
            Tilemap.tilemapTileChanged -= HandleTilemapChange;
        }

        /// <summary>
        /// For each updated tile in the <see cref="DataTilemap"/>, update the <see cref="RenderTilemap"/>.
        /// </summary>
        /// <param name="tilemap"></param>
        /// <param name="tileChanges"></param>
        internal void HandleTilemapChange(Tilemap tilemap, Tilemap.SyncTile[] tileChanges)
        {
            if (tilemap == DataTilemap)
            {
                if (Tile == null)
                {
                    Debug.LogError($"Cannot update render tilemap, because tile is not set in dual grid module.", RenderTilemap);
                    return;
                }

                Undo.RecordObject(RenderTilemap, $"Updated {tileChanges.Length} render tile(s)");

                foreach (Tilemap.SyncTile tileChange in tileChanges)
                {
                    UpdateRenderTilemapFromDataTile(tileChange.position);
                }
            }
        }

        /// <summary>
        /// Fully refreshes the <see cref="RenderTilemap"/> by forcing an update from all tiles in the <see cref="DataTilemap"/>.
        /// </summary>
        internal void RefreshRenderTiles()
        {
            if (Tile == null)
            {
                Debug.LogError($"Cannot refresh render tilemap, because tile is not set in dual grid module.", RenderTilemap);
                return;
            }

            Undo.RecordObject(RenderTilemap, "Refreshed render tiles");

            RenderTilemap.ClearAllTiles();
            foreach (var position in DataTilemap.cellBounds.allPositionsWithin)
            {
                if (DataTilemap.HasTile(position)) UpdateRenderTilemapFromDataTile(position);
            }
        }

        private void UpdateRenderTilemapFromDataTile(Vector3Int dataTilePosition)
        {
            bool isTileBeingAdded = DataTilemap.HasTile(dataTilePosition);

            foreach (Vector3Int renderTilePosition in DualGridUtils.GetRenderTilePositions(dataTilePosition))
            {
                if (isTileBeingAdded)
                {
                    SetRenderTile(renderTilePosition);
                }
                else
                {
                    UnsetRenderTile(renderTilePosition);
                }
            }
        }

        private void SetRenderTile(Vector3Int renderTilePosition)
        {
            if (!RenderTilemap.HasTile(renderTilePosition))
            {
                RenderTilemap.SetTile(renderTilePosition, Tile);
            }
            else
            {
                RenderTilemap.RefreshTile(renderTilePosition);
            }
        }

        private void UnsetRenderTile(Vector3Int renderTilePosition)
        {
            if (!IsInUseByDataTilemap(renderTilePosition) && RenderTilemap.HasTile(renderTilePosition))
            {
                RenderTilemap.SetTile(renderTilePosition, null);
            }
            else
            {
                RenderTilemap.RefreshTile(renderTilePosition);
            }
        }

        /// <summary>
        /// Checks if the render tile at <paramref name="renderTilePosition"/> is in use by any data tile.
        /// </summary>
        /// <param name="renderTilePosition"></param>
        /// <returns></returns>
        private bool IsInUseByDataTilemap(Vector3Int renderTilePosition)
        {
            foreach (Vector3Int dataTilePosition in DualGridUtils.GetDataTilePositions(renderTilePosition))
            {
                if (DataTilemap.HasTile(dataTilePosition)) return true;
            }

            return false;
        }

    }
}