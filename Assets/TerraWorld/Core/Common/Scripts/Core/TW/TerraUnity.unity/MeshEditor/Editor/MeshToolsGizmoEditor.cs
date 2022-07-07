#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TerraUnity.UI;

namespace TerraUnity.Runtime
{
    [CustomEditor(typeof(MeshToolsGizmo))]
    public class MeshToolsGizmoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();
            MeshToolsGizmo meshEditorGizmo = (MeshToolsGizmo)target;
            GUILayout.Space(20);
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            style.fixedWidth = 128;
            style.fixedHeight = 32;
            GUI.color = Color.red;
            THelpersUI.GUI_Button(new GUIContent("EXIT EDIT MODE", "Finish editing placement"), style, EditPlacement);
            GUI.color = Color.white;
            GUILayout.Space(10);
        }

        private void EditPlacement()
        {
            MeshToolsGizmo meshEditorGizmo = (MeshToolsGizmo)target;
            meshEditorGizmo._parent.EditMode = false;
        }
    }
}
#endif
#endif

