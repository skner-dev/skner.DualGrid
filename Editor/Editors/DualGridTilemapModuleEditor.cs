using skner.DualGrid.Utils;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    [CustomEditor(typeof(DualGridTilemapModule))]
    public class DualGridTilemapModuleEditor : UnityEditor.Editor
    {

        private DualGridTilemapModule _targetComponent;

        private Tilemap _dataTilemap;
        private Tilemap _renderTilemap;

        private bool _showDataTileBoundaries = false;

        private bool _showRenderTileBoundaries = false;
        private bool _showRenderTileConnections = false;

        private void OnEnable()
        {
            _targetComponent = (DualGridTilemapModule)target;

            InitializeRenderTilemap();
        }

        internal void InitializeRenderTilemap()
        {
            if (_targetComponent.RenderTilemap == null)
            {
                CreateRenderTilemapObject();
                Debug.Log($"Created child RenderTilemap for DualGridTilemapModule {_targetComponent.name}.");
            }

            _dataTilemap = _targetComponent.DataTilemap;
            _renderTilemap = _targetComponent.RenderTilemap;

            DestroyTilemapRendererInDataTilemap();
        }

        private GameObject CreateRenderTilemapObject()
        {
            GameObject renderTilemapObject = new GameObject("RenderTilemap");
            renderTilemapObject.transform.parent = _targetComponent.transform;
            renderTilemapObject.transform.localPosition = new Vector3(-0.5f, -0.5f, 0f); // Offset by half a tile (TODO: Confirm if tiles can have different dynamic sizes, this might not work under those conditions)

            Tilemap renderTilemap = renderTilemapObject.AddComponent<Tilemap>();
            renderTilemapObject.AddComponent<TilemapRenderer>();

            return renderTilemapObject;
        }

        private void DestroyTilemapRendererInDataTilemap()
        {
            TilemapRenderer renderer = _targetComponent.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                Debug.Log("Dual Grid Tilemaps cannot have TilemapRenderers in the same GameObject. TilemapRenderer has been destroyed.");
                DestroyImmediate(renderer);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("DualGridTilemapModule manages the RenderTilemap linked to the DataTilemap.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();

            GUILayout.Space(5);
            GUILayout.Label("Tools", EditorStyles.boldLabel);

            if (EditorGUI.EndChangeCheck()) // Required so that it can update the tilemap if the rule tile assigned changes
                _targetComponent.RefreshRenderTiles();

            if (GUILayout.Button("Refresh Render Tilemap"))
                _targetComponent.RefreshRenderTiles();

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
