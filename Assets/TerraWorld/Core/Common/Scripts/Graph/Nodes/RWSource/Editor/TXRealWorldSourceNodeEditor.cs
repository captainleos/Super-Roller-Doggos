#if TERRAWORLD_XPRO


using XNodeEditor;
using UnityEditor;
using TerraUnity.Edittime;
using UnityEngine;
using UnityEngine.UI;

namespace TerraUnity.Graph.Editor
{
    [CustomNodeEditor(typeof(TXRealWorldSourceNode))]
    public class TXRealWorldSourceNodeEditor : TXNodeEditor
    {
        private TXRealWorldSourceNode module;

        // private TXHeightmapSourceNode RelatedNode
        // {
        //     get
        //     {
        //         return (TXHeightmapSourceNode)target;
        //     }
        // }

        public override void OnBodyGUI()
        {
            //base.OnBodyGUI();
            if (module == null) module = target as TXRealWorldSourceNode;
      
            //Output / Mask Input
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("HeightmapSource"), new GUIContent("Heightmap Source"));
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("highestResolution"), new GUIContent("Highest Resolution"));
            if (!module.highestResolution)
            {
                GUILayout.Label(new GUIContent("RESOLUTION", "Heightmap data will be resampled to given resolution"));
                module.HeightmapResolution = EditorGUILayout.IntSlider(Mathf.ClosestPowerOfTwo(module.HeightmapResolution), 32, 4096);
            }
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Heightmap"), new GUIContent("Heightmap"));

           // EditorGUILayout.Space();

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("ImagerySource"), new GUIContent("Image Source"));
            GUILayout.Label(new GUIContent("RESOLUTION", "Heightmap data will be resampled to given resolution"));
            module.ImageryResolution = EditorGUILayout.IntSlider(Mathf.ClosestPowerOfTwo(module.ImageryResolution), 32, 4096);
            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Image"), new GUIContent("Satellite Image"));

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Landcover_Data"), new GUIContent("Landcover Data"));
            //    // Update serialized object's representation
            //    serializedObject.Update();
            //
            //    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("a"));
            //    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("b"));
            //    //UnityEditor.EditorGUILayout.LabelField("The value is " + simpleNode.Sum());
            //    NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("sum"));

            // Apply property modifications
            serializedObject.ApplyModifiedProperties();
      
        }




            //
            //    public override string GetTitle()
            //    {
            //        return TITLE;
            //    }
            //
            //    public override Color GetTint()
            //    {
            //        return EditorColors.TintBiome;
            //    }
            //
            //    /// <summary>
            //    /// Shows a map in this node with the passed information
            //    /// </summary>
            //    /// <param name="mapProperty">Name of the serialized map generator property</param>
            //    /// <param name="minMaxMaskProperty">Name of the serialized float min max mask property</param>
            //    /// <param name="useMapProperty">Name of the serialized bool use map property</param>
            //    /// <param name="displayName">Display name of this map (heightmap, temperature, etc)</param>
            //    private void ShowMapField(string mapProperty, string minMaxMaskProperty, string useMapProperty, string displayName)
            //    {
            //        SerializedProperty mapProp = serializedObject.FindProperty(mapProperty);
            //        SerializedProperty minMaxProp = serializedObject.FindProperty(minMaxMaskProperty);
            //        SerializedProperty useMapProp = serializedObject.FindProperty(useMapProperty);
            //
            //        EditorGUILayout.PropertyField(useMapProp, new GUIContent(displayName));
            //
            //        //Use this map as a mask
            //        if (useMapProp.boolValue)
            //        {
            //            EditorGUI.indentLevel++;
            //
            //            NodeEditorGUILayout.PropertyField(mapProp, new GUIContent("Generator"));
            //
            //            //Min / Max Slider
            //            Rect ctrl = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            //            EditorGUI.BeginProperty(ctrl, GUIContent.none, minMaxProp);
            //
            //            EditorGUI.BeginChangeCheck();
            //
            //            Vector2 minMaxVal = minMaxProp.vector2Value;
            //            EditorGUI.MinMaxSlider(ctrl, ref minMaxVal.x, ref minMaxVal.y, 0f, 1f);
            //
            //            //Modify serialized value if changed
            //            if (EditorGUI.EndChangeCheck())
            //            {
            //                minMaxProp.vector2Value = minMaxVal;
            //            }
            //
            //            EditorGUI.EndProperty();
            //            EditorGUI.indentLevel--;
            //        }
            //    }
    }
}

#endif
