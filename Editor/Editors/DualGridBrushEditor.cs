using skner.DualGrid.Editor.Extensions;
using skner.DualGrid.Extensions;
using System;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;

namespace skner.DualGrid.Editor
{

    /// <summary>
    /// The custom editor for <see cref="DualGridBrush"/>.
    /// </summary>
    /// <remarks>
    /// This editor is completely compatible with the default <see cref="GridBrush"/>, but with added functionality for Dual Grid Tilemaps.
    /// <para></para>
    /// It's responsible for preview tiles and Undo operations.
    /// </remarks>
    [CustomEditor(typeof(DualGridBrush), false)]
    public class DualGridBrushEditor : GridBrushEditor
    {

        private DualGridTilemapModule _lastDualGridTilemapModule;

        private DualGridPreviewTile _previewTile;
        private DualGridPreviewTile _emptyPreviewTile;

        private BoundsInt? _lastBounds;
        private GridBrushBase.Tool? _lastTool;

        /// <summary>
        /// Whether a preview is shown while painting a Tilemap in the Flood Fill Tool.
        /// </summary>
        /// <remarks>
        /// Editor Preference taken from Grid Brush Preferences.
        /// </remarks>
        private static bool ShowFloodFillPreview => EditorPrefs.GetBool("GridBrush.EnableFloodFillPreview", true);

        public override void OnToolActivated(GridBrushBase.Tool tool)
        {
            if (_previewTile == null)
                _previewTile = DualGridPreviewTile.Filled;

            if (_emptyPreviewTile == null)
                _emptyPreviewTile = DualGridPreviewTile.NotFilled;

            ProtectAgainstEditingRenderTilemap();

            base.OnToolActivated(tool);
        }

        /// <summary>
        /// Controls whether this brush should actively prevent any direct changes to the a Render Tilemap.
        /// </summary>
        protected virtual void ProtectAgainstEditingRenderTilemap()
        {
            var currentSelection = Selection.activeObject as GameObject;
            if (currentSelection == null) return;

            var dualGridTilemapModuleFromRenderTilemap = currentSelection.GetComponentInImmediateParent<DualGridTilemapModule>();
            bool isPaintingOnRenderTilemap = dualGridTilemapModuleFromRenderTilemap != null;
            if (isPaintingOnRenderTilemap)
            {
                Debug.LogWarning($"Current selection {currentSelection.name} is a Render Tilemap and painting on it is not permitted. Changed to associated Data Tilemap {dualGridTilemapModuleFromRenderTilemap.DataTilemap.name}.");
                Selection.activeObject = dualGridTilemapModuleFromRenderTilemap.DataTilemap.gameObject;
            }
        }

        public override void OnPaintSceneGUI(GridLayout gridLayout, GameObject brushTarget, BoundsInt bounds, GridBrushBase.Tool tool, bool executing)
        {
            _lastDualGridTilemapModule = brushTarget.GetComponent<DualGridTilemapModule>();

            base.OnPaintSceneGUI(gridLayout, brushTarget, bounds, tool, executing);
        }

        public override void PaintPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            if (brushTarget.TryGetComponent(out DualGridTilemapModule dualGridTilemapModule))
            {
                BoundsInt bounds = GetBrushBounds(position);
                DualGridPaintPreview(dualGridTilemapModule, bounds);
            }
            else
            {
                base.PaintPreview(gridLayout, brushTarget, position);
            }
        }

        protected virtual void DualGridPaintPreview(DualGridTilemapModule dualGridTilemapModule, BoundsInt bounds)
        {
            foreach (var position in bounds.allPositionsWithin)
            {
                dualGridTilemapModule.SetEditorPreviewTile(position, _previewTile);
            }

            _lastBounds = bounds;
            _lastTool = GridBrushBase.Tool.Paint;
        }

#if UNITY_2023_1_OR_NEWER
        public override void ErasePreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            if (brushTarget.TryGetComponent(out DualGridTilemapModule dualGridTilemapModule))
            {
                BoundsInt bounds = GetBrushBounds(position);
                DualGridErasePreview(dualGridTilemapModule, bounds);
            }
            else
            {
                base.ErasePreview(gridLayout, brushTarget, position);
            }
        }

        private void DualGridErasePreview(DualGridTilemapModule dualGridTilemapModule, BoundsInt bounds)
        {
            foreach (var position in bounds.allPositionsWithin)
            {
                dualGridTilemapModule.SetEditorPreviewTile(position, _emptyPreviewTile);
            }

            _lastBounds = bounds;
            _lastTool = GridBrushBase.Tool.Erase;
        }
#endif

        public override void BoxFillPreview(GridLayout gridLayout, GameObject brushTarget, BoundsInt bounds)
        {
            if (brushTarget.TryGetComponent(out DualGridTilemapModule dualGridTilemapModule))
            {
                DualGridBoxFillPreview(dualGridTilemapModule, bounds);
            }
            else
            {
                base.BoxFillPreview(gridLayout, brushTarget, bounds);
            }
        }

        protected virtual void DualGridBoxFillPreview(DualGridTilemapModule dualGridTilemapModule, BoundsInt bounds)
        {
            foreach (var position in bounds.allPositionsWithin)
            {
                dualGridTilemapModule.SetEditorPreviewTile(position, _previewTile);
            }

            _lastBounds = bounds;
            _lastTool = GridBrushBase.Tool.Box;
        }

        public override void FloodFillPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            if (brushTarget.TryGetComponent(out DualGridTilemapModule dualGridTilemapModule))
            {
                DualGridFloodFillPreview(dualGridTilemapModule, position);
            }
            else
            {
                base.FloodFillPreview(gridLayout, brushTarget, position);
            }
        }

        protected virtual void DualGridFloodFillPreview(DualGridTilemapModule dualGridTilemapModule, Vector3Int position)
        {
            if (!ShowFloodFillPreview) return;

            // Determines what cell to use in flood fill
            var cell = brush.cells[0];
            var validFloodFillTile = cell.tile != null;
            var floodFillTile = validFloodFillTile ? _previewTile : _emptyPreviewTile;

            // Applies flood fill to Dual Grid Tilemap
            dualGridTilemapModule.DataTilemap.EditorPreviewFloodFill(position, floodFillTile);
            dualGridTilemapModule.RefreshRenderTilemap();

            // Set floodfill bounds as tilemap bounds
            var bounds = new BoundsInt(position, Vector3Int.one);
            var origin = dualGridTilemapModule.DataTilemap.origin;
            bounds.min = origin;
            bounds.max = origin + dualGridTilemapModule.DataTilemap.size;

            _lastBounds = bounds;
            _lastTool = GridBrushBase.Tool.FloodFill;
        }

        public override void ClearPreview()
        {
            if (_lastDualGridTilemapModule != null)
            {
                DualGridClearPreview();
            }
            else
            {
                base.ClearPreview();
            }
        }

        protected virtual void DualGridClearPreview()
        {
            if (_lastBounds == null || _lastTool == null)
                return;

            switch (_lastTool)
            {
                case GridBrushBase.Tool.FloodFill:
                    {
                        _lastDualGridTilemapModule.ClearAllPreviewTiles();
                        break;
                    }
                case GridBrushBase.Tool.Box:
                    {
                        Vector3Int min = _lastBounds.Value.position;
                        Vector3Int max = min + _lastBounds.Value.size;
                        var bounds = new BoundsInt(min, max - min);
                        ClearEditorPreviewTiles(_lastDualGridTilemapModule, bounds);
                        break;
                    }
                case GridBrushBase.Tool.Erase:
                case GridBrushBase.Tool.Paint:
                    {
                        ClearEditorPreviewTiles(_lastDualGridTilemapModule, _lastBounds.Value);
                        break;
                    }
            }

            _lastBounds = null;
            _lastTool = null;
        }

        public override void RegisterUndo(GameObject brushTarget, GridBrushBase.Tool tool)
        {
            if (brushTarget.TryGetComponent(out DualGridTilemapModule dualGridTilemapModule))
            {
                // Clears any preview tiles, so they don't interfer with the Undo register call
                if (_lastBounds.HasValue) ClearEditorPreviewTiles(dualGridTilemapModule, _lastBounds.Value);

                Undo.RegisterCompleteObjectUndo(new UnityEngine.Object[] { dualGridTilemapModule.DataTilemap, dualGridTilemapModule.RenderTilemap }, $"{GetVerb(tool)} dual grid {dualGridTilemapModule.name}");
            }
            else
            {
                base.RegisterUndo(brushTarget, tool);
            }

            static string GetVerb(GridBrushBase.Tool tool)
            {
                string toolName = tool.ToString();

                if (toolName.EndsWith("e", StringComparison.OrdinalIgnoreCase))
                    toolName = toolName.Substring(0, toolName.Length - 1);

                return toolName + "ing";
            }
        }

        private BoundsInt GetBrushBounds(Vector3Int position)
        {
            var min = position - brush.pivot;
            var max = min + brush.size;
            return new BoundsInt(min, max - min);
        }

        private static void ClearEditorPreviewTiles(DualGridTilemapModule dualGridTilemapModule, BoundsInt bounds)
        {
            foreach (Vector3Int location in bounds.allPositionsWithin)
            {
                dualGridTilemapModule.ClearEditorPreviewTile(location);
            }
        }

    }
}
