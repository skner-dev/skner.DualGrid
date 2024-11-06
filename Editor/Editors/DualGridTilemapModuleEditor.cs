using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

namespace skner.DualGrid.Editor
{
    [CustomEditor(typeof(DualGridTilemapModule))]
    public class DualGridTilemapModuleEditor : UnityEditor.Editor
    {

        private DualGridTilemapModule _targetComponent;

        private void OnEnable()
        {
            _targetComponent = (DualGridTilemapModule)target;

            InitializeRenderTilemap();
        }

        internal void InitializeRenderTilemap()
        {
            if (_targetComponent.RenderTilemap == null)
            {
                CreateRenderTilemapObject();
                Debug.Log($"Created child RenderTilemap for DualGridTilemapModule {_targetComponent.name}.");
            }

            DestroyTilemapRendererInDataTilemap();
        }

        private GameObject CreateRenderTilemapObject()
        {
            GameObject renderTilemapObject = new GameObject("RenderTilemap");
            renderTilemapObject.transform.parent = _targetComponent.transform;
            renderTilemapObject.transform.localPosition = new Vector3(-0.5f, -0.5f, 0f); // Offset by half a tile (TODO: Confirm if tiles can have different dynamic sizes, this might not work under those conditions)

            Tilemap renderTilemap = renderTilemapObject.AddComponent<Tilemap>();
            renderTilemapObject.AddComponent<TilemapRenderer>();

            return renderTilemapObject;
        }

        private void DestroyTilemapRendererInDataTilemap()
        {
            TilemapRenderer renderer = _targetComponent.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                Debug.Log("Dual Grid Tilemaps cannot have TilemapRenderers in the same GameObject. TilemapRenderer has been destroyed.");
                DestroyImmediate(renderer);
            }
        }

        public override void OnInspectorGUI()
        {
            // Additional custom inspector options could go here if needed
            EditorGUILayout.HelpBox("DualGridTilemapModule manages the RenderTilemap linked to the DataTilemap.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            DrawDefaultInspector();

            if (EditorGUI.EndChangeCheck())
                _targetComponent.RefreshRenderTiles();

            if (GUILayout.Button("Refresh Render Tilemap"))
                _targetComponent.RefreshRenderTiles();
        }

    }
}
