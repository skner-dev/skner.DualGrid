using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    public static class DualGridRuleTilePreviewer
    {

        private static Scene _previewScene;
        private static Camera _previewCamera;
        private static DualGridTilemapModule _previewDualGridTilemapModule;

        private static RenderTexture _renderTexture;

        /// <summary>
        /// Offset used to spawn preview objects, so they are outside of the active scene view when rendered
        /// <para></para>
        /// This is used because cameras don't work properly in preview scenes. 
        /// The best workaround found was to move the preview objects into the active scene, render the camera and move them back into the preview scene.
        /// <para></para>
        /// Thanks Unity!
        /// </summary>
        private static Vector3 _previewObjectsPositionOffset = new(100000, 100000, 0);

        /// <summary>
        /// Loads the preview scene with a specific tile.
        /// <para></para>
        /// The preview scene and objects will be initialized if not already.
        /// </summary>
        /// <param name="tile"></param>
        public static void LoadPreviewScene(DualGridRuleTile tile)
        {
            if (_previewScene == default)
            {
                _previewScene = EditorSceneManager.NewPreviewScene();
            }

            if (_previewDualGridTilemapModule == null)
            {
                _previewDualGridTilemapModule = CreateDualGridTilemapModule(tile);
                EditorSceneManager.MoveGameObjectToScene(_previewDualGridTilemapModule.transform.parent.gameObject, _previewScene);
            }

            if (_previewDualGridTilemapModule.RenderTile != tile)
            {
                UpdateDualGridTile(_previewDualGridTilemapModule, tile);
            }

            if (_previewCamera == null)
            {
                _previewCamera = CreateCamera();
                EditorSceneManager.MoveGameObjectToScene(_previewCamera.gameObject, _previewScene);
            }

            if (_renderTexture == null)
            {
                _renderTexture = new RenderTexture(1350, 420, 16, RenderTextureFormat.Default);
            }
        }

        /// <summary>
        /// Forcefully renders the preview dual grid tilemap, by temporarily moving the preview objects (camera and tilemap) into the active scene,
        /// so that they are rendered, and then back into the preview scene, so that they are hidden.
        /// <para></para>
        /// This is only done because temporary preview scenes don't allow cameras to work properly.
        /// </summary>
        public static void UpdateRenderTexture()
        {
            MovePreviewObjectsToScene(EditorSceneManager.GetActiveScene());

            _previewCamera.targetTexture = _renderTexture;
            _previewCamera.Render();
            _previewCamera.targetTexture = null;

            MovePreviewObjectsToScene(_previewScene);

            static void MovePreviewObjectsToScene(Scene scene)
            {
                EditorSceneManager.MoveGameObjectToScene(_previewDualGridTilemapModule.transform.parent.gameObject, scene);
                EditorSceneManager.MoveGameObjectToScene(_previewCamera.gameObject, scene);
            }
        }

        /// <summary>
        /// Returns the current tilemap preview render texture.
        /// </summary>
        /// <returns></returns>
        public static RenderTexture GetRenderTexture()
        {
            if (_renderTexture == null)
                Debug.LogError("RenderTexture not initialized. Make sure the preview scene is loaded.");

            return _renderTexture;
        }

        private static DualGridTilemapModule CreateDualGridTilemapModule(DualGridRuleTile dualGridRuleTile)
        {
            var dualGridTilemapModule = DualGridTilemapModuleEditor.CreateNewDualGridTilemap();

            dualGridTilemapModule.transform.parent.position += _previewObjectsPositionOffset;
            UpdateDualGridTile(dualGridTilemapModule, dualGridRuleTile);
            PaintSampleTiles(dualGridTilemapModule);

            return dualGridTilemapModule;
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("PreviewCamera");

            Camera camera = cameraObject.AddComponent<Camera>();

            camera.orthographic = true;
            camera.transform.position = new Vector3(0, -5.5f, -10) + _previewObjectsPositionOffset;
            camera.orthographicSize = 3;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 15f;
            camera.backgroundColor = Color.gray;
            camera.cullingMask = -1;

            return camera;
        }

        private static void PaintSampleTiles(DualGridTilemapModule previewDualGridTilemapModule)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            
            previewDualGridTilemapModule.DataTilemap.ClearAllTiles();

            // Two dots
            SetTile(-9, -4);
            SetTile(-7, -4);

            // O shape
            SetTile(-9, -6);
            SetTile(-9, -7);
            SetTile(-9, -8);
            SetTile(-8, -6);
            SetTile(-8, -8);
            SetTile(-7, -6);
            SetTile(-7, -7);
            SetTile(-7, -8);

            // Horizontal line
            SetTile(-5, -4);
            SetTile(-4, -4);
            SetTile(-3, -4);

            // 3x3 square
            SetTile(-5, -6);
            SetTile(-4, -6);
            SetTile(-3, -6);
            SetTile(-5, -7);
            SetTile(-4, -7);
            SetTile(-3, -7);
            SetTile(-5, -8);
            SetTile(-4, -8);
            SetTile(-3, -8);

            // Exclamation Point
            SetTile(-1, -4);
            SetTile(-1, -5);
            SetTile(-1, -6);
            SetTile(-1, -8);

            // Plus Symbol
            SetTile(2, -4);
            SetTile(1, -5);
            SetTile(2, -5);
            SetTile(3, -5);
            SetTile(2, -6);

            // Another horizontal line
            SetTile(1, -8);
            SetTile(2, -8);
            SetTile(3, -8);

            // Top Shuriken thing
            SetTile(5, -4);
            SetTile(5, -5);
            SetTile(6, -5);
            SetTile(7, -4);
            SetTile(8, -4);
            SetTile(8, -5);

            // Bottom Shuriken thing
            SetTile(5, -7);
            SetTile(5, -8);
            SetTile(6, -7);
            SetTile(8, -7);
            SetTile(7, -8);
            SetTile(8, -8);

            void SetTile(int x, int y)
            {
                previewDualGridTilemapModule.DataTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        private static void UpdateDualGridTile(DualGridTilemapModule dualGridTilemapModule, DualGridRuleTile dualGridRuleTile)
        {
            dualGridTilemapModule.RenderTile = dualGridRuleTile;
            dualGridTilemapModule.RefreshRenderTilemap();
        }

    }
}
