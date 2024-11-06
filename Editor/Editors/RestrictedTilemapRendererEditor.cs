using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace skner.DualGrid.Editor
{
    [CustomEditor(typeof(TilemapRenderer))]
    public class RestrictedTilemapRendererEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            TilemapRenderer tilemap = (TilemapRenderer)target;

            // Check if the Tilemap is part of a DualGridTilemapModule
            if (tilemap.GetComponentInParent<DualGridTilemapModule>() != null)
            {
                EditorGUILayout.HelpBox($"Editing is disabled on a RenderTilemap. The tilemap renderer is managed by the {nameof(DualGridTilemapModule)}.", MessageType.Info);
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