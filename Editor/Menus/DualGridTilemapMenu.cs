using UnityEditor;
using UnityEngine;

namespace skner.DualGrid.Editor
{
    public static class DualGridTilemapMenu
    {

        [MenuItem("GameObject/2D Object/Tilemap/Dual Grid Tilemap ", false, 0)]
        private static void CreateDualGridTilemapMenu()
        {
            Grid selectedGrid = Selection.activeGameObject?.GetComponent<Grid>();

            var newDualGridTilemapModule = DualGridTilemapModuleEditor.CreateNewDualGridTilemap(selectedGrid);

            Selection.activeGameObject = newDualGridTilemapModule.gameObject;
        }

    }
}
