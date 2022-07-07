using UnityEditor;
using UnityEngine;
using TerraUnity.UI;

namespace TerraUnity.Runtime
{
    [CustomEditor(typeof(GrassLayer))]
    public class GrassLayerEditor : TBrushEditorGrass
    {
        // Generic Parameters
        private float m_LastEditorUpdateTime;
        private bool applyChanges = false;
        private float UIDelay = 1f;

        // Layer Classes
        private GrassLayer script;
        private string[] waterPlacementMode = new string[] { "BYPASS", "UNDER WATER", "ON WATER" };
        private int selectionIndexPlacement = 0;
        private SerializedObject serializedAsset;


        // Generic Methods
        //--------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += OnEditorUpdate;
            if (script == null) script = (GrassLayer)target;
            if (WT == null) WT = (GrassLayer)target;
            if (serializedAsset == null) serializedAsset = new SerializedObject(WT.MGP);
            ModelPreviewUI.InitPreview(serializedAsset.FindProperty("Material").objectReferenceValue);

            Terrain terrain = null;
            TTerraWorldTerrainManager[] TTM = FindObjectsOfType<TTerraWorldTerrainManager>();
            if (TTM != null && TTM.Length > 0 && TTM[0] != null) terrain = TTM[0].gameObject.GetComponent<Terrain>();

            if (terrain != null)
            {
                int terrainLayersCount = terrain.terrainData.terrainLayers.Length;

                if (WT.MGP.exclusionOpacities != null && WT.MGP.exclusionOpacities.Length != 0 && WT.MGP.exclusionOpacities.Length == terrainLayersCount)
                {
                    WT.exclusion = new float[terrainLayersCount];

                    for (int i = 0; i < WT.MGP.exclusionOpacities.Length; i++)
                        WT.exclusion[i] = (1f - WT.MGP.exclusionOpacities[i]) * 100f;
                }
            }

            TBrushFunctions.ConvertMaskDataToImage(WT.MGP.maskData);
#endif
        }

        protected virtual void OnDisable()
        {
            ModelPreviewUI.DestroyPreviewEditor();

#if UNITY_EDITOR
            EditorApplication.update -= OnEditorUpdate;
#endif
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected virtual void OnEditorUpdate()
        {
            if (applyChanges && Time.realtimeSinceStartup - m_LastEditorUpdateTime > UIDelay)
            {
                UpdatePlacement();
                applyChanges = false;
            }
        }

        public override void UpdatePlacement()
        {
            serializedAsset.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
            script.UpdateLayer();
        }

        public override void UpdatePlacementDelayed()
        {
            m_LastEditorUpdateTime = Time.realtimeSinceStartup;
            applyChanges = true;
        }


        // Layer Specific UI
        //--------------------------------------------------------------------------------------------------------------------------------------


        public override void OnInspectorGUI()
        {
            serializedAsset.Update();
            serializedObject.Update();

            GUILayout.Space(20);
            MaskEditorGUI();

            GUI.color = Color.white;
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
            GUILayout.Space(25);

            if (serializedAsset.FindProperty("Material").objectReferenceValue != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                style = new GUIStyle(EditorStyles.helpBox);
                ModelPreviewUI.ModelPreview(serializedAsset.FindProperty("Material").objectReferenceValue, style);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            //EditorGUI.BeginChangeCheck(); // Check for any UI changes
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();
            script.active = THelpersUIRuntime.GUI_Toggle(new GUIContent("ACTIVE", "Enable/Disable this layer"), script.active, -10);
            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(!script.active);

            EditorGUILayout.PropertyField(serializedAsset.FindProperty("Material"), new GUIContent("MATERIAL", "Material used for rendering"));
            if (EditorGUI.EndChangeCheck()) ModelPreviewUI.InitPreview(serializedAsset.FindProperty("Material").objectReferenceValue);

            EditorGUILayout.PropertyField(serializedAsset.FindProperty("Seed"), new GUIContent("SEED#", "Seed number for unique procedural values"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("BuilderType"), new GUIContent("MODEL TYPE", "Select between Quad (Billboards) and 3D Mesh for rendering"));

            if (script.MGP.BuilderType == Mewlist.MassiveGrass.BuilderType.FromMesh)
            {
                EditorGUILayout.PropertyField(serializedAsset.FindProperty("Mesh"), new GUIContent("MESH", "Mesh used for grass rendering"));
                if (serializedAsset.FindProperty("Mesh") == null) THelpersUIRuntime.GUI_Alert();
            }

            EditorGUILayout.PropertyField(serializedAsset.FindProperty("Scale"), new GUIContent("SCALE", "Scale of grass/plant blades"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("Slant"), new GUIContent("SLANT", "Slant amount for grass/plant blades"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("Radius"), new GUIContent("RENDERING DISTANCE", "Rendering Distance to draw vegetation"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("AmountPerBlock"), new GUIContent("AMOUNT PER BLOCK", "Amount of instances per block"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("GridSize"), new GUIContent("GRID SIZE", "Grid Size for patches"));

            //EditorGUILayout.PropertyField(serializedAsset.FindProperty("AlphaMapThreshold"), new GUIContent("MASK THRESOLD", "Masked area thresold"));
            //EditorGUILayout.PropertyField(serializedAsset.FindProperty("DensityFactor"), new GUIContent("MASK DENSITY", "Masked area density"));
            script.MGP.AlphaMapThreshold = 0.5f;
            script.MGP.DensityFactor = 0.5f;

            EditorGUILayout.PropertyField(serializedAsset.FindProperty("NormalType"), new GUIContent("NORMAL TYPE", "Normal type for rendering"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("GroundOffset"), new GUIContent("GROUND OFFSET", "Ground offset in vertical direction"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("CastShadows"), new GUIContent("SHADOW CASTING", "Shadow Casting mode for each instance"));

            GUILayout.Space(20);
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("layerBasedPlacement"), new GUIContent("CHECK LAYERS", "Raycast against underlying surfaces to decide on placement based on detected layers"));

            if (serializedAsset.FindProperty("layerBasedPlacement").boolValue)
            {
                THelpersUIRuntime.GUI_HelpBox("THIS FEATURE WILL AFFECT PERFORMANCE ON DENSER LAYERS", MessageType.Warning);
                THelpersUIRuntime.GUI_HelpBox("PLACEMENT ON WATER", MessageType.None, 20);

                if (serializedAsset.FindProperty("bypassWater").boolValue)
                    selectionIndexPlacement = 0;
                else if (serializedAsset.FindProperty("underWater").boolValue)
                    selectionIndexPlacement = 1;
                else if (serializedAsset.FindProperty("onWater").boolValue)
                    selectionIndexPlacement = 2;

                EditorGUI.BeginChangeCheck();
                style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 100; style.fixedHeight = 20;
                selectionIndexPlacement = THelpersUIRuntime.GUI_SelectionGrid(selectionIndexPlacement, waterPlacementMode, style, -10);

                if (EditorGUI.EndChangeCheck())
                {
                    if (selectionIndexPlacement == 0)
                    {
                        serializedAsset.FindProperty("bypassWater").boolValue = true;
                        serializedAsset.FindProperty("underWater").boolValue = false;
                        serializedAsset.FindProperty("onWater").boolValue = false;
                    }
                    else if (selectionIndexPlacement == 1)
                    {
                        serializedAsset.FindProperty("bypassWater").boolValue = false;
                        serializedAsset.FindProperty("underWater").boolValue = true;
                        serializedAsset.FindProperty("onWater").boolValue = false;
                    }
                    else if (selectionIndexPlacement == 2)
                    {
                        serializedAsset.FindProperty("bypassWater").boolValue = false;
                        serializedAsset.FindProperty("underWater").boolValue = false;
                        serializedAsset.FindProperty("onWater").boolValue = true;
                    }
                }

                script.MGP.unityLayerMask = THelpersUIRuntime.GUI_MaskField(new GUIContent("LAYER MASK", "Object placement will be applied on selected layer(s)"), script.MGP.unityLayerMask, 10);
                serializedAsset.FindProperty("unityLayerMask").intValue = script.MGP.unityLayerMask;
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (script.MGP.BuilderType != Mewlist.MassiveGrass.BuilderType.FromMesh || script.MGP.Mesh != null)
                    UpdatePlacement();
            }

            EditorGUI.EndDisabledGroup();

            style = new GUIStyle(EditorStyles.toolbarButton);
            style.fixedWidth = 128;
            style.fixedHeight = 20;
            THelpersUIRuntime.GUI_Button(new GUIContent("FORCE UPDATE", "Force update placement"), style, UpdatePlacement, 40);

            //TODO: Display instances count updated only when synced with new changes
            //THelpersUIRuntime.GUI_HelpBox(parameters._patches.Length.ToString(), MessageType.None, 20);

            GUILayout.Space(30);
            serializedAsset.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();

            //if (EditorGUI.EndChangeCheck())  // Check for any UI changes
            //{
            //    //EditorUtility.SetDirty(script); // Marks target object as dirty to create Undo state and save changed asset to disk.
            //    //EditorUtility.SetDirty(script.MGP);
            //    //SceneManagement.MarkSceneDirty(); // Marks scene as dirty which needs saving before closing Unity or switching scenes
            //    //SceneView.RepaintAll();
            //}
        }
    }
}

