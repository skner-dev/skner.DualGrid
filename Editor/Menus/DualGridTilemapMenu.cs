using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    public static class DualGridTilemapMenu
    {

        [MenuItem("GameObject/2D Object/Tilemap/Dual Grid Tilemap ", false, 0)]
        private static void CreateDualGridTilemapMenu()
        {
            Grid selectedGrid = Selection.activeGameObject.GetComponent<Grid>();
            bool isGridSelected = selectedGrid != null;

            var newDualGrid = isGridSelected ? selectedGrid : CreateDualGrid();
            CreateDualGridTilemapModule(newDualGrid);

            Selection.activeGameObject = newDualGrid.gameObject;
        }

        private static Grid CreateDualGrid()
        {
            var newDualGrid = new GameObject("Dual Grid");
            return newDualGrid.AddComponent<Grid>();
        }

        private static void CreateDualGridTilemapModule(Grid newDualGrid)
        {
            var newDataTilemap = new GameObject("DataTilemap");
            newDataTilemap.AddComponent<Tilemap>();
            var dualGridTilemapModule = newDataTilemap.AddComponent<DualGridTilemapModule>();
            newDataTilemap.transform.parent = newDualGrid.transform;

            var dualGridTilemapModuleEditor = UnityEditor.Editor.CreateEditor(dualGridTilemapModule) as DualGridTilemapModuleEditor;
            dualGridTilemapModuleEditor.InitializeRenderTilemap();
        }

    }
}
