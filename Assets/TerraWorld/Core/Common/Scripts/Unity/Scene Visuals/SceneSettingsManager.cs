#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor;
using TerraUnity.Runtime;

namespace TerraUnity.Edittime
{
    public class SceneSettingsManager
    {
        //private static FXParams _fxParams = new FXParams(true);

        private static TTerraWorldManager TerraWorldManager { get => TTerraWorldManager.TerraWorldManagerScript; }
        private static GameObject mainTerrainGO { get => TTerraWorldManager.MainTerrainGO; }
        private static Terrain mainTerrain { get => TTerraWorldManager.MainTerrain; }
        private static GameObject sceneSettingsGO { get => TTerraWorldManager.SceneSettingsGO1; }

        public static GlobalTimeManager globalTimeManager { get => TTerraWorldManager.GlobalTimeManagerScript; }
        public static TimeOfDay timeOfDay { get => TTerraWorldManager.TimeOfDayManagerScript; }
        public static CloudsManager cloudsManager { get => TTerraWorldManager.CloudsManagerScript; }
        public static WindManager windManager { get => TTerraWorldManager.WindManagerScript; }
        public static WaterManager waterManager { get => TTerraWorldManager.WaterManagerScript; }
        //public static TTerraWorldGraph worldGraph { get => TTerraWorld.WorldGraph; }
        public static HorizonFog horizonFog { get => TTerraWorldManager.HorizonFogManagerScript; }
        public static GameObject horizonFogGameObject { get => TTerraWorldManager.HorizonFogManagerScript.gameObject; }
        private static Crepuscular crepuscular { get => TTerraWorldManager.GodRaysManagerScript; }
        private static PostProcessLayer postProcessLayer { get => TTerraWorldManager.PostProcessLayerScript; }

#if UNITY_STANDALONE_WIN
        public static AtmosphericScattering atmosphericScattering { get => TTerraWorldManager.AtmosphericScatteringManagerScript; }
        private static AtmosphericScatteringSun atmosphericScatteringSun { get => TTerraWorldManager.AtmosphericScatteringSunScript; }
        private static AtmosphericScatteringDeferred atmosphericScatteringDeferred { get => TTerraWorldManager.AtmosphericScatteringDeferredScript; }
        public static VolumetricFog volumetricFog { get => TTerraWorldManager.VolumetricFogManagerScript; }
#endif

        public static PostProcessVolume postProcessVolume { get => TTerraWorldManager.PostProcessVolumeManagerScript; }

        // Get/Set Snow Settings
        public static bool IsProceduralSnow { get => isSnow(); set => SetSnow(value); }
        public static float SnowHeight { get => snowHeight(); set => SetSnowHeight(value); }
        public static float SnowFalloff { get => snowFalloff(); set => SetSnowFalloff(value); }
        public static float SnowThickness { get => snowThickness(); set => SetSnowThickness(value); }
        public static float SnowDamping { get => snowDamping(); set => SetSnowDamping(value); }

        // Get/Set Wind Settings
        public static bool IsWind { get => isWind(); set => SetWind(value); }
        public static float WindTime { get => windTime(); set => SetWindTime(value); }
        public static float WindSpeed { get => windSpeed(); set => SetWindSpeed(value); }
        public static float WindBending { get => windBending(); set => SetWindBending(value); }

        // Get/Set Flat Shading Settings
        public static bool IsFlatShadingObjects { get => isFlatShadingObjects(); set => SetFlatShadingObjects(value); }
        public static bool IsFlatShadingClouds { get => isFlatShadingClouds(); set => SetFlatShadingClouds(value); }

        public static FXParams FXParameters { get => GetFXParams(); set => SetFXParams(value); }

        public static FXParams GetFXParams()
        {
            FXParams _fxParams = TTerraWorld.WorldGraph.FXGraph.GetEntryNode().fxParams;

            if (sceneSettingsGO == null)
                return _fxParams;
            else
            {
                // Snow Settings
                _fxParams.hasSnow = IsProceduralSnow;
                _fxParams.snowStartHeight = SnowHeight;
                _fxParams.heightFalloff = SnowFalloff;
                _fxParams.snowThickness = SnowThickness;
                _fxParams.snowDamping = SnowDamping;

                // Wind Settings
                _fxParams.hasWind = IsWind;
                _fxParams.windTime = WindTime;
                _fxParams.windSpeed = WindSpeed;
                _fxParams.windBending = WindBending;

                // Flat Shading Settings
                //_fxParams.isFlatShading = IsFlatShadingObjects && IsFlatShadingClouds;
                //_fxParams.isFlatShadingTerrain = TerrainRenderingManager.isFlatShading;
                _fxParams.isFlatShading = IsFlatShadingObjects;
                _fxParams.isFlatShadingClouds = IsFlatShadingClouds;

                // Global Time Manager Settings
                _fxParams.dayNightControl = TerraWorldManager.timeOfDayMode;
                _fxParams.elevation = globalTimeManager.Elevation;
                _fxParams.azimuth = globalTimeManager.Azimuth;
                _fxParams.globalSpeed = globalTimeManager.GlobalSpeedX;

                // Time Of Day Settings
                _fxParams.dayNightUpdateIntervalInSeconds = timeOfDay.updateIntervalInSeconds;

                TTerraWorld.WorldGraph.FXGraph.GetEntryNode().fxParams = _fxParams;

                return TTerraWorld.WorldGraph.FXGraph.GetEntryNode().fxParams;
            }
        }

        private static void SetFXParams(FXParams fxParams)
        {
            TTerraWorld.WorldGraph.FXGraph.GetEntryNode().fxParams = fxParams;
            ApplyFXParams();
        }

        public static void ApplyFXParams()
        {
            if (sceneSettingsGO == null) return;
            FXParams _fxParams = TTerraWorld.WorldGraph.FXGraph.GetEntryNode().fxParams;

            if (_fxParams.selectionIndexVFX == 0)
            {
                sceneSettingsGO.SetActive(true);

                // Snow Settings
                IsProceduralSnow = _fxParams.hasSnow;
                SnowHeight = _fxParams.snowStartHeight;
                SnowFalloff = _fxParams.heightFalloff;
                SnowThickness = _fxParams.snowThickness;
                SnowDamping = _fxParams.snowDamping;

                // Wind Settings
                IsWind = _fxParams.hasWind;
                WindTime = _fxParams.windTime;
                WindSpeed = _fxParams.windSpeed;
                WindBending = _fxParams.windBending;

                // Flat Shading Settings
                //TerrainRenderingManager.isFlatShading = _fxParams.isFlatShadingTerrain;
                IsFlatShadingObjects = _fxParams.isFlatShading;
                IsFlatShadingClouds = _fxParams.isFlatShadingClouds;

                // Global Time Manager Settings
                //TerraWorldManager.timeOfDayMode = _fxParams.dayNightControl;
                //globalTimeManager.Elevation = _fxParams.elevation;
                //globalTimeManager.Azimuth = _fxParams.azimuth;
                //globalTimeManager.GlobalSpeedX = _fxParams.globalSpeed;
            }
            else
                sceneSettingsGO.SetActive(false);
        }

        private static void AssignWorldResources()
        {
            TTerraWorldManager.worldIsInitialized = true;
            string cloudsMaterialPath = TTerraWorld.WorkDirectoryLocalPath + TTerraWorldManager.cloudsMaterialName;
            string cloudsPrefabPath = TTerraWorld.WorkDirectoryLocalPath + TTerraWorldManager.cloudsPrefabName;
            string godRaysMaterialPath = TTerraWorld.WorkDirectoryLocalPath + TTerraWorldManager.godRaysMaterialName;
            string skyMaterialPath = TTerraWorld.WorkDirectoryLocalPath + TTerraWorldManager.skyMaterialName;
            string starsPrefabPath = TTerraWorld.WorkDirectoryLocalPath + TTerraWorldManager.starsPrefabName;
            string horizonMaterialPath = TTerraWorld.WorkDirectoryLocalPath + TTerraWorldManager.horizonMaterialName;
            string postProcessingProfilePath = TTerraWorld.WorkDirectoryLocalPath + TTerraWorldManager.postProcessingProfileName;
            //string waterMaterialPath = TTerraWorld.WorkDirectoryLocalPath + TTerraWorldManager.waterMaterialName;

            AssetDatabase.Refresh();

            // Copy & duplicate resource assets into corresponding world directory
            TResourcesManager.LoadAllResources();
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.skyMat), skyMaterialPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.starsPrefab), starsPrefabPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.cloudsMaterial), cloudsMaterialPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.cloudPrefab), cloudsPrefabPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.godRaysMaterial), godRaysMaterialPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.volumetricHorizonMaterial), horizonMaterialPath);
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.postProcessingAsset), postProcessingProfilePath);
            //AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.waterMaterial), waterMaterialPath);

            // Assign created resources to scene scripts
            cloudsManager.cloudMesh = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(TResourcesManager.cloudMesh), typeof(Mesh)) as Mesh;
            cloudsManager.cloudsMaterial = AssetDatabase.LoadAssetAtPath(cloudsMaterialPath, typeof(Material)) as Material;
            cloudsManager.cloudPrefab = AssetDatabase.LoadAssetAtPath(cloudsPrefabPath, typeof(GameObject)) as GameObject;
            crepuscular.material = AssetDatabase.LoadAssetAtPath(godRaysMaterialPath, typeof(Material)) as Material;
            horizonFog.volumetricMaterial = AssetDatabase.LoadAssetAtPath(horizonMaterialPath, typeof(Material)) as Material;

            postProcessVolume.profile = AssetDatabase.LoadAssetAtPath(postProcessingProfilePath, typeof(PostProcessProfile)) as PostProcessProfile;
            EditorUtility.SetDirty(postProcessVolume.profile);

            //waterManager.waterMaterial = AssetDatabase.LoadAssetAtPath(waterMaterialPath, typeof(Material)) as Material;

            AssetDatabase.SaveAssets();
            TTerraWorldManager.worldIsInitialized = false;
        }

        public static GameObject InstantiateSceneSettings()
        {
            GameObject TerraWorldGO = TTerraWorldManager.IsMainTerraworldGameObject;
            if (TerraWorldGO == null)
                throw new System.Exception("No TerraWorld Game Object Found!");

            if (sceneSettingsGO != null)
                return sceneSettingsGO;

            if (TResourcesManager.sceneSettingsPrefab == null)
                TResourcesManager.LoadAllResources();

             GameObject _sceneSettingsGO = MonoBehaviour.Instantiate(TResourcesManager.sceneSettingsPrefab);
            _sceneSettingsGO.name = "Scene Settings";
            _sceneSettingsGO.transform.parent = TerraWorldGO.transform;
            _sceneSettingsGO.layer = LayerMask.NameToLayer("TransparentFX");
            AssignWorldResources();
            ApplyFXParams();
            SwitchFX(FXParameters);

            return _sceneSettingsGO;
        }

        public static void ApplyWaterMaterial(Material waterMaterial)
        {
            if (waterMaterial != null && sceneSettingsGO != null && waterManager != null)
                waterManager.waterMaterial = waterMaterial;
        }

        public static void UpdateClouds(FXParams fxParams)
        {
            //FXNode FXModule = worldGraph.FXGraph.GetEntryNode();
            //RenderingNode renderingModule = worldGraph.renderingGraph.GetEntryNode();
            //if (TTerraWorld.worldReference == null) TTerraWorld.worldReference = GameObject.Find(renderingModule.renderingParams.worldName);
            //cloudsManager.emitProbability = FXModule.fxParams.emitProbability;

            cloudsManager.enabled = true;

            if (IsTerrainAvailable())
            {
                if (mainTerrain != null && mainTerrain.terrainData != null)
                    cloudsManager.areaSize = mainTerrain.terrainData.size.x * 10;
                else
                    cloudsManager.areaSize = 100000;
            }

            cloudsManager.visibilityDistance = cloudsManager.areaSize;
            cloudsManager.cloudCount = (int)(cloudsManager.areaSize / 16000f * 20);
            cloudsManager.cloudSize = new Vector3(fxParams.cloudSize, fxParams.cloudSize, fxParams.cloudSize);
            cloudsManager.particleSize = fxParams.cloudSize * 7f;
            cloudsManager.altitude = fxParams.cloudsAltitude;
            cloudsManager.castShadows = fxParams.cloudShadows;
            timeOfDay.cloudsColor = new Color(fxParams.cloudColor.X, fxParams.cloudColor.Y, fxParams.cloudColor.Z, fxParams.cloudColor.W);

            cloudsManager.meshMode = fxParams.meshMode;

            if (cloudsManager.meshMode == 0)
                cloudsManager.cloudMeshTexture = null;
            else if (cloudsManager.meshMode == 1)
                cloudsManager.cloudMeshTexture = AssetDatabase.LoadAssetAtPath(fxParams.cloudMeshTexturePath, typeof(Texture2D)) as Texture2D;

            if (fxParams.isFlatShadingClouds)
                cloudsManager.isFlatShading = true;
            else
                cloudsManager.isFlatShading = false;

            // Update Clouds Rendering
            cloudsManager.update = true;

#if UNITY_EDITOR
            SceneManagement.MarkSceneDirty();
#endif
        }

        public static void SwitchFX(FXParams fxParams)
        {
            // Avoid performing this function during builds
            if (BuildPipeline.isBuildingPlayer) return;

            if (sceneSettingsGO == null) TTerraWorldManager.CreateSceneSettingsGameObject();

            if (fxParams.selectionIndexVFX == 0)
            {
                UpdateParams(fxParams, true);

                EnableVisualFX(fxParams);

                SwitchDayNightControl(fxParams, true);
                //SwitchGodRays(fxParams);
                //SwitchClouds(fxParams);
                //UpdateWaterColors(fxParams);
                //UpdateWaterRendering(fxParams);
                //SwitchHorizonFog(fxParams);
                //SwitchPostProcessing(fxParams);
                //SwitchVolumetricFog(fxParams);
                //SwitchRealTimeReflections();

                // Apply on all scene materials
                SwitchSnow(fxParams);
                SwitchFlatShading(fxParams);
                SwitchWind(fxParams);
            }
            else
            {
                TurnOffSnow();
                TurnOffWind();
                TurnOffFlatShading();
                DisableWaterFX();
                DisablePostProcessing();
                DisableVisualFX();
                //TurnOffRealTimeReflections();
            }

            ApplyFXParams();

#if UNITY_EDITOR
            SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
#endif
        }

        public static void DefaultFXSettings()
        {
            TTerraWorld.WorldGraph.FXGraph.GetEntryNode().fxParams = new FXParams(true);
            SwitchFX(TTerraWorld.WorldGraph.FXGraph.GetEntryNode().fxParams);

#if UNITY_EDITOR
            SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
#endif
        }

        private static bool IsTerrainAvailable()
        {
            if (mainTerrainGO != null && mainTerrain != null)
                return true;
            else
                return false;
        }

        public static void SwitchDayNightControl(FXParams fxParams, bool forcedUpdate = false)
        {
            TerraWorldManager.timeOfDayMode = fxParams.dayNightControl;
            if (globalTimeManager == null) return;

            globalTimeManager.Elevation = fxParams.elevation;
            globalTimeManager.Azimuth = fxParams.azimuth;
            globalTimeManager.GlobalSpeedX = fxParams.globalSpeed;
            timeOfDay.updateIntervalInSeconds = fxParams.dayNightUpdateIntervalInSeconds;

            if (fxParams.dayNightControl == 0)
            {
                globalTimeManager.enabled = false;
                globalTimeManager.EnableTimeOfDay = false;
                timeOfDay.enabled = false;
            }
            else if (fxParams.dayNightControl == 1)
            {
                globalTimeManager.enabled = true;
                globalTimeManager.EnableTimeOfDay = false;
                timeOfDay.enabled = true;
                if (fxParams.lightmappingControls) Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.Iterative;
            }
            else if (fxParams.dayNightControl == 2)
            {
                globalTimeManager.enabled = true;
                globalTimeManager.EnableTimeOfDay = true;
                timeOfDay.enabled = true;
                if (fxParams.lightmappingControls) Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
            }

            if (timeOfDay.skyMaterial != null && timeOfDay.stars != null)
                globalTimeManager.OnValidate();

#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        public static void SwitchGodRays(FXParams fxParams)
        {
            if (fxParams.hasGodRays)
                crepuscular.enabled = true;
            else
                crepuscular.enabled = false;
        }

        public static void SwitchClouds(FXParams fxParams)
        {
            if (fxParams.hasClouds)
                UpdateClouds(fxParams);
            else if (cloudsManager.clouds != null)
            {
                MonoBehaviour.DestroyImmediate(cloudsManager.clouds);
#if UNITY_EDITOR
                SceneManagement.MarkSceneDirty();
#endif
            }
        }

        public static void SwitchPostProcessing(FXParams fxParams)
        {
            if (fxParams.isPostProcessing == 0)
                postProcessLayer.enabled = true;
            else
                postProcessLayer.enabled = false;
        }

        private static void DisablePostProcessing()
        {
            postProcessLayer.enabled = false;
        }

        public static void UpdateWaterColors(FXParams fxParams)
        {
            if (waterManager == null) return;

            waterManager.waterBaseColor = new Color(fxParams.waterBaseColor.X, fxParams.waterBaseColor.Y, fxParams.waterBaseColor.Z, fxParams.waterBaseColor.W);
            waterManager.waterReflectionColor = new Color(fxParams.waterReflectionColor.X, fxParams.waterReflectionColor.Y, fxParams.waterReflectionColor.Z, fxParams.waterReflectionColor.W);
        }

        public static void UpdateWaterRendering(FXParams fxParams)
        {
            TResourcesManager.LoadWaterResources();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                {
                    if (go.GetComponent<WaterBase>() != null)
                    {
                        if (waterManager != null && waterManager.waterMaterial != null)
                            go.GetComponent<WaterBase>().sharedMaterial = waterManager.waterMaterial;
                        else
                            go.GetComponent<WaterBase>().sharedMaterial = TResourcesManager.waterMaterial;

                        go.GetComponent<WaterBase>().waterQuality = (Runtime.WaterQuality)fxParams.waterQuality;

                        if (fxParams.waterQuality == WaterQuality.Low || fxParams.waterQuality == WaterQuality.Medium)
                            go.GetComponent<WaterBase>().edgeBlend = false;
                        else
                            go.GetComponent<WaterBase>().edgeBlend = fxParams.edgeBlend;
                    }

                    if (go.GetComponent<SpecularLighting>() != null)
                        go.GetComponent<SpecularLighting>().enabled = fxParams.specularLighting;

                    if (go.GetComponent<PlanarReflection>() != null)
                    {
                        if (fxParams.waterQuality == WaterQuality.Low)
                            go.GetComponent<PlanarReflection>().enabled = false;
                        else
                        {
                            go.GetComponent<PlanarReflection>().enabled = fxParams.planarReflection;
                            go.GetComponent<PlanarReflection>().quality = fxParams.reflectionQuality;
                            go.GetComponent<PlanarReflection>().renderingDistance = fxParams.reflectionDistance;
                        }

                        if (go.GetComponent<PlanarReflection>().reflectionCamera != null) MonoBehaviour.DestroyImmediate(go.GetComponent<PlanarReflection>().reflectionCamera);
                    }

                    if (go.GetComponent<GerstnerDisplace>() != null)
                    {
                        if (fxParams.waterQuality == WaterQuality.Low)
                            go.GetComponent<GerstnerDisplace>().enabled = false;
                        else
                            go.GetComponent<GerstnerDisplace>().enabled = fxParams.GerstnerWaves;
                    }

                    if (go.GetComponent<GetWaterPlaneHeight>() != null)
                    {
                        if (fxParams.waterQuality == WaterQuality.Low)
                            go.GetComponent<GetWaterPlaneHeight>().enabled = false;
                        else
                            go.GetComponent<GetWaterPlaneHeight>().enabled = fxParams.planarReflection;
                    }

                    // Assign water material to all water tiles
                    if (go.GetComponent<WaterTile>() != null && go.GetComponent<MeshRenderer>() != null)
                    {
                        if (waterManager != null && waterManager.waterMaterial != null)
                            go.GetComponent<MeshRenderer>().sharedMaterial = waterManager.waterMaterial;
                        else
                            go.GetComponent<MeshRenderer>().sharedMaterial = TResourcesManager.waterMaterial;
                    }
                }
            }
#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        public static void DisableWaterFX()
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                //if (go == null || go.transform == null || go.transform.root == null || go.transform.root.gameObject == null) continue;
                //if (!EditorUtility.IsPersistent(go.transform.root.gameObject) && !(go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave))
                if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                {
                    if (go.GetComponent<WaterBase>() != null)
                        go.GetComponent<WaterBase>().edgeBlend = false;

                    if (go.GetComponent<SpecularLighting>() != null)
                        go.GetComponent<SpecularLighting>().enabled = false;

                    if (go.GetComponent<PlanarReflection>() != null)
                    {
                        go.GetComponent<PlanarReflection>().enabled = false;
                        if (go.GetComponent<PlanarReflection>().reflectionCamera != null) MonoBehaviour.DestroyImmediate(go.GetComponent<PlanarReflection>().reflectionCamera);
                    }

                    if (go.GetComponent<GerstnerDisplace>() != null)
                        go.GetComponent<GerstnerDisplace>().enabled = false;

                    if (go.GetComponent<GetWaterPlaneHeight>() != null)
                        go.GetComponent<GetWaterPlaneHeight>().enabled = false;
                }
            }
#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        public static void SwitchHorizonFog(FXParams fxParams)
        {
            UpdateHorizonFog(fxParams);

            if (horizonFogGameObject != null && horizonFog != null)
            {
                if (fxParams.hasHorizonFog)
                    horizonFogGameObject.SetActive(true);
                else
                    horizonFogGameObject.SetActive(false);
            }
        }

        private static void UpdateHorizonFog(FXParams fxParams)
        {
            horizonFog.autoColor = fxParams.autoColor;

            if (fxParams.autoColor)
            {
                horizonFog.visibility = fxParams.horizonFogDensityAuto;
                horizonFog.strength = fxParams.horizonFogStrengthAuto;
                horizonFog.startOffset = fxParams.horizonFogStartHeightAuto;
                horizonFog.endOffset = fxParams.horizonFogEndHeightAuto;
            }
            else
            {
                horizonFog.visibility = fxParams.horizonFogDensityManual;
                horizonFog.strength = fxParams.horizonFogStrengthManual;
                horizonFog.startOffset = fxParams.horizonFogStartHeightManual;
                horizonFog.endOffset = fxParams.horizonFogEndHeightManual;

                horizonFog.volumeColor = new Color(fxParams.horizonFogColor.X, fxParams.horizonFogColor.Y, fxParams.horizonFogColor.Z, fxParams.horizonFogColor.W);
            }

            SetHorizonBlendMode(fxParams);
            horizonFog.UpdateParams();
        }

        private static void SetHorizonBlendMode(FXParams fxParams)
        {
            if (horizonFog.volumetricMaterial == null) return;
            HorizonBlendMode horizonBlendMode = HorizonBlendMode.Overlay;
            if (fxParams.autoColor) horizonBlendMode = fxParams.horizonBlendModeAuto;
            else horizonBlendMode = fxParams.horizonBlendModeManual;

            switch (horizonBlendMode)
            {
                case HorizonBlendMode.Overlay:
                    horizonFog.volumetricMaterial.SetOverrideTag("Queue", "Overlay-1");
                    horizonFog.volumetricMaterial.SetOverrideTag("IgnoreProjector", "True");
                    horizonFog.volumetricMaterial.SetOverrideTag("RenderType", "Transparent");
                    horizonFog.volumetricMaterial.renderQueue = (int)RenderQueue.Overlay - 1;
                    break;
                case HorizonBlendMode.Transparent:
                    horizonFog.volumetricMaterial.SetOverrideTag("Queue", "Transparent");
                    horizonFog.volumetricMaterial.SetOverrideTag("IgnoreProjector", "True");
                    horizonFog.volumetricMaterial.SetOverrideTag("RenderType", "Transparent");
                    horizonFog.volumetricMaterial.renderQueue = (int)RenderQueue.Transparent;
                    break;
            }
        }

        private static void ApplyWind(bool _State, float _windTime, float _windSpeed, float _windBending)
        {
            if (TTerraWorld.windManager == null) return;
            TTerraWorld.windManager.ApplyWind(_State, _windTime, _windSpeed, _windBending);
        }

        private static bool isWind()
        {
            if (TTerraWorld.windManager == null) return false;
            return TTerraWorld.windManager.WindState;
        }

        private static void SetWind(bool enabled)
        {
            if (TTerraWorld.windManager == null) return;
            TTerraWorld.windManager.WindState = enabled;
        }

        private static float windTime()
        {
            if (TTerraWorld.windManager == null) return 0;
            return TTerraWorld.windManager.WindTime;
        }

        private static void SetWindTime(float value)
        {
            if (TTerraWorld.windManager == null) return;
            TTerraWorld.windManager.WindTime = value;
        }

        private static float windSpeed()
        {
            if (TTerraWorld.windManager == null) return 0;
            return TTerraWorld.windManager.WindSpeed;
        }

        private static void SetWindSpeed(float value)
        {
            if (TTerraWorld.windManager == null) return;
            TTerraWorld.windManager.WindSpeed = value;
        }

        private static float windBending()
        {
            if (TTerraWorld.windManager == null) return 0;
            return TTerraWorld.windManager.WindBending;
        }

        private static void SetWindBending(float value)
        {
            if (TTerraWorld.windManager == null) return;
            TTerraWorld.windManager.WindBending = value;
        }

        public static void SwitchWind(FXParams fxParams)
        {
            ApplyWind(fxParams.hasWind, fxParams.windTime, fxParams.windSpeed, fxParams.windBending);

#if UNITY_EDITOR
            SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
#endif
        }

        private static void TurnOffWind()
        {
            TTerraWorld.windManager.RemoveWind();

#if UNITY_EDITOR
            SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
#endif
        }

        public static void SwitchSnow(FXParams fxParams)
        {
            if (IsTerrainAvailable())
            {
                RenderingParams renderingParams = TerrainRenderingManager.GetRenderingParams();
                if (renderingParams.modernRendering)
                {
                    renderingParams.proceduralSnow = fxParams.hasSnow;
                    TerrainRenderingManager.ApplyRenderingParams(renderingParams);
                }

            }

//            TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.proceduralSnow = fxParams.hasSnow;

            ApplySnow(fxParams.hasSnow, fxParams.snowStartHeight, fxParams.heightFalloff, fxParams.snowThickness, fxParams.snowDamping);

#if UNITY_EDITOR
            SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
#endif
        }

        private static void TurnOffSnow()
        {
            TTerraWorld.snowManager.RemoveSnow();

#if UNITY_EDITOR
            SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
#endif
        }

        private static void ApplySnow(bool _snowState, float _snowHeight, float _snowFalloff, float _snowThickness, float _snowDamping)
        {
            if (TTerraWorld.snowManager == null) return;

            TTerraWorld.snowManager.ApplySnow(_snowState, _snowHeight, _snowFalloff, _snowThickness, _snowDamping);

            //  if (TerrainRenderingManager.isModernRendering)
            //      TerrainRenderingManager.isProceduralSnow = _snowState;
            RenderingParams renderingParams = TerrainRenderingManager.GetRenderingParams();
            if (renderingParams.modernRendering)
            {
                renderingParams.proceduralSnow = _snowState;
                TerrainRenderingManager.ApplyRenderingParams(renderingParams);
            }
        }

        private static bool isSnow()
        {
            if (TTerraWorld.snowManager == null) return false;
            return TTerraWorld.snowManager.SnowState;
        }

        private static void SetSnow(bool enabled)
        {
            if (TTerraWorld.snowManager == null) return;
            TTerraWorld.snowManager.SnowState = enabled;
        }

        private static float snowHeight()
        {
            if (TTerraWorld.snowManager == null) return 0;
            return TTerraWorld.snowManager.SnowHeight;
        }

        private static void SetSnowHeight(float value)
        {
            if (TTerraWorld.snowManager == null) return;
            TTerraWorld.snowManager.SnowHeight = value;
        }

        private static float snowFalloff()
        {
            if (TTerraWorld.snowManager == null) return 0;
            return TTerraWorld.snowManager.SnowFalloff;
        }

        private static void SetSnowFalloff(float value)
        {
            if (TTerraWorld.snowManager == null) return;
            TTerraWorld.snowManager.SnowFalloff = value;
        }

        private static float snowThickness()
        {
            if (TTerraWorld.snowManager == null) return 0;
            return TTerraWorld.snowManager.SnowThickness;
        }

        private static void SetSnowThickness(float value)
        {
            if (TTerraWorld.snowManager == null) return;
            TTerraWorld.snowManager.SnowThickness = value;
        }

        private static float snowDamping()
        {
            if (TTerraWorld.snowManager == null) return 0;
            return TTerraWorld.snowManager.SnowDamping;
        }

        private static void SetSnowDamping(float value)
        {
            if (TTerraWorld.snowManager == null) return;
            TTerraWorld.snowManager.SnowDamping = value;
        }

        private static void ApplyFlatShading(bool _flatShadingStateObjects)
        {
            if (TTerraWorld.snowManager == null) return;
            TTerraWorld.flatShadingManager.ApplyFlatShading(_flatShadingStateObjects);
        }

        private static bool isFlatShadingObjects()
        {
            if (TTerraWorld.flatShadingManager == null) return false;
            return TTerraWorld.flatShadingManager.FlatShadingStateObjects;
        }

        private static void SetFlatShadingObjects(bool enabled)
        {
            if (TTerraWorld.flatShadingManager == null) return;
            TTerraWorld.flatShadingManager.FlatShadingStateObjects = enabled;
        }

        private static bool isFlatShadingClouds()
        {
            if
            (
                cloudsManager != null &&
                cloudsManager.cloudsMaterial != null &&
                cloudsManager.cloudsMaterial.HasProperty("_Mode") &&
                cloudsManager.cloudsMaterial.GetFloat("_Mode") == 1
            )
                return true;
            else
                return false;
        }

        private static void SetFlatShadingClouds(bool enabled)
        {
            if (cloudsManager.cloudsMaterial.HasProperty("_Mode"))
            {
                SetStandardMaterialParams.BlendMode blendMode = SetStandardMaterialParams.BlendMode.Fade;
                if (enabled) blendMode = SetStandardMaterialParams.BlendMode.Cutout;
                SetStandardMaterialParams.SwitchMaterialBlendingType(cloudsManager.cloudsMaterial, blendMode);
            }

            SceneManagement.MarkSceneDirty();
        }

        public static void SwitchFlatShading(FXParams fxParams)
        {
            // if (IsTerrainAvailable())
            //     TerrainRenderingManager.isFlatShading = fxParams.isFlatShadingTerrain;

            ApplyFlatShading(fxParams.isFlatShading);
        }

        private static void TurnOffFlatShading()
        {
            TTerraWorld.flatShadingManager.RemoveFlatShading();

#if UNITY_EDITOR
            SceneView.RepaintAll();
            SceneManagement.MarkSceneDirty();
#endif
        }

        public static void SwitchVolumetricFog(FXParams fxParams)
        {
#if UNITY_STANDALONE_WIN
            if (fxParams.hasVolumetricFog)
            {
                if (timeOfDay.player.GetComponent<VolumetricFog>() == null)
                    timeOfDay.player.AddComponent<VolumetricFog>();

                timeOfDay.player.GetComponent<VolumetricFog>().enabled = true;
                timeOfDay.player.GetComponent<VolumetricFog>().m_Wind = windManager;
            }
            else if (timeOfDay.player.GetComponent<VolumetricFog>() != null)
                timeOfDay.player.GetComponent<VolumetricFog>().enabled = false;
#endif
        }

        //public static void SwitchRealTimeReflections()
        //{
        //    //if (sceneSettings == null) InitSceneManagers();
        //    //if (sceneSettings == null) return;
        //
        //    if (TerrainRenderingManager.isProceduralPuddles)
        //    {
        //        if (sceneSettingsGO.transform.Find("Realtime Reflection") != null) return;
        //        GameObject reflectionProbeObject = new GameObject("Realtime Reflection");
        //        reflectionProbeObject.transform.parent = sceneSettingsGO.transform;
        //        reflectionProbeObject.AddComponent<RealtimeReflectionsManager>();
        //        ReflectionProbe reflectionProbe = reflectionProbeObject.AddComponent<ReflectionProbe>();
        //        reflectionProbe.mode = ReflectionProbeMode.Realtime;
        //        reflectionProbe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
        //        reflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
        //        reflectionProbe.intensity = 0.75f;
        //        reflectionProbe.blendDistance = 20f;
        //        reflectionProbe.size = new Vector3(20f, 20f, 20f);
        //        reflectionProbe.resolution = 128;
        //        reflectionProbe.farClipPlane = 100f;
        //    }
        //    else
        //        TurnOffRealTimeReflections();
        //}
        //
        //public static void TurnOffRealTimeReflections()
        //{
        //    if (sceneSettingsGO.transform.Find("Realtime Reflection") != null) MonoBehaviour.DestroyImmediate(sceneSettingsGO.transform.Find("Realtime Reflection").gameObject);
        //}

        public static void GetCameraControls(out bool grabEditorCamMatrix, out bool gxtendedFlyCam)
        {
            grabEditorCamMatrix = false;
            gxtendedFlyCam = false;
            GameObject mainCam = Camera.main.gameObject;

            if (mainCam != null)
            {
#if UNITY_EDITOR
                GrabEditorCamMatrix GECM = mainCam.GetComponent<GrabEditorCamMatrix>();

                if (GECM == null)
                    grabEditorCamMatrix = false;
                else
                    grabEditorCamMatrix = GECM.enabled;
#endif
                ExtendedFlyCam EFC = mainCam.GetComponent<ExtendedFlyCam>();

                if (EFC == null)
                    gxtendedFlyCam = false;
                else
                    gxtendedFlyCam = EFC.enabled;
            }
        }

        public static void SetCameraControls(bool grabEditorCamMatrix, bool gxtendedFlyCam)
        {
            GameObject mainCam = Camera.main.gameObject;

            if (mainCam != null)
            {
#if UNITY_EDITOR
                GrabEditorCamMatrix GECM = mainCam.GetComponent<GrabEditorCamMatrix>();

                if (GECM == null)
                {
                    if (grabEditorCamMatrix)
                        mainCam.AddComponent<GrabEditorCamMatrix>();
                }
                else if (!grabEditorCamMatrix)
                    MonoBehaviour.DestroyImmediate(GECM);
#endif
                ExtendedFlyCam EFC = mainCam.GetComponent<ExtendedFlyCam>();

                if (EFC == null)
                {
                    if (gxtendedFlyCam)
                        mainCam.AddComponent<ExtendedFlyCam>();
                }
                else if (!gxtendedFlyCam)
                    MonoBehaviour.DestroyImmediate(EFC);
            }
        }

        public static void UpdateParams(FXParams fxParams, bool updateClouds)
        {
            SwitchHorizonFog(fxParams);
            SwitchPostProcessing(fxParams);
            UpdateWaterColors(fxParams);
            UpdateWaterRendering(fxParams);
            SwitchVolumetricFog(fxParams);
            if (updateClouds) SwitchClouds(fxParams);
            SwitchGodRays(fxParams);
            SwitchDayNightControl(fxParams);
            //SwitchRealTimeReflections();

            if (crepuscular != null && crepuscular.material != null)
            {
                crepuscular.material.SetFloat("_NumSamples", fxParams.godRaySamples);
                crepuscular.material.SetFloat("_Density", fxParams.godRayDensity);
                crepuscular.material.SetFloat("_Weight", fxParams.godRayWeight);
                crepuscular.material.SetFloat("_Decay", fxParams.godRayDecay);
                crepuscular.material.SetFloat("_Exposure", fxParams.godRayExposure);
            }

            // Clouds settings
            if (cloudsManager != null)
            {
                cloudsManager.seed = fxParams.cloudsSeed;
                cloudsManager.density = fxParams.cloudsDensity;
            }

#if UNITY_STANDALONE_WIN
            // Atmospheric Scattering settings
            if (atmosphericScattering != null)
            {
                atmosphericScattering.enabled = fxParams.hasAtmosphericScattering;
                atmosphericScattering.heightRayleighIntensity = fxParams.atmosphericFogIntensity;
                atmosphericScattering.heightRayleighDensity = fxParams.atmosphericFogDensity;
                atmosphericScattering.heightDistance = fxParams.atmosphericFogDistance;
                timeOfDay.worldRayleighColorIntensity = fxParams.volumetricLightIntensity;
            }

            if (atmosphericScatteringSun != null)
                atmosphericScatteringSun.enabled = fxParams.hasAtmosphericScattering;

            if (atmosphericScatteringDeferred != null)
            {
                // Allow Atmospheric Scattering only when camera renders in Deferred rendering mode
                if (Camera.main?.renderingPath == RenderingPath.DeferredShading) atmosphericScatteringDeferred.enabled = fxParams.hasAtmosphericScattering;
                else atmosphericScatteringDeferred.enabled = false;
            }

            // Volumetric Fog settings
            if (volumetricFog != null)
            {
                volumetricFog.enabled = fxParams.hasVolumetricFog;
                volumetricFog.m_GlobalDensityMult = fxParams.fogStrength;
                windManager.m_Speed = fxParams.fogWindSpeed;
                volumetricFog.m_NearClip = fxParams.fogNearClip;
                volumetricFog.m_FarClipMax = fxParams.fogFarClip;
                volumetricFog.m_ConstantFog = fxParams.volumetricFogDensity;
                volumetricFog.m_NoiseFogAmount = fxParams.fogNoiseAmount;
                volumetricFog.m_NoiseFogScale = fxParams.fogNoiseScale;
                timeOfDay.volumetricFogColor = new Color(fxParams.volumetricFogColor.X, fxParams.volumetricFogColor.Y, fxParams.volumetricFogColor.Z, fxParams.volumetricFogColor.W); ;
            }
#endif

            // Horizon Fog settings
            //if (horizonFog == null) SwitchHorizonFog(fxParams);
            if (horizonFog != null)
                UpdateHorizonFog(fxParams);

            //timeOfDay?.OnValidate();
        }

        public static void EnableVisualFX(FXParams fxParams)
        {
            if (cloudsManager.clouds != null) cloudsManager.clouds.SetActive(fxParams.hasClouds);

#if UNITY_STANDALONE_WIN
            if (Camera.main != null && fxParams.hasAtmosphericScattering && Camera.main?.renderingPath != RenderingPath.DeferredShading) Camera.main.renderingPath = RenderingPath.DeferredShading;
            if (atmosphericScattering != null) atmosphericScattering.enabled = fxParams.hasAtmosphericScattering;
            if (atmosphericScatteringSun != null) atmosphericScatteringSun.enabled = fxParams.hasAtmosphericScattering;

            if (atmosphericScatteringDeferred != null)
            {
                if (Camera.main?.renderingPath == RenderingPath.DeferredShading)
                    atmosphericScatteringDeferred.enabled = fxParams.hasAtmosphericScattering;
                else
                    atmosphericScatteringDeferred.enabled = false;
            }
#endif
        }

        public static void DisableVisualFX()
        {
            if (globalTimeManager != null) globalTimeManager.EnableTimeOfDay = false;
            if (crepuscular != null) crepuscular.enabled = false;

#if UNITY_STANDALONE_WIN
            if (atmosphericScattering != null) atmosphericScattering.enabled = false;
            if (atmosphericScatteringSun != null) atmosphericScatteringSun.enabled = false;
            if (atmosphericScatteringDeferred != null) atmosphericScatteringDeferred.enabled = false;
            if (volumetricFog != null) volumetricFog.enabled = false;
#endif

            if (postProcessLayer != null) postProcessLayer.enabled = false;
            if (cloudsManager.clouds != null) cloudsManager.clouds.SetActive(false);

#if UNITY_EDITOR
            if (Application.isEditor) RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
#endif
        }

        //public static bool SaveSceneSettingsPreFab(string path)
        //{
        //    bool success = false;
        //    InitSceneManagers();
        //    PrefabUtility.SaveAsPrefabAsset(sceneSettings, path, out success);
        //    return success;
        //}

        //public static void DestroySceneObjects()
        //{
        //    // if (timeOfDay?.transform.Find("Night Stars") != null) DestroyImmediate(timeOfDay.transform.Find("Night Stars").gameObject);
        //    // if (timeOfDay?.player?.transform.Find("Night Lights") != null) DestroyImmediate(timeOfDay.player.transform.Find("Night Lights").gameObject);
        //    if (cloudsManager != null && cloudsManager.clouds != null) MonoBehaviour.DestroyImmediate(cloudsManager.clouds);
        //}
    }
}
#endif
#endif

