#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace TerraUnity.Edittime
{
    public class TerrainPlatform : EditorWindow
    {
        [MenuItem("Tools/TerraUnity/Utils/Terrain Platform", false, 11)]
        static void Init()
        {
            TerrainPlatform window = (TerrainPlatform)GetWindow(typeof(TerrainPlatform));
            window.position = new Rect(5, 135, 480, 800);
            window.titleContent = new GUIContent("Terrain Platform", "Creates a 3D platform around the edges of terrain");
        }

        private GUIStyle style;
        private GameObject terrainObj;
        [Range(0f, 1f)] private float platformStartHeight = 0f;
        private static float[,] originalHeights;
        private static bool isRecovered = true;

        public void OnGUI()
        {
            GUI_HelpBoxInfo("TERRAIN PLATFORM is a helper component to extrude the edges of the inserted terrain surface in order to create platform at its base", 25);

            EditorGUI.BeginChangeCheck();
            terrainObj = (GameObject)GUI_ObjectField(new GUIContent("TERRAIN", "Insert Terrain to be processed here!"), terrainObj, typeof(GameObject), null, 40);
            if (EditorGUI.EndChangeCheck())
            {
                if (terrainObj != null && terrainObj.GetComponent<Terrain>() == null)
                {
                    EditorUtility.DisplayDialog("TERRAIN UNAVAILABLE", "Insert a gameobject with Terrain component on it!", "Ok");
                    terrainObj = null;
                    return;
                }
            }

            platformStartHeight = GUI_Slider(new GUIContent("PLATFORM START HEIGHT", "Height for the bottom of the platform"), platformStartHeight, 0f, 1f);

            style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 170; style.fixedHeight = 40;
            GUI_Button(new GUIContent("CREATE PLATFORM", "Create 3D platform for the terrain"), style, CreatePlatform, 20);
            GUI_Button(new GUIContent("RECOVER TERRAIN", "Recover terrain heights before creating the platform!"), style, RecoverTerrain);
        }

        private void CreatePlatform()
        {
            if (terrainObj == null)
            {
                EditorUtility.DisplayDialog("TERRAIN UNAVAILABLE", "Please first insert a terrain from the scene!", "Ok");
                return;
            }

            if (EditorUtility.DisplayDialog("3D PLATFORM CREATION", "Are you sure you want to create platform for the inserted terrain?\n\nYou can press RECOVER TERRAIN button to take terrain to its initial state later!", "No", "Yes"))
                return;

            Terrain terrain = terrainObj.GetComponent<Terrain>();
            TerrainData tData = terrain.terrainData;
            int terrainWidth = tData.heightmapResolution;
            int terrainHeight = tData.heightmapResolution;

            if (isRecovered)
                originalHeights = tData.GetHeights(0, 0, terrainWidth, terrainHeight);

            float[,] platformHeights = tData.GetHeights(0, 0, terrainWidth, terrainHeight);

            // Top Row
            for (int i = 0; i < terrainWidth; i++)
                platformHeights[i, 0] = platformStartHeight;

            // Bottom Row
            for (int i = 0; i < terrainWidth; i++)
                platformHeights[i, terrainHeight - 1] = platformStartHeight;

            // Left Column
            for (int i = 0; i < terrainHeight; i++)
                platformHeights[0, i] = platformStartHeight;

            // Right Column
            for (int i = 0; i < terrainHeight; i++)
                platformHeights[terrainWidth - 1, i] = platformStartHeight;

            tData.SetHeights(0, 0, platformHeights);
            terrain.Flush();
            isRecovered = false;
        }

        private void RecoverTerrain()
        {
            if (terrainObj == null)
            {
                EditorUtility.DisplayDialog("TERRAIN UNAVAILABLE", "Please first insert a terrain from the scene!", "Ok");
                return;
            }

            if (originalHeights == null || originalHeights.Length == 0)
            {
                EditorUtility.DisplayDialog("TERRAIN UNAVAILABLE", "No created platforms detected!", "Ok");
                return;
            }

            Terrain terrain = terrainObj.GetComponent<Terrain>();
            TerrainData tData = terrain.terrainData;
            tData.SetHeights(0, 0, originalHeights);
            terrain.Flush();
            isRecovered = true;
        }


        // GUI Helpers
        //-----------------------------------------------------------------------------------------------------------------------------------

        private void InputColors(Color guiColor, Color backgroundColor, Color contentColor)
        {
            if (guiColor.a != 0) GUI.color = guiColor;
            if (backgroundColor.a != 0) GUI.backgroundColor = backgroundColor;
            if (contentColor.a != 0) GUI.contentColor = contentColor;
        }

        private void OutputColors(Color guiColor, Color backgroundColor, Color contentColor)
        {
            if (guiColor.a != 0) GUI.color = Color.white;
            if (backgroundColor.a != 0) GUI.backgroundColor = Color.white;
            if (contentColor.a != 0) GUI.contentColor = Color.white;
        }

        private void GUI_HelpBoxInfo(string text, int extraSpace = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
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

        private UnityEngine.Object GUI_ObjectField(GUIContent content, UnityEngine.Object obj, Type filter, GUILayoutOption[] layoutOptions = null, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
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

        private float GUI_Slider(GUIContent content, float value, float min, float max, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color(), string endText = "")
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

        private void GUI_Button(GUIContent content, GUIStyle style, Action action, int extraSpace = 0, int padLeft = 0, Color guiColor = new Color(), Color backgroundColor = new Color(), Color contentColor = new Color())
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
    }
}
#endif

