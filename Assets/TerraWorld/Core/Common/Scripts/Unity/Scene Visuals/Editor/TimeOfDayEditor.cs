using UnityEditor;
using UnityEngine;


namespace TerraUnity.Runtime
{
    [CustomEditor(typeof(TimeOfDay))]
    public class TimeOfDayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            //TimeOfDay script = (TimeOfDay)target;
            //THelpersUIRuntime.GUI_MinMaxSlider(new GUIContent("Stars Show/Hide normalized angle", "Minimum & Maximum of sun angles which stars will rendered."), ref script.starsRendererStartAngle, ref script.starsRendererEndAngle, 0f, 0.99f, -10);
        }
    }
}

