#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using TerraUnity.Runtime;
using UnityEngine.Windows;
#if TERRAWORLD_XPRO
using XNode;
using TerraUnity.Graph;
using XNodeEditor;
#endif
using TerraUnity.Utils;
using TerraUnity.UI;

namespace TerraUnity.Edittime
{
    public class TTerraworldGenerator //: MonoBehaviour
    {
        public static bool finalizeStarted = false;
        private static int finalizationDelay = 5000; // in milliseconds
        private static DateTime startProgressTime = DateTime.Now;
        private static RenderingParams renderingParams1 = new RenderingParams(true);

#if TERRAWORLD_XPRO
        public static TXGraph XGraph { get => TTerraWorldManager.XGraph; }

        public static void EditXGraph()
        {
            NodeEditorWindow.Open(XGraph);
        }
#endif
        public static void RunLoadedGraph(bool reGenerate)
        {
            TDebug.DebugMode = TProjectSettings.DebugLogSystem;
            TDebug.Initialize();
            TDebug.TraceMessage();
            startProgressTime = DateTime.Now;
            renderingParams1 = TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams;
            if (reGenerate)
            {
                TTerrainGenerator.progressID = TProgressBar.StartProgressBar("TERRAWORLD", "Fetching Raw Data From Servers ...", TProgressBar.ProgressOptionsList.Managed, false);
                TTerrainGenerator.CreateWorld(OnFinished);
            }
            else
            ApplyRealTimeSettings();
        }

        public static void CreateWorld(bool reGenerate)
        {
            //TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams = TerrainRenderingManager.RenderingParams;
            //TTerraWorld.SaveWorldGraphFromScene();
            TTerraWorldManager.SaveGraph();
            RunLoadedGraph(reGenerate);
        }

        public static void ApplyRealTimeSettings()
        {
            // if (string.IsNullOrEmpty(TTerraWorldManager.TerraWorldGraphPath))
            //     throw new Exception("Error Code 132");

//            TerrainRenderingManager.RenderingParams = TTerraWorldGraph.GetSavedRenderingParams(TTerraWorldManager.TerraWorldGraphPath);
            TerrainRenderingManager.ApplyRenderingParams(renderingParams1);
            SceneSettingsManager.FXParameters = TTerraWorldGraph.GetSavedVFXParams(TTerraWorldManager.TerraWorldGraphPath);

            //TerrainRenderingManager.RenderingParams = TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams;
            //SceneSettingsManager.FXParameters = TTerraWorld.WorldGraph.FXGraph.GetEntryNode().fxParams;
            //TerrainRenderingManager.ApplyRenderingParams();
            //FXParams fXParams = TTerraWorld.WorldGraph.FXGraph.GetEntryNode().fxParams;
            SceneSettingsManager.ApplyFXParams();
            SceneSettingsManager.SwitchFX(SceneSettingsManager.FXParameters);
            TDebug.TraceMessage("Finish!");
        }

        public static void OnFinished(TTerrain tTerrainData, Terrain terrain, List<GameObject> waterSurfacesParent, List<GameObject> riversParent)
        {
            try
            {
                TDebug.TraceMessage();
                Transform sunTransform = null;
                string waterTag = "Respawn";

                foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                {
                    if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                        if (go.GetComponent<Light>() != null && go.GetComponent<Light>().type == LightType.Directional)
                        {
                            sunTransform = go.transform;
                            break;
                        }
                }

                TTerrainGenerator.ShowProgressWindow("Generating Rivers, Lakes & Oceans...", 0.75f, TTerrainGenerator.progressID);

                // Lakes & Rivers
                if (TTerraWorldManager.WaterManagerScript != null && TTerraWorldManager.WaterManagerScript.waterMaterial!= null)
                {
                    foreach (GameObject g in waterSurfacesParent)
                    {
                        if (g.transform.childCount < 1) continue;

                        //Material waterMaterial = TTerraWorldManager.WaterManagerScript.waterMaterial;
                        Material waterMaterial = null;

                        if (g.transform.GetChild(0) != null && g.transform.GetChild(0).GetComponent<Renderer>() != null && g.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial != null)
                            waterMaterial = g.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;
                        else
                            waterMaterial = TTerraWorldManager.WaterManagerScript.waterMaterial;

                        if (waterMaterial.shader.name == "TerraUnity/Water4")
                        {
                            if (g.GetComponent<WaterBase>() == null)
                            {
                                WaterBase waterBase = g.AddComponent<WaterBase>();
                                if (waterMaterial != null) waterBase.sharedMaterial = waterMaterial;
                                waterBase.BumpFlowSpeed = 0.025f;
                                waterBase.FoamFlowSpeed = 0.025f;
                            }

                            if (g.GetComponent<SpecularLighting>() == null)
                            {
                                SpecularLighting specularLighting = g.AddComponent<SpecularLighting>();
                                if (sunTransform != null) specularLighting.specularLight = sunTransform;
                            }

                            if (g.GetComponent<PlanarReflection>() == null)
                            {
                                PlanarReflection planarReflection = g.AddComponent<PlanarReflection>();
                                planarReflection.reflectionMask = ~0;
                                planarReflection.reflectSkybox = true;
                                planarReflection.clearColor = UnityEngine.Color.white;
                            }

                            if (g.GetComponent<GerstnerDisplace>() == null)
                            {
                                GerstnerDisplace gerstnerDisplace = g.AddComponent<GerstnerDisplace>();
                                gerstnerDisplace.enabled = false;
                            }

                            if (g.GetComponent<GetWaterPlaneHeight>() == null)
                            {
                                GetWaterPlaneHeight getWaterPlane = g.AddComponent<GetWaterPlaneHeight>();
                                getWaterPlane.searchTag = waterTag;
                            }

                            foreach (Transform t in g.GetComponentsInChildren<Transform>(true))
                            {
                                if (t != g.transform && t.gameObject.GetComponent<WaterTile>() == null)
                                    t.gameObject.AddComponent<WaterTile>();
                            }
                        }
                    }
                }

                //ShowAreaBiomes(OSMParser.areaBiomes, 10f);
                //ShowAreaBiomes(TLandcoverProccessor.areaBiomes, 10f);
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
            }
            finally
            {
                finalizeStarted = true;

                if (tTerrainData.InstanceScatterLayers != null && tTerrainData.InstanceScatterLayers.Count > 0)
                {
                    InitInstancesLayer(terrain, tTerrainData);
                    ThreadHandler TH = new ThreadHandler();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    TH.WaitOnOtherThread(UpdateInstancesLayer, terrain, tTerrainData, finalizationDelay);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                    FinalizeWorld(terrain);
            }
        }

        public static void InitInstancesLayer(Terrain terrain, TTerrain tTerrainData)
        {
            try
            {
                TTerrainGenerator.ShowProgressWindow("Generating Objects & Instances...", 0.8f, TTerrainGenerator.progressID);
                TDebug.TraceMessage("InitInstancesLayer");
                List<TInstanceScatterLayer> _instanceLayers = tTerrainData.InstanceScatterLayers;

                if (_instanceLayers != null && _instanceLayers.Count > 0)
                {
                    foreach (TInstanceScatterLayer _instancelayer in _instanceLayers)
                    {
                        GameObject instanceLayer = new GameObject(_instancelayer.layerName);
                        instanceLayer.transform.parent = terrain.transform;
                        //instanceLayer.transform.localPosition = Vector3.zero;

                        GameObject dataHanlder = new GameObject(_instancelayer.layerName);
                        dataHanlder.transform.parent = instanceLayer.transform;
                        TScatterParams _GPULayer = dataHanlder.AddComponent<TScatterParams>();
                        dataHanlder.hideFlags = HideFlags.HideInHierarchy;
                        instanceLayer.AddComponent<GPUInstanceLayer>();

                        //TScatterParams terraLayerManager = instanceLayer.AddComponent<TScatterParams>();
                        //terraLayerManager.terrain = terrain;
                        //TerrainData data = terrain.terrainData;

                        _GPULayer.averageDistance = _instancelayer.averageDistance;
                        _GPULayer.scale = TUtils.CastToUnity(_instancelayer.ScaleMultiplier);
                        _GPULayer.minScale = _instancelayer.MinScale;
                        _GPULayer.maxScale = _instancelayer.MaxScale;
                        _GPULayer.positionVariation = _instancelayer.PositionVariation;
                        _GPULayer.lock90DegreeRotation = _instancelayer.rotation90Degrees;
                        _GPULayer.lockYRotation = _instancelayer.lockYRotation;
                        _GPULayer.getSurfaceAngle = _instancelayer.getSurfaceAngle;
                        _GPULayer.seedNo = _instancelayer.SeedNo;
                        _GPULayer.priority = _instancelayer.Priority;
                        _GPULayer.unityLayerName = _instancelayer.UnityLayerName;
                        _GPULayer.unityLayerMask = _instancelayer.UnityLayerMask;
                        _GPULayer.positionVariation = _instancelayer.PositionVariation;
                        _GPULayer.positionVariation = _instancelayer.PositionVariation;
                        _GPULayer.positionOffset = TUtils.CastToUnity(_instancelayer.Offset);
                        _GPULayer.rotationOffset = TUtils.CastToUnity(_instancelayer.RotationOffset);
                        _GPULayer.minRotationRange = _instancelayer.MinRotationRange;
                        _GPULayer.maxRotationRange = _instancelayer.MaxRotationRange;
                        _GPULayer.minAllowedAngle = _instancelayer.MinSlope;
                        _GPULayer.maxAllowedAngle = _instancelayer.MaxSlope;
                        _GPULayer.minAllowedHeight = _instancelayer.MinElevation;
                        _GPULayer.maxAllowedHeight = _instancelayer.MaxElevation;
                        _GPULayer.shadowCastMode = (UnityEngine.Rendering.ShadowCastingMode)_instancelayer.shadowCastingMode;
                        _GPULayer.receiveShadows = _instancelayer.receiveShadows;
                        _GPULayer.bypassLake = _instancelayer.bypassLake;
                        _GPULayer.underLake = _instancelayer.underLake;
                        _GPULayer.underLakeMask = _instancelayer.underLakeMask;
                        _GPULayer.onLake = _instancelayer.onLake;
                        _GPULayer.maxDistance = _instancelayer.maxDistance;
                        _GPULayer.LODMultiplier = _instancelayer.LODMultiplier;
                        _GPULayer.maxDistance = _instancelayer.maxDistance;
                        _GPULayer.gridResolution = _instancelayer.gridResolution;
                        _GPULayer.frustumMultiplier = _instancelayer.frustumMultiplier;
                        _GPULayer.checkBoundingBox = _instancelayer.checkBoundingBox;
                        _GPULayer.occlusionCulling = _instancelayer.occlusionCulling;
                        //_GPULayer.activeAreaBounds = _instancelayer.AreaBounds;
                        //_GPULayer.hasCollision = _instancelayer.HasCollider;

                        //if (_instancelayer.mask != null) _GPULayer.filter = _instancelayer.mask.GetTexture(_instancelayer.layerName + "_mask");
                        _GPULayer.maskData = new TScatterLayer.MaskData[_instancelayer.maskData.GetLength(0)];

                        for (int i = 0; i < _instancelayer.maskData.GetLength(0); i++)
                        {
                            _GPULayer.maskData[i].row = new float[_instancelayer.maskData.GetLength(1)];

                            for (int j = 0; j < _instancelayer.maskData.GetLength(1); j++)
                                _GPULayer.maskData[i].row[j] = _instancelayer.maskData[i, j];
                        }

                        // ToDo: Just for Ver 1.15 - It should be overridden in next version  
                        _GPULayer.SetPrefabWithoutUpdatePatches(AssetDatabase.LoadAssetAtPath(_instancelayer.prefabName, typeof(GameObject)) as GameObject);

                        //_GPULayer.Prefab = AssetDatabase.LoadAssetAtPath(_instancelayer.prefabName, typeof(GameObject)) as GameObject;
                        //if (_instancelayer.LODDistances != null && _GPULayer.LODDistances!= null && _instancelayer.LODDistances.Count > 0 && _instancelayer.LODDistances.Count == _GPULayer.LODDistances.Count)
                            //_GPULayer.LODDistances = _instancelayer.LODDistances;
                    }
                }
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
            }
        }

        public static void UpdateInstancesLayer(Terrain terrain, TTerrain tTerrainData)
        {
            try
            {
                foreach (Transform t in terrain?.GetComponentsInChildren(typeof(Transform), true))
                {
                    if (t == null) continue;
                    TScatterParams _GPULayer = t?.GetComponent<TScatterParams>();
            
                    //TODO: Just for Ver 1.15 - It should be overridden in next version  
                    if (_GPULayer != null)
                        _GPULayer.UpdateLayer();
                }
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
            }
            finally
            {
                FinalizeWorld(terrain);
            }
        }

        public static void FinalizeWorld(Terrain terrain)
        {
            try
            {
                TTerrainGenerator.ShowProgressWindow("Finilizing & Cleaning up ...", 0.85f, TTerrainGenerator.progressID);
                SetCameraClipPlanes(terrain);
                CleanUp(terrain);
                InteractiveTargets.GetPlayerTargets(true);
                TTerrainGenerator.ShowProgressWindow("Finilizing & Cleaning up ...", 0.9f, TTerrainGenerator.progressID);
                ClearMemory();

                //ResetWorldCenterPos(terrain);
                if (!string.IsNullOrEmpty(TTerraWorld.TemplateName))
                {
                    TimeSpan timeDiff = DateTime.Now - startProgressTime;
                    TTerraWorld.FeedbackEvent(EventCategory.Templates, "ProcessTime (min)", TTerraWorld.TemplateName + "-" + (int)timeDiff.TotalMinutes);
                }

#if TERRAWORLD_PRO
                //TerrainRenderingManager.SyncMaterials();
                FXNode fXNode = TTerraWorld.WorldGraph.FXGraph.GetEntryNode();

                if (string.IsNullOrEmpty(TTerraWorld.TemplateName))
                {
                    if (fXNode.fxParams.selectionIndexVFX == 0)
                    {
                        TTerraWorld.FeedbackEvent(EventCategory.Params, "selectionIndexVFX", 1);

                        TTerraWorld.FeedbackEvent(EventCategory.Params, "dayNightControl", fXNode.fxParams.dayNightControl);

                        if (fXNode.fxParams.hasGodRays)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasGodRays", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasGodRays", 0);

                        if (fXNode.fxParams.hasClouds)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasClouds", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasClouds", 0);

                        if (fXNode.fxParams.hasAtmosphericScattering)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasAtmosphericScattering", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasAtmosphericScattering", 0);

                        if (fXNode.fxParams.hasVolumetricFog)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasVolumetricFog", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasVolumetricFog", 0);

                        if (fXNode.fxParams.hasWind)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasWind", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasWind", 0);

                        if (fXNode.fxParams.hasSnow)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasSnow", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasSnow", 0);

                        TTerraWorld.FeedbackEvent(EventCategory.Params, "waterQuality", (int)fXNode.fxParams.waterQuality);

                        if (fXNode.fxParams.hasReflection)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasReflection", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasReflection", 0);

                        TTerraWorld.FeedbackEvent(EventCategory.Params, "isPostProcessing", fXNode.fxParams.isPostProcessing);

                        if (fXNode.fxParams.hasHorizonFog)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasHorizonFog", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "hasHorizonFog", 0);

                        if (fXNode.fxParams.isFlatShading)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "isFlatShading", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "isFlatShading", 0);

                        if (fXNode.fxParams.isFlatShadingClouds)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "isFlatShadingClouds", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "isFlatShadingClouds", 0);
                    }
                    else
                        TTerraWorld.FeedbackEvent(EventCategory.Params, "selectionIndexVFX", 0);

                    RenderingParams renderingParams = TerrainRenderingManager.GetRenderingParams();
                    if (renderingParams.modernRendering)
                    {
                        TTerraWorld.FeedbackEvent(EventCategory.Params, "modernRendering", 1);

                        //if (rnNode.renderingParams.instancedDrawing)
                        //    TTerraWorld.FeedbackEvent(EventCategory.Params, "instancedDrawing", 1);
                        //else
                        //    TTerraWorld.FeedbackEvent(EventCategory.Params, "instancedDrawing", 0);

                        if (renderingParams.tessellation)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "tessellation", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "tessellation", 0);

                        if (renderingParams.heightmapBlending)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "heightmapBlending", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "heightmapBlending", 0);

                        if (renderingParams.colormapBlending)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "colormapBlending", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "colormapBlending", 0);

                        if (renderingParams.proceduralSnow)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "proceduralSnow", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "proceduralSnow", 0);

                        if (renderingParams.proceduralPuddles)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "proceduralPuddles", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "proceduralPuddles", 0);

                        if (renderingParams.isFlatShading)
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "isFlatShading", 1);
                        else
                            TTerraWorld.FeedbackEvent(EventCategory.Params, "isFlatShading", 0);
                    }
                    else
                        TTerraWorld.FeedbackEvent(EventCategory.Params, "modernRendering", 0);
                }
#endif

                SceneManagement.MarkSceneDirty();
                SceneView.RepaintAll();
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
            }
            finally
            {
                finalizeStarted = false;
                TTerrainGenerator.SetStatusToIdle();
                TProgressBar.RemoveAllProgressBarsWithTitle("TERRWORLD");
                ApplyRealTimeSettings();
            }
        }

        private static void ForceRecompile ()
        {
            AssetDatabase.StartAssetEditing();
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();

            foreach (string assetPath in allAssetPaths)
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath(assetPath, typeof(MonoScript)) as MonoScript;
                if (script != null)
                {
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    break;
                }
            }

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        public static void CleanUp(Terrain terrain)
        {
            TDebug.TraceMessage("CleanUp");
            Vector3 origin = Vector3.zero;
            int xStep = 5;
            int yStep = 5;
            Physics.autoSimulation = false;
            Physics.Simulate(Time.fixedDeltaTime);

            foreach (Transform t in terrain.GetComponentsInChildren(typeof(Transform), true))
            {
                if (t.GetComponent<MeshCollider>() != null && t.GetComponent<MeshFilter>() != null && t.GetComponent<Renderer>() != null)
                {
                    bool objectIsVisible = false;
                    Bounds objectBound = t.GetComponent<MeshRenderer>().bounds;

                    for (int i = (int)(objectBound.center.x - objectBound.extents.x); i < (int)(objectBound.center.x + objectBound.extents.x); i += xStep)
                    {
                        if (objectIsVisible) break;

                        for (int j = (int)(objectBound.center.z - objectBound.extents.z); j < (int)(objectBound.center.z + objectBound.extents.z); j += yStep)
                        {
                            origin.x = i;
                            origin.y = 100000;
                            origin.z = j;

                            Ray ray = new Ray(origin, Vector3.down);
                            RaycastHit hit;

                            if (!Raycasts.RaycastNonAllocSorted(ray, false, false, out hit))
                                continue;

                            if (hit.transform == t)
                            {
                                objectIsVisible = true;
                                break;
                            }
                        }
                    }

                    if (!objectIsVisible)
                        MonoBehaviour.DestroyImmediate(t.gameObject);
                }
            }

            Physics.autoSimulation = true;
        }

        private static void SetCameraClipPlanes(Terrain terrain)
        {
            //if (TTerraWorld.WorldGraph.globalGraph.GlobalNode.BGMountains)
            //    Camera.main.farClipPlane = terrain.terrainData.size.x * TTerraWorld.WorldGraph.globalGraph.GlobalNode.BGTerrainScaleMultiplier;
            //else
            //    Camera.main.farClipPlane = terrain.terrainData.size.x;

            Camera.main.farClipPlane = 200000f;
        }

        //public static void CloseProgressWindow()
        //{
        //    TProgressBar.RemoveProgressBar(TTerrainGenerator.progressID);
        //}

        private static void ClearMemory()
        {
            TTerrainGenerator.ClearMemory();
        }


        // Debugging
        //-------------------------------------------------------------------------------------------------------

        public static void ShowAreaBiomes(List<T2DObject> waysEntity, float scale)
        {
            string str = "";
            
            if (waysEntity?.Count > 0)
            {
                GameObject oceansDebug = new GameObject("Ocean Dummies");
            
                for (int i = 0; i < waysEntity.Count; i++)
                {
                    for (int j = 0; j < waysEntity[i].AroundPoints.Count; j++)
                    {
                        double worldSizeX = TTerraWorld.Area._areaSizeLon * 1000;
                        double worldSizeY = TTerraWorld.Area._areaSizeLat * 1000;
                        System.Numerics.Vector2 latlonDeltaNormalized = GetLatLongNormalizedPositionN(waysEntity[i].AroundPoints[j]);
                        System.Numerics.Vector2 worldPositionXZ = AreaBounds.GetWorldPositionFromTile(latlonDeltaNormalized.X, latlonDeltaNormalized.Y, worldSizeY, worldSizeX);
                        GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
                        if (waysEntity[i].AroundPoints[j].pointType == PointType.InputSide)
                            str = "-InputSide";
                        else if (waysEntity[i].AroundPoints[j].pointType == PointType.OutputSide)
                            str = "-OutputSide";
                        else
                            str = "-InSide";
            
                        if (waysEntity[i].property == Property.Outer)
                            dummy.name = "Outer" + i + "-" + j + str;
                        else if (waysEntity[i].property == Property.Inner)
                            dummy.name = "Inner" + i + "-" + j + str;
                        else
                            dummy.name = "Object" + i + "-" + j + str;
            
                        dummy.transform.position = new Vector3(worldPositionXZ.X - ((float)worldSizeX / 2f), 0, worldPositionXZ.Y - ((float)worldSizeY / 2f));
                        dummy.transform.localScale = new Vector3(scale, scale, scale);
                        dummy.transform.parent = oceansDebug.transform;
                    }
                }
            }
        }

        public static System.Numerics.Vector2 GetLatLongNormalizedPositionN(TGlobalPoint geoPoint)
        {
            double yMaxTop = AreaBounds.LatitudeToMercator(OSMParser.sAreaBounds._top);
            double xMinLeft = AreaBounds.LongitudeToMercator(OSMParser.sAreaBounds._left);
            double yMinBottom = AreaBounds.LatitudeToMercator(OSMParser.sAreaBounds._bottom);
            double xMaxRight = AreaBounds.LongitudeToMercator(OSMParser.sAreaBounds._right);
            double latSize = Math.Abs(yMaxTop - yMinBottom);
            double lonSize = Math.Abs(xMinLeft - xMaxRight);
            double LAT = AreaBounds.LatitudeToMercator(geoPoint.latitude);
            double LON = AreaBounds.LongitudeToMercator(geoPoint.longitude);
            double[] latlonDeltaNormalized = AreaBounds.GetNormalizedDeltaN(LAT, LON, yMaxTop, xMinLeft, latSize, lonSize);
            
            return new System.Numerics.Vector2((float)latlonDeltaNormalized[0], (float)latlonDeltaNormalized[1]);
        }
    }
}
#endif
#endif

