using UnityEditor;
using UnityEngine;
using TerraUnity.UI;

namespace TerraUnity.Runtime
{
    [CustomEditor(typeof(GPUInstanceLayer))]
    public class GPUInstanceLayerEditor : TBrushEditorGPU
    {
        // Generic Parameters
        private float m_LastEditorUpdateTime;
        private bool applyChanges = false;
        private float UIDelay = 1f;

        // Layer Classes
        private GPUInstanceLayer script;
        private TScatterParams parameters;
        private SerializedObject serializedAsset;

        // Layer Specific Parameters
        private int selectionIndexPlacement = 0;
        private string[] waterPlacementMode = new string[] { "BYPASS", "UNDER WATER", "ON WATER" };


        // Generic Methods
        //--------------------------------------------------------------------------------------------------------------------------------------

        //void OnEnable()
        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update += OnEditorUpdate;
            if (script == null) script = (GPUInstanceLayer)target;
            if (WT == null) WT = (GPUInstanceLayer)target;
            if (parameters == null) parameters = script.transform.GetChild(0).GetComponent<TScatterParams>();
            if (serializedAsset == null) serializedAsset = new SerializedObject(parameters);
            ModelPreviewUI.InitPreview(serializedAsset.FindProperty("prefab").objectReferenceValue);

            if (parameters.terrain != null)
            {
                int terrainLayersCount = parameters.terrain.terrainData.terrainLayers.Length;

                if (parameters.exclusionOpacities != null && parameters.exclusionOpacities.Length != 0 && parameters.exclusionOpacities.Length == terrainLayersCount)
                {
                    WT.exclusion = new float[terrainLayersCount];

                    for (int i = 0; i < parameters.exclusionOpacities.Length; i++)
                        WT.exclusion[i] = (1f - parameters.exclusionOpacities[i]) * 100f;
                }
            }

            TBrushFunctions.ConvertMaskDataToImage(parameters.maskData);

            //serializedAsset.Update();
            //serializedObject.Update();
            //
            //Object xxx = serializedAsset.FindProperty("prefab").objectReferenceValue;
            //serializedAsset.FindProperty("prefab").objectReferenceValue = null;
            ////EditorUtility.SetDirty(serializedAsset.FindProperty("prefab").objectReferenceValue);
            //serializedAsset.FindProperty("prefab").objectReferenceValue = xxx;
            //EditorUtility.SetDirty(serializedAsset.FindProperty("prefab").objectReferenceValue);
            //
            //serializedAsset.Update();
            //serializedObject.Update();
            //
            //UpdatePlacement();

            //Debug.Log("haha");
#endif
        }

        protected virtual void OnDisable()
        {
            ModelPreviewUI.DestroyPreviewEditor();

#if UNITY_EDITOR
            EditorApplication.update -= OnEditorUpdate;
#endif
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
            parameters.UpdateLayer();
            parameters.SetCullingLODEditor(true);
            SceneView.RepaintAll();
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

            if (serializedAsset.FindProperty("prefab").objectReferenceValue != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                style = new GUIStyle(EditorStyles.helpBox);
                ModelPreviewUI.ModelPreview(serializedAsset.FindProperty("prefab").objectReferenceValue, style);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.Space(20);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("prefab"), new GUIContent("Prefab", "Prefab used for placement"));
            if (EditorGUI.EndChangeCheck())
            {
                ModelPreviewUI.InitPreview(serializedAsset.FindProperty("prefab").objectReferenceValue);
                UpdatePlacement();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("averageDistance"), new GUIContent("Models Distance", "Distance between placed instances"));
            if (EditorGUI.EndChangeCheck()) UpdatePlacementDelayed();
            
            GUILayout.Space(40);
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("LODMultiplier"), new GUIContent("LOD Bias", "LOD Bias for rendering instances"));
            if (EditorGUI.EndChangeCheck()) parameters.SetCullingLODEditor(true);
            
            if (serializedAsset.FindProperty("LODGroupNotDetected").boolValue)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(serializedAsset.FindProperty("maxDistance"), new GUIContent("Max Distance", "Max Distance for rendering"));
                if (EditorGUI.EndChangeCheck()) UpdatePlacementDelayed();
            }
            
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("shadowCastMode"), new GUIContent("Cast Shadows", "Cast shadows on models?"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("receiveShadows"), new GUIContent("Receive Shadows", "Receive Shadows on models?"));
            
            THelpersUIRuntime.GUI_HelpBox("PLACEMENT ON WATER SURFACE", MessageType.None, 20);
            
            if (serializedAsset.FindProperty("bypassLake").boolValue)
                selectionIndexPlacement = 0;
            else if (serializedAsset.FindProperty("underLake").boolValue)
                selectionIndexPlacement = 1;
            else if (serializedAsset.FindProperty("onLake").boolValue)
                selectionIndexPlacement = 2;
            
            EditorGUI.BeginChangeCheck();
            style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 100; style.fixedHeight = 20;
            selectionIndexPlacement = THelpersUIRuntime.GUI_SelectionGrid(selectionIndexPlacement, waterPlacementMode, style, -10);
            if (EditorGUI.EndChangeCheck())
            {
                if (selectionIndexPlacement == 0)
                {
                    serializedAsset.FindProperty("bypassLake").boolValue = true;
                    serializedAsset.FindProperty("underLake").boolValue = false;
                    serializedAsset.FindProperty("underLakeMask").boolValue = true;
                    serializedAsset.FindProperty("onLake").boolValue = false;
                }
                else if (selectionIndexPlacement == 1)
                {
                    serializedAsset.FindProperty("bypassLake").boolValue = false;
                    serializedAsset.FindProperty("underLake").boolValue = true;
                    serializedAsset.FindProperty("underLakeMask").boolValue = true;
                    serializedAsset.FindProperty("onLake").boolValue = false;
                }
                else if (selectionIndexPlacement == 2)
                {
                    serializedAsset.FindProperty("bypassLake").boolValue = false;
                    serializedAsset.FindProperty("underLake").boolValue = false;
                    serializedAsset.FindProperty("underLakeMask").boolValue = true;
                    serializedAsset.FindProperty("onLake").boolValue = true;
                }
                
                UpdatePlacement();
            }
            
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(20);
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("seedNo"), new GUIContent("SEED #", "Seed number for unique procedural values"));
            if (EditorGUI.EndChangeCheck()) UpdatePlacementDelayed();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("frustumMultiplier"), new GUIContent("Frustum Multiplier", "Camera Frustum range for wider frustum range"));
            if (EditorGUI.EndChangeCheck()) parameters.SetCullingLODEditor(true);
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("checkBoundingBox"), new GUIContent("CHECK BOUNDING BOX", "Check model's bounding box for placement?\nIf checked, placement will only be applied when the whole model is inside masked areas\nIf unchecked, mask is only checked in pivot position of the model"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("getSurfaceAngle"), new GUIContent("SURFACE ROTATION", "Rotation is calculated from the underlying surface angle"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("lock90DegreeRotation"), new GUIContent("90° ROTATION", "Rotation is only performed in 90 degree multiplies 0°, 90°, 180° & 270°"));
            if (!serializedAsset.FindProperty("getSurfaceAngle").boolValue) EditorGUILayout.PropertyField(serializedAsset.FindProperty("lockYRotation"), new GUIContent("HORIZONTAL ROTATION", "Rotation is locked in horizontal rotation axis (Y) only"));
            if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(10);
            if (!serializedAsset.FindProperty("lock90DegreeRotation").boolValue)
            {
                EditorGUILayout.PropertyField(serializedAsset.FindProperty("minRotationRange"), new GUIContent("MIN. ROTATION RANGE", "Minimum rotation in degrees for object"));
                EditorGUILayout.PropertyField(serializedAsset.FindProperty("maxRotationRange"), new GUIContent("MAX. ROTATION RANGE", "Maximum rotation in degrees for object"));
                if (serializedAsset.FindProperty("minRotationRange").floatValue > serializedAsset.FindProperty("maxRotationRange").floatValue)
                    serializedAsset.FindProperty("minRotationRange").floatValue = serializedAsset.FindProperty("maxRotationRange").floatValue - 0.001f;
            }
            //if (!parameters.lock90DegreeRotation) THelpersUIRuntime.GUI_MinMaxSlider(new GUIContent("ROTATION RANGE", "Minimum & Maximum rotation in degrees for object"), ref parameters.minRotationRange, ref parameters.maxRotationRange, 0f, 359f);
            
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("positionVariation"), new GUIContent("RANDOM POSITION", "Percentage of the random position for instance"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("scale"), new GUIContent("SCALE", "Original scale of the instance"));
            
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("minScale"), new GUIContent("MIN. SCALE RANGE", "Minimum random scale variation which would multiply to SCALE"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("maxScale"), new GUIContent("MAX. SCALE RANGE", "Maximum random scale variation which would multiply to SCALE"));
            if (serializedAsset.FindProperty("minScale").floatValue > serializedAsset.FindProperty("maxScale").floatValue)
                serializedAsset.FindProperty("minScale").floatValue = serializedAsset.FindProperty("maxScale").floatValue - 0.001f;
            //THelpersUIRuntime.GUI_MinMaxSlider(new GUIContent("SCALE RANGE", "Minimum & Maximum random scale variation which would multiply to SCALE"), ref parameters.minScale, ref parameters.maxScale, 0.1f, 20f);
            
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("minAllowedAngle"), new GUIContent("MIN. SLOPE RANGE", "Minimum slope in degrees compared to horizon level for object placement"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("maxAllowedAngle"), new GUIContent("MAX. SLOPE RANGE", "Maximum slope in degrees compared to horizon level for object placement"));
            if (serializedAsset.FindProperty("minAllowedAngle").floatValue > serializedAsset.FindProperty("maxAllowedAngle").floatValue)
                serializedAsset.FindProperty("minAllowedAngle").floatValue = serializedAsset.FindProperty("maxAllowedAngle").floatValue - 0.001f;
            //THelpersUIRuntime.GUI_MinMaxSlider(new GUIContent("SLOPE RANGE", "Minimum & Maximum slope in degrees compared to horizon level for object placement"), ref parameters.minAllowedAngle, ref parameters.maxAllowedAngle, 0f, 90f);
            
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("minAllowedHeight"), new GUIContent("MIN. HEIGHT RANGE", "Minimum height in meters (units) for object placement"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("maxAllowedHeight"), new GUIContent("MAX. HEIGHT RANGE", "Maximum height in meters (units) for object placement"));
            if (serializedAsset.FindProperty("minAllowedHeight").floatValue > serializedAsset.FindProperty("maxAllowedHeight").floatValue)
                serializedAsset.FindProperty("minAllowedHeight").floatValue = serializedAsset.FindProperty("maxAllowedHeight").floatValue - 0.001f;
            //THelpersUIRuntime.GUI_MinMaxSlider(new GUIContent("HEIGHT RANGE", "Minimum & Maximum height in meters (units) for object placement"), ref parameters.minAllowedHeight, ref parameters.maxAllowedHeight, -100000f, 100000f);
            
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("positionOffset"), new GUIContent("POSITION OFFSET", "Placement offset of the object in 3 dimensions"));
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("rotationOffset"), new GUIContent("ROTATION OFFSET", "Rotation offset of the object in 3 dimensions"));
            if (EditorGUI.EndChangeCheck()) UpdatePlacementDelayed();
            
            EditorGUI.BeginChangeCheck();
            parameters.unityLayerName = THelpersUIRuntime.GUI_LayerField(new GUIContent("UNITY LAYER", "Select object's Unity layer"));
            serializedAsset.FindProperty("unityLayerName").stringValue = parameters.unityLayerName;
            parameters.unityLayerMask = THelpersUIRuntime.GUI_MaskField(new GUIContent("LAYER MASK", "Object placement will be applied on selected layer(s)"), parameters.unityLayerMask);
            serializedAsset.FindProperty("unityLayerMask").intValue = parameters.unityLayerMask;
            if (EditorGUI.EndChangeCheck()) UpdatePlacement();
            
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedAsset.FindProperty("occlusionCulling"), new GUIContent("OCCLUSION CULLING", "If selected, layer will be checked for Real-time Occlusion Culling against all objects with Static flag of \"Occluder Static\" in Game mode"));

            style = new GUIStyle(EditorStyles.toolbarButton); style.fixedWidth = 128; style.fixedHeight = 20;
            THelpersUIRuntime.GUI_Button(new GUIContent("FORCE UPDATE", "Force update placement"), style, UpdatePlacement, 40);

            //TODO: Display instances count updated only when synced with new changes
            //THelpersUIRuntime.GUI_HelpBox(parameters._patches.Length.ToString(), MessageType.None, 20);

            GUILayout.Space(30);
            serializedAsset.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
        }
    }
}

