using skner.DualGrid.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor.Extensions
{
    public static class DualGridTilemapModuleExtensions
    {

        public static void SetEditorPreviewTile(this DualGridTilemapModule dualGridTilemapModule, Vector3Int position, TileBase tile)
        {
            dualGridTilemapModule.DataTilemap.SetEditorPreviewTile(position, tile);
            dualGridTilemapModule.UpdatePreviewRenderTiles(position);
        }

        public static void ClearEditorPreviewTile(this DualGridTilemapModule dualGridTilemapModule, Vector3Int position)
        {
            dualGridTilemapModule.DataTilemap.SetEditorPreviewTile(position, null);
            dualGridTilemapModule.UpdatePreviewRenderTiles(position);
        }

        public static void UpdatePreviewRenderTiles(this DualGridTilemapModule dualGridTilemapModule, Vector3Int previewDataTilePosition)
        {
            bool hasPreviewDataTile = dualGridTilemapModule.DataTilemap.HasEditorPreviewTile(previewDataTilePosition);
            bool isPreviewDataTileVisible = dualGridTilemapModule.DataTilemap.GetEditorPreviewTile<DualGridPreviewTile>(previewDataTilePosition) is DualGridPreviewTile previewTile && previewTile.IsFilled;

            foreach (Vector3Int renderTilePosition in DualGridUtils.GetRenderTilePositions(previewDataTilePosition))
            {
                if (hasPreviewDataTile && isPreviewDataTileVisible)
                {
                    SetPreviewRenderTile(dualGridTilemapModule, renderTilePosition);
                }
                else
                {
                    UnsetPreviewRenderTile(dualGridTilemapModule, renderTilePosition);
                }
            }
        }

        public static void UpdateAllPreviewRenderTiles(this DualGridTilemapModule dualGridTilemapModule)
        {
            foreach (var position in dualGridTilemapModule.DataTilemap.cellBounds.allPositionsWithin)
            {
                dualGridTilemapModule.UpdatePreviewRenderTiles(position);
            }
        }

        public static void ClearAllPreviewTiles(this DualGridTilemapModule dualGridTilemapModule)
        {
            dualGridTilemapModule.DataTilemap.ClearAllEditorPreviewTiles();
            dualGridTilemapModule.RenderTilemap.ClearAllEditorPreviewTiles();
        }

        private static void SetPreviewRenderTile(DualGridTilemapModule dualGridTilemapModule, Vector3Int previewRenderTilePosition)
        {
            dualGridTilemapModule.RenderTilemap.SetEditorPreviewTile(previewRenderTilePosition, dualGridTilemapModule.RenderTile);
            dualGridTilemapModule.RenderTilemap.RefreshTile(previewRenderTilePosition);
        }

        private static void UnsetPreviewRenderTile(DualGridTilemapModule dualGridTilemapModule, Vector3Int previewRenderTilePosition)
        {
            dualGridTilemapModule.RenderTilemap.SetEditorPreviewTile(previewRenderTilePosition, null);
            dualGridTilemapModule.RenderTilemap.RefreshTile(previewRenderTilePosition);
        }

    }
}
