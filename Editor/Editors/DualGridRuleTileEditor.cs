using skner.DualGrid.Editor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Tilemaps.Tile;

namespace skner.DualGrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DualGridRuleTile), true)]
    public class DualGridRuleTileEditor : RuleTileEditor
    {

        private static class Styles
        {
            public static readonly GUIContent DefaultSprite = EditorGUIUtility.TrTextContent("Default Sprite", "The default sprite will be used as a last resort when no tiling rules are valid.");
            public static readonly GUIContent DefaultGameObject = EditorGUIUtility.TrTextContent("GameObject", "Depending on the configuration on the Dual Grid Tilemap Module, this GameObject will be used for every tile.");
            public static readonly GUIContent DefaultCollider = EditorGUIUtility.TrTextContent("Collider", "The collider type that will be used for this Dual Grid Rule Tile.");

            public static readonly GUIContent OriginalTexture = EditorGUIUtility.TrTextContent("Original Texture", "The original Texture2D associated with this Dual Grid Rule Tile. Only textures splitted in 16 pieces are considered valid.");

            public static readonly GUIContent TilingRules = EditorGUIUtility.TrTextContent("Tiling Rules List");
            public static readonly GUIContent TilingRulesGameObject = EditorGUIUtility.TrTextContent("GameObject", "Depending on the configuration on the Dual Grid Tilemap Module, this GameObject will be used for this specific Tiling Rule.");
            public static readonly GUIContent TilingRulesCollider = EditorGUIUtility.TrTextContent("Collider", "Colliders per Tiling Rule are not supported. They are set for the entire Dual Grid Rule Tile.");
            public static readonly GUIContent TilingRulesOutput = EditorGUIUtility.TrTextContent("Output", "The Output for the tile which fits this Rule. Each Output type has its own properties.");

            public static readonly GUIContent TilingRulesNoise = EditorGUIUtility.TrTextContent("Noise", "The Perlin noise factor when placing the tile.");
            public static readonly GUIContent TilingRulesShuffle = EditorGUIUtility.TrTextContent("Shuffle", "The randomized transform given to the tile when placing it.");
            public static readonly GUIContent TilingRulesRandomSize = EditorGUIUtility.TrTextContent("Size", "The number of Sprites to randomize from.");

            public static readonly GUIContent TilingRulesMinSpeed = EditorGUIUtility.TrTextContent("Min Speed", "The minimum speed at which the animation is played.");
            public static readonly GUIContent TilingRulesMaxSpeed = EditorGUIUtility.TrTextContent("Max Speed", "The maximum speed at which the animation is played.");
            public static readonly GUIContent TilingRulesAnimationSize = EditorGUIUtility.TrTextContent("Size", "The number of Sprites in the animation.");

            public static readonly GUIStyle extendNeighborsLightStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 10,
                normal = new GUIStyleState()
                {
                    textColor = Color.black
                }
            };

            public static readonly GUIStyle extendNeighborsDarkStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 10,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                }
            };
        }

        private DualGridRuleTile _targetDualGridRuleTile;

        private const string PreviewActiveStatusKey = "PreviewActiveStatusKey";
        private bool _isPreviewActive;
        private ReorderableList _tilingRulesReorderableList;

        private bool _hasMultipleTargets = false;
        private List<DualGridRuleTile> _targetDualGridRuleTiles = new();

        public override void OnEnable()
        {
            _targetDualGridRuleTile = (DualGridRuleTile)target;
            _hasMultipleTargets = targets.Length > 1;

            if (_hasMultipleTargets) _targetDualGridRuleTiles = targets.Cast<DualGridRuleTile>().ToList();
            else _targetDualGridRuleTiles = new List<DualGridRuleTile>() { target as DualGridRuleTile };

            _isPreviewActive = EditorPrefs.GetBool(PreviewActiveStatusKey);

            _tilingRulesReorderableList = new ReorderableList(tile != null ? tile.m_TilingRules : null, typeof(RuleTile.TilingRule), true, true, false, false);
            _tilingRulesReorderableList.drawHeaderCallback = OnDrawHeader;
            _tilingRulesReorderableList.drawElementCallback = OnDrawElement;
            _tilingRulesReorderableList.elementHeightCallback = GetElementHeight;
            _tilingRulesReorderableList.onChangedCallback = ListUpdated;

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_hasMultipleTargets) Undo.RecordObjects(_targetDualGridRuleTiles.ToArray(), $"Updated {_targetDualGridRuleTiles.Count} Dual Grid Rule Tiles");
            else Undo.RecordObject(_targetDualGridRuleTile, $"Updated '{_targetDualGridRuleTile.name}' Dual Grid Rule Tile");

            EditorGUI.BeginChangeCheck();

            var shouldContinue = DrawRuleTileOriginalTexture();
            if (shouldContinue) shouldContinue = DrawRuleTileSettings();
            if (shouldContinue) shouldContinue = DrawRuleTileTools();
            if (shouldContinue) shouldContinue = DrawRuleTilePreview();
            if (shouldContinue) shouldContinue = DrawTilingRulesList();

            if (EditorGUI.EndChangeCheck())
            {
                SaveSelectedTiles();
                _targetDualGridRuleTiles.ForEach(dualGridRuleTile => dualGridRuleTile.RefreshDataTile());
            }
        }

        /// <returns>If the Inspector should continue the drawing pipeline.</returns>
        protected virtual bool DrawRuleTileOriginalTexture()
        {
            EditorGUILayout.LabelField("Dual Grid Settings", EditorStyles.boldLabel);

            if (_targetDualGridRuleTiles.Any(dualGridRuleTile => dualGridRuleTile.OriginalTexture == null && dualGridRuleTile.m_TilingRules.Count == 0))
            {
                DrawDragAndDropArea();
                return false;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets && _targetDualGridRuleTiles.HasDifferentValues(dualGridRuleTile => dualGridRuleTile.OriginalTexture);
            Texture2D appliedTexture = EditorGUILayout.ObjectField(Styles.OriginalTexture, _targetDualGridRuleTile.OriginalTexture, typeof(Texture2D), false) as Texture2D;
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var dualGridRuleTile in _targetDualGridRuleTiles)
                {
                    bool wasTextureApplied = dualGridRuleTile.TryApplyTexture2D(appliedTexture);
                    if (wasTextureApplied == false) break; // Invalid texture, stop applying to other selected tiles
                }
            }

            EditorGUI.showMixedValue = false;
            if (appliedTexture == null) return false;

            return true;
        }

        private void DrawDragAndDropArea()
        {
            if (_hasMultipleTargets)
            {
                EditorGUILayout.HelpBox("At least one of the selected Dual Grid Rule Tiles are missing an Original Texture.\n" +
                    "Please select the individual empty Dual Grid Rule Tile to set the Texture.", MessageType.Error);
            }
            else
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

                            foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                            {
                                OnDropObjectInTextureArea(draggedObject);
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void OnDropObjectInTextureArea(UnityEngine.Object draggedObject)
        {
            if (draggedObject is Texture2D texture)
            {
                foreach (var dualGridRuleTile in _targetDualGridRuleTiles)
                {
                    bool wasTextureApplied = dualGridRuleTile.TryApplyTexture2D(texture);
                    if (wasTextureApplied == false) return; // Invalid texture, stop applying to other selected tiles
                }
                Repaint();
            }
        }

        /// <returns>If the Inspector should interrupt the drawing pipeline.</returns>
        protected virtual bool DrawRuleTileSettings()
        {
            EditorGUILayout.LabelField("Rule Tile Settings", EditorStyles.boldLabel);

            var shouldUpdateAffectedModules = false;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets && _targetDualGridRuleTiles.HasDifferentValues(dualGridRuleTile => dualGridRuleTile.m_DefaultSprite);
            var defaultSprite = EditorGUILayout.ObjectField(Styles.DefaultSprite, _targetDualGridRuleTiles.First().m_DefaultSprite, typeof(Sprite), false) as Sprite;
            if (EditorGUI.EndChangeCheck())
            {
                _targetDualGridRuleTiles.ForEach(dualGridRuleTile => dualGridRuleTile.m_DefaultSprite = defaultSprite);
                shouldUpdateAffectedModules = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets && _targetDualGridRuleTiles.HasDifferentValues(dualGridRuleTile => dualGridRuleTile.m_DefaultGameObject);
            var defaultGameObject = EditorGUILayout.ObjectField(Styles.DefaultGameObject, tile.m_DefaultGameObject, typeof(GameObject), false) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                _targetDualGridRuleTiles.ForEach(dualGridRuleTile => dualGridRuleTile.m_DefaultGameObject = defaultGameObject);
                shouldUpdateAffectedModules = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = _hasMultipleTargets && _targetDualGridRuleTiles.HasDifferentValues(dualGridRuleTile => dualGridRuleTile.m_DefaultColliderType);
            var defaultColliderType = (ColliderType)EditorGUILayout.EnumPopup(Styles.DefaultCollider, tile.m_DefaultColliderType);
            if (EditorGUI.EndChangeCheck())
            {
                _targetDualGridRuleTiles.ForEach(dualGridRuleTile => dualGridRuleTile.m_DefaultColliderType = defaultColliderType);
                shouldUpdateAffectedModules = true;
            }

            EditorGUI.showMixedValue = false;
            EditorGUILayout.Space();

            if (shouldUpdateAffectedModules)
            {
                _targetDualGridRuleTile.RefreshDataTile();

                // Update all Tilemap using updated Dual Grid Rule Tiles
                var dualGridModules = UnityEngine.Object.FindObjectsByType<DualGridTilemapModule>(FindObjectsSortMode.None);
                foreach (var module in dualGridModules)
                {
                    if (_targetDualGridRuleTiles.Contains(module.RenderTile))
                    {
                        DualGridTilemapModuleEditor.UpdateTilemapColliderComponents(module, shouldLogWarnings: false);
                        module.RefreshRenderTilemap();
                    }
                }
            }

            return true;
        }

        /// <returns>If the Inspector should interrupt the drawing pipeline.</returns>
        protected virtual bool DrawRuleTileTools()
        {
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Apply Default GameObject to all Tile Rules"))
            {
                _targetDualGridRuleTiles.ForEach(dualGridRuleTile => dualGridRuleTile.m_TilingRules.ForEach(tilingRule => tilingRule.m_GameObject = dualGridRuleTile.m_DefaultGameObject));
            }

            EditorGUILayout.Space();

            return true;
        }

        /// <returns>If the Inspector should interrupt the drawing pipeline.</returns>
        protected virtual bool DrawRuleTilePreview()
        {
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
                if (_hasMultipleTargets)
                {
                    EditorGUILayout.LabelField("Preview not available when inspecting multiple Dual Grid Rule Tiles.", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    DualGridRuleTilePreviewer.LoadPreviewScene(_targetDualGridRuleTile);

                    DualGridRuleTilePreviewer.UpdateRenderTexture();
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

            EditorGUILayout.Space();

            return true;
        }

        /// <returns>If the Inspector should interrupt the drawing pipeline.</returns>
        protected virtual bool DrawTilingRulesList()
        {
            EditorGUILayout.LabelField("Dual Grid Tiling Rules", EditorStyles.boldLabel);

            if (GUILayout.Button("Apply Automatic Rule Tiling"))
            {
                if (_hasMultipleTargets) Undo.RegisterCompleteObjectUndo(_targetDualGridRuleTiles.ToArray(), $"Auto tiling {_targetDualGridRuleTiles.Count} Dual Grid Rule Tiles");
                else Undo.RegisterCompleteObjectUndo(_targetDualGridRuleTile, $"Auto tiling Dual Grid Rule Tile '{_targetDualGridRuleTile.name}'");

                _targetDualGridRuleTiles.ForEach(dualGridRuleTile => dualGridRuleTile.TryApplyTexture2D(dualGridRuleTile.OriginalTexture, ignoreAutoSlicePrompt: true));
                _targetDualGridRuleTiles.ForEach(dualGridRuleTile => AutoDualGridRuleTileProvider.ApplyConfigurationPreset(ref dualGridRuleTile));
            }

            EditorGUILayout.Space();

            if (tile.m_TilingRules.Count != 16)
            {
                EditorGUILayout.HelpBox($"This Dual Grid Tile has {tile.m_TilingRules.Count} rules, but only exactly 16 is supported.\nPlease apply automatic rule tiling to fix it.", MessageType.Error);
                return false;
            }

            if (_hasMultipleTargets) EditorGUILayout.HelpBox("Editing Tiling Rules manually is not supported when multiple Dual Grid Rule Tiles are selected", MessageType.Warning, true);
            else _tilingRulesReorderableList?.DoLayoutList();

            return true;
        }

        protected virtual void SaveSelectedTiles()
        {
            serializedObject.ApplyModifiedProperties();
            _targetDualGridRuleTiles.ForEach(dualGridRuleTile => EditorUtility.SetDirty(dualGridRuleTile));
            SceneView.RepaintAll();

            _targetDualGridRuleTiles.ForEach(dualGridRuleTile => UpdateAffectedOverrideTiles(dualGridRuleTile));
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

        protected override void OnDrawElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            // This code was copied from the base RuleTileEditor.OnDrawElement, because there are no good ways to extend it.
            // The changes were marked with a comment

            RuleTile.TilingRule rule = tile.m_TilingRules[index];
            BoundsInt bounds = GetRuleGUIBounds(rule.GetBounds(), rule);

            float yPos = rect.yMin + 2f;
            float height = rect.height - k_PaddingBetweenRules;
            Vector2 matrixSize = GetMatrixSize(bounds);

            Rect spriteRect = new Rect(rect.xMax - k_DefaultElementHeight - 5f, yPos, k_DefaultElementHeight, k_DefaultElementHeight);
            Rect matrixRect = new Rect(rect.xMax - matrixSize.x - spriteRect.width - 10f, yPos, matrixSize.x, matrixSize.y);
            Rect inspectorRect = new Rect(rect.xMin, yPos, rect.width - matrixSize.x - spriteRect.width - 20f, height);

            DualGridRuleInspectorOnGUI(inspectorRect, rule); // Used to call base.RuleInspectorOnGUI. But this method isn't virtual, so it can't be ovewritten. Thanks Unity!
            RuleMatrixOnGUI(tile, matrixRect, bounds, rule);
            SpriteOnGUI(spriteRect, rule);
        }

        protected virtual void DualGridRuleInspectorOnGUI(Rect rect, RuleTile.TilingRule tilingRule)
        {
            float y = rect.yMin;
            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.TilingRulesGameObject);
            tilingRule.m_GameObject = (GameObject)EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), "", tilingRule.m_GameObject, typeof(GameObject), false);
            y += k_SingleLineHeight;

            using (new EditorGUI.DisabledScope(true))
            {
                // Collider setting is disabled because it's not supported
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.TilingRulesCollider);
                tilingRule.m_ColliderType = (ColliderType)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_ColliderType);
                y += k_SingleLineHeight;
            }

            GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.TilingRulesOutput);
            tilingRule.m_Output = (RuleTile.TilingRuleOutput.OutputSprite)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Output);
            y += k_SingleLineHeight;

            if (tilingRule.m_Output == RuleTile.TilingRuleOutput.OutputSprite.Animation)
            {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.TilingRulesMinSpeed);
                tilingRule.m_MinAnimationSpeed = EditorGUI.FloatField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_MinAnimationSpeed);
                y += k_SingleLineHeight;
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.TilingRulesMaxSpeed);
                tilingRule.m_MaxAnimationSpeed = EditorGUI.FloatField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_MaxAnimationSpeed);
                y += k_SingleLineHeight;
            }
            if (tilingRule.m_Output == RuleTile.TilingRuleOutput.OutputSprite.Random)
            {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.TilingRulesNoise);
                tilingRule.m_PerlinScale = EditorGUI.Slider(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_PerlinScale, 0.001f, 0.999f);
                y += k_SingleLineHeight;

                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight), Styles.TilingRulesShuffle);
                tilingRule.m_RandomTransform = (RuleTile.TilingRuleOutput.Transform)EditorGUI.EnumPopup(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_RandomTransform);
                y += k_SingleLineHeight;
            }

            if (tilingRule.m_Output != RuleTile.TilingRuleOutput.OutputSprite.Single)
            {
                GUI.Label(new Rect(rect.xMin, y, k_LabelWidth, k_SingleLineHeight)
                    , tilingRule.m_Output == RuleTile.TilingRuleOutput.OutputSprite.Animation ? Styles.TilingRulesAnimationSize : Styles.TilingRulesRandomSize);
                EditorGUI.BeginChangeCheck();
                int newLength = EditorGUI.DelayedIntField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Sprites.Length);
                if (EditorGUI.EndChangeCheck())
                    Array.Resize(ref tilingRule.m_Sprites, Math.Max(newLength, 1));
                y += k_SingleLineHeight;
                for (int i = 0; i < tilingRule.m_Sprites.Length; i++)
                {
                    tilingRule.m_Sprites[i] = EditorGUI.ObjectField(new Rect(rect.xMin + k_LabelWidth, y, rect.width - k_LabelWidth, k_SingleLineHeight), tilingRule.m_Sprites[i], typeof(Sprite), false) as Sprite;
                    y += k_SingleLineHeight;
                }
            }
        }

        public new virtual void OnDrawHeader(Rect rect)
        {
            GUI.Label(rect, Styles.TilingRules);
        }

        private float GetElementHeight(int index)
        {
            RuleTile.TilingRule rule = tile.m_TilingRules[index];
            return base.GetElementHeight(rule);
        }

    }

}

