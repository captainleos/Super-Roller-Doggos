using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

#if TERRAWORLD_PRO
using UnityEngine.Rendering.PostProcessing;
using TerraUnity.Edittime;
#endif

#if TERRAWORLD_XPRO
using TerraUnity.Graph;
using XNodeEditor;
#endif

namespace TerraUnity.Runtime
{
    [ExecuteInEditMode]
    public class TTerraWorldManager : MonoBehaviour
    {

#pragma warning disable CS0414 // Add readonly modifier

#if  !TERRAWORLD_DEBUG
        [HideInInspector]
#endif
        public string _workDirectoryLocalPath1 = "";
        public static bool isQuitting = false;
#pragma warning restore CS0414 // Add readonly modifier

        public static bool worldIsInitialized = false;

        public bool _isTessalation = false;
        public static bool IsTessalation
        {
            get
            {
                return TerraWorldManagerScript._isTessalation;
            }
            set
            {
                TerraWorldManagerScript._isTessalation = value;
            }
        }

        public static TTerraWorldManager TerraWorldManagerScript { get => GetTerraWorldManager(); }
        private static TTerraWorldManager _terraWorldManagerScript;

        public static GameObject SceneSettingsGO1 { get => GetSceneSettingsGameObject(); }
        private static GameObject _sceneSettingsGO;

        public static TTerraWorldTerrainManager TerrainParamsScript { get => GetTerrainParams(); }
        private static TTerraWorldTerrainManager _terrainParamsScript;

        public static GameObject IsMainTerraworldGameObject { get => GetMainTerraworldGameObject(); }
        public static GameObject CreateAndGetTerraworldGameObject { get => CreateTerraworldGameObject(); }
        private static GameObject _mainTerraworldGameObject;

        public static GameObject MainTerrainGO { get => GetMainTerrainGameObject(); }
        private static GameObject _mainTerrainGO;

        public static Terrain MainTerrain { get => GetMainTerrain(); }
        private static Terrain _mainTerrain;

        public static GameObject BackgroundTerrainGO { get => GetBackgroundTerrainGameObject(); }
        private static GameObject _backgroundTerrainGO;

        public static Terrain BackgroundTerrain { get => GetBackgroundTerrain(); }
        private static Terrain _backgroundTerrain;

        public static GlobalTimeManager GlobalTimeManagerScript { get => GetGlobalTimeManagerScript(); }
        private static GlobalTimeManager _globalTimeManagerScript;
        [HideInInspector, Range(1, 3)] public int timeOfDayMode = 1; // 0 = off (Disabled for user controls), 1 = Manual (Controlled from TerraWorld), 2 = Auto (Day/Night Cycle)

        public static CloudsManager CloudsManagerScript { get => GetCloudsManagerScript(); }
        private static CloudsManager _cloudsManagerScript;

        public static Crepuscular GodRaysManagerScript { get => GetGodRaysManagerScript(); }
        private static Crepuscular _godRaysManagerScript;

        public static TimeOfDay TimeOfDayManagerScript { get => GetTimeOfDayManagerScript(); }
        private static TimeOfDay _timeOfDayManagerScript;

        public static HorizonFog HorizonFogManagerScript { get => GetHorizonFogManagerScript(); }
        private static HorizonFog _horizonFogManagerScript;

        public static WaterManager WaterManagerScript { get => GetWaterManagerScript(); }
        private static WaterManager _waterManagerScript;

        public static SnowManager SnowManagerScript { get => GetSnowManagerScript(); }
        private static SnowManager _snowManagerScript;

        public static WindManager WindManagerScript { get => GetWindManagerScript(); }
        private static WindManager _windManagerScript;

        public static FlatShadingManager FlatShadingManagerScript { get => GetFlatShadingManagerScript(); }
        private static FlatShadingManager _flatShadingManagerScript;

#if UNITY_STANDALONE_WIN
        public static AtmosphericScattering AtmosphericScatteringManagerScript { get => GetAtmosphericScatteringManagerScript(); }
        private static AtmosphericScattering _atmosphericScatteringManagerScript;

        public static AtmosphericScatteringSun AtmosphericScatteringSunScript { get => GetAtmosphericScatteringSunScript(); }
        private static AtmosphericScatteringSun _atmosphericScatteringSunScript;

        public static AtmosphericScatteringDeferred AtmosphericScatteringDeferredScript { get => GetAtmosphericScatteringDeferredScript(); }
        private static AtmosphericScatteringDeferred _atmosphericScatteringDeferredScript;
#endif

#if UNITY_STANDALONE_WIN
        public static VolumetricFog VolumetricFogManagerScript { get => GetVolumetricFogManagerScript(); }
        private static VolumetricFog _volumetricFogManagerScript;

        public static LightManagerFogLights LightFogManagerScript { get => GetLightFogManagerScript(); }
        private static LightManagerFogLights _lightFogManagerScript;
        public static float _fogLightIntensity;
        public static float _fogLightRange;
#endif
        public static string WorkDirectoryLocalPath { get => getWorkDirectoryLocalPath(); }
        private static string defaultMainTerraworldGameObjectName = "TerraWorld";

        public static string cloudsMaterialName = "Clouds Material.mat";
        public static string cloudsPrefabName = "Clouds Prefab.prefab";
        public static string godRaysMaterialName = "GodRays Material.mat";
        public static string skyMaterialName = "Sky Material.mat";
        public static string starsPrefabName = "Stars Prefab.prefab";
        public static string horizonMaterialName = "Horizon Material.mat";
        public static string postProcessingProfileName = "PostProcessing Profile.asset";
        //public static string waterMaterialName = "Water Material.mat";
        private static string getWorkDirectoryLocalPath()
        {
            string path = TerraWorldManagerScript.GetWorkingDirectoryLocalName();
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        private static GameObject CreateTerraworldGameObject()
        {
#if UNITY_EDITOR
            if (IsMainTerraworldGameObject == null)
            {
                _mainTerraworldGameObject = new GameObject(defaultMainTerraworldGameObjectName);
                _terraWorldManagerScript = _mainTerraworldGameObject.AddComponent<TTerraWorldManager>();
                _mainTerraworldGameObject.AddComponent<WorldTools>();
            }
#endif
            return _mainTerraworldGameObject;
        }

        private static GameObject GetMainTerraworldGameObject()
        {
            if (_mainTerraworldGameObject == null)
            {
                _mainTerraworldGameObject = GameObject.Find(defaultMainTerraworldGameObjectName);

                foreach (GameObject go in FindObjectsOfType(typeof(GameObject)) as GameObject[])
                {
                    if (go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                        if (go.GetComponent<TTerraWorldManager>() != null)
                        {
                            _mainTerraworldGameObject = go;
                            break;
                        }
                }

#if UNITY_EDITOR
                if (_mainTerraworldGameObject == null)
                    foreach (GameObject go in FindObjectsOfType(typeof(GameObject)) as GameObject[])
                    {
                        if (go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                            if (go.GetComponent<WorldTools>() != null)
                            {
                                _mainTerraworldGameObject = go;
                                break;
                            }
                    }
#endif
            }

            return _mainTerraworldGameObject;

        }

        private static TTerraWorldManager GetTerraWorldManager()
        {
            if (_terraWorldManagerScript == null)
            {
                _terraWorldManagerScript = CreateAndGetTerraworldGameObject.GetComponent<TTerraWorldManager>();

                if (_terraWorldManagerScript == null)
                    _terraWorldManagerScript = CreateAndGetTerraworldGameObject.AddComponent<TTerraWorldManager>();
            }

            return _terraWorldManagerScript;
        }

        private static GameObject GetMainTerrainGameObject()
        {
            if (IsMainTerraworldGameObject == null) return null;
            if (_mainTerrainGO == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    Terrain terrain = t.GetComponent<Terrain>();

                    if (terrain != null && terrain.GetComponent<TTerraWorldTerrainManager>() != null)
                    {
                        _mainTerrainGO = t.gameObject;
                        break;
                    }
                }
            }

            return _mainTerrainGO;
        }

        private static Terrain GetMainTerrain()
        {
            if (IsMainTerraworldGameObject == null) return null;
            if (_mainTerrain == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    Terrain terrain = t.GetComponent<Terrain>();

                    if (terrain != null && terrain.GetComponent<TTerraWorldTerrainManager>() != null)
                    {
                        _mainTerrain = terrain;
                        break;
                    }
                }
            }

            return _mainTerrain;
        }

        private static GameObject GetBackgroundTerrainGameObject()
        {
            if (MainTerrainGO == null) return null;
            if (_backgroundTerrainGO == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    Terrain terrain = t.GetComponent<Terrain>();

                    if (terrain != null && terrain.gameObject.name == "Background Terrain")
                    {
                        _backgroundTerrainGO = t.gameObject;
                        break;
                    }
                }
            }

            return _backgroundTerrainGO;
        }

        private static Terrain GetBackgroundTerrain()
        {
            if (GetBackgroundTerrainGameObject() == null) return null;
            if (_backgroundTerrain == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    Terrain terrain = t.GetComponent<Terrain>();

                    if (terrain != null && terrain.gameObject.name == "Background Terrain")
                    {
                        _backgroundTerrain = terrain;
                        break;
                    }
                }
            }

            return _backgroundTerrain;
        }

        private static TTerraWorldTerrainManager GetTerrainParams()
        {
            if (MainTerrainGO == null) return null;

            if (_terrainParamsScript == null)
            {
                _terrainParamsScript = MainTerrainGO.GetComponent<TTerraWorldTerrainManager>();

                if (_terrainParamsScript == null)
                    _terrainParamsScript = MainTerrainGO.AddComponent<TTerraWorldTerrainManager>();
            }

            return _terrainParamsScript;
        }

        private static GlobalTimeManager GetGlobalTimeManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;
            if (_globalTimeManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    GlobalTimeManager script = t.GetComponent<GlobalTimeManager>();

                    if (script != null)
                    {
                        _globalTimeManagerScript = script;
                        break;
                    }
                }

                if (_globalTimeManagerScript == null)
                    if (SceneSettingsGO1 != null)
                    {
                        _globalTimeManagerScript = SceneSettingsGO1.AddComponent<GlobalTimeManager>();
                        _globalTimeManagerScript.SetDefaults();
                    }
            }

            return _globalTimeManagerScript;
        }

        private static CloudsManager GetCloudsManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;
            if (_cloudsManagerScript == null)
            {
                foreach (Transform t in _mainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    CloudsManager script = t.GetComponent<CloudsManager>();

                    if (script != null)
                    {
                        _cloudsManagerScript = script;
                        break;
                    }
                }

                if (_cloudsManagerScript == null)
                    if (SceneSettingsGO1 != null)
                        _cloudsManagerScript = SceneSettingsGO1.AddComponent<CloudsManager>();
            }
            else
            {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
                //if (!worldIsInitialized)
                {
                    if (_cloudsManagerScript.cloudsMaterial == null)
                    {
                        Material mat = AssetDatabase.LoadAssetAtPath(WorkDirectoryLocalPath + cloudsMaterialName, typeof(Material)) as Material;

                        if (mat != null)
                            _cloudsManagerScript.cloudsMaterial = mat;
                        else
                        {
                            TResourcesManager.LoadCloudsResources();
                            _cloudsManagerScript.cloudsMaterial = TResourcesManager.cloudsMaterial;
                        }
                    }

                    if (_cloudsManagerScript.cloudPrefab == null)
                    {
                        GameObject go = AssetDatabase.LoadAssetAtPath(WorkDirectoryLocalPath + cloudsPrefabName, typeof(GameObject)) as GameObject;

                        if (go != null)
                            _cloudsManagerScript.cloudPrefab = go;
                        else
                        {
                            TResourcesManager.LoadCloudsResources();
                            _cloudsManagerScript.cloudPrefab = TResourcesManager.cloudPrefab;
                        }
                    }
                }

                if (_cloudsManagerScript.cloudMesh == null)
                {
                    TResourcesManager.LoadCloudsResources();
                    _cloudsManagerScript.cloudMesh = TResourcesManager.cloudMesh;
                }
#endif
#endif
            }

            return _cloudsManagerScript;
        }

        private static Crepuscular GetGodRaysManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;
            if (_godRaysManagerScript == null)
            {
                if (Camera.main != null)
                {
                    Crepuscular script = Camera.main.gameObject.GetComponent<Crepuscular>();

                    if (script != null)
                        _godRaysManagerScript = script;
                    else
                        _godRaysManagerScript = Camera.main.gameObject.AddComponent<Crepuscular>();
                }

                if (_godRaysManagerScript == null)
                {
                    if (TimeOfDayManagerScript != null && TimeOfDayManagerScript.player != null)
                    {
                        Crepuscular script = TimeOfDayManagerScript.player.GetComponent<Crepuscular>();

                        if (script != null)
                            _godRaysManagerScript = script;
                        else
                            _godRaysManagerScript = TimeOfDayManagerScript.player.AddComponent<Crepuscular>();
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
                //if (!worldIsInitialized)
                {
                    if (_godRaysManagerScript.material == null)
                    {
                        Material mat = AssetDatabase.LoadAssetAtPath(WorkDirectoryLocalPath + godRaysMaterialName, typeof(Material)) as Material;

                        if (mat != null)
                            _godRaysManagerScript.material = mat;
                        else
                        {
                            TResourcesManager.LoadGodRaysResources();
                            _godRaysManagerScript.material = TResourcesManager.godRaysMaterial;
                        }
                    }
                }
#endif
#endif
                if (_godRaysManagerScript.sun == null)
                {
                    if (TimeOfDayManagerScript != null && TimeOfDayManagerScript.sun != null)
                        _godRaysManagerScript.sun = TimeOfDayManagerScript.sun;
                }
            }

            return _godRaysManagerScript;
        }

        private static TimeOfDay GetTimeOfDayManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_timeOfDayManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    TimeOfDay script = t.GetComponent<TimeOfDay>();

                    if (script != null)
                    {
                        _timeOfDayManagerScript = script;
                        break;
                    }
                }

                if (_timeOfDayManagerScript == null)
                    if (SceneSettingsGO1 != null)
                        _timeOfDayManagerScript = SceneSettingsGO1.AddComponent<TimeOfDay>();

                if (_timeOfDayManagerScript != null)
                    SetTimeOfDayParams();
            }
            else
                SetTimeOfDayParams();

            return _timeOfDayManagerScript;
        }

        public static void SetTimeOfDayParams()
        {
            if (SceneSettingsGO1 == null) return;

            if (!isQuitting && SceneSettingsGO1 != null)
            {
                if (_timeOfDayManagerScript == null) GetTimeOfDayManagerScript();

                if (_timeOfDayManagerScript.sun == null)
                {
                    foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                        if (go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                            if (go.GetComponent<Light>() != null && go.GetComponent<Light>().type == LightType.Directional)
                            {
                                _timeOfDayManagerScript.sun = go;
                                break;
                            }
                }

                if (_timeOfDayManagerScript.sun == null) return;

                if (_timeOfDayManagerScript.sunLight == null)
                    _timeOfDayManagerScript.sunLight = _timeOfDayManagerScript.sun.GetComponent<Light>();

                if (_timeOfDayManagerScript.player == null) _timeOfDayManagerScript.player = Camera.main?.gameObject;
                if (_timeOfDayManagerScript.player == null)
                {
                    foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                        if (go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                            if (go.GetComponent<Camera>() != null)
                            {
                                _timeOfDayManagerScript.player = go;
                                break;
                            }
                }

                //if (!worldIsInitialized)
                {
                    if (_timeOfDayManagerScript.stars == null)
                    {
                        foreach (Transform t in SceneSettingsGO1.GetComponentsInChildren<Transform>(true))
                            if (t != null && t.name == "Night Stars")
                            {
                                _timeOfDayManagerScript.stars = t.gameObject;
                                break;
                            }

#if UNITY_EDITOR
                        if (_timeOfDayManagerScript.stars == null)
                        {
                            string starsPrefabPath = "";

                            if (File.Exists(WorkDirectoryLocalPath + starsPrefabName))
                                starsPrefabPath = WorkDirectoryLocalPath + starsPrefabName;
                            else
                            {
                                TResourcesManager.LoadTimeOfDayResources();
                                starsPrefabPath = AssetDatabase.GetAssetPath(TResourcesManager.starsPrefab);
                            }

                            GameObject starsPrefabGO = AssetDatabase.LoadAssetAtPath(starsPrefabPath, typeof(GameObject)) as GameObject;

                            foreach (Transform t in SceneSettingsGO1.GetComponentsInChildren<Transform>(true))
                                if (t != null && t.name == "Night Stars")
                                    MonoBehaviour.DestroyImmediate(t.gameObject);
                            if (starsPrefabGO != null)
                            {
                                _timeOfDayManagerScript.stars = MonoBehaviour.Instantiate(starsPrefabGO);
                                _timeOfDayManagerScript.stars.name = "Night Stars";
                                _timeOfDayManagerScript.stars.transform.parent = _timeOfDayManagerScript.transform;
                                _timeOfDayManagerScript.stars.transform.localPosition = Vector3.zero;

                                _timeOfDayManagerScript.UpdateAtmosphere(true);
                            }

                        }
#endif
                    }

                    if (_timeOfDayManagerScript.stars != null && _timeOfDayManagerScript.starsRenderer == null)
                    {
                        _timeOfDayManagerScript.starsRenderer = _timeOfDayManagerScript.stars.GetComponent<ParticleSystemRenderer>();

#if UNITY_EDITOR
                        if (_timeOfDayManagerScript.starsRenderer != null)
                        {
                            PreviewAllParticlesInEditorScene();
                            //ParticleSystem particleSystem = timeOfDay.stars.GetComponent<ParticleSystem>();
                            //particleSystem?.Simulate(2);
                            //particleSystem?.Play();
                        }
                        else
                            throw new Exception("Stars Renderer Error!");
#endif

                        _timeOfDayManagerScript.UpdateAtmosphere(true);
                    }


                    if (_timeOfDayManagerScript.skyMaterial != null)
                        RenderSettings.skybox = _timeOfDayManagerScript.skyMaterial;
                    else
                    {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
                        string skyMaterialPath = "";

                        if (File.Exists(WorkDirectoryLocalPath + skyMaterialName))
                            skyMaterialPath = WorkDirectoryLocalPath + skyMaterialName;
                        else
                        {
                            TResourcesManager.LoadTimeOfDayResources();
                            skyMaterialPath = AssetDatabase.GetAssetPath(TResourcesManager.starsPrefab);
                        }

                        _timeOfDayManagerScript.skyMaterial = AssetDatabase.LoadAssetAtPath(skyMaterialPath, typeof(Material)) as Material;
                        RenderSettings.skybox = _timeOfDayManagerScript.skyMaterial;

                        _timeOfDayManagerScript.UpdateAtmosphere(true);
#endif
#endif
                    }
                }
            }
        }

#if UNITY_EDITOR
        private static void PreviewAllParticlesInEditorScene()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Editor));
            if (assembly == null) return;

            Type particleSystemEditorUtilsType = assembly.GetType("UnityEditor.ParticleSystemEditorUtils");
            if (particleSystemEditorUtilsType == null) return;

            PropertyInfo previewLayers = particleSystemEditorUtilsType.GetProperty("previewLayers", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (previewLayers == null) return;

            uint allLayers = Convert.ToUInt32(uint.MaxValue);
            previewLayers.SetValue(null, allLayers);
        }
#endif

        private static HorizonFog GetHorizonFogManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;
            if (_horizonFogManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    HorizonFog script = t.GetComponent<HorizonFog>();

                    if (script != null)
                    {
                        _horizonFogManagerScript = script;
                        break;
                    }
                }

                if (_horizonFogManagerScript == null)
                {
                    if (SceneSettingsGO1 != null)
                    {
                        GameObject horizonFogGameObject = new GameObject("Horizon Fog");
                        horizonFogGameObject.transform.parent = SceneSettingsGO1.transform;
                        horizonFogGameObject.transform.position = new Vector3(0, -90000f, 0);
                        horizonFogGameObject.transform.eulerAngles = new Vector3(180, 0, 0);
                        horizonFogGameObject.AddComponent<MeshFilter>();
                        horizonFogGameObject.AddComponent<MeshRenderer>();
                        horizonFogGameObject.AddComponent<CameraXZ>();
                        _horizonFogManagerScript = horizonFogGameObject.AddComponent<HorizonFog>();
                        _horizonFogManagerScript.texture = null;
                        _horizonFogManagerScript.textureScale = 1;
                        _horizonFogManagerScript.textureMovement = Vector3.zero;
                        _horizonFogManagerScript.coneHeight = 100000;
                        _horizonFogManagerScript.coneAngle = 60;
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
                //if (!worldIsInitialized)
                {
                    if (_horizonFogManagerScript.volumetricMaterial == null)
                    {
                        Material mat = AssetDatabase.LoadAssetAtPath(WorkDirectoryLocalPath + horizonMaterialName, typeof(Material)) as Material;

                        if (mat != null)
                            _horizonFogManagerScript.volumetricMaterial = mat;
                        else
                        {
                            TResourcesManager.LoadHorizonFogResources();
                            _horizonFogManagerScript.volumetricMaterial = TResourcesManager.volumetricHorizonMaterial;
                        }
                    }
                }
#endif
#endif
            }

            return _horizonFogManagerScript;
        }

        private static WaterManager GetWaterManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;
            if (_waterManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    WaterManager script = t.GetComponent<WaterManager>();

                    if (script != null)
                    {
                        _waterManagerScript = script;
                        break;
                    }
                }

                if (_waterManagerScript == null)
                    if (SceneSettingsGO1 != null)
                        _waterManagerScript = SceneSettingsGO1.AddComponent<WaterManager>();
            }

            return _waterManagerScript;
        }

        private static SnowManager GetSnowManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_snowManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    SnowManager script = t.GetComponent<SnowManager>();

                    if (script != null)
                    {
                        _snowManagerScript = script;
                        break;
                    }
                }

                if (_snowManagerScript == null)
                {
                    if (SceneSettingsGO1 != null)
                    {
                        _snowManagerScript = SceneSettingsGO1.AddComponent<SnowManager>();
                        _snowManagerScript.Init();
                    }
                }
            }

            return _snowManagerScript;
        }

        private static WindManager GetWindManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_windManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    WindManager script = t.GetComponent<WindManager>();

                    if (script != null)
                    {
                        _windManagerScript = script;
                        break;
                    }
                }

                if (_windManagerScript == null)
                {
                    if (SceneSettingsGO1 != null)
                    {
                        _windManagerScript = SceneSettingsGO1.AddComponent<WindManager>();
                        _windManagerScript.Init();
                    }
                }
            }

            return _windManagerScript;
        }

        private static FlatShadingManager GetFlatShadingManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_flatShadingManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    FlatShadingManager script = t.GetComponent<FlatShadingManager>();

                    if (script != null)
                    {
                        _flatShadingManagerScript = script;
                        break;
                    }
                }

                if (_flatShadingManagerScript == null)
                {
                    if (SceneSettingsGO1 != null)
                    {
                        _flatShadingManagerScript = SceneSettingsGO1.AddComponent<FlatShadingManager>();
#if TERRAWORLD_PRO
#if UNITY_EDITOR
                        _flatShadingManagerScript.Init();
#endif
#endif
                    }
                }
            }

            return _flatShadingManagerScript;
        }

#if UNITY_STANDALONE_WIN
        private static AtmosphericScattering GetAtmosphericScatteringManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_atmosphericScatteringManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    AtmosphericScattering script = t.GetComponent<AtmosphericScattering>();

                    if (script != null)
                    {
                        _atmosphericScatteringManagerScript = script;
                        break;
                    }
                }

                if (_atmosphericScatteringManagerScript == null)
                    if (SceneSettingsGO1 != null)
                        _atmosphericScatteringManagerScript = SceneSettingsGO1.AddComponent<AtmosphericScattering>();
            }
            else
            {
                GetAtmosphericScatteringSunScript();
                GetAtmosphericScatteringDeferredScript();
            }

            return _atmosphericScatteringManagerScript;
        }

        private static AtmosphericScatteringSun GetAtmosphericScatteringSunScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_atmosphericScatteringSunScript == null)
            {
                if (TimeOfDayManagerScript != null && TimeOfDayManagerScript.sun != null)
                {
                    _atmosphericScatteringSunScript = TimeOfDayManagerScript.sun.GetComponent<AtmosphericScatteringSun>();

                    if (_atmosphericScatteringSunScript == null)
                        _atmosphericScatteringSunScript = TimeOfDayManagerScript.sun.AddComponent<AtmosphericScatteringSun>();
                }
            }

            return _atmosphericScatteringSunScript;
        }

        private static AtmosphericScatteringDeferred GetAtmosphericScatteringDeferredScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_atmosphericScatteringDeferredScript == null)
            {
                if (TimeOfDayManagerScript != null && TimeOfDayManagerScript.player != null)
                {
                    _atmosphericScatteringDeferredScript = TimeOfDayManagerScript.player.GetComponent<AtmosphericScatteringDeferred>();

                    if (_atmosphericScatteringDeferredScript == null)
                        _atmosphericScatteringDeferredScript = TimeOfDayManagerScript.player.AddComponent<AtmosphericScatteringDeferred>();
                }
            }

            return _atmosphericScatteringDeferredScript;
        }
#endif

#if UNITY_STANDALONE_WIN
        private static VolumetricFog GetVolumetricFogManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_volumetricFogManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    VolumetricFog script = t.GetComponent<VolumetricFog>();

                    if (script != null && t.GetComponent<Camera>() != null)
                    {
                        _volumetricFogManagerScript = script;
                        break;
                    }
                }

                if (_volumetricFogManagerScript == null)
                {
                    if (TimeOfDayManagerScript != null && TimeOfDayManagerScript.player != null)
                    {
                        _volumetricFogManagerScript = TimeOfDayManagerScript.player.GetComponent<VolumetricFog>();

                        if (_volumetricFogManagerScript == null)
                            _volumetricFogManagerScript = TimeOfDayManagerScript.player.AddComponent<VolumetricFog>();
                    }
                }
            }
            else
                GetLightFogManagerScript();

            return _volumetricFogManagerScript;
        }

        private static LightManagerFogLights GetLightFogManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_lightFogManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    LightManagerFogLights script = t.GetComponent<LightManagerFogLights>();

                    if (script != null)
                    {
                        _lightFogManagerScript = script;
                        break;
                    }
                }

                if (_lightFogManagerScript == null)
                    if (SceneSettingsGO1 != null)
                        _lightFogManagerScript = SceneSettingsGO1.AddComponent<LightManagerFogLights>();

                AddFogToAllLights();
            }

            return _lightFogManagerScript;
        }

        private static void AddFogToAllLights()
        {
            if (VolumetricFogManagerScript == null || LightFogManagerScript == null || !VolumetricFogManagerScript.enabled) return;
            _fogLightIntensity = 1;
            _fogLightRange = 1;

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                {
                    if (go.GetComponent<Light>() != null && go.GetComponent<Light>().type != LightType.Directional)
                    {
                        FogLight fogLight = go.GetComponent<FogLight>();

                        if (fogLight != null)
                        {
                            _fogLightIntensity = fogLight.m_IntensityMult;
                            _fogLightRange = fogLight.m_RangeMult;
                            //MonoBehaviour.DestroyImmediate(fogLight);
                        }
                        else
                        {
                            fogLight = go.AddComponent<FogLight>();
                            fogLight.m_IntensityMult = _fogLightIntensity;
                            fogLight.m_RangeMult = _fogLightRange;
                        }
                    }
                }
            }
        }
#endif


        private string GetWorkingDirectoryLocalName()
        {
#if UNITY_EDITOR
#if TERRAWORLD_PRO

            if (string.IsNullOrEmpty(_workDirectoryLocalPath1))
            {
                System.Random rand = new System.Random((int)DateTime.Now.Ticks);
                int WorldID = rand.Next();
                _workDirectoryLocalPath1 = TAddresses.GetWorkDirectoryPath(WorldID);
            }

            if (!Directory.Exists(_workDirectoryLocalPath1))
                Directory.CreateDirectory(_workDirectoryLocalPath1);

#endif
#endif
            return _workDirectoryLocalPath1;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
            EditorApplication.quitting += Quit;
#endif
#endif
        }

        private void Awake()
        {
#if UNITY_STANDALONE_WIN
            AddFogToAllLights();
#endif
        }

#if UNITY_EDITOR
#if !TERRAWORLD_DEBUG 
        [HideInInspector]
#endif
        public UnityEngine.Object graphFile;
        private static int oldGraphFileHash = 0;


#if TERRAWORLD_XPRO
#if !TERRAWORLD_DEBUG 
        [HideInInspector]
#endif
        public UnityEngine.Object xGraphFile;
#endif

#if TERRAWORLD_PRO
        private static TTerraWorldGraph oldGraph = null;
        static void Quit()
        {
            isQuitting = true;
        }
#endif
#endif

#if UNITY_EDITOR
#if TERRAWORLD_PRO

        public static TTerraWorldGraph WorldGraph { get => GetOldGraph(); set => SaveOldGraph(value); }

        private static void SaveOldGraph(TTerraWorldGraph Graph1)
        {
            oldGraph = Graph1;
            SaveGraph();
        }

        private static TTerraWorldGraph GetOldGraph()
        {
            if (TerraWorldManagerScript.graphFile == null) oldGraph = null;
            else if (oldGraphFileHash != TerraWorldManagerScript.graphFile.GetHashCode()) oldGraph = null;

            if (oldGraph != null) 
                return oldGraph;

            oldGraph = TTerraWorldGraph.GetNewWorldGraph(TVersionController.MajorVersion, TVersionController.MinorVersion);
            try
            {
                if (!string.IsNullOrEmpty(TerraWorldGraphPath))
                {
                    string path = TerraWorldGraphPath;

                    if (TTerraWorldGraph.CheckGraph(TerraWorldGraphPath))
                    {
                        bool reGenerate = oldGraph.LoadGraph(path, false);
                    }
                    else
                    {
                        oldGraph = null;
                        throw new Exception("Internal Error 276!");
                    }
                }
                else if (File.Exists(TTerraWorld.WorkDirectoryFullPath + "graph.xml"))
                {
                    string path = TTerraWorld.WorkDirectoryFullPath + "graph.xml";

                    if (TTerraWorldGraph.CheckGraph(path))
                    {
                        bool reGenerate = oldGraph.LoadGraph(path, false);
                        SaveOldGraph(oldGraph);
                    }
                    else
                    {
                        oldGraph = null;
                        throw new Exception("Internal Error 277!");
                    }

                }
                else if (File.Exists("Assets/TerraWorld/Core/Presets/Graph.xml"))
                {
                    string path = TAddresses.projectPath + "Assets/TerraWorld/Core/Presets/Graph.xml";

                    if (TTerraWorldGraph.CheckGraph(path))
                    {
                        bool reGenerate = oldGraph.LoadGraph(path, false);
                        SaveOldGraph(oldGraph);
                    }
                    else
                    {
                        oldGraph = null;
                        throw new Exception("Internal Error 278!");
                    }
                }
                else
                ResetOldGraph();

                SaveGraph();
                return oldGraph;
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
                return null;
            }
        }

        public static void ResetOldGraph()
        {
            TAreaGraph OldareaGraphArea = null;

            if (oldGraph != null)
                OldareaGraphArea = oldGraph.areaGraph;

            oldGraph = TTerraWorldGraph.GetNewWorldGraph(TVersionController.MajorVersion, TVersionController.MinorVersion);

            if (OldareaGraphArea != null)
                oldGraph.areaGraph = OldareaGraphArea;

            SaveOldGraph(oldGraph);
        }

        public static void SaveGraph()
        {
            try
            {
                string savedPath = WorkDirectoryLocalPath + "graph.xml";
                oldGraph.SaveGraph(savedPath);
                AssetDatabase.Refresh();
                TerraWorldManagerScript.graphFile = AssetDatabase.LoadAssetAtPath(savedPath, typeof(UnityEngine.Object));
                oldGraphFileHash = TerraWorldManagerScript.graphFile.GetHashCode();
            }
            catch {}
        }

        public static string TerraWorldGraphPath { get => TerraWorldManagerScript.GraphFilePath; set => TerraWorldManagerScript.GraphFilePath = value; }

        private void CheckGraphFile()
        {
            if (graphFile != null)
            {
                string path = AssetDatabase.GetAssetPath(graphFile);

                if (!TTerraWorldGraph.CheckGraph(path))
                    graphFile = null;
            }
        }

        public string GraphFilePath
        {
            get
            {
                if (graphFile != null)
                {
                    string path = TAddresses.projectPath + AssetDatabase.GetAssetPath(graphFile);
                    return path;
                }
                else
                {

                }
                    return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    //AssetDatabase.Refresh();
                    value = value.Replace(TAddresses.projectPath, "");
                    graphFile = AssetDatabase.LoadAssetAtPath(value, typeof(UnityEngine.Object));
                    CheckGraphFile();
                }
                else
                    graphFile = null;
            }
        }

        public static void UpdateWorldGraphFromScene()
        {

            if (TimeOfDayManagerScript != null)
            {
                WorldGraph.TimeOfDayParams = TimeOfDayManagerScript.GetParams();
                WorldGraph._timeOfDayParamsSaved = true;
            }
            else
                WorldGraph._timeOfDayParamsSaved = false;

            if (SceneSettingsGO1 != null)
                WorldGraph.FXGraph.GetEntryNode().fxParams = SceneSettingsManager.FXParameters;

            if (TerrainRenderingManager.TerrainMaterial != null)
                WorldGraph.renderingGraph.GetEntryNode().renderingParams = TerrainRenderingManager.GetRenderingParams();

            SaveOldGraph(WorldGraph);
        }

#endif

#if TERRAWORLD_XPRO

        public static TXGraph XGraph { get => GetXGraph(); set => SaveXGraph(value); }

        private static void SaveXGraph(TXGraph XGraph1)
        {
            string savedPath = WorkDirectoryLocalPath + "xgraph.asset";
            TerraWorldManagerScript.xGraphFile = XGraph1;
            if (File.Exists(savedPath)) AssetDatabase.DeleteAsset(savedPath);
            AssetDatabase.CreateAsset(TerraWorldManagerScript.xGraphFile, savedPath);
            AssetDatabase.Refresh();
        }

        private static TXGraph GetXGraph()
        {
            try
            {
                if (TerraWorldManagerScript.xGraphFile != null) 
                    return (TXGraph)TerraWorldManagerScript.xGraphFile;
                else
                {
                    ResetXGraph();
                    return (TXGraph)TerraWorldManagerScript.xGraphFile;
                }
            }
            catch (Exception e)
            {
                TDebug.LogErrorToUnityUI(e);
                return null;
            }
        }

        public static void ResetXGraph()
        {
            XGraph = TTerraWorldGraph.GetNewXGraph(TVersionController.MajorVersion, TVersionController.MinorVersion);
        }

#endif

#endif

        private static GameObject GetSceneSettingsGameObject()
        {
            // if (_mainTerraworldGameObject == null) return null;
            if (IsMainTerraworldGameObject == null) return null;

            if (_sceneSettingsGO == null)
            {
                foreach (Transform t in _mainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                    if (t.hideFlags != HideFlags.NotEditable && t.hideFlags != HideFlags.HideAndDontSave && t.gameObject.scene.IsValid())
                        if (t.name == "Scene Settings" && t.GetComponent<SceneSettingsGameObjectManager>() != null)
                        {
                            _sceneSettingsGO = t.gameObject;
                            break;
                        }
            }

            return _sceneSettingsGO;
        }

        public static void CreateSceneSettingsGameObject()
        {
#if UNITY_EDITOR
#if TERRAWORLD_PRO
            if (!isQuitting && SceneSettingsGO1 == null)
                _sceneSettingsGO = SceneSettingsManager.InstantiateSceneSettings();
#endif
#endif
        }

#if TERRAWORLD_PRO
        public static PostProcessLayer PostProcessLayerScript { get => GetPostProcessLayerScript(); }
        private static PostProcessLayer _postProcessLayerScript;

        public static PostProcessVolume PostProcessVolumeManagerScript { get => GetPostProcessVolumeManagerScript(); }

        private static PostProcessVolume _postProcessVolumeManagerScript;

        private static PostProcessLayer GetPostProcessLayerScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_postProcessLayerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    PostProcessLayer script = t.GetComponent<PostProcessLayer>();

                    if (script != null)
                    {
                        _postProcessLayerScript = script;
                        break;
                    }
                }

                if (_postProcessLayerScript == null)
                {
                    if (TimeOfDayManagerScript != null && TimeOfDayManagerScript.player != null)
                    {
                        _postProcessLayerScript = TimeOfDayManagerScript.player.GetComponent<PostProcessLayer>();

                        if (_postProcessLayerScript == null)
                            _postProcessLayerScript = TimeOfDayManagerScript.player.AddComponent<PostProcessLayer>();

                        _postProcessLayerScript.volumeLayer = LayerMask.GetMask("TransparentFX");
                        _postProcessLayerScript.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                        _postProcessLayerScript.enabled = true;
                        PostProcessVolumeManagerScript.enabled = true;
                    }
                }
            }

            return _postProcessLayerScript;
        }

        private static PostProcessVolume GetPostProcessVolumeManagerScript()
        {
            if (SceneSettingsGO1 == null) return null;

            if (_postProcessVolumeManagerScript == null)
            {
                foreach (Transform t in IsMainTerraworldGameObject.GetComponentsInChildren(typeof(Transform), true))
                {
                    PostProcessVolume script = t.GetComponent<PostProcessVolume>();

                    if (script != null)
                    {
                        _postProcessVolumeManagerScript = script;
                        break;
                    }
                }

                if (_postProcessVolumeManagerScript == null)
                {
                    if (SceneSettingsGO1 != null)
                    {
                        _postProcessVolumeManagerScript = SceneSettingsGO1.GetComponent<PostProcessVolume>();

                        if (_postProcessVolumeManagerScript == null)
                        {
                            _postProcessVolumeManagerScript = SceneSettingsGO1.AddComponent<PostProcessVolume>();
                            _postProcessVolumeManagerScript.isGlobal = true;
                        }
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                //if (!worldIsInitialized)
                {
                    if (_postProcessVolumeManagerScript.sharedProfile == null)
                    {
                        PostProcessProfile PPP = AssetDatabase.LoadAssetAtPath(WorkDirectoryLocalPath + postProcessingProfileName, typeof(PostProcessProfile)) as PostProcessProfile;

                        if (PPP != null)
                            _postProcessVolumeManagerScript.sharedProfile = PPP;
                        //_postProcessVolumeManagerScript.profile = PPP;
                        else
                        {
                            TResourcesManager.LoadPostProcessingResources();
                            _postProcessVolumeManagerScript.sharedProfile = TResourcesManager.postProcessingAsset;
                            //_postProcessVolumeManagerScript.profile = TResourcesManager.postProcessingAsset;
                        }

                        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                    }
                }
#endif
            }

            if (_postProcessVolumeManagerScript != null && !_postProcessVolumeManagerScript.isGlobal)
                _postProcessVolumeManagerScript.isGlobal = true;

            return _postProcessVolumeManagerScript;
        }

#endif
    }
}

