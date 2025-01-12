using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid
{
    public class DualGridDataTile : Tile
    {

        private DualGridTilemapModule _dualGridTilemapModule;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            SetDataTilemap(tilemap);

            base.GetTileData(position, tilemap, ref tileData);

            // Sets the tile data's GameObject based on the associated DualGridTilemapModule's setting
            if (_dualGridTilemapModule != null && _dualGridTilemapModule.GameObjectOrigin != GameObjectOrigin.DataTilemap)
            {
                tileData.gameObject = null;
            }
        }

        private void SetDataTilemap(ITilemap tilemap)
        {
            var originTilemap = tilemap.GetComponent<Tilemap>();
            _dualGridTilemapModule = originTilemap.GetComponent<DualGridTilemapModule>();
        }

    }
}
