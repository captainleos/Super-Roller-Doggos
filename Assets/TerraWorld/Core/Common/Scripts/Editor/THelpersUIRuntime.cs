#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using TerraUnity.Utils;

namespace TerraUnity.Runtime
{
    public class THelpersUIRuntime : EditorWindow
    {
        private static Color enabledColor = Color.white;
        private static int objectMaskValue = 0;
        private static List<UnityEngine.Object> dynamicObjectsList = new List<UnityEngine.Object>();
        public static Texture2D nodeMask;
        public static bool showOutputMask;
        public static bool showPreviewMask;
        public static Vector3 editorCameraPosition;
        public static Quaternion editorCameraRotation;
        public static Color UIColor = new Color(0.3413334f, 0.457867f, 0.5019608f, 0.3215686f);
        public static Color SubUIColor = new Color(0.1413334f, 0.257867f, 0.3019608f, 0.1215686f);

        private static void InputColors(Color guiColor, Color backgroundColor, Color contentColor)
        {
            if (guiColor.a != 0) GUI.color = guiColor;
            if (backgroundColor.a != 0) GUI.backgroundColor = backgroundColor;
            if (contentColor.a != 0) GUI.contentColor = contentColor;
        }

        private static void OutputColors(Color guiColor, Color backgroundColor, Color contentColor)
        {
            if (guiColor.a != 0) GUI.color = Color.white;
            if (backgroundColor.a != 0) GUI.backgroundColor = Color.white;
            if (contentColor.a != 0) GUI.contentColor = Color.white;
        }

        public static float GUI_Slider(GUIContent content, float value, float min, float max, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color(), string endText = "")
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Space(padLeft);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Slider(value, min, max);
            if (!string.IsNullOrEmpty(endText)) GUILayout.Label(endText);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return value;
        }

        // Generic GUI Elements
        //---------------------------------------------------------------------------------------------------------------------------------------------------

        public static Enum GUI_EnumPopup(GUIContent content, Enum enumList, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            enumList = EditorGUILayout.EnumPopup(enumList);
            GUILayout.Space(30);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return enumList;
        }

        public static void GUI_HelpBox(GUIContent content, bool wide = true, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox(content, wide);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_HelpBoxTitleWide (GUIContent content, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.fixedWidth = Screen.width;
            style.alignment = TextAnchor.MiddleLeft;
            style.fontSize = 18;
            //style.fontStyle = FontStyle.Bold;

            InputColors(guiColor, backgroundColor, contentColor);
            EditorGUILayout.SelectableLabel(content.text, style);

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_HelpBoxTitleLeft (GUIContent content, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;
            //style.fontStyle = FontStyle.Bold;

            style.fixedWidth = style.CalcSize(content).x + 10;

            InputColors(guiColor, backgroundColor, contentColor);
            EditorGUILayout.SelectableLabel(content.text, style);

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_HelpBoxInfo (string text, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 9;

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.SelectableLabel(text, style, GUILayout.Height(85));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_HelpBox(string text, MessageType messageType = MessageType.None, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox(text, messageType);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_HelpBox(string title, string text, MessageType messageType = MessageType.None, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(title);
            EditorGUILayout.HelpBox(text, messageType);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static int GUI_IntSlider(GUIContent content, int value, int min, int max, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.IntSlider(value, min, max);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return value;
        }

        public static int GUI_SelectionGrid(int selectionIndex, Texture2D[] icons, GUIStyle style, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            selectionIndex = GUILayout.SelectionGrid(selectionIndex, icons, icons.Length, style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return selectionIndex;
        }

        public static int GUI_SelectionGrid(int selectionIndex, string[] titles, GUIStyle style, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            selectionIndex = GUILayout.SelectionGrid(selectionIndex, titles, titles.Length, style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return selectionIndex;
        }

        public static int GUI_SelectionGrid(int selectionIndex, GUIContent text, string[] titles, GUIStyle style, int padLeft = 0, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            GUI_HelpBox(text, true);
            //GUILayout.Space(-padLeft);
            selectionIndex = GUILayout.SelectionGrid(selectionIndex, titles, titles.Length, style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return selectionIndex;
        }

        public static int GUI_SelectionGridToolbar(GUIContent content, int selectionIndex, string[] titles, GUIStyle style, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.Space(10);
            selectionIndex = GUILayout.SelectionGrid(selectionIndex, titles, titles.Length, style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return selectionIndex;
        }

        public static int GUI_SelectionGridWithTitle(GUIContent content, int selectionIndex, Texture[] images, GUIStyle style, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            selectionIndex = GUILayout.SelectionGrid(selectionIndex, images, images.Length, style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return selectionIndex;
        }

        public static void GUI_MinMaxSlider(GUIContent content, ref float minValue, ref float maxValue, float minLimit, float maxLimit, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            EditorGUILayout.MinMaxSlider(ref minValue, ref maxValue, minLimit, maxLimit);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            GUIStyle style = new GUIStyle(EditorStyles.numberField);
            style.fixedWidth = 100;
            style.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.HelpBox("MIN", MessageType.None);
            GUILayout.FlexibleSpace();
            minValue = EditorGUILayout.FloatField((float)TUtils.Clamp(minLimit, maxLimit, minValue), style);
            GUILayout.Space(100);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.HelpBox("MAX", MessageType.None);
            GUILayout.FlexibleSpace();
            maxValue = EditorGUILayout.FloatField((float)TUtils.Clamp(minLimit, maxLimit, maxValue), style);
            GUILayout.Space(100);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_MinMaxSlider(GUIContent content, ref double minValue, ref double maxValue, double minLimit, double maxLimit, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            float minValueF = (float)minValue;
            float maxValueF = (float)maxValue;
            float minLimitF = (float)minLimit;
            float maxLimitF = (float)maxLimit;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            EditorGUILayout.MinMaxSlider(ref minValueF, ref maxValueF, minLimitF, maxLimitF);

            minValue = minValueF;
            maxValue = maxValueF;

            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            GUIStyle style = new GUIStyle(EditorStyles.numberField);
            style.fixedWidth = 100;
            style.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.HelpBox("MIN", MessageType.None);
            GUILayout.FlexibleSpace();
            minValue = EditorGUILayout.FloatField((float)TUtils.Clamp(minLimit, maxLimit, minValue), style);
            GUILayout.Space(100);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            EditorGUILayout.HelpBox("MAX", MessageType.None);
            GUILayout.FlexibleSpace();
            maxValue = EditorGUILayout.FloatField((float)TUtils.Clamp(minLimit, maxLimit, maxValue), style);
            GUILayout.Space(100);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static string GUI_TextField(GUIContent content, string text, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            text = EditorGUILayout.TextField(text);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return text;
        }

        public static void GUI_LabelField(GUIContent content, string text, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(text);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static int GUI_Popup(string text, int value, string[] names, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Popup(value, names);
            GUILayout.Space(30);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return value;
        }

        public static int GUI_PopupCentered(string text, int value, string[] names, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(text);
            value = EditorGUILayout.Popup(value, names);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return value;
        }

        public static int GUI_PopupToolbar(string text, int value, string[] names, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(text);
            GUILayout.Space(10);
            value = EditorGUILayout.Popup(value, names);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return value;
        }

        public static Color GUI_ColorField(GUIContent content, Color color, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Space(padLeft);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            color = EditorGUILayout.ColorField(color);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return color;
        }

        public static int GUI_IntField(GUIContent content, int value, int minValue = int.MinValue, int maxValue = int.MaxValue, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.IntField(value);
            if (value < minValue) value = minValue;
            if (value > maxValue) value = maxValue;
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return value;
        }

        public static int GUI_DelayedIntField(GUIContent content, int value, int minValue = int.MinValue, int maxValue = int.MaxValue, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.DelayedIntField(value);
            if (value < minValue) value = minValue;
            if (value > maxValue) value = maxValue;
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return value;
        }

        public static float GUI_FloatField(GUIContent content, float value, float minValue = float.MinValue, float maxValue = float.MaxValue, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.FloatField(value);
            if (value < minValue) value = minValue;
            if (value > maxValue) value = maxValue;
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return value;
        }

        public static float GUI_DelayedFloatField(GUIContent content, float value, float minValue = float.MinValue, float maxValue = float.MaxValue, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.DelayedFloatField(value);
            if (value < minValue) value = minValue;
            if (value > maxValue) value = maxValue;
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return value;
        }

        public static bool GUI_Toggle(GUIContent content, bool state, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Space(padLeft);
            GUILayout.Label(content);
            state = EditorGUILayout.Toggle(state);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return state;
        }

        public static Vector2 GUI_Vector2Field(GUIContent content, Vector3 vector2D, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            vector2D = EditorGUILayout.Vector2Field("", vector2D);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return vector2D;
        }

        public static Vector3 GUI_Vector3Field(GUIContent content, Vector3 vector3D, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            vector3D = EditorGUILayout.Vector3Field("", vector3D);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return vector3D;
        }

        public static string GUI_LayerField(GUIContent content, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            GUIStyle style = new GUIStyle(EditorStyles.toolbarPopup);
            List<string> layers = new List<string>();

            for (int i = 0; i < 32; i++)
                if (!string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                    layers.Add(LayerMask.LayerToName(i));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            objectMaskValue = EditorGUILayout.Popup(objectMaskValue, layers.ToArray(), style);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return layers[objectMaskValue];
        }

        public static int GUI_MaskField(GUIContent content, LayerMask layerMask, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            GUIStyle style = new GUIStyle(EditorStyles.toolbarPopup);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();
            LayerMask tempMask = EditorGUILayout.MaskField(UnityEditorInternal.InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), UnityEditorInternal.InternalEditorUtility.layers, style);
            GUILayout.Space(20);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            layerMask = UnityEditorInternal.InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
            return layerMask;
        }

        public static void GUI_Label(Texture2D icon, Rect rect, GUIStyle style, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(padLeft);
            GUI.Label(rect, icon, style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_Label(Texture2D icon, GUIStyle style, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(padLeft);
            GUILayout.Label(icon, style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_Label(Texture2D icon, int width, int height, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(padLeft);
            GUILayout.Label(icon, GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_Label(GUIContent content, Rect rect, GUIStyle style, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(padLeft);
            GUI.Label(rect, content, style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_Button(Texture2D icon, GUIStyle style, Action action, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(padLeft);
            if (GUILayout.Button(icon, style))
            {
                if (action != null)
                    action.Invoke();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_Button(Texture2D icon, Rect rect, GUIStyle style, Action action, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(padLeft);
            if (GUI.Button(rect, icon, style))
            {
                if (action != null)
                    action.Invoke();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static void GUI_Button(GUIContent content, GUIStyle style, Action action, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(padLeft);
            if (GUILayout.Button(content, style))
            {
                if (action != null)
                    action.Invoke();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);
        }

        public static UnityEngine.Object GUI_ObjectField (GUIContent content, UnityEngine.Object obj, Type filter, GUILayoutOption[] layoutOptions = null, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Space(padLeft);
            GUILayout.Label(content);
            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();

            if (layoutOptions != null)
                obj = EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), true, layoutOptions);
            else
                obj = EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), true);

            if (EditorGUI.EndChangeCheck())
            {
                if (obj != null && filter != null)
                {
                    if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj)) && File.Exists(Path.GetFullPath(AssetDatabase.GetAssetPath(obj))))
                    {
                        Type t = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(obj), typeof(UnityEngine.Object)).GetType();

                        if (t != filter)
                        {
                            EditorUtility.DisplayDialog("INVALID FILE-TYPE", "Insert a file with type: " + filter.Name, "Ok");
                            return null;
                        }
                    }
                }
            }

            GUILayout.Space(30);
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return obj;
        }

        public static UnityEngine.Object GUI_ObjectFieldCentered (GUIContent content, UnityEngine.Object obj, Type filter, GUILayoutOption[] layoutOptions = null, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(padLeft);
            GUILayout.Label(content);

            EditorGUI.BeginChangeCheck();

            if (layoutOptions != null)
                obj = EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), true, layoutOptions);
            else
                obj = EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), true);

            if (EditorGUI.EndChangeCheck())
            {
                if (obj != null && filter != null)
                {
                    if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj)) && File.Exists(Path.GetFullPath(AssetDatabase.GetAssetPath(obj))))
                    {
                        Type t = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(obj), typeof(UnityEngine.Object)).GetType();

                        if (t != filter)
                        {
                            EditorUtility.DisplayDialog("INVALID FILE-TYPE", "Insert a file with type: " + filter.Name, "Ok");
                            return null;
                        }
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            OutputColors(guiColor, backgroundColor, contentColor);

            return obj;
        }

        public static void GetMaskValue(string layerName)
        {
            List<string> layers = new List<string>();
            int index = 0;

            for (int i = 0; i < 32; i++)
            {
                if (!string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    if (LayerMask.LayerToName(i) == layerName)
                    {
                        objectMaskValue = index;
                        break;
                    }

                    index++;
                }
            }
        }

        public static void InitDynamicObjectsList(List<string> names)
        {
            dynamicObjectsList = new List<UnityEngine.Object>();

            for (int i = 0; i < names.Count; i++)
                if(!string.IsNullOrEmpty(names[i])) dynamicObjectsList.Add(AssetDatabase.LoadAssetAtPath(names[i], typeof(UnityEngine.Object)));
        }

        public static T InitObjects<T>(string name) where T : UnityEngine.Object
        {
            if (!string.IsNullOrEmpty(name) && File.Exists(name))
                return AssetDatabase.LoadAssetAtPath(name, typeof(T)) as T;
            else
                return null;
        }

        public static Texture2D GUI_ShowMask(Texture2D icon, GUIStyle style, Action<Texture2D> action, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        {
            GUILayout.Space(10 + extraSpace);

            InputColors(guiColor, backgroundColor, contentColor);

            Texture2D image = new Texture2D(1, 1);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(icon, style))
            {
                if (action != null)
                    action.Invoke(image);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;

            return image;
        }

        //public static void GUI_LiveMaskPreview(int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
        //{
        //    if (filteredMask == null)
        //        return;
        //
        //    GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
        //    style.fixedWidth = maskResolution;
        //    style.fixedHeight = maskResolution;
        //
        //    GUI_Label(filteredMask, style);
        //}

        //public static void GUI_BoundingBox()
        //{
        //    TNode module = ActiveNode;
        //
        //    if (Foldout("BOUNDING BOX", ref module.uIToggles.BoundingBox))
        //    {
        //        GUIStyle style = new GUIStyle(EditorStyles.helpBox);
        //        style.fixedWidth = windowWidth;
        //
        //        if (!TAreaPreview.BoundsVisible)
        //        {
        //            TAreaPreview.GetBBox();
        //            SceneView.RepaintAll();
        //        }
        //
        //        if (module.uIToggles.BoundingBox)
        //        {
        //            GUILayout.Space(20);
        //
        //            if (TAreaPreview.BoundsVisible)
        //            {
        //                //Tools.current = Tool.Rect;
        //
        //                if (TBoundingBox.boundingBox != null && !TBoundingBox.boundingBox.activeSelf)
        //                    TBoundingBox.boundingBox.SetActive(true);
        //
        //                EditorGUI.BeginChangeCheck();
        //                GUI_MinMaxSlider(new GUIContent("WIDTH", "Module bounds Top geo-point"), ref module.areaBounds.left, ref module.areaBounds.right, (float)TTerraWorld.Area._left, (float)TTerraWorld.Area._right);
        //                GUI_MinMaxSlider(new GUIContent("LENGTH", "Module bounds Top geo-point"), ref module.areaBounds.bottom, ref module.areaBounds.top, (float)TTerraWorld.Area._bottom, (float)TTerraWorld.Area._top, -10);
        //                GUI_MinMaxSlider(new GUIContent("HEIGHT", "Minimum/Maximum elevation of the module bounds to perform action"), ref module.areaBounds.minElevation, ref module.areaBounds.maxElevation, TAreaPreview.MinElevation-50, TAreaPreview.MaxElevation+50, -10);
        //
        //                if (EditorGUI.EndChangeCheck())
        //                {
        //                    TBoundingBox.UpdateBoundingBox();
        //                    SceneView.RepaintAll();
        //                }
        //
        //                GUILayout.Space(20);
        //                style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 45; style.fixedHeight = 45;
        //
        //                EditorGUILayout.BeginHorizontal();
        //                GUILayout.FlexibleSpace();
        //                if (GUILayout.Button(new GUIContent(TResourcesManager.resetIcon, "Reset to World bounds"), style))
        //                {
        //                    TBoundingBox.ResetBoundingBox();
        //                }
        //                GUILayout.FlexibleSpace();
        //                EditorGUILayout.EndHorizontal();
        //
        //                GUILayout.Space(50);
        //            }
        //        }
        //    }
        //    else if (TAreaPreview.BoundsVisible)
        //    {
        //        TAreaPreview.Visible = false;
        //        SceneView.RepaintAll();
        //
        //        if (TBoundingBox.boundingBox != null && TBoundingBox.boundingBox.activeSelf)
        //            TBoundingBox.boundingBox.SetActive(false);
        //    }
        //}

        //public static void SwitchBoundingBox ()
        //{
        //    if (!TAreaPreview.BoundsVisible)
        //        TAreaPreview.GetBBox();
        //    else
        //        TAreaPreview.Visible = false;
        //
        //    SceneView.RepaintAll();
        //}

        public static void GUI_Alert()
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.x = lastRect.width - 30;
            lastRect.y -= 8;
            GUIStyle style = new GUIStyle();
            style.fixedWidth = 30;
            style.fixedHeight = 30;
            GUI_Label(TResourcesManager.cautionIcon, lastRect, style);
        }

        public static void DrawUILine (float space = 0)
        {
            GUILayout.Space(space);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        public static void DrawUILine (Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        public static bool Foldout (string sectionName, ref bool sectionToggle)
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.fixedWidth = Screen.width;
            style.fixedHeight = 20;
            sectionToggle = EditorGUILayout.Foldout(sectionToggle, "");
            GUILayout.Space(-35);
            GUI.backgroundColor = UIColor;
            sectionToggle = EditorGUILayout.Foldout(sectionToggle, sectionName, true, style);
            GUI.backgroundColor = enabledColor;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            return sectionToggle;
        }

        public static bool SectionSettings (ref bool sectionToggle, int padLeft = 0)
        {
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(padLeft);
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.fixedWidth = 80;
            style.fixedHeight = 20;
            style.alignment = TextAnchor.MiddleCenter;
            sectionToggle = EditorGUILayout.Foldout(sectionToggle, "");
            GUILayout.Space(-35);
            GUI.backgroundColor = SubUIColor;
            sectionToggle = EditorGUILayout.Foldout(sectionToggle, "SETTINGS", true, style);
            GUI.backgroundColor = enabledColor;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            return sectionToggle;
        }

        public static bool SectionSettingsWithTitle (ref bool sectionToggle, string title, int padLeft = 0)
        {
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(padLeft);
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.alignment = TextAnchor.MiddleCenter;

            //style.fixedWidth = style.CalcSize(new GUIContent(title, "")).x + 10;
            style.fixedWidth = 160;
            style.fixedHeight = 20;

            sectionToggle = EditorGUILayout.Foldout(sectionToggle, "");
            GUILayout.Space(-35);
            GUI.backgroundColor = UIColor;
            sectionToggle = EditorGUILayout.Foldout(sectionToggle, title.ToUpper(), true, style);
            GUI.backgroundColor = enabledColor;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            return sectionToggle;
        }
    }
}
#endif

