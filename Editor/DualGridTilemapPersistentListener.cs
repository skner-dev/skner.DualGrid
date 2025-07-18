using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    /// <summary>
    /// Ensures that all <see cref="DualGridTilemapModule"/>s are updated when outside of PlayMode.
    /// </summary>
    [InitializeOnLoad]
    public static class DualGridTilemapPersistentListener
    {
        static DualGridTilemapPersistentListener()
        {
            Tilemap.tilemapTileChanged += HandleTilemapChange;
        }

        private static void HandleTilemapChange(Tilemap tilemap, Tilemap.SyncTile[] tiles)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
                return;

            var dualGridModules = Object.FindObjectsByType<DualGridTilemapModule>(FindObjectsSortMode.None);
            foreach (var module in dualGridModules)
            {
                if (module.DataTilemap == tilemap)
                    module.HandleTilemapChange(tilemap, tiles);
            }
        }
    }
}
