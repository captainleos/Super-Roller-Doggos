#if (TERRAWORLD_PRO || TERRAWORLD_LITE)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TerraUnity.UI;

namespace TerraUnity.Edittime
{
    public class SearchResultsDisplay : EditorWindow
    {
        public Vector2 scrollPosition = Vector2.zero;
        public List<double[]> coordinates;
        public List<string> locations;

        private float windowWidth = 500;
        private float windowHeight = 384;
        private int res, gridX, gridY;

        private void OnEnable ()
        {
            TResourcesManager.LoadSearchResultResources();
            res = TResourcesManager.BG.width;
            gridX = (int)(windowWidth / res) + 1;
            gridY = (int)(windowHeight / res) + 1;
        }

        public void OnGUI ()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);

            if (coordinates != null && locations != null)
            {
                GUILayout.Space(20);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("SEARCH RESULTS", style);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                style.fixedWidth = windowWidth;
                style.fixedHeight = windowHeight;

                if (Event.current.isMouse)
                {
                    windowWidth = GetWindow(this.GetType()).position.width;
                    windowHeight = GetWindow(this.GetType()).position.height;
                    gridX = (int)(windowWidth / res) + 1;
                    gridY = (int)(windowHeight / res) + 1;
                }
                GUI.color = new Color(1, 1, 1, 0.4f);
                for (int x = 0; x < gridX; x++)
                {
                    for (int y = 1; y < gridY; y++)
                    {
                        GUILayout.BeginArea(new Rect(x * res, y * res, res + 2, res + 2));
                        GUILayout.Label(TResourcesManager.BG, style);
                        GUILayout.EndArea();
                    }
                }
                GUI.color = Color.white;

                GUILayout.Space(34);

                for (int i = 0; i < coordinates.Count; i++)
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                
                    style = new GUIStyle(EditorStyles.label);
                    style.fontSize = 11;
                
                    GUILayout.Label(locations[i], style);
                
                    GUILayout.Space(5);
                    EditorGUILayout.TextArea(coordinates[i][0].ToString() + " " + coordinates[i][1].ToString());
                    GUILayout.Space(5);

                    //GUI.backgroundColor = new Color(1, 1, 1, 0.4f);
                    style = new GUIStyle(EditorStyles.toolbarButton);
                    style.fixedWidth = 36;
                    style.fixedHeight = 36;

                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    lastRect.x = windowWidth - 40;// Screen.width - 40;
                    lastRect.y -= 8;
                    lastRect.width = 36;
                    lastRect.height = 36;

                    if (GUI.Button(lastRect, TResourcesManager.placeMarker, style))
                    {
                        WorldArea worldArea = TTerraWorld.WorldGraph.areaGraph.WorldArea;
                        worldArea.latitude = coordinates[i][0].ToString();
                        worldArea.longitude = coordinates[i][1].ToString();
                        TerraWorld.ShowMapAndRefresh();
                        this.Close();
                    }
                
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                
                    GUILayout.Space(14);
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}

#endif