using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    /// <summary>
    /// A custom brush completely compatible with the standard tilemaps, with the added Dual Grid functionality.
    /// </summary>
    /// <remarks>
    /// It's responsible for updating the RenderTilemap when any tiles are updated in the DataTilemap.
    /// </remarks>
    [CustomGridBrush(true, true, true, "Dual Grid Brush")]
    public class DualGridBrush : GridBrush
    {

        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt bounds)
        {
            if (brushTarget.TryGetComponent(out DualGridTilemapModule dualGridTilemapModule))
            {
                SetDualGridTiles(dualGridTilemapModule, dualGridTilemapModule.DataTile, bounds);
            }
            else
            {
                base.BoxFill(gridLayout, brushTarget, bounds);
            }
        }

        public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt bounds)
        {
            if (brushTarget.TryGetComponent(out DualGridTilemapModule dualGridTilemapModule))
            {
                SetDualGridTiles(dualGridTilemapModule, null, bounds);
            }
            else
            {
                base.BoxErase(gridLayout, brushTarget, bounds);
            }
        }

        public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            if (brushTarget.TryGetComponent(out DualGridTilemapModule dualGridTilemapModule))
            {
                dualGridTilemapModule.DataTilemap.FloodFill(position, dualGridTilemapModule.DataTile);
            }
            else
            {
                base.FloodFill(gridLayout, brushTarget, position);
            }
        }

        private void SetDualGridTiles(DualGridTilemapModule dualGridTilemapModule, DualGridDataTile dualGridDataTile, BoundsInt bounds)
        {
            var tileChangeData = new List<TileChangeData>();

            foreach (var position in bounds.allPositionsWithin)
            {
                tileChangeData.Add(new TileChangeData { position = position, tile = dualGridDataTile });
            }

            dualGridTilemapModule.DataTilemap.SetTiles(tileChangeData.ToArray(), ignoreLockFlags: false);
            RefreshDualGridTilemap(dualGridTilemapModule, bounds);
        }

        protected virtual void RefreshDualGridTilemap(DualGridTilemapModule dualGridTilemapModule, BoundsInt bounds)
        {
            foreach (var position in bounds.allPositionsWithin)
            {
                dualGridTilemapModule.RefreshRenderTiles(position);
                dualGridTilemapModule.DataTilemap.RefreshTile(position);
            }
        }

    }
}
