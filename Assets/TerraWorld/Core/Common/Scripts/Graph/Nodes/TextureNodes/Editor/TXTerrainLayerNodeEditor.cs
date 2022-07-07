#if TERRAWORLD_XPRO

using XNodeEditor;
using UnityEditor;
using TerraUnity.Edittime;
using UnityEngine;

namespace TerraUnity.Graph.Editor
{
    [CustomNodeEditor(typeof(TXTerrainLayerNode))]
    public class TXTerrainLayerNodeEditor : TXNodeEditor
    {
        private TXTerrainLayerNode module;

        public override void OnBodyGUI()
        {
            //base.OnBodyGUI();
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (module == null) module = target as TXTerrainLayerNode;

            if (module.textureFrom == TXTerrainLayerNode.TextureFrom.Image)
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Image"), new GUIContent("Input Image"));

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Mask"), new GUIContent("Input Mask", "Drop Area here!"));

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("textureFrom"), new GUIContent("Texture Source"));
            if (module.textureFrom == TXTerrainLayerNode.TextureFrom.TerrainLayer)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("terrainLayer"), new GUIContent("Terrain Layer"));
            }
            else if (module.textureFrom == TXTerrainLayerNode.TextureFrom.Texture2D)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("terrainDiffuse"), new GUIContent("Diffuse"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("terrainNormalmap"), new GUIContent("Normal"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("terrainMaskmap"), new GUIContent("Mask"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("tiling"), new GUIContent("Tiling"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("tilingOffset"), new GUIContent("TilingOffset"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("specular"), new GUIContent("Specular"));

                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("metallic"), new GUIContent("Metallic"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("smoothness"), new GUIContent("Smoothness"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("normalScale"), new GUIContent("NormalScale"));

            }
            else if (module.textureFrom == TXTerrainLayerNode.TextureFrom.Image)
            {
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("tilingMode"), new GUIContent("Tiling Image Method"));
                if (module.tilingMode == TXTerrainLayerNode.TilingMode.Tiled)
                {
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("tiling"), new GUIContent("Tiling"));
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("tilingOffset"), new GUIContent("TilingOffset"));
                    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("specular"), new GUIContent("Specular"));
                }
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("metallic"), new GUIContent("Metallic"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("smoothness"), new GUIContent("Smoothness"));
                NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("normalScale"), new GUIContent("NormalScale"));

            }


            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("opacity"), new GUIContent("Opacity"));

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("TerrainLayer"), new GUIContent("Output TerrainLayer"));


            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

        }
    }
}

#endif
