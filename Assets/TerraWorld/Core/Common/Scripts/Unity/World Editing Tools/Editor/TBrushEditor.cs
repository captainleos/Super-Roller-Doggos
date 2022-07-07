#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using TerraUnity.UI;

namespace TerraUnity.Runtime
{
    public class TBrushEditor : Editor
    {
        public WorldTools script { get => (WorldTools)target; }
        private Event e;

        public void MaskEditorGUI()
        {
            //Repaint();
            e = Event.current;
            SwitchEditMode();

            bool isLayerAvailableGPU = false;
            bool isLayerAvailableGrass = false;
            if (script.maskDataListGPU != null) isLayerAvailableGPU = true;
            if (script.maskDataListGrass != null) isLayerAvailableGrass = true;

            if (isLayerAvailableGPU || isLayerAvailableGrass)
            {
                TBrushFunctions.WT = script;
                WorldToolsParams.globalMode = true;

                if (script.liveSyncState == 0) GUI.color = WorldToolsParams.liveSyncColor;
                else if (script.liveSyncState == 1) GUI.color = WorldToolsParams.bypassColor;
                THelpersUIRuntime.GUI_HelpBox(new GUIContent("LIVE SYNC MODE", "If enabled, all terrain heights sculpting or texture painting changes will be detected and TerraWorld layers will be synced and updated based on new changes"), true);
                GUI.color = Color.white;

                int liveSyncState = script.liveSyncState;
                EditorGUI.BeginChangeCheck();
                liveSyncState = THelpersUIRuntime.GUI_SelectionGrid(liveSyncState, WorldToolsParams.onOffSelection, new GUIStyle(EditorStyles.toolbarButton));
                if (EditorGUI.EndChangeCheck())
                {
                    script.liveSyncState = liveSyncState;
                    DetectTerrainChanges.liveSync = !Convert.ToBoolean(script.liveSyncState);
                }

                if (script.liveSyncState == 0)
                    THelpersUIRuntime.GUI_HelpBox("LAYERS WILL AUTO-UPDATE WHEN TERRAIN HEIGHTS/TEXTURES CHANGE", MessageType.Info, -5);
                else if (script.liveSyncState == 1)
                    THelpersUIRuntime.GUI_HelpBox("LAYERS WILL NOT BE UPDATED IF TERRAIN HEIGHTS/TEXTURES CHANGE!", MessageType.Warning, -5);

                THelpersUIRuntime.DrawUILine(20);

                THelpersUIRuntime.GUI_HelpBox(new GUIContent("UNDO OPERATIONS", ""), true, 10);
                int undoMode = script.undoMode;
                EditorGUI.BeginChangeCheck();
                undoMode = THelpersUIRuntime.GUI_SelectionGrid(undoMode, WorldToolsParams.onOffSelection, new GUIStyle(EditorStyles.toolbarButton), -10);
                if (EditorGUI.EndChangeCheck()) script.undoMode = undoMode;

                if (script.undoMode == 0)
                    THelpersUIRuntime.GUI_HelpBox("TURN OFF FOR FASTER WORLD EDITING!", MessageType.Info, -5);
                else if (script.undoMode == 1)
                    THelpersUIRuntime.GUI_HelpBox("UNDO IS BYPASSED FOR FASTER WORLD EDITING!", MessageType.Warning, -5);

                GUILayout.Space(10);

                if (script.maskDataListGPU == null && script.maskDataListGrass == null)
                    THelpersUIRuntime.GUI_HelpBox("NO LAYERS ARE DETECTED IN SCENE!", MessageType.Warning);
                else
                {
                    GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 128; style.fixedHeight = 32;

                    if (WorldToolsParams.isEditMode)
                    {
                        THelpersUIRuntime.GUI_Button(new GUIContent("EXIT EDIT MODE", "Finish editing placement"), style, TBrushFunctions.EditPlacement, 10, 0, Color.red);
                        THelpersUIRuntime.GUI_HelpBox(WorldToolsParams.editingText, MessageType.None);

                        GUILayout.Space(30);

                        int maximumBrushSize = (int)script.Terrain.terrainData.size.x / 4;
                        int minimumBrushSize = Mathf.Clamp(Mathf.CeilToInt(script.Terrain.terrainData.size.x / WorldToolsParams.dataWidthBrush), 1, maximumBrushSize);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("brushRadius"), new GUIContent("BRUSH RADIUS", "Brush Radius for painting and editing placement"));
                        script.brushRadius = Mathf.Clamp(script.brushRadius, minimumBrushSize, maximumBrushSize);
                        GUILayout.Space(5);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("brushDensity"), new GUIContent("BRUSH DENSITY", "Brush Density for painting and editing placement"));

                        GUILayout.Space(30);
                        script.sectionToggle1 = THelpersUIRuntime.Foldout("LAYERS", ref script.sectionToggle1);

                        if (script.sectionToggle1)
                        {
                            if (isLayerAvailableGPU || isLayerAvailableGrass)
                            {
                                EditorGUI.BeginChangeCheck();
                                script.hideAllLayers = THelpersUIRuntime.GUI_Toggle(new GUIContent("HIDE ALL LAYERS", "Hide all layers in world"), script.hideAllLayers, 15);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    bool previousState = !script.hideAllLayers;

                                    if (script.hideAllLayers && !previousState)
                                        TBrushFunctions.IsolatePreview(null, true, true);
                                    else if (!script.hideAllLayers && previousState)
                                        TBrushFunctions.RevertPreview(null, true, true);
                                }
                            }

                            if (!script.hideAllLayers)
                            {
                                GUILayout.Space(15);

                                if (isLayerAvailableGPU)
                                {
                                    for (int i = 0; i < script.GPULayers.Count + 1; i++)
                                    {
                                        if (i < script.GPULayers.Count)
                                        {
                                            bool layerEditingState = i != WorldToolsParams.isolatedIndexGPU && WorldToolsParams.isolatedIndexGPU != -1;
                                            if (WorldToolsParams.isolatedIndexGrass != -1 && WorldToolsParams.isolatedIndexGrass != -2) layerEditingState = true;

                                            EditorGUI.BeginDisabledGroup(layerEditingState); //Freeze layer based on the isolated layer
                                            THelpersUIRuntime.DrawUILine(THelpersUIRuntime.SubUIColor, 2, 20);

                                            if (script.GPULayers[i] == null)
                                            {
                                                TBrushFunctions.RefreshLayers();
                                                return;
                                            }

                                            GameObject layerObject = script.GPULayers[i].transform.parent.gameObject;
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("editableGPU").GetArrayElementAtIndex(i), new GUIContent("EDITABLE", "Is layer editable?"));

                                            if (i == WorldToolsParams.isolatedIndexGPU || WorldToolsParams.isolatedIndexGPU == -1)
                                            {
                                                EditorGUI.BeginChangeCheck();
                                                GUILayout.Space(10);
                                                EditorGUILayout.PropertyField(serializedObject.FindProperty("isolateLayer"), new GUIContent("ISOLATE LAYER", "Isolate and only edit this layer"));
                                                if (EditorGUI.EndChangeCheck())
                                                {
                                                    serializedObject.ApplyModifiedProperties();

                                                    if (script.isolateLayer)
                                                    {
                                                        WorldToolsParams.isolatedIndexGPU = i;
                                                        WorldToolsParams.isolatedIndexGrass = -2;
                                                    }
                                                    else
                                                    {
                                                        WorldToolsParams.isolatedIndexGPU = -1;
                                                        WorldToolsParams.isolatedIndexGrass = -1;
                                                    }
                                                }
                                            }
                                            else GUILayout.Space(30);

                                            GUILayout.Space(10);

                                            EditorGUI.BeginDisabledGroup(!layerObject.activeSelf);
                                            if (script.GPULayers[i].Prefab != null)
                                            {
                                                if (!layerObject.activeSelf || layerEditingState) GUI.color = WorldToolsParams.disabledColor;
                                                else GUI.color = WorldToolsParams.enabledColor;
                                                ModelPreviewUI.ModelPreviewList(script.GPULayers[i].Prefab, new GUIStyle(EditorStyles.helpBox), 128);
                                                GUI.color = WorldToolsParams.enabledColor;
                                            }

                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.FlexibleSpace();
                                            THelpersUIRuntime.GUI_ObjectField(new GUIContent("Layer " + (i + 1).ToString(), "GPU Instance layer in scene"), layerObject, typeof(GameObject));
                                            GUILayout.FlexibleSpace();
                                            EditorGUILayout.EndHorizontal();
                                            EditorGUI.EndDisabledGroup();

                                            int state = 0;
                                            if (!layerObject.activeSelf) state = 1;
                                            EditorGUI.BeginChangeCheck();
                                            state = THelpersUIRuntime.GUI_SelectionGrid(state, WorldToolsParams.enableDisableSelection, new GUIStyle(EditorStyles.toolbarButton), 10);
                                            if (EditorGUI.EndChangeCheck()) layerObject.SetActive(!Convert.ToBoolean(state));

                                            style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 40; style.fixedHeight = 30;
                                            style.alignment = TextAnchor.MiddleCenter;
                                            style.fontSize = 25;

                                            GUILayout.Space(20);
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.FlexibleSpace();
                                            if (GUILayout.Button(new GUIContent("-", "Remove layer"), style))
                                            {
                                                TBrushFunctions.RemoveLayer(layerObject);
                                            }
                                            GUILayout.Space(10);
                                            if (GUILayout.Button(new GUIContent("+", "Duplicate layer"), style))
                                            {
                                                TBrushFunctions.DuplicateLayer(layerObject);
                                            }
                                            GUILayout.FlexibleSpace();
                                            EditorGUILayout.EndHorizontal();

                                            style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 150; style.fixedHeight = 32;
                                            GUILayout.Space(35);
                                            GUI.color = new Color(0.2f, 0.25f, 0.2f, 0.75f);
                                            EditorGUILayout.BeginHorizontal();
                                            GUILayout.FlexibleSpace();
                                            if (GUILayout.Button(new GUIContent("GO TO LAYER SETTINGS", "Press this button to go to this layer's Editing Tools and Settings!"), style))
                                            {
                                                TBrushFunctions.GoToEditorLayer(layerObject);
                                            }
                                            GUILayout.FlexibleSpace();
                                            EditorGUILayout.EndHorizontal();
                                            GUI.color = Color.white;

                                            GUILayout.Space(40);
                                            EditorGUI.EndDisabledGroup(); //Freeze layer based on the isolated layer
                                        }
                                        else
                                        {
                                            //THelpersUIRuntime.DrawUILine(THelpersUIRuntime.SubUIColor, 2, 20);
                                            //style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 100; style.fixedHeight = 40;
                                            //style.alignment = TextAnchor.MiddleCenter;
                                            //style.fontSize = 40;
                                            //THelpersUIRuntime.GUI_Button(new GUIContent("+", "Add layer"), style, AddLayer, 20);
                                        }
                                    }
                                }

                                if (isLayerAvailableGrass)
                                {
                                    for (int i = 0; i < script.grassLayers.Count; i++)
                                    {
                                        bool layerEditingState = i != WorldToolsParams.isolatedIndexGrass && WorldToolsParams.isolatedIndexGrass != -1;
                                        if (WorldToolsParams.isolatedIndexGPU != -1 && WorldToolsParams.isolatedIndexGPU != -2) layerEditingState = true;

                                        EditorGUI.BeginDisabledGroup(layerEditingState); //Freeze layer based on selected isolated layer
                                        THelpersUIRuntime.DrawUILine(THelpersUIRuntime.SubUIColor, 2, 20);
                                        GameObject layerObject = script.grassLayers[i].transform.gameObject;

                                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editableGrass").GetArrayElementAtIndex(i), new GUIContent("EDITABLE", "Is layer editable?"));

                                        if (i == WorldToolsParams.isolatedIndexGrass || WorldToolsParams.isolatedIndexGrass == -1)
                                        {
                                            EditorGUI.BeginChangeCheck();
                                            GUILayout.Space(10);
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("isolateLayer"), new GUIContent("ISOLATE LAYER", "Isolate and only edit this layer"));
                                            if (EditorGUI.EndChangeCheck())
                                            {
                                                serializedObject.ApplyModifiedProperties();

                                                if (script.isolateLayer)
                                                {
                                                    WorldToolsParams.isolatedIndexGrass = i;
                                                    WorldToolsParams.isolatedIndexGPU = -2;
                                                }
                                                else
                                                {
                                                    WorldToolsParams.isolatedIndexGrass = -1;
                                                    WorldToolsParams.isolatedIndexGPU = -1;
                                                }
                                            }
                                        }
                                        else GUILayout.Space(30);

                                        GUILayout.Space(10);

                                        EditorGUI.BeginDisabledGroup(!layerObject.activeSelf);
                                        if (script.grassLayers[i].MGP.Material != null)
                                        {
                                            if (!layerObject.activeSelf || layerEditingState) GUI.color = WorldToolsParams.disabledColor;
                                            else GUI.color = WorldToolsParams.enabledColor;
                                            ModelPreviewUI.ModelPreviewList(script.grassLayers[i].MGP.Material, new GUIStyle(EditorStyles.helpBox), 128);
                                            GUI.color = WorldToolsParams.enabledColor;
                                        }

                                        EditorGUILayout.BeginHorizontal();
                                        GUILayout.FlexibleSpace();
                                        THelpersUIRuntime.GUI_ObjectField(new GUIContent("Layer " + (i + 1).ToString(), "Grass layer in scene"), layerObject, typeof(GameObject));
                                        GUILayout.FlexibleSpace();
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUI.EndDisabledGroup();

                                        int state = 0;
                                        if (!layerObject.activeSelf) state = 1;
                                        EditorGUI.BeginChangeCheck();
                                        state = THelpersUIRuntime.GUI_SelectionGrid(state, WorldToolsParams.enableDisableSelection, new GUIStyle(EditorStyles.toolbarButton), 10);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            layerObject.SetActive(!Convert.ToBoolean(state));
                                            script.grassLayers[i].active = !Convert.ToBoolean(state);
                                            script.grassLayers[i].massiveGrass.Refresh();
                                        }

                                        style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 150; style.fixedHeight = 32;
                                        GUILayout.Space(35);
                                        GUI.color = new Color(0.2f, 0.25f, 0.2f, 0.75f);
                                        EditorGUILayout.BeginHorizontal();
                                        GUILayout.FlexibleSpace();
                                        if (GUILayout.Button(new GUIContent("GO TO LAYER SETTINGS", "Press this button to go to this layer's Editing Tools and Settings!"), style))
                                        {
                                            TBrushFunctions.GoToEditorLayer(layerObject);
                                        }
                                        GUILayout.FlexibleSpace();
                                        EditorGUILayout.EndHorizontal();
                                        GUI.color = Color.white;

                                        GUILayout.Space(40);
                                        EditorGUI.EndDisabledGroup(); //Freeze layer based on the isolated layer
                                    }
                                }
                            }
                        }

                        THelpersUIRuntime.GUI_HelpBoxTitleWide(new GUIContent("PLACEMENT FILTERING", ""), 40, Color.white, THelpersUIRuntime.UIColor);
                        THelpersUIRuntime.GUI_HelpBox("CHANGING VALUES IN THIS SECTION WILL OVERRIDE PER LAYER SETTINGS", MessageType.Warning, 10);
                        GUILayout.Space(20);
                        script.sectionToggleTexturesFiltering = THelpersUIRuntime.Foldout("TERRAIN TEXTURES FILTERING", ref script.sectionToggleTexturesFiltering);

                        if (script.sectionToggleTexturesFiltering)
                        {
                            THelpersUIRuntime.GUI_HelpBox("SELECT EACH TERRAIN TEXTURE'S EXCLUSION PERCENTAGE FOR PLACING MODELS ON THEM", MessageType.Info, 20);
                            style = new GUIStyle(); style.fixedWidth = 64; style.fixedHeight = 64;
                            int terrainLayersCount = script.Terrain.terrainData.terrainLayers.Length;
                            if (WorldToolsParams.terrainLayers == null || WorldToolsParams.terrainLayers.Length == 0 || WorldToolsParams.terrainLayers.Length != terrainLayersCount) WorldToolsParams.terrainLayers = script.Terrain.terrainData.terrainLayers;

                            if (script.exclusionGPU == null || script.exclusionGPU.Length == 0 || script.exclusionGPU.Length != terrainLayersCount)
                            {
                                serializedObject.Update();
                                script.exclusionGPU = new float[terrainLayersCount];
                                for (int i = 0; i < script.exclusionGPU.Length; i++) script.exclusionGPU[i] = 0f;
                                serializedObject.ApplyModifiedProperties();
                            }

                            if (script.exclusionGrass == null || script.exclusionGrass.Length == 0 || script.exclusionGrass.Length != terrainLayersCount)
                            {
                                serializedObject.Update();
                                script.exclusionGrass = new float[terrainLayersCount];
                                for (int i = 0; i < script.exclusionGrass.Length; i++) script.exclusionGrass[i] = 0f;
                                serializedObject.ApplyModifiedProperties();
                            }

                            GUILayout.Space(10);

                            for (int i = 0; i < terrainLayersCount; i++)
                            {
                                GUILayout.Space(60);
                                WorldToolsParams.lastRect = GUILayoutUtility.GetLastRect();
                                WorldToolsParams.lastRect.x = (Screen.width / 2) - (64 / 2);
                                WorldToolsParams.lastRect.y += 30;
                                THelpersUIRuntime.GUI_Label(WorldToolsParams.terrainLayers[i].diffuseTexture, WorldToolsParams.lastRect, style);
                                int space = 20;
                                int currentSliderIndex = -1;

                                EditorGUI.BeginChangeCheck();
                                if (isLayerAvailableGPU)
                                {
                                    for (int j = 0; j < script.exclusionOpacitiesListGPU.Count; j++)
                                    {
                                        if (j == 0)
                                        {
                                            GUILayout.Space(space);
                                            EditorGUI.BeginChangeCheck();
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("exclusionGPU").GetArrayElementAtIndex(i), new GUIContent("GPU LAYER EXCLUSION  %", "Filtering percentage to remove models based on this terrain layer's alphamap weight"));
                                            if (EditorGUI.EndChangeCheck()) currentSliderIndex = i;
                                        }

                                        if (currentSliderIndex == i && script.exclusionOpacitiesListGPU[j] != null && script.exclusionOpacitiesListGPU[j].Length == terrainLayersCount)
                                        {
                                            float normalizedValueReverse = (100 - script.exclusionGPU[i]) / 100f;
                                            script.exclusionOpacitiesListGPU[j][i] = normalizedValueReverse;
                                            script.GPULayers[j].exclusionOpacities[i] = normalizedValueReverse;
                                        }
                                    }
                                }
                                if (EditorGUI.EndChangeCheck()) TBrushFunctions.SyncDelayedGPU();

                                if (isLayerAvailableGPU && isLayerAvailableGrass) space = 0;
                                currentSliderIndex = -1;

                                EditorGUI.BeginChangeCheck();
                                if (isLayerAvailableGrass)
                                {
                                    for (int j = 0; j < script.exclusionOpacitiesListGrass.Count; j++)
                                    {
                                        if (j == 0)
                                        {
                                            GUILayout.Space(space);
                                            EditorGUI.BeginChangeCheck();
                                            EditorGUILayout.PropertyField(serializedObject.FindProperty("exclusionGrass").GetArrayElementAtIndex(i), new GUIContent("GRASS EXCLUSION  %", "Filtering percentage to remove models based on this terrain layer's alphamap weight"));
                                            if (EditorGUI.EndChangeCheck()) currentSliderIndex = i;
                                        }

                                        if (currentSliderIndex == i && script.exclusionOpacitiesListGrass[j] != null && script.exclusionOpacitiesListGrass[j].Length == terrainLayersCount)
                                        {
                                            float normalizedValueReverse = (100 - script.exclusionGrass[i]) / 100f;
                                            script.exclusionOpacitiesListGrass[j][i] = normalizedValueReverse;
                                            script.grassLayers[j].MGP.exclusionOpacities[i] = normalizedValueReverse;
                                        }
                                    }
                                }
                                if (EditorGUI.EndChangeCheck()) TBrushFunctions.SyncDelayedGrass();
                            }
                        }

                        GUILayout.Space(20);
                        script.sectionToggleLayersFiltering = THelpersUIRuntime.Foldout("LAYER BASED FILTERING", ref script.sectionToggleLayersFiltering);

                        if (script.sectionToggleLayersFiltering)
                        {
                            THelpersUIRuntime.GUI_HelpBox("SETUP LAYERMASK TO ALLOW PLACEMENT ON MODELS & SURFACES WITH SELECTED UNITY LAYERS", MessageType.Info, 20);
                            EditorGUI.BeginChangeCheck();
                            script.layerMask = THelpersUIRuntime.GUI_MaskField(new GUIContent("LAYER MASK", "Object placement will be applied on all selected layers"), script.layerMask, 20);
                            serializedObject.FindProperty("layerMask").intValue = script.layerMask;

                            script.includeGrassLayerFiltering = THelpersUIRuntime.GUI_Toggle(new GUIContent("INCLUDE GRASS LAYERS", "Do you want the layer-based filtering also apply on grass layers!"), script.includeGrassLayerFiltering, 25);

                            if (script.includeGrassLayerFiltering)
                                THelpersUIRuntime.GUI_HelpBox("THIS WILL DECREASE PERFORMANCE IF THERE ARE MULTIPLE DENSE LAYERS!", MessageType.Warning, -5);

                            if (EditorGUI.EndChangeCheck()) TBrushFunctions.SyncAllLayersWithLayerMask(script.layerMask, script.includeGrassLayerFiltering);
                        }

                        style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 128; style.fixedHeight = 20;
                        THelpersUIRuntime.GUI_Button(new GUIContent("FORCE UPDATE", "Force update & sync all layers' placement"), style, TBrushFunctions.SyncAllLayers, 60);
                        THelpersUIRuntime.GUI_Button(new GUIContent("REFRESH LAYERS", "Refresh layers list for latest scene changes"), style, TBrushFunctions.RefreshLayers);

                        if (WorldToolsParams.painting) THelpersUIRuntime.GUI_HelpBox("\n\tPAINTING\t\n", MessageType.None, 0, Color.cyan);
                        if (WorldToolsParams.erasing) THelpersUIRuntime.GUI_HelpBox("\n\tERASING\t\n", MessageType.None, 0, Color.cyan);
                        GUILayout.Space(10);
                    }
                    else
                    {
                        style = new GUIStyle(EditorStyles.toolbarButton);
                        style.fixedWidth = 128;
                        style.fixedHeight = 32;
                        THelpersUIRuntime.GUI_Button(new GUIContent("EDIT WORLD", "Paint and edit placement in scene"), style, TBrushFunctions.EditPlacement, 10);
                    }
                }

                GUILayout.Space(20);
            }
        }

        private void SwitchEditMode ()
        {
            if (e != null && e.keyCode == KeyCode.E)
            {
                if (!WorldToolsParams.isEditMode) WorldToolsParams.isEditMode = true;
                Repaint();
            }
            else if (e != null && e.keyCode == KeyCode.Escape)
            {
                if (WorldToolsParams.isEditMode) WorldToolsParams.isEditMode = false;
                Repaint();
            }
        }

        public void OnDestroy()
        {
            TBrushFunctions.OnDestroy();
        }
    }
}
#endif

