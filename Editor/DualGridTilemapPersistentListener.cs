using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    [InitializeOnLoad]
    public static class DualGridTilemapPersistentListener
    {
        static DualGridTilemapPersistentListener()
        {
            Tilemap.tilemapTileChanged += HandleTilemapChange;
        }

        private static void HandleTilemapChange(Tilemap tilemap, Tilemap.SyncTile[] tiles)
        {
            var dualGridModules = Object.FindObjectsByType<DualGridTilemapModule>(FindObjectsSortMode.None);
            foreach (var module in dualGridModules)
            {
                module.HandleTilemapChange(tilemap, tiles);
            }
        }
    }
}
