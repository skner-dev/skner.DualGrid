using UnityEngine.Tilemaps;

namespace skner.DualGrid
{
    public class DualGridPreviewTile : TileBase
    {

        public bool IsFilled { get; private set; }

        public static DualGridPreviewTile Filled => Create(isFilled: true);
        public static DualGridPreviewTile NotFilled => Create(isFilled: false);

        private static DualGridPreviewTile Create(bool isFilled)
        {
            var dualGridPreviewTile = CreateInstance<DualGridPreviewTile>();
            dualGridPreviewTile.name = $"{(isFilled ? "Filled" : "Empty")} Dual Grid Preview Tile";
            dualGridPreviewTile.IsFilled = isFilled;
            return dualGridPreviewTile;
        }
    }

}
