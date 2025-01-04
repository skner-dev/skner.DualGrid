using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using skner.DualGrid.Extensions;

namespace skner.DualGrid.Editor
{
    [CustomEditor(typeof(Tilemap))]
    public class RestrictedTilemapEditor : UnityEditor.Editor
    {

        private bool showInfoFoldout = true;

        public override void OnInspectorGUI()
        {
            Tilemap tilemap = (Tilemap)target;

            tilemap.animationFrameRate = EditorGUILayout.FloatField("Animation Frame Rate", tilemap.animationFrameRate);
            tilemap.color = EditorGUILayout.ColorField("Color", tilemap.color);

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
        }

        private void DisplayTilemapInfo(Tilemap tilemap)
        {
            var uniqueTiles = new HashSet<TileBase>();
            var uniqueSprites = new HashSet<Sprite>();

            foreach (var position in tilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = tilemap.GetTile(position);
                if (tile != null)
                {
                    uniqueTiles.Add(tile);
                }

                Sprite sprite = tilemap.GetSprite(position);
                if (sprite != null)
                {
                    uniqueSprites.Add(sprite);
                }
            }

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
                EditorGUILayout.ObjectField(sprite, typeof(TileBase), false);
            }
        }

    }
}
