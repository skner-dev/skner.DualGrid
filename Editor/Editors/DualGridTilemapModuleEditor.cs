using skner.DualGrid.Utils;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    [CustomEditor(typeof(DualGridTilemapModule))]
    public class DualGridTilemapModuleEditor : UnityEditor.Editor
    {

        private static class Styles
        {
            public static readonly GUIContent RenderTile = EditorGUIUtility.TrTextContent("RenderTile", "The Render Tile that will be applied in the Render Tilemap.");
            public static readonly GUIContent EnableTilemapCollider = EditorGUIUtility.TrTextContent("Enable Tilemap Collider", "If a TilemapCollider2D should be active based on the Dual Grid Rule Tile's default collider type.");
            public static readonly GUIContent GameObjectOrigin = EditorGUIUtility.TrTextContent("Game Object Origin", "Determines which tilemap the GameObjects defined in the Dual Grid Rule Tile should be in.");
        }

        private DualGridTilemapModule _targetComponent;

        private Tilemap _dataTilemap;
        private Tilemap _renderTilemap;

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

            var dualGridTilemapModuleEditor = UnityEditor.Editor.CreateEditor(dualGridTilemapModule) as DualGridTilemapModuleEditor;
            dualGridTilemapModuleEditor.InitializeRenderTilemap();

            return dualGridTilemapModule;
        }

        private void OnEnable()
        {
            if (target != null)
                _targetComponent = (DualGridTilemapModule)target;

            InitializeRenderTilemap();
        }

        private void InitializeRenderTilemap()
        {
            if (_targetComponent == null) return;

            if (_targetComponent.RenderTilemap == null)
            {
                CreateRenderTilemapObject(_targetComponent);
            }

            _dataTilemap = _targetComponent.DataTilemap;
            _renderTilemap = _targetComponent.RenderTilemap;

            DestroyTilemapRendererInDataTilemap();
            UpdateTilemapColliderComponents();
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

        private void DestroyTilemapRendererInDataTilemap()
        {
            TilemapRenderer renderer = _targetComponent.GetComponent<TilemapRenderer>();
            DestroyComponentIfExists(renderer, "Dual Grid Tilemaps cannot have TilemapRenderers in the same GameObject. TilemapRenderer has been destroyed.");
        }

        private void UpdateTilemapColliderComponents(bool shouldLogWarnings = true)
        {
            TilemapCollider2D tilemapColliderFromDataTilemap = _targetComponent.DataTilemap.GetComponent<TilemapCollider2D>();
            TilemapCollider2D tilemapColliderFromRenderTilemap = _targetComponent.RenderTilemap.GetComponent<TilemapCollider2D>();

            string warningMessage;
            if (_targetComponent.EnableTilemapCollider == false)
            {
                warningMessage = "Dual Grid Tilemaps cannot have Tilemap Colliders 2D if not enabled in Dual Grid Tilemap Module.";
                DestroyComponentIfExists(tilemapColliderFromDataTilemap, shouldLogWarnings ? warningMessage : null);
                DestroyComponentIfExists(tilemapColliderFromRenderTilemap, shouldLogWarnings ? warningMessage : null);
                return;
            }

            switch (_targetComponent.DataTile.colliderType)
            {
                case Tile.ColliderType.None:
                    warningMessage = "Dual Grid Tilemaps cannot have Tilemap Colliders 2D if Dual Grid Tile has collider type set to none.";
                    DestroyComponentIfExists(tilemapColliderFromDataTilemap, shouldLogWarnings ? warningMessage : null);
                    DestroyComponentIfExists(tilemapColliderFromRenderTilemap, shouldLogWarnings ? warningMessage : null);
                    break;
                case Tile.ColliderType.Sprite:
                    warningMessage = "Dual Grid Tilemaps cannot have Tilemap Colliders 2D in the Data Tilemap if Dual Grid Tile has collider type set to Sprite.";
                    DestroyComponentIfExists(tilemapColliderFromDataTilemap, shouldLogWarnings ? warningMessage : null);
                    if (tilemapColliderFromRenderTilemap == null) _targetComponent.RenderTilemap.gameObject.AddComponent<TilemapCollider2D>();
                    break;
                case Tile.ColliderType.Grid:
                    warningMessage = "Dual Grid Tilemaps cannot have Tilemap Colliders 2D in the Render Tilemap if Dual Grid Tile has collider type set to Grid.";
                    if (tilemapColliderFromDataTilemap == null) _targetComponent.DataTilemap.gameObject.AddComponent<TilemapCollider2D>();
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
            EditorGUILayout.HelpBox("DualGridTilemapModule manages the RenderTilemap linked to the DataTilemap.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginChangeCheck();
            _targetComponent.RenderTile = EditorGUILayout.ObjectField(Styles.RenderTile, _targetComponent.RenderTile, typeof(DualGridRuleTile), false) as DualGridRuleTile;
            if (EditorGUI.EndChangeCheck())
            {
                _targetComponent.DataTilemap.RefreshAllTiles();
                _targetComponent.RefreshRenderTilemap();
            }

            EditorGUI.BeginChangeCheck();
            _targetComponent.EnableTilemapCollider = EditorGUILayout.Toggle(Styles.EnableTilemapCollider, _targetComponent.EnableTilemapCollider);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateTilemapColliderComponents(shouldLogWarnings: false);
            }

            EditorGUI.BeginChangeCheck();
            _targetComponent.GameObjectOrigin = (GameObjectOrigin)EditorGUILayout.EnumPopup(Styles.GameObjectOrigin, _targetComponent.GameObjectOrigin);
            if (EditorGUI.EndChangeCheck())
            {
                _targetComponent.DataTilemap.RefreshAllTiles();
                _targetComponent.RefreshRenderTilemap();
            }

            GUILayout.Space(5);
            GUILayout.Label("Tools", EditorStyles.boldLabel);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_targetComponent);
            }

            GUILayout.Label("Visualization Handles", EditorStyles.boldLabel);
            _showDataTileBoundaries = EditorGUILayout.Toggle("Data Tile Boundaries", _showDataTileBoundaries);
            _showRenderTileBoundaries = EditorGUILayout.Toggle("Render Tile Boundaries", _showRenderTileBoundaries);
            _showRenderTileConnections = EditorGUILayout.Toggle("Render Tile Connections", _showRenderTileConnections);
        }

        private void OnSceneGUI()
        {
            DrawDataTileHandles();
            DrawRenderTileHandles();
        }

        private void DrawDataTileHandles()
        {
            if (!_showDataTileBoundaries) return;

            foreach (var position in _dataTilemap.cellBounds.allPositionsWithin)
            {
                if (!_dataTilemap.HasTile(position)) continue;

                Vector3 tileCenter = _dataTilemap.GetCellCenterWorld(position);

                Handles.color = Color.green;
                DrawTileBoundaries(_dataTilemap, tileCenter, thickness: 3);
            }
        }

        private void DrawRenderTileHandles()
        {
            if (!_showRenderTileBoundaries && !_showRenderTileConnections) return;

            foreach (var renderTilePosition in _renderTilemap.cellBounds.allPositionsWithin)
            {
                if (!_renderTilemap.HasTile(renderTilePosition)) continue;

                Vector3 tileCenter = _renderTilemap.GetCellCenterWorld(renderTilePosition);

                Handles.color = Color.yellow;
                if (_showRenderTileBoundaries) DrawTileBoundaries(_renderTilemap, tileCenter, thickness: 1);

                Handles.color = Color.red;
                if (_showRenderTileConnections) DrawRenderTileConnections(_dataTilemap, _renderTilemap, renderTilePosition, tileCenter);
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
