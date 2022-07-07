/*
using System;
using UnityEngine;
using XNodeEditor;

namespace TerraUnity.Graph.Editor
{
    [CustomNodeEditor(typeof(TXHeightMapEndNode))]
    public class TXEndNodeEditor : TXNodeEditor
    {

        private TXHeightMapEndNode module;

        private readonly int NODE_WIDTH = 250;

        public override int GetWidth()
        {
            return NODE_WIDTH;
        }

        public override void OnBodyGUI()
        {
            // Draw default editor
            base.OnBodyGUI();

            if (module == null) module = target as TXHeightMapEndNode;

            //     // Update serialized object's representation
            //     serializedObject.Update();
            //     
            //     NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("HeightmapRawData"));
            //     //NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("b"));
            //     UnityEditor.EditorGUILayout.LabelField("Heighmap Data");
            //     //NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("sum"));
            //
            //     // Apply property modifications
            //     serializedObject.ApplyModifiedProperties();
        }
    }
}
*/
