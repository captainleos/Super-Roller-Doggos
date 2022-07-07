#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Xml.Serialization;

namespace TerraUnity.Edittime
{
    public enum FXModules
    {
        FXModule
    }

    public abstract class TFXModules : TNode
    {
        public FXParams fxParams = new FXParams(true);

        public TFXModules() : base()
        {
        }
    }

    public enum WaterQuality
    {
        Low,
        Medium,
        High
    }

    public enum HorizonBlendMode
    {
        Overlay,
        Transparent
    }

    public struct FXParams
    {
        public int selectionIndexVFX;

        // Time of day settings
        public int dayNightControl; // 0 = off, 1 = Manual, 2 = Auto
        public bool dayNightControlState;
        public float elevation;
        public float azimuth;
        public float globalSpeed;
        public bool lightmappingControls;
        public float dayNightUpdateIntervalInSeconds;

        // Crepuscular Rays settings
        public bool hasGodRays;
        public float godRaySamples;
        public float godRayDensity;
        public float godRayWeight;
        public float godRayDecay;
        public float godRayExposure;

        // Clouds settings
        public bool hasClouds;
        public bool cloudShadows;
        public int cloudsSeed;
        public float cloudsDensity;
        public float cloudSize;
        public float cloudsAltitude;
        public int emitProbability;
        public Vector4 cloudColor;
        public int meshMode;
        //public string Modelpath;
        //public string MeshName;
        public float cloudNormalOffset;
        public string cloudMeshTexturePath;

        // Atmospheric Scattering settings
        public bool hasAtmosphericScattering;
        public float volumetricLightIntensity;
        public float atmosphericFogIntensity;
        public float atmosphericFogDensity;
        public float atmosphericFogDistance;

        // Volumetric Fog settings
        public bool hasVolumetricFog;
        public float fogStrength;
        public float fogWindSpeed;
        public float fogNearClip;
        public float fogFarClip;
        public float volumetricFogDensity;
        public float fogNoiseAmount;
        public float fogNoiseScale;
        public Vector4 volumetricFogColor;

        // Wind settings
        public bool hasWind;
        public float windTime;
        public float windSpeed;
        public float windBending;

        // Weather settings
        public bool hasSnow;
        public float snowStartHeight;
        public float snowThickness;
        public float snowDamping;
        public bool BGTerrainHasSnow;
        public float heightFalloff;

        // Water settings
        public WaterQuality waterQuality;
        public Vector4 waterBaseColor;
        public Vector4 waterReflectionColor;
        public float reflectionQuality;
        public float reflectionDistance;
        public bool edgeBlend;
        public bool specularLighting;
        public bool planarReflection;
        public bool GerstnerWaves;

        // Real-time Reflections settings
        public bool hasReflection;

        // Post Processing settings
        public int isPostProcessing; // 0 is true, 1 is false

        // Horizon Fog settings
        public bool hasHorizonFog;
        public bool autoColor;
        public Vector4 horizonFogColor;
        public float horizonFogDensityAuto;
        public float horizonFogStrengthAuto;
        public float horizonFogStartHeightAuto;
        public float horizonFogEndHeightAuto;
        public HorizonBlendMode horizonBlendModeAuto;
        public float horizonFogDensityManual;
        public float horizonFogStrengthManual;
        public float horizonFogStartHeightManual;
        public float horizonFogEndHeightManual;
        public HorizonBlendMode horizonBlendModeManual;

        // Flat Shading settings
        public bool isFlatShading;
        //public bool isFlatShadingTerrain;
        //public bool isFlatShadingObjects;
        public bool isFlatShadingClouds;
        public float minSlopeFlatShading;
        public float maxSlopeFlatShading;
        public float flatShadingStrengthTerrain;
        public float flatShadingStrengthObject;

        public FXParams(bool newparameters)
        {
            if (!newparameters) throw new Exception("Undefined FXParams");

            selectionIndexVFX = 0;

            // Time of day settings
            dayNightControl = 1; // 0 = off, 1 = Manual, 2 = Auto = true;
            dayNightControlState = true;
            elevation = 45f;
            azimuth = 0f;
            globalSpeed = 10f;
            lightmappingControls = false;
            dayNightUpdateIntervalInSeconds = 0.1f;

            // Crepuscular Rays settings
            hasGodRays = true;
            godRaySamples = 128;
            godRayDensity = 1f;
            godRayWeight = 0.07f;
            godRayDecay = 1f;
            godRayExposure = 1f;

            // Clouds settings
            hasClouds = true;
            cloudShadows = false;
            cloudsSeed = 55555;
            cloudsDensity = 0.5f;
            cloudSize = 250;
            cloudsAltitude = 5000;
            emitProbability = 0;
            cloudColor = new Vector4(0.8f, 0.8f, 0.8f, 1f);
            meshMode = 0;
            //Modelpath = "";
            //MeshName = "";
            cloudNormalOffset = 1;
            cloudMeshTexturePath = "";

            // Atmospheric Scattering settings
            hasAtmosphericScattering = true;
            volumetricLightIntensity = 12f;
            atmosphericFogIntensity = 1.7f;
            atmosphericFogDensity = 0.0001f;
            atmosphericFogDistance = 1200f;

            // Volumetric Fog settings
            hasVolumetricFog = true;
            fogStrength = 5f;
            fogWindSpeed = 20f;
            fogNearClip = 0.01f;
            fogFarClip = 500f;
            volumetricFogDensity = 0.1f;
            fogNoiseAmount = 0.5f;
            fogNoiseScale = 0.2f;
            volumetricFogColor = new Vector4(0.8f, 0.9f, 1f, 1f);

            // Wind settings
            hasWind = true;
            windTime = 0.85f;
            windSpeed = 1.25f;
            windBending = 1.33f;

            // Weather settings
            hasSnow = true;
            snowStartHeight = 5000f;
            snowThickness = 0f; // 1f;
            snowDamping = 0f; // 0.5f;
            heightFalloff = 1000;
            BGTerrainHasSnow = false;

            // Real-time Reflections settings
            hasReflection = false;

            // Water settings
            waterQuality = WaterQuality.High;
            waterBaseColor = new Vector4(0.153709f, 0.2681301f, 0.2985074f, 0.8196079f);
            waterReflectionColor = new Vector4(0.5794164f, 0.6849238f, 0.761194f, 0.4313726f);
            reflectionQuality = 0.5f;
            reflectionDistance = 500f;
            edgeBlend = true;
            specularLighting = true;
            planarReflection = false;
            GerstnerWaves = true;

            // Post Processing settings
            isPostProcessing = 0; // 0 is true, 1 is false

            // Horizon Fog settings
            hasHorizonFog = false;
            autoColor = true;
            horizonFogColor = new Vector4(0f, 0.45f, 0.25f, 1f);
            horizonFogDensityAuto = 25000f;
            horizonFogStrengthAuto = 1f;
            horizonFogStartHeightAuto = 0f;
            horizonBlendModeAuto = HorizonBlendMode.Overlay;
            horizonBlendModeManual = HorizonBlendMode.Overlay;
            horizonFogEndHeightAuto = 53000f;
            horizonFogDensityManual = 5000f;
            horizonFogStrengthManual = 4f;
            horizonFogStartHeightManual = 0f;
            horizonFogEndHeightManual = 90000f;

            // Flat Shading settings
            isFlatShading = false;
            //isFlatShadingTerrain = false;
            //isFlatShadingObjects = false;
            isFlatShadingClouds = false;
            minSlopeFlatShading = 0f;
            maxSlopeFlatShading = 90f;
            flatShadingStrengthTerrain = 0f;
            flatShadingStrengthObject = 0.2f;
        }
    }

    [XmlType("FXNode")]
    public class FXNode : TFXModules
    {
        public FXNode() : base()
        {
            //type = typeof(FXNode).FullName;
            Data.moduleType = ModuleType.Processor;
            Data.name = "FX";
            isRemovable = false;
            isRunnable = false;
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.FXParameters;
        }

        public override List<string> GetResourcePaths()
        {
            //List<string> result = new List<string>();
            //if (!string.IsNullOrEmpty(fxParams.Modelpath) && File.Exists(Path.GetFullPath(fxParams.Modelpath))) result.Add(fxParams.Modelpath);
            //else result.Add(null);
            //return result;

            return null;
        }

        public void SetFXParams
        (
            FXParams newfxParams,
            bool Apply_FXGraph_selectionIndexVFX,
            bool Apply_FXGraph_TimeOfDay,
            bool Apply_FXGraph_CrepuscularRay,
            bool Apply_FXGraph_CloudsSettings,
            bool Apply_FXGraph_AtmosphericScatteringSettings,
            bool Apply_FXGraph_VolumetricFogSettings,
            bool Apply_FXGraph_WindSettings,
            bool Apply_FXGraph_WeatherSettings,
            bool Apply_FXGraph_ReflectionSettings,
            bool Apply_FXGraph_WaterSettings,
            bool Apply_FXGraph_PostProcessSettings,
            bool Apply_FXGraph_HorizonFogSettings,
            bool Apply_FXGraph_FlatShadingSettings
        )
        {
            if (Apply_FXGraph_selectionIndexVFX)
            {
                fxParams.selectionIndexVFX = newfxParams.selectionIndexVFX;
            }

            if (Apply_FXGraph_TimeOfDay)
            {
                fxParams.dayNightControl = newfxParams.dayNightControl; // 0 = off, 1 = Manual, 2 = Auto = true;
                fxParams.elevation = newfxParams.elevation;
                fxParams.azimuth = newfxParams.azimuth;
                fxParams.globalSpeed = newfxParams.globalSpeed;
            }

            if (Apply_FXGraph_CrepuscularRay)
            {
                fxParams.hasGodRays = newfxParams.hasGodRays;
                fxParams.godRaySamples = newfxParams.godRaySamples;
                fxParams.godRayDensity = newfxParams.godRayDensity;
                fxParams.godRayWeight = newfxParams.godRayWeight;
                fxParams.godRayDecay = newfxParams.godRayDecay;
                fxParams.godRayExposure = newfxParams.godRayExposure;
            }

            if (Apply_FXGraph_CloudsSettings)
            {
                fxParams.hasClouds = newfxParams.hasClouds;
                fxParams.cloudShadows = newfxParams.cloudShadows;
                fxParams.cloudsSeed = newfxParams.cloudsSeed;
                fxParams.cloudsDensity = newfxParams.cloudsDensity;
                fxParams.cloudSize = newfxParams.cloudSize;
                fxParams.cloudsAltitude = newfxParams.cloudsAltitude;
                fxParams.emitProbability = newfxParams.emitProbability;
                fxParams.cloudColor = newfxParams.cloudColor;
                fxParams.meshMode = newfxParams.meshMode;
                //fxParams.MeshName = newfxParams.MeshName;
                //fxParams.Modelpath = newfxParams.Modelpath;
            }

            if (Apply_FXGraph_AtmosphericScatteringSettings)
            {
                fxParams.hasAtmosphericScattering = newfxParams.hasAtmosphericScattering;
                fxParams.volumetricLightIntensity = newfxParams.volumetricLightIntensity;
                fxParams.atmosphericFogIntensity = newfxParams.atmosphericFogIntensity;
                fxParams.atmosphericFogDensity = newfxParams.atmosphericFogDensity;
                fxParams.atmosphericFogDistance = newfxParams.atmosphericFogDistance;
            }

            if (Apply_FXGraph_VolumetricFogSettings)
            {
                fxParams.hasVolumetricFog = newfxParams.hasVolumetricFog;
                fxParams.fogStrength = newfxParams.fogStrength;
                fxParams.fogWindSpeed = newfxParams.fogWindSpeed;
                fxParams.fogNearClip = newfxParams.fogNearClip;
                fxParams.fogFarClip = newfxParams.fogFarClip;
                fxParams.volumetricFogDensity = newfxParams.volumetricFogDensity;
                fxParams.fogNoiseAmount = newfxParams.fogNoiseAmount;
                fxParams.fogNoiseScale = newfxParams.fogNoiseScale;
                fxParams.volumetricFogColor = newfxParams.volumetricFogColor;
            }

            if (Apply_FXGraph_WindSettings)
            {
                fxParams.hasWind = newfxParams.hasWind;
                fxParams.windTime = newfxParams.windTime;
                fxParams.windSpeed = newfxParams.windSpeed;
                fxParams.windBending = newfxParams.windBending;
            }

            if (Apply_FXGraph_WeatherSettings)
            {
                // Weather settings
                fxParams.hasSnow = newfxParams.hasSnow;
                fxParams.snowStartHeight = newfxParams.snowStartHeight;
                fxParams.snowThickness = newfxParams.snowThickness; // 1f;
                fxParams.snowDamping = newfxParams.snowDamping; // 0.5f;
                fxParams.heightFalloff = newfxParams.heightFalloff;
                fxParams.BGTerrainHasSnow = newfxParams.BGTerrainHasSnow;
            }

            if (Apply_FXGraph_ReflectionSettings)
            {
                // Real-time Reflections settings
                fxParams.hasReflection = newfxParams.hasReflection;
            }

            if (Apply_FXGraph_WaterSettings)
            {
                // Water settings
                fxParams.waterQuality = newfxParams.waterQuality;
                fxParams.waterBaseColor = newfxParams.waterBaseColor;
                fxParams.waterReflectionColor = newfxParams.waterReflectionColor;
                fxParams.reflectionQuality = newfxParams.reflectionQuality;
                fxParams.reflectionDistance = newfxParams.reflectionDistance;
                fxParams.edgeBlend = newfxParams.edgeBlend;
                fxParams.specularLighting = newfxParams.specularLighting;
                fxParams.planarReflection = newfxParams.planarReflection;
                fxParams.GerstnerWaves = newfxParams.GerstnerWaves;
            }

            if (Apply_FXGraph_PostProcessSettings)
            {
                // Post Processing settings
                fxParams.isPostProcessing = newfxParams.isPostProcessing; // 0 is true, 1 is false
            }

            if (Apply_FXGraph_HorizonFogSettings)
            {
                // Horizon Fog settings
                fxParams.hasHorizonFog = newfxParams.hasHorizonFog;
                fxParams.autoColor = newfxParams.autoColor;
                fxParams.horizonFogColor = newfxParams.horizonFogColor;
                fxParams.horizonFogDensityAuto = newfxParams.horizonFogDensityAuto;
                fxParams.horizonFogStrengthAuto = newfxParams.horizonFogStrengthAuto;
                fxParams.horizonFogStartHeightAuto = newfxParams.horizonFogStartHeightAuto;
                fxParams.horizonFogEndHeightAuto = newfxParams.horizonFogEndHeightAuto;
                fxParams.horizonFogDensityManual = newfxParams.horizonFogDensityManual;
                fxParams.horizonFogStrengthManual = newfxParams.horizonFogStrengthManual;
                fxParams.horizonFogStartHeightManual = newfxParams.horizonFogStartHeightManual;
                fxParams.horizonFogEndHeightManual = newfxParams.horizonFogEndHeightManual;
            }

            if (Apply_FXGraph_FlatShadingSettings)
            {
                // Flat Shading settings
                fxParams.isFlatShading = newfxParams.isFlatShading;
                fxParams.isFlatShadingClouds = newfxParams.isFlatShadingClouds;
                fxParams.minSlopeFlatShading = newfxParams.minSlopeFlatShading;
                fxParams.maxSlopeFlatShading = newfxParams.maxSlopeFlatShading;
                fxParams.flatShadingStrengthTerrain = newfxParams.flatShadingStrengthTerrain;
                fxParams.flatShadingStrengthObject = newfxParams.flatShadingStrengthObject;
            }
        }

        public object this[string propertyName]
        {
            get
            {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(FXNode);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                return myPropInfo.GetValue(this, null);
            }
            set
            {
                Type myType = typeof(FXNode);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }
    }

    public class TFXGraph : TGraph
    {
        public TFXGraph() : base(ConnectionDataType.Global, "FX Graph")
        {
        }

        // -----------------------------------------------------------------------------------------------------------------------------------

        public void InitGraph(TTerraWorldGraph terraWorldGraph)
        {
            worldGraph = terraWorldGraph;
            _title = "FX";

            if (nodes.Count > 0) return;

            FXNode node = new FXNode();
            node.Init(this);
            nodes.Add(node);
        }

        public FXNode GetEntryNode()
        {
            return (nodes[0] as FXNode);
        }
    }
}
#endif
#endif

