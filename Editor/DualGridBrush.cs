using UnityEditor.Tilemaps;
using UnityEngine;

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
            base.BoxFill(gridLayout, brushTarget, bounds);

            RefreshDualGridTilemap(brushTarget, bounds);
        }

        public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt bounds)
        {
            base.BoxErase(gridLayout, brushTarget, bounds);

            RefreshDualGridTilemap(brushTarget, bounds);
        }

        protected virtual void RefreshDualGridTilemap(GameObject brushTarget, BoundsInt bounds)
        {
            if (brushTarget.TryGetComponent(out DualGridTilemapModule dualGridTilemapModule))
            {
                foreach (var position in bounds.allPositionsWithin)
                {
                    dualGridTilemapModule.RefreshRenderTiles(position);
                }
            }
        }

    }
}
