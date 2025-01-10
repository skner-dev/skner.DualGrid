using skner.DualGrid.Utils;
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
            bool isPreviewDataTileInvisible = dualGridTilemapModule.DataTilemap.GetEditorPreviewTile<Tile>(previewDataTilePosition) is Tile previewTile && previewTile.sprite == null;

            foreach (Vector3Int renderTilePosition in DualGridUtils.GetRenderTilePositions(previewDataTilePosition))
            {
                if (hasPreviewDataTile && !isPreviewDataTileInvisible)
                {
                    SetPreviewRenderTile(dualGridTilemapModule, renderTilePosition);
                }
                else
                {
                    UnsetPreviewRenderTile(dualGridTilemapModule, renderTilePosition);
                }
            }
        }

        private static void SetPreviewRenderTile(DualGridTilemapModule dualGridTilemapModule, Vector3Int previewRenderTilePosition)
        {
            dualGridTilemapModule.RenderTilemap.SetEditorPreviewTile(previewRenderTilePosition, dualGridTilemapModule.Tile);
            dualGridTilemapModule.RenderTilemap.RefreshTile(previewRenderTilePosition);
        }

        private static void UnsetPreviewRenderTile(DualGridTilemapModule dualGridTilemapModule, Vector3Int previewRenderTilePosition)
        {
            dualGridTilemapModule.RenderTilemap.SetEditorPreviewTile(previewRenderTilePosition, null);
            dualGridTilemapModule.RenderTilemap.RefreshTile(previewRenderTilePosition);
        }

        internal static void ClearAllPreviewTiles(this DualGridTilemapModule dualGridTilemapModule)
        {
            foreach (var position in dualGridTilemapModule.DataTilemap.cellBounds.allPositionsWithin)
            {
                if (dualGridTilemapModule.DataTilemap.HasEditorPreviewTile(position))
                {
                    dualGridTilemapModule.DataTilemap.SetEditorPreviewTile(position, null);
                    dualGridTilemapModule.UpdatePreviewRenderTiles(position);
                }
            }
        }

    }
}
