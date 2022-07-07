#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TerraUnity.UI
{
    public class ModelPreviewUI : Editor
    {
        private static Editor gameObjectEditor;
        private static List<Editor> gameObjectEditorList;
        private static List<Object> targetObjects;

        public static void InitPreview(Object targetObject)
        {
            gameObjectEditor = CreateEditor(targetObject);
        }

        public static void ModelPreview(Object targetObject, GUIStyle style, int resolution = 128)
        {
            if (gameObjectEditor == null)
                InitPreview(targetObject);
            else
                gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(resolution, resolution), style);
        }

        public static void DestroyPreviewEditor()
        {
            try { if (Event.current.type == EventType.Repaint && gameObjectEditor != null) DestroyImmediate(gameObjectEditor); } catch {}
        }

        public static void ModelPreviewList(Object targetObject, GUIStyle style, int resolution = 128)
        {
            try
            {
                if (targetObject != null)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    int index = InitPreviewList(targetObject);
                    if (index != -1) gameObjectEditorList[index].OnInteractivePreviewGUI(GUILayoutUtility.GetRect(resolution, resolution), style);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(20);
                }
            }
#if TERRAWORLD_DEBUG
                catch (System.Exception e)
                {
                    throw e;
                }
#else
            catch { }
#endif
        }

        private static int InitPreviewList(Object targetObject)
        {
            if (targetObjects == null)
            {
                DestroyPreviewEditorList();
                targetObjects = new List<Object>();
            }

            if (!targetObjects.Contains(targetObject)) targetObjects.Add(targetObject);

            if (gameObjectEditorList == null || gameObjectEditorList.Count == 0 || gameObjectEditorList.Count != targetObjects.Count)
            {
                gameObjectEditorList = new List<Editor>();

                for (int i = 0; i < targetObjects.Count; i++)
                    gameObjectEditorList.Add(null);
            }

            int index = -1;

            for (int i = 0; i < targetObjects.Count; i++)
            {
                if (gameObjectEditorList[i] == null)
                    gameObjectEditorList[i] = CreateEditor(targetObjects[i]);

                if (targetObject == targetObjects[i]) index = i;
            }

            return index;
        }

        public static void DestroyPreviewEditorList()
        {
            if (gameObjectEditorList != null)
                for (int i = 0; i < gameObjectEditorList.Count; i++)
                    if (gameObjectEditorList[i] != null)
                        DestroyImmediate(gameObjectEditorList[i]);

            if (targetObjects != null) targetObjects.Clear();
        }
    }
}
#endif

