using skner.DualGrid.Editor.Extensions;
using UnityEditor;
using UnityEngine;

namespace skner.DualGrid.Editor
{
    [CustomEditor(typeof(DualGridRuleTile), true)]
    public class DualGridRuleTileEditor : RuleTileEditor
    {

        private DualGridRuleTile _targetDualGridRuleTile;

        private const string PreviewActiveStatusKey = "PreviewActiveStatusKey";
        private bool _isPreviewActive;

        public override void OnEnable()
        {
            _targetDualGridRuleTile = (DualGridRuleTile)target;

            _isPreviewActive = EditorPrefs.GetBool(PreviewActiveStatusKey);

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Dual Grid Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            if (_targetDualGridRuleTile.OriginalTexture == null && _targetDualGridRuleTile.m_TilingRules.Count == 0)
            {
                DrawDragAndDropArea();
            }

            EditorGUI.BeginChangeCheck();
            Texture2D appliedTexture = (Texture2D)EditorGUILayout.ObjectField("Original Texture", _targetDualGridRuleTile.OriginalTexture, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                _targetDualGridRuleTile.TryApplyTexture2D(appliedTexture);
            }

            if (_targetDualGridRuleTile.OriginalTexture == null) return;

            if (appliedTexture.GetSplitSpritesFromTexture().Count != 16)
            {
                EditorGUILayout.HelpBox("Selected texture is not split in exactly 16 sprites.\nPlease provide a valid texture.", MessageType.Error);
                return;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Apply Default GameObject to all Tile Rules"))
            {
                _targetDualGridRuleTile.m_TilingRules.ForEach(tilingRule => tilingRule.m_GameObject = _targetDualGridRuleTile.m_DefaultGameObject);
            }

            if (GUILayout.Button("Apply Default Collider to all Tile Rules"))
            {
                _targetDualGridRuleTile.m_TilingRules.ForEach(tilingRule => tilingRule.m_ColliderType = _targetDualGridRuleTile.m_DefaultColliderType);
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Apply Automatic Rule Tiling"))
            {
                _targetDualGridRuleTile.TryApplyTexture2D(_targetDualGridRuleTile.OriginalTexture, ignoreAutoSlicePrompt: true);
                AutoDualGridRuleTileProvider.ApplyConfigurationPreset(ref _targetDualGridRuleTile);
            }

            GUILayout.Space(5);

            DrawRuleTilePreview();

            EditorGUILayout.LabelField("Rule Tile Settings", EditorStyles.boldLabel);

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                _targetDualGridRuleTile.RefreshDataTile();
                EditorUtility.SetDirty(_targetDualGridRuleTile);
            }
        }

        private void DrawDragAndDropArea()
        {
            Rect dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "", EditorStyles.helpBox);
            GUI.Box(dropArea, "Drag and drop a texture\nto start creating this Dual Grid Rule Tile", EditorStyles.centeredGreyMiniLabel);

            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is Texture2D texture)
                            {
                                _targetDualGridRuleTile.TryApplyTexture2D(texture);
                                Repaint();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void DrawRuleTilePreview()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tilemap Preview", EditorStyles.boldLabel);

            if (!_isPreviewActive)
            {
                if (GUILayout.Button("Show Preview"))
                {
                    _isPreviewActive = true;
                    EditorPrefs.SetBool(PreviewActiveStatusKey, _isPreviewActive);
                }
            }
            else
            {
                if (GUILayout.Button("Hide Preview"))
                {
                    _isPreviewActive = false;
                    EditorPrefs.SetBool(PreviewActiveStatusKey, _isPreviewActive);
                }
            }

            if (_isPreviewActive)
            {
                DualGridRuleTilePreviewer.LoadPreviewScene(_targetDualGridRuleTile);

                DualGridRuleTilePreviewer.UpdateRenderTexture(); // TODO: Slower than ideal, but can't find any better option to check for changes...
                RenderTexture previewTexture = DualGridRuleTilePreviewer.GetRenderTexture();

                if (previewTexture != null)
                {
                    float aspectRatio = (float)previewTexture.width / previewTexture.height;

                    float desiredWidth = EditorGUIUtility.currentViewWidth;
                    float desiredHeight = desiredWidth / aspectRatio;

                    GUILayout.Box(new GUIContent(previewTexture), GUILayout.Width(desiredWidth - 22), GUILayout.Height(desiredHeight - 3));
                }
                else
                {
                    EditorGUILayout.LabelField("Preview not available.", EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        public override void RuleMatrixOnGUI(RuleTile tile, Rect rect, BoundsInt bounds, RuleTile.TilingRule tilingRule)
        {
            // This code was copied from the base RuleTileEditor.RuleMatrixOnGUI, because there are no good ways to extend it.
            // The changes were marked with a comment

            Handles.color = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0.2f);
            float w = rect.width / bounds.size.x;
            float h = rect.height / bounds.size.y;

            for (int y = 0; y <= bounds.size.y; y++)
            {
                float top = rect.yMin + y * h;
                Handles.DrawLine(new Vector3(rect.xMin, top), new Vector3(rect.xMax, top));
            }
            for (int x = 0; x <= bounds.size.x; x++)
            {
                float left = rect.xMin + x * w;
                Handles.DrawLine(new Vector3(left, rect.yMin), new Vector3(left, rect.yMax));
            }
            Handles.color = Color.white;

            var neighbors = tilingRule.GetNeighbors();

            // Incremented for cycles by 1 to workaround new GetBounds(), while perserving corner behaviour
            for (int y = -1; y < 1; y++)
            {
                for (int x = -1; x < 1; x++)
                {
                    // Pos changed here to workaround for the new 2x2 matrix, only considering the corners, while not changing the Rect r
                    Vector3Int pos = new Vector3Int(x == 0 ? 1 : x, y == 0 ? 1 : y, 0);

                    Rect r = new Rect(rect.xMin + (x - bounds.xMin) * w, rect.yMin + (-y + bounds.yMax - 1) * h, w - 1, h - 1);
                    RuleMatrixIconOnGUI(tilingRule, neighbors, pos, r);
                }
            }
        }

        public override BoundsInt GetRuleGUIBounds(BoundsInt bounds, RuleTile.TilingRule rule)
        {
            return new BoundsInt(-1, -1, 0, 2, 2, 0);
        }

        public override Vector2 GetMatrixSize(BoundsInt bounds)
        {
            float matrixCellSize = 27;
            return new Vector2(bounds.size.x * matrixCellSize, bounds.size.y * matrixCellSize);
        }

    }

}

