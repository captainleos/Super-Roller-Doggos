using UnityEngine;
using UnityEditor;
using System;
using TerraUnity.UI;

namespace TerraUnity.Runtime
{
    [CustomEditor(typeof(PlayerInteractions))]
    public class PlayerInteractionsEditor : Editor
    {
        // Generic Parameters
        private PlayerInteractions script { get => (PlayerInteractions)target; }
        private Color enabledColor = Color.white;
        private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
        private string[] convertSelection = new string[] { "CONVERT", "BYPASS" };
        public bool advancedState = true;
        public bool targetState = true;

        private void OnEnable()
        {
            InteractiveTargets.GetPlayerTargets();
        }

        public override void OnInspectorGUI()
        {
            if (InteractiveTargets.playerTargets == null || InteractiveTargets.playerTargets.Count == 0) return;
            //DrawDefaultInspector();
            //Repaint();

            for (int i = 0; i < InteractiveTargets.playerTargets.Count; i++)
            {
                // Only display current target's settings
                if (InteractiveTargets.playerTargets[i] != script.gameObject) continue;

                PlayerInteractions playerInteractions = InteractiveTargets.playerTargets[i].GetComponent<PlayerInteractions>();

                if (playerInteractions.GPULayersList == null || playerInteractions.GPULayersList.Length == 0)
                    continue;

                if (playerInteractions.GPULayers != null && playerInteractions.GPULayers.Count > 0)
                {
                    if (THelpersUIRuntime.Foldout("ADVANCED SETTINGS", ref advancedState))
                    {
                        playerInteractions.maximumObjects = THelpersUIRuntime.GUI_IntSlider(new GUIContent("MAX. INTERACTIVE OBJECTS (POOL SIZE)", "Maximum count for interactive objects around target for each layer which will be updated dynamically in runtime"), playerInteractions.maximumObjects, 1, 128, 20);
                        playerInteractions.deadZoneMeters = THelpersUIRuntime.GUI_Slider(new GUIContent("DEAD ZONE UNITS", "Only update interactables if target moves in all directions every Dead Zone Units\n\nHigher value gives better performance but with less accurate GPU to CPU object conversion and suited for less dense environments\n\n0 means it will update interactive objects every frame"), playerInteractions.deadZoneMeters, 0f, 10f, -10);
                        playerInteractions.checkDistance3D = THelpersUIRuntime.GUI_Toggle(new GUIContent("3D DISTANCE CHECK", "If this option is enabled, all distances will be calculated as a 3D sphere around target\n\nIf disabled, all distances will be calculated based on XZ plane ignoring the Y axis (Height) of the target suited for Top-Down view games\n\nEnable this option for the best performance"), playerInteractions.checkDistance3D, -10);
                        //playerInteractions.neighborPatchesCount = THelpersUIRuntime.GUI_IntSlider(new GUIContent("NEIGHBOR PATCHES COUNT", "Defines how many neighbor rendering patches should be taken into account from target position to convert GPU Instances to Interactable Objects\n\nLower value gives better performance as the instance list will get smaller"), playerInteractions.neighborPatchesCount, 1, 10, -10);

                        GUILayout.Space(30);
                    }

                    if (!THelpersUIRuntime.Foldout("LAYERS", ref targetState)) continue;

                    THelpersUIRuntime.GUI_HelpBox("Assign which layers around selected target should convert to interactable objects in game.\n\nFor the best performance, only select layers with colliders or if need specific interactions.", MessageType.Info, 20);
                    GUILayout.Space(20);

                    //THelpersUIRuntime.GUI_HelpBox("GPU LAYERS", MessageType.None);
                    //GUILayout.Space(20);

                    for (int j = 0; j < playerInteractions.GPULayers.Count; j++)
                    {
                        if (playerInteractions.GPULayers[j] == null) InteractiveTargets.GetPlayerTargets();
                        continue;
                    }

                    // Runtime GPU Instance layers in scene
                    for (int j = 0; j < playerInteractions.GPULayers.Count; j++)
                    {
                        THelpersUIRuntime.DrawUILine(THelpersUIRuntime.SubUIColor, 2, 20);
                        GameObject layerObject = playerInteractions.GPULayers[j].transform.parent.gameObject;

                        if (!layerObject.activeSelf)
                            THelpersUIRuntime.GUI_HelpBox("Layer is disabled in scene!", MessageType.Error, 20);

                        EditorGUI.BeginDisabledGroup(!layerObject.activeSelf || !playerInteractions.statesGPU[j]);
                        if (!layerObject.activeSelf) playerInteractions.statesGPU[j] = false;

                        int state = Convert.ToInt32(!playerInteractions.statesGPU[j]);
                        EditorGUI.BeginChangeCheck();
                        state = THelpersUIRuntime.GUI_SelectionGrid(state, convertSelection, new GUIStyle(EditorStyles.toolbarButton));
                        if (EditorGUI.EndChangeCheck()) playerInteractions.statesGPU[j] = !Convert.ToBoolean(state);

                        GUILayout.Space(5);

                        if (playerInteractions.GPULayersList[j] == null || playerInteractions.GPULayersList[j].Prefab == null)
                            continue;

                        if (playerInteractions.GPULayersList[j] == null || playerInteractions.GPULayersList[j].Prefab == null)
                            continue;

                        if (playerInteractions.GPULayersList[j].Prefab != null)
                        {
                            if (!layerObject.activeSelf || !playerInteractions.statesGPU[j]) GUI.color = disabledColor;
                            else GUI.color = enabledColor;
                            ModelPreviewUI.ModelPreviewList(playerInteractions.GPULayersList[j].Prefab, new GUIStyle(EditorStyles.helpBox), 128);
                            GUI.color = enabledColor;
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        THelpersUIRuntime.GUI_ObjectField(new GUIContent("Layer " + (j + 1).ToString(), "GPU Instance layer for this target"), playerInteractions.GPULayers[j].transform.parent.gameObject, typeof(GameObject));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (!playerInteractions.GPULayersList[j].GetPrefabCollider())
                            THelpersUIRuntime.GUI_HelpBox("No Colliders detected on this layer's prefab!\nModels in this layer will not interact with Collision Detection & Physics!", MessageType.Warning);

                        playerInteractions.GPULayersDistances[j] = THelpersUIRuntime.GUI_Slider(new GUIContent("DISTANCE RADIUS", "Radius around player where GPU Instances will become normal GameObjects to be intractable"), playerInteractions.GPULayersDistances[j], 1, 500, 10);
                        if (playerInteractions.GPULayersDistances[j] > 200) THelpersUIRuntime.GUI_HelpBox("HIGHER VALUES MAY AFFECT PERFORMANCE ON DENSE LAYERS!", MessageType.Warning);

                        GUILayout.Space(40);
                        EditorGUI.EndDisabledGroup();
                    }
                }
                else
                    THelpersUIRuntime.GUI_HelpBox("No layers in scene! Options will become available after world generation!", MessageType.Warning);

                //if (playerInteractions.CollisionLayers != null && playerInteractions.CollisionLayers.Count > 0)
                //{
                //    THelpersUIRuntime.GUI_HelpBox("COLLISION LAYERS", MessageType.None);
                //    GUILayout.Space(20);
                //
                //    // GPU Collision layers in scene
                //    for (int j = 0; j < playerInteractions.CollisionLayers.Count; j++)
                //    {
                //        EditorGUILayout.BeginHorizontal();
                //        GUILayout.FlexibleSpace();
                //
                //        if (playerInteractions.CollisionLayers[j].activeSelf)
                //        {
                //            THelpersUIRuntime.GUI_ObjectField(new GUIContent("COLLISION " + (j + 1), "Collision layer for this target"), playerInteractions.CollisionLayers[j], typeof(GameObject));
                //            playerInteractions.statesCollision[j] = THelpersUIRuntime.GUI_Toggle(new GUIContent("", "Enable/Disable this layer"), playerInteractions.statesCollision[j]);
                //        }
                //        else
                //        {
                //            GUI.backgroundColor = disabledColor;
                //            THelpersUIRuntime.GUI_ObjectField(new GUIContent("COLLISION " + (j + 1), "Layer is disabled in hierarchy"), playerInteractions.CollisionLayers[j], typeof(GameObject));
                //            playerInteractions.statesCollision[j] = THelpersUIRuntime.GUI_Toggle(new GUIContent("", "Layer is disabled in hierarchy"), false);
                //            GUI.backgroundColor = Color.white;
                //        }
                //
                //        GUILayout.FlexibleSpace();
                //        EditorGUILayout.EndHorizontal();
                //    }
                //}
                //
                //if (playerInteractions.GameobjectLayers != null && playerInteractions.GameobjectLayers.Count > 0)
                //{
                //    THelpersUIRuntime.GUI_HelpBox("OBJECT LAYERS", MessageType.None);
                //    GUILayout.Space(20);
                //
                //    // Runtime Gameobject layers in scene
                //    for (int j = 0; j < playerInteractions.GameobjectLayers.Count; j++)
                //    {
                //        EditorGUILayout.BeginHorizontal();
                //        GUILayout.FlexibleSpace();
                //        THelpersUIRuntime.GUI_ObjectField(new GUIContent("OBJECT " + (j + 1), "Gameobject layer for this target"), playerInteractions.GameobjectLayers[j], typeof(GameObject));
                //        playerInteractions.statesGameobject[j] = THelpersUIRuntime.GUI_Toggle(new GUIContent("", "Enable/Disable this layer"), playerInteractions.statesGameobject[j]);
                //        GUILayout.FlexibleSpace();
                //        EditorGUILayout.EndHorizontal();
                //    }
                //}
                //
                //if (playerInteractions.FXLayers != null && playerInteractions.FXLayers.Count > 0)
                //{
                //    THelpersUIRuntime.GUI_HelpBox("FX LAYERS", MessageType.None);
                //    GUILayout.Space(20);
                //
                //    // Runtime FX layers in scene
                //    for (int j = 0; j < playerInteractions.FXLayers.Count; j++)
                //    {
                //        EditorGUILayout.BeginHorizontal();
                //        GUILayout.FlexibleSpace();
                //        THelpersUIRuntime.GUI_ObjectField(new GUIContent("FX " + (j + 1), "FX layer for this target"), playerInteractions.FXLayers[j], typeof(GameObject));
                //        playerInteractions.statesFX[j] = THelpersUIRuntime.GUI_Toggle(new GUIContent("", "Enable/Disable this layer"), playerInteractions.statesFX[j]);
                //        GUILayout.FlexibleSpace();
                //        EditorGUILayout.EndHorizontal();
                //    }
                //}

                THelpersUIRuntime.DrawUILine(30);
            }

            GUILayout.Space(30);
        }
    }
}

