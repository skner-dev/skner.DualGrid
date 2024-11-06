using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    [CustomEditor(typeof(Tilemap))]
    public class RestrictedTilemapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            Tilemap tilemap = (Tilemap)target;

            // Check if the Tilemap is part of a DualGridTilemapModule
            if (tilemap.GetComponentInParent<DualGridTilemapModule>() != null)
            {
                EditorGUILayout.HelpBox($"Editing is disabled on a RenderTilemap. The tilemap is managed by the {nameof(DualGridTilemapModule)}.", MessageType.Info);
                GUI.enabled = false;
                DrawDefaultInspector();
                GUI.enabled = true;
            }
            else
            {
                DrawDefaultInspector();
            }
        }
    }
}
