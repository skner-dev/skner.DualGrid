using skner.DualGrid.Editor.Extensions;
using skner.DualGrid.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DualGridTilemapModule))]
    public class DualGridTilemapModuleEditor : UnityEditor.Editor
    {

        private static class Styles
        {
            public static readonly GUIContent RenderTile = EditorGUIUtility.TrTextContent("Dual Grid Rule Tile", "The Render Tile that will be applied in the Render Tilemap.");
            public static readonly GUIContent EnableTilemapCollider = EditorGUIUtility.TrTextContent("Enable Tilemap Collider", "If a TilemapCollider2D should be active based on the Dual Grid Rule Tile's default collider type.");
            public static readonly GUIContent GameObjectOrigin = EditorGUIUtility.TrTextContent("Game Object Origin", "Determines which tilemap the GameObjects defined in the Dual Grid Rule Tile should be in.");
        }

        private DualGridTilemapModule _targetDualGridTilemapModule;

        private bool _hasMultipleTargets = false;
        private List<DualGridTilemapModule> _targetDualGridTilemapModules = new();

        private bool _showDataTileBoundaries = false;

        private bool _showRenderTileBoundaries = false;
        private bool _showRenderTileConnections = false;

        public static Grid CreateNewDualGrid()
        {
            var newDualGrid = new GameObject("Dual Grid");
            return newDualGrid.AddComponent<Grid>();
        }

        public static DualGridTilemapModule CreateNewDualGridTilemap(Grid grid = null)
        {
            if (grid == null) grid = CreateNewDualGrid();

            var newDataTilemap = new GameObject("DataTilemap");
            newDataTilemap.AddComponent<Tilemap>();
            var dualGridTilemapModule = newDataTilemap.AddComponent<DualGridTilemapModule>();
            newDataTilemap.transform.parent = grid.transform;

            InitializeRenderTilemap(dualGridTilemapModule);

            return dualGridTilemapModule;
        }

        private void OnEnable()
        {
            _targetDualGridTilemapModule = (DualGridTilemapModule)target;

            _hasMultipleTargets = targets.Length > 1;

            if (_hasMultipleTargets) _targetDualGridTilemapModules = targets.Cast<DualGridTilemapModule>().ToList();
            else _targetDualGridTilemapModules = new List<DualGridTilemapModule>() { target as DualGridTilemapModule };

            _targetDualGridTilemapModules.ForEach(dualGridTilemapModule => InitializeRenderTilemap(dualGridTilemapModule));
        }

        private static void InitializeRenderTilemap(DualGridTilemapModule dualGridTilemapModule)
        {
            if (dualGridTilemapModule == null) return;

            if (dualGridTilemapModule.RenderTilemap == null)
            {
                CreateRenderTilemapObject(dualGridTilemapModule);
            }

            DestroyTilemapRendererInDataTilemap(dualGridTilemapModule);
            UpdateTilemapColliderComponents(dualGridTilemapModule);
        }

        internal static GameObject CreateRenderTilemapObject(DualGridTilemapModule targetModule)
        {
            var renderTilemapObject = new GameObject("RenderTilemap");
            renderTilemapObject.transform.parent = targetModule.transform;
            renderTilemapObject.transform.localPosition = new Vector3(-0.5f, -0.5f, 0f); // Offset by half a tile (TODO: Confirm if tiles can have different dynamic sizes, this might not work under those conditions)

            renderTilemapObject.AddComponent<Tilemap>();
            renderTilemapObject.AddComponent<TilemapRenderer>();

            return renderTilemapObject;
        }

        private static void DestroyTilemapRendererInDataTilemap(DualGridTilemapModule dualGridTilemapModule)
        {
            TilemapRenderer renderer = dualGridTilemapModule.GetComponent<TilemapRenderer>();
            DestroyComponentIfExists(renderer, "Dual Grid Tilemaps cannot have TilemapRenderers in the same GameObject. TilemapRenderer has been destroyed.");
        }

        private static void UpdateTilemapColliderComponents(DualGridTilemapModule dualGridTilemapModule, bool shouldLogWarnings = true)
        {
            TilemapCollider2D tilemapColliderFromDataTilemap = dualGridTilemapModule.DataTilemap.GetComponent<TilemapCollider2D>();
            TilemapCollider2D tilemapColliderFromRenderTilemap = dualGridTilemapModule.RenderTilemap.GetComponent<TilemapCollider2D>();

            string warningMessage;
            if (dualGridTilemapModule.EnableTilemapCollider == false)
            {
                warningMessage = "Dual Grid Tilemaps cannot have Tilemap Colliders 2D if not enabled in Dual Grid Tilemap Module.";
                DestroyComponentIfExists(tilemapColliderFromDataTilemap, shouldLogWarnings ? warningMessage : null);
                DestroyComponentIfExists(tilemapColliderFromRenderTilemap, shouldLogWarnings ? warningMessage : null);
                return;
            }

            switch (dualGridTilemapModule.DataTile.colliderType)
            {
                case Tile.ColliderType.None:
                    warningMessage = "Dual Grid Tilemaps cannot have Tilemap Colliders 2D if Dual Grid Tile has collider type set to none.";
                    DestroyComponentIfExists(tilemapColliderFromDataTilemap, shouldLogWarnings ? warningMessage : null);
                    DestroyComponentIfExists(tilemapColliderFromRenderTilemap, shouldLogWarnings ? warningMessage : null);
                    break;
                case Tile.ColliderType.Sprite:
                    warningMessage = "Dual Grid Tilemaps cannot have Tilemap Colliders 2D in the Data Tilemap if Dual Grid Tile has collider type set to Sprite.";
                    DestroyComponentIfExists(tilemapColliderFromDataTilemap, shouldLogWarnings ? warningMessage : null);
                    if (tilemapColliderFromRenderTilemap == null) dualGridTilemapModule.RenderTilemap.gameObject.AddComponent<TilemapCollider2D>();
                    break;
                case Tile.ColliderType.Grid:
                    warningMessage = "Dual Grid Tilemaps cannot have Tilemap Colliders 2D in the Render Tilemap if Dual Grid Tile has collider type set to Grid.";
                    if (tilemapColliderFromDataTilemap == null) dualGridTilemapModule.DataTilemap.gameObject.AddComponent<TilemapCollider2D>();
                    DestroyComponentIfExists(tilemapColliderFromRenderTilemap, shouldLogWarnings ? warningMessage : null);
                    break;
                default:
                    break;
            }
        }

        private static void DestroyComponentIfExists(Component component, string warningMessage = null)
        {
            if (component != null)
            {
                if (warningMessage != null)
                    Debug.LogWarning(warningMessage);

                DestroyImmediate(component);
            }
        }

        public override void OnInspectorGUI()
        {
            if (_hasMultipleTargets) Undo.RecordObjects(_targetDualGridTilemapModules.ToArray(), $"Updated {_targetDualGridTilemapModules.Count} Dual Grid Tilemap Modules");
            else Undo.RecordObject(_targetDualGridTilemapModule, $"Updated '{_targetDualGridTilemapModule.name}' Dual Grid Rule Tile");

            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets && _targetDualGridTilemapModules.HasDifferentValues(dualGridTilemapModule => dualGridTilemapModule.RenderTile);
            var renderTile = EditorGUILayout.ObjectField(Styles.RenderTile, _targetDualGridTilemapModule.RenderTile, typeof(DualGridRuleTile), false) as DualGridRuleTile;
            if (EditorGUI.EndChangeCheck())
            {
                foreach(var dualGridTilemapModule in _targetDualGridTilemapModules)
                {
                    dualGridTilemapModule.RenderTile = renderTile;
                    dualGridTilemapModule.DataTilemap.RefreshAllTiles();
                    dualGridTilemapModule.RefreshRenderTilemap();
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets && _targetDualGridTilemapModules.HasDifferentValues(dualGridTilemapModule => dualGridTilemapModule.EnableTilemapCollider);
            var enableTilemapCollider = EditorGUILayout.Toggle(Styles.EnableTilemapCollider, _targetDualGridTilemapModule.EnableTilemapCollider);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var dualGridTilemapModule in _targetDualGridTilemapModules)
                {
                    dualGridTilemapModule.EnableTilemapCollider = enableTilemapCollider;
                    UpdateTilemapColliderComponents(dualGridTilemapModule, shouldLogWarnings: false);
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets && _targetDualGridTilemapModules.HasDifferentValues(dualGridTilemapModule => dualGridTilemapModule.GameObjectOrigin);
            var gameObjectOrigin = (GameObjectOrigin)EditorGUILayout.EnumPopup(Styles.GameObjectOrigin, _targetDualGridTilemapModule.GameObjectOrigin);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var dualGridTilemapModule in _targetDualGridTilemapModules)
                {
                    dualGridTilemapModule.GameObjectOrigin = gameObjectOrigin;
                    dualGridTilemapModule.DataTilemap.RefreshAllTiles();
                    dualGridTilemapModule.RefreshRenderTilemap();
                }
            }

            GUILayout.Space(5);
            GUILayout.Label("Tools", EditorStyles.boldLabel);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_targetDualGridTilemapModule);
            }

            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Visualization Handles", EditorStyles.boldLabel);
            _showDataTileBoundaries = EditorGUILayout.Toggle("Data Tile Boundaries", _showDataTileBoundaries);
            _showRenderTileBoundaries = EditorGUILayout.Toggle("Render Tile Boundaries", _showRenderTileBoundaries);
            _showRenderTileConnections = EditorGUILayout.Toggle("Render Tile Connections", _showRenderTileConnections);

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            foreach (var dualGridTilemapModule in _targetDualGridTilemapModules)
            {
                DrawDataTileHandles(dualGridTilemapModule);
                DrawRenderTileHandles(dualGridTilemapModule);
            }
        }

        private void DrawDataTileHandles(DualGridTilemapModule dualGridTilemapModule)
        {
            if (!_showDataTileBoundaries) return;

            foreach (var position in dualGridTilemapModule.DataTilemap.cellBounds.allPositionsWithin)
            {
                if (!dualGridTilemapModule.DataTilemap.HasTile(position)) continue;

                Vector3 tileCenter = dualGridTilemapModule.DataTilemap.GetCellCenterWorld(position);

                Handles.color = Color.green;
                DrawTileBoundaries(dualGridTilemapModule.DataTilemap, tileCenter, thickness: 3);
            }
        }

        private void DrawRenderTileHandles(DualGridTilemapModule dualGridTilemapModule)
        {
            if (!_showRenderTileBoundaries && !_showRenderTileConnections) return;

            foreach (var renderTilePosition in dualGridTilemapModule.RenderTilemap.cellBounds.allPositionsWithin)
            {
                if (!dualGridTilemapModule.RenderTilemap.HasTile(renderTilePosition)) continue;

                Vector3 tileCenter = dualGridTilemapModule.RenderTilemap.GetCellCenterWorld(renderTilePosition);

                Handles.color = Color.yellow;
                if (_showRenderTileBoundaries) DrawTileBoundaries(dualGridTilemapModule.RenderTilemap, tileCenter, thickness: 1);

                Handles.color = Color.red;
                if (_showRenderTileConnections) DrawRenderTileConnections(dualGridTilemapModule.DataTilemap, dualGridTilemapModule.RenderTilemap, renderTilePosition, tileCenter);
            }
        }

        private static void DrawTileBoundaries(Tilemap tilemap, Vector3 tileCenter, float thickness)
        {
            if (tilemap == null) return;

            Handles.DrawSolidDisc(tileCenter, Vector3.forward, radius: 0.05f);

            Vector3 topLeft = tileCenter + new Vector3(-tilemap.cellSize.x / 2, tilemap.cellSize.y / 2, 0);
            Vector3 topRight = tileCenter + new Vector3(tilemap.cellSize.x / 2, tilemap.cellSize.y / 2, 0);
            Vector3 bottomLeft = tileCenter + new Vector3(-tilemap.cellSize.x / 2, -tilemap.cellSize.y / 2, 0);
            Vector3 bottomRight = tileCenter + new Vector3(tilemap.cellSize.x / 2, -tilemap.cellSize.y / 2, 0);

            Handles.DrawLine(topLeft, topRight, thickness);
            Handles.DrawLine(topRight, bottomRight, thickness);
            Handles.DrawLine(bottomRight, bottomLeft, thickness);
            Handles.DrawLine(bottomLeft, topLeft, thickness);
        }

        private static void DrawRenderTileConnections(Tilemap dataTilemap, Tilemap renderTilemap, Vector3Int renderTilePosition, Vector3 tileCenter)
        {
            if (dataTilemap == null || renderTilemap == null) return;

            Vector3Int[] dataTilemapPositions = DualGridUtils.GetDataTilePositions(renderTilePosition);

            foreach (Vector3Int dataTilePosition in dataTilemapPositions)
            {
                if (dataTilemap.HasTile(dataTilePosition))
                {
                    Vector3Int dataTileOffset = dataTilePosition - renderTilePosition;
                    Vector3Int neighborOffset = DualGridUtils.ConvertDataTileOffsetToNeighborOffset(dataTileOffset);

                    Vector3 corner = tileCenter + new Vector3(neighborOffset.x * renderTilemap.cellSize.x * 0.3f, neighborOffset.y * renderTilemap.cellSize.y * 0.3f, 0f);

                    DrawArrow(tileCenter, corner);
                }
            }

            static void DrawArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.15f, float arrowHeadAngle = 30f)
            {
                // Draw the main line
                Handles.DrawLine(start, end);

                // Calculate direction of the line
                Vector3 direction = (end - start).normalized;

                // Calculate the points for the arrowhead
                Vector3 right = Quaternion.Euler(0, 0, arrowHeadAngle) * -direction;
                Vector3 left = Quaternion.Euler(0, 0, -arrowHeadAngle) * -direction;

                // Draw the arrowhead lines
                Handles.DrawLine(end, end + right * arrowHeadLength);
                Handles.DrawLine(end, end + left * arrowHeadLength);
            }
        }

    }
}
