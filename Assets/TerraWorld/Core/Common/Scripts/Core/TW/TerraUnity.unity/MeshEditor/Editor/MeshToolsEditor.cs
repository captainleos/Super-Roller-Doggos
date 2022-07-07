#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace TerraUnity.Runtime
{
    [CustomEditor(typeof(MeshTools))]
    public class MeshToolsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //  DrawDefaultInspector();

            MeshTools meshEditor = (MeshTools)target;

            GUILayout.Space(20);
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            style.fixedWidth = 128;
            style.fixedHeight = 32;

            if (meshEditor.EditMode)
            {
                GUI.color = Color.red;
                THelpersUIRuntime.GUI_Button(new GUIContent("EXIT EDIT MODE", "Finish editing placement"), style, EditPlacement);
                GUI.color = Color.white;
            }
            else
                THelpersUIRuntime.GUI_Button(new GUIContent("EDIT OBJECT", "Paint and edit placement in scene"), style, EditPlacement);

            GUILayout.Space(10);
        }

        private void EditPlacement()
        {
            MeshTools meshEditor = (MeshTools)target;
            meshEditor.EditMode = !meshEditor.EditMode;
        }
    }
}
#endif
#endif

