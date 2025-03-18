using skner.DualGrid.Editor.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace skner.DualGrid.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DualGridDataTile), true)]
    public class DualGridDataTileEditor : UnityEditor.Editor
    {

        private DualGridDataTile _targetDualGridRuleTile;

        private bool _hasMultipleTargets = false;
        private List<DualGridDataTile> _targetDualGridRuleTiles = new();

        public void OnEnable()
        {
            _targetDualGridRuleTile = (DualGridDataTile)target;
            _hasMultipleTargets = targets.Length > 1;

            if (_hasMultipleTargets) _targetDualGridRuleTiles = targets.Cast<DualGridDataTile>().ToList();
            else _targetDualGridRuleTiles = new List<DualGridDataTile>() { target as DualGridDataTile };
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This Dual Grid Data Tile contains important metadata for the correct usage of Dual Grid Tilemaps. Do not update these values.", MessageType.Warning);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.showMixedValue = targets.Length > 1 && _targetDualGridRuleTiles.HasDifferentValues(dualGridRuleTile => dualGridRuleTile.colliderType);
                EditorGUILayout.EnumPopup(new GUIContent("Collider"), _targetDualGridRuleTiles.First().colliderType);

                EditorGUI.showMixedValue = targets.Length > 1 && _targetDualGridRuleTiles.HasDifferentValues(dualGridRuleTile => dualGridRuleTile.gameObject);
                EditorGUILayout.ObjectField(new GUIContent("GameObject"), _targetDualGridRuleTiles.First().gameObject, typeof(GameObject), false);
            }
        }

    }
}
