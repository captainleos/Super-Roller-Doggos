using UnityEngine;
using UnityEditor;

public class MaterialManager : EditorWindow
{
    //[MenuItem("Tools/TerraUnity/TerraWorld/Misc/Material Manager", false, 0)]
    public static void Init()
    {
        MaterialManager window = (MaterialManager)GetWindow(typeof(MaterialManager));
        window.position = new Rect(5, 135, 430, 800);
    }

    Vector2 scrollPosition = Vector2.zero;
    Transform[] objects;
    float tessellationQuality = 1;
    float tessellationMaxDistance = 0;
    float tessellationDisplacement = 0;
    Material[] parentMaterials;
    Material[] childMaterials;

    private enum TessellationMode
    {
        Distance,
        EdgeLength,
    }
    TessellationMode tessellationMode = TessellationMode.EdgeLength;

    public void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        GUILayout.Space(50);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("TESSELLATION SETTINGS", MessageType.None, true);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(30);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("MODE", MessageType.None, true);
        tessellationMode = (TessellationMode)EditorGUILayout.EnumPopup(tessellationMode);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("QUALITY", MessageType.None, true);
        tessellationQuality = EditorGUILayout.Slider(tessellationQuality, 1f, 150f);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("MAX DISTANCE", MessageType.None, true);
        tessellationMaxDistance = EditorGUILayout.Slider(tessellationMaxDistance, 0f, 10000f);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("DISPLACEMENT", MessageType.None, true);
        tessellationDisplacement = EditorGUILayout.Slider(tessellationDisplacement, 0f, 64f);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        objects = Selection.transforms;

        for (int i = 0; i < objects.Length; i++)
        {
            Renderer parentRenderer = objects[i].GetComponent<Renderer>();
            if (parentRenderer != null) parentMaterials = parentRenderer.sharedMaterials;

            foreach (Material m in parentMaterials)
            {
                if (tessellationMode == TessellationMode.EdgeLength)
                {
                    m.EnableKeyword("FT_EDGE_TESS");
                    m.SetFloat("_TessMode", 1);
                }  
                else
                {
                    m.DisableKeyword("FT_EDGE_TESS");
                    m.SetFloat("_TessMode", 0);
                }

                m.SetFloat("_Tess", tessellationQuality);
                m.SetFloat("_maxDist", tessellationMaxDistance);
                m.SetFloat("_Displacement", tessellationDisplacement);
            }  

            foreach(Transform t in objects[i].GetComponentsInChildren(typeof(Transform), true))
            {
                Renderer childRenderer = t.GetComponent<Renderer>();
                if (childRenderer != null) childMaterials = childRenderer.sharedMaterials;

                foreach (Material m in childMaterials)
                {
                    if (tessellationMode == TessellationMode.EdgeLength)
                    {
                        m.EnableKeyword("FT_EDGE_TESS");
                        m.SetFloat("_TessMode", 1);
                    }
                    else
                    {
                        m.DisableKeyword("FT_EDGE_TESS");
                        m.SetFloat("_TessMode", 0);
                    }

                    m.SetFloat("_Tess", tessellationQuality);
                    m.SetFloat("_maxDist", tessellationMaxDistance);
                    m.SetFloat("_Displacement", tessellationDisplacement);
                }
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
}

