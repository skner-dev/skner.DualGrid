using skner.DualGrid.Extensions;
using UnityEditor;
using UnityEngine;

namespace skner.DualGrid.Editor
{
    [CustomEditor(typeof(Transform))]
    public class RestrictedTransformEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            Transform transform = (Transform)target;

            // Check if this transform is from the RenderTilemap of a DualGridTilemapModule
            if (transform.GetComponentInImmediateParent<DualGridTilemapModule>() != null)
            {
                EditorGUILayout.HelpBox($"Editing is disabled on a RenderTilemap. The transform is managed by the {nameof(DualGridTilemapModule)}.", MessageType.Info);
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