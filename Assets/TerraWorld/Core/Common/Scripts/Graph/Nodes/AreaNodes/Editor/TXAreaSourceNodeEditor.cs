/*
 * using XNodeEditor;
using UnityEditor;
using TerraUnity.Edittime;
using UnityEngine;

namespace TerraUnity.Graph.Editor
{
    [CustomNodeEditor(typeof(TXAreaSourceNode))]
    public class TXAreaSourceNodeEditor : TXNodeEditor
    {
        private TXAreaSourceNode module;

        public override void OnBodyGUI()
        {
            if (GUILayout.Button(new GUIContent(TResourcesManager.locationIcon, "Display location on Interactive Map"), GUILayout.Width(50), GUILayout.Height(50)))
            {
                TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "DisplayLocation");
                TerraUnity.UI.InteractiveMapGUI.Init();
            }

            NodeEditorGUILayout.PropertyField(serializedObject.FindProperty("Output"), new GUIContent("Real World Position"));
        }
    }
}
*/

