using skner.DualGrid.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Tilemap))]
    public class RestrictedTilemapEditor : UnityEditor.Editor
    {

        private bool showInfoFoldout = true;

        public override void OnInspectorGUI()
        {
            Tilemap tilemap = (Tilemap)target;

            SerializedProperty animationFrameRate = serializedObject.FindProperty("m_AnimationFrameRate");
            SerializedProperty color = serializedObject.FindProperty("m_Color");

            EditorGUILayout.PropertyField(animationFrameRate, new GUIContent("Animation Frame Rate"));
            EditorGUILayout.PropertyField(color, new GUIContent("Color"));

            // Check if the Tilemap is part of a DualGridTilemapModule
            bool isRenderTilemap = tilemap.GetComponentInImmediateParent<DualGridTilemapModule>() != null;
            if (isRenderTilemap)
            {
                GUILayout.Space(2);
                EditorGUILayout.HelpBox("Editing the position and orientation of a RenderTilemap is restricted.", MessageType.Info);
                GUILayout.Space(2);
            }

            using (new EditorGUI.DisabledScope(isRenderTilemap))
            {
                EditorGUILayout.Vector3Field("Tile Anchor", tilemap.tileAnchor);
                EditorGUILayout.EnumPopup("Orientation", tilemap.orientation);
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Vector3Field("Offset", tilemap.tileAnchor);
                EditorGUILayout.Vector3Field("Rotation", tilemap.transform.rotation.eulerAngles);
                EditorGUILayout.Vector3Field("Scale", tilemap.transform.localScale);

                showInfoFoldout = EditorGUILayout.Foldout(showInfoFoldout, "Info");
                if (showInfoFoldout)
                {
                    DisplayTilemapInfo(tilemap);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DisplayTilemapInfo(Tilemap tilemap)
        {
            TileBase[] uniqueTiles = GetUniqueTilesFromTilemap(tilemap);
            Sprite[] uniqueSprites = GetUniqueSpritesFromTilemap(tilemap);

            // Display unique tiles
            EditorGUILayout.LabelField("Tiles", EditorStyles.boldLabel);
            foreach (var tile in uniqueTiles)
            {
                EditorGUILayout.ObjectField(tile, typeof(TileBase), false);
            }

            // Display unique sprites
            EditorGUILayout.LabelField("Sprites", EditorStyles.boldLabel);
            foreach (var sprite in uniqueSprites)
            {
                EditorGUILayout.ObjectField(sprite, typeof(Sprite), false);
            }
        }

        private TileBase[] _usedTilesCache;
        private TileBase[] GetUniqueTilesFromTilemap(Tilemap tilemap)
        {
            int usedTilesCount = tilemap.GetUsedTilesCount();
            if (_usedTilesCache == null || _usedTilesCache.Length != usedTilesCount)
            {
                _usedTilesCache = new TileBase[usedTilesCount];
            }
            tilemap.GetUsedTilesNonAlloc(_usedTilesCache);

            return _usedTilesCache;
        }

        private Sprite[] _usedSpritesCache;
        private Sprite[] GetUniqueSpritesFromTilemap(Tilemap tilemap)
        {
            int usedSpritesCount = tilemap.GetUsedSpritesCount();
            if (_usedSpritesCache == null || _usedSpritesCache.Length != usedSpritesCount)
            {
                _usedSpritesCache = new Sprite[usedSpritesCount];
            }
            tilemap.GetUsedSpritesNonAlloc(_usedSpritesCache);

            return _usedSpritesCache;
        }

    }
}
