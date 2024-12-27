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
            if (tilemap.GetComponent<DualGridTilemapModule>() != null)
            {
                EditorGUILayout.HelpBox($"Editing is disabled on this Tilemap because it is managed by the {nameof(DualGridTilemapModule)} on the same GameObject.", MessageType.Warning);
                GUI.enabled = false; 
                DrawDefaultInspector();
                GUI.enabled = true; 
            }
            else if (tilemap.GetComponentInParent<DualGridTilemapModule>() != null)
            {
                EditorGUILayout.HelpBox($"This Tilemap is managed by the {nameof(DualGridTilemapModule)} on a parent GameObject. Only the 'Color' property is editable.", MessageType.Info);

                serializedObject.Update();

                SerializedProperty property = serializedObject.GetIterator();
                property.NextVisible(true);

                while (property.NextVisible(false)) 
                {
                    // For Tilemap that are child of data tilemap I want to be able to modify color
                    // I will never understand how color is a property of Tilemap and not TilemapRenderer
                    GUI.enabled = property.name == "m_Color";

                    EditorGUILayout.PropertyField(property, true); 
                }

                GUI.enabled = true;
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                DrawDefaultInspector();
            }
        }
    }
}
