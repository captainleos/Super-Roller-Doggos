using UnityEngine;
using UnityEngine.Rendering;

namespace TerraUnity.Runtime
{
    public struct TimeOfDayParams
    {
        public bool enableTimeOfDay;
        public Gradient nightDayColor;
        public float sunPosNormalized;
        public Color heightRayleighColor;
        public Color volumetricFogColor;
        public Color cloudsColor;
        public Color worldRayleighColorDay;
        public Color worldRayleighColorNight;
        public float worldRayleighColorIntensity;
        public Color cloudsEmissionColor;
        public float maxIntensity;
        public float minIntensity;
        public float minPoint;
        public float maxAmbient;
        public float minAmbient;
        public float minAmbientPoint;
        public float dayAtmosphereThickness;
        public float nightAtmosphereThickness;
        public Vector3 dayRotateSpeed;
        public Vector3 nightRotateSpeed;
        public float dayMaxExposureStrength;
        public float dayMinExposureStrength;
        public float nightMaxExposureStrength;
        public Color daySkyTint;
        public Color dayGroundColor;
        public Color nightSkyTint;
        public Color nightGroundColor;
        //public bool gradualEnvironmentLightingUpdate;
        public float starsRendererNormilizedSunAngle;
    }

    [ExecuteAlways]
    public class TimeOfDay : MonoBehaviour
    {
        //Resources
        public Material skyMaterial;
        public GameObject stars;
        public ParticleSystemRenderer starsRenderer;

        public GameObject player;
        public GameObject sun;
        public Light sunLight;

        private CloudsManager cloudsManager { get => TTerraWorldManager.CloudsManagerScript; }
        private GlobalTimeManager globalTimeManager { get => TTerraWorldManager.GlobalTimeManagerScript; }

#if UNITY_STANDALONE_WIN
        private AtmosphericScattering atmosphericScattering { get => TTerraWorldManager.AtmosphericScatteringManagerScript; }
        private VolumetricFog volumetricFog { get => TTerraWorldManager.VolumetricFogManagerScript; }
#endif

        [Range(0f, 60f)] public float updateIntervalInSeconds = 0.1f;
        [HideInInspector] public bool isDay;
        [HideInInspector] public bool isNight;
        [HideInInspector] public bool enableTimeOfDay;
        public Gradient nightDayColor;
        public float sunPosNormalized;
        public Color heightRayleighColor = new Color(0.3843137f, 0.5117647f, 0.6392157f, 1);
        public Color volumetricFogColor = Color.white;
        public Color cloudsColor = Color.white;
        public Color worldRayleighColorDay = Color.white;
        public Color worldRayleighColorNight = Color.white;
        public float worldRayleighColorIntensity = 10f;
        public Color cloudsEmissionColor = new Color(0.8f, 0.8f, 0.8f, 1);
        public float maxIntensity = 1.5f;
        public float minIntensity = 1.33f;
        public float minPoint = -0.2f;
        public float maxAmbient = 1.75f;
        public float minAmbient = 1.5f;
        public float minAmbientPoint = -0.2f;
        public float dayAtmosphereThickness = 1.5f;
        [Range(0f, 1f)] public float nightAtmosphereThickness = 0.5f;
        public Vector3 dayRotateSpeed = new Vector3(1, 0, 0);
        public Vector3 nightRotateSpeed = new Vector3(3, 0, 0);
        [Range(0f, 80f)] public float dayMaxExposureStrength = 1.5f;
        [Range(0f, 80f)] public float dayMinExposureStrength = 0.5f;
        [Range(0f, 80f)] public float nightMaxExposureStrength = 12f;
        public Color daySkyTint = new Color(0.25f, 0.25f, 0.25f, 1f);
        public Color dayGroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        public Color nightSkyTint = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Color nightGroundColor = new Color(0.1f, 0.125f, 0.15f, 1f);
        //public bool gradualEnvironmentLightingUpdate = true;
        [Range(0f, 80f)] public float starsRendererNormilizedSunAngle = 0.25f;

        private bool runOnceToggle = false;
        private float skySpeed = 100f;
        private float exposure;
        private float hue, saturation, value;
        private float minCloudBrightness = 0.0f;
        private float elapsedTime = float.MinValue;
        private float skippedFrames = 1;
        private Vector3 daySpeed = Vector3.zero;
        private Vector3 nightSpeed = Vector3.zero;

        public TimeOfDayParams GetParams()
        {
            TimeOfDayParams result = new TimeOfDayParams();

            result.enableTimeOfDay = enableTimeOfDay;
            result.nightDayColor = nightDayColor;
            result.sunPosNormalized = sunPosNormalized;
            result.heightRayleighColor = heightRayleighColor;
            result.volumetricFogColor = volumetricFogColor;
            result.cloudsColor = cloudsColor;
            result.worldRayleighColorDay = worldRayleighColorDay;
            result.worldRayleighColorNight = worldRayleighColorNight;
            result.worldRayleighColorIntensity = worldRayleighColorIntensity;
            result.cloudsEmissionColor = cloudsEmissionColor;
            result.maxIntensity = maxIntensity;
            result.minIntensity = minIntensity;
            result.minPoint = minPoint;
            result.maxAmbient = maxAmbient;
            result.minAmbient = minAmbient;
            result.minAmbientPoint = minAmbientPoint;
            result.dayAtmosphereThickness = dayAtmosphereThickness;
            result.nightAtmosphereThickness = nightAtmosphereThickness;
            result.dayRotateSpeed = dayRotateSpeed;
            result.nightRotateSpeed = nightRotateSpeed;
            result.dayMaxExposureStrength = dayMaxExposureStrength;
            result.dayMinExposureStrength = dayMinExposureStrength;
            result.nightMaxExposureStrength = nightMaxExposureStrength;
            result.daySkyTint = daySkyTint;
            result.dayGroundColor = dayGroundColor;
            result.nightSkyTint = nightSkyTint;
            result.nightGroundColor = nightGroundColor;
            //result.gradualEnvironmentLightingUpdate = gradualEnvironmentLightingUpdate;
            result.starsRendererNormilizedSunAngle = starsRendererNormilizedSunAngle;

            return result;
        }

        public void SetParams(TimeOfDayParams timeOfDayParams)
        {
            enableTimeOfDay = timeOfDayParams.enableTimeOfDay;
            nightDayColor = timeOfDayParams.nightDayColor;
            sunPosNormalized = timeOfDayParams.sunPosNormalized;
            heightRayleighColor = timeOfDayParams.heightRayleighColor;
            volumetricFogColor = timeOfDayParams.volumetricFogColor;
            cloudsColor = timeOfDayParams.cloudsColor;
            worldRayleighColorDay = timeOfDayParams.worldRayleighColorDay;
            worldRayleighColorNight = timeOfDayParams.worldRayleighColorNight;
            worldRayleighColorIntensity = timeOfDayParams.worldRayleighColorIntensity;
            cloudsEmissionColor = timeOfDayParams.cloudsEmissionColor;
            maxIntensity = timeOfDayParams.maxIntensity;
            minIntensity = timeOfDayParams.minIntensity;
            minPoint = timeOfDayParams.minPoint;
            maxAmbient = timeOfDayParams.maxAmbient;
            minAmbient = timeOfDayParams.minAmbient;
            minAmbientPoint = timeOfDayParams.minAmbientPoint;
            dayAtmosphereThickness = timeOfDayParams.dayAtmosphereThickness;
            nightAtmosphereThickness = timeOfDayParams.nightAtmosphereThickness;
            dayRotateSpeed = timeOfDayParams.dayRotateSpeed;
            nightRotateSpeed = timeOfDayParams.nightRotateSpeed;
            dayMaxExposureStrength = timeOfDayParams.dayMaxExposureStrength;
            dayMinExposureStrength = timeOfDayParams.dayMinExposureStrength;
            nightMaxExposureStrength = timeOfDayParams.nightMaxExposureStrength;
            daySkyTint = timeOfDayParams.daySkyTint;
            dayGroundColor = timeOfDayParams.dayGroundColor;
            nightSkyTint = timeOfDayParams.nightSkyTint;
            nightGroundColor = timeOfDayParams.nightGroundColor;
            //gradualEnvironmentLightingUpdate = timeOfDayParams.gradualEnvironmentLightingUpdate;
            starsRendererNormilizedSunAngle = timeOfDayParams.starsRendererNormilizedSunAngle;
        }

        public void OnValidate()
        {
            if (!Application.isPlaying)
            {
                SetEnvironmentReflections();
                UpdateAtmosphere(true);
            }
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += OnEditorUpdate;
#endif

            if (!Application.isPlaying)
                SetEnvironmentReflections();
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= OnEditorUpdate;
#endif
        }

#if UNITY_EDITOR
        protected virtual void OnEditorUpdate()
        {
            Update();
        }
#endif

        private void Awake()
        {
            SetEnvironmentReflections();
        }

        private void Start()
        {
            ResetParams();
            ResetSun();
        }

        void Update()
        {
            UpdateAtmosphere();
        }

        // Needed for proper World's Abmient Lighting and Reflections syncing with Day/Night Cycle in builds
        private void SetEnvironmentReflections()
        {
            if (enableTimeOfDay)
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        }

        private void ResetParams()
        {
            if (Application.isPlaying)
            {
                runOnceToggle = false;
                elapsedTime = float.MinValue;
                skippedFrames = 1;
            }
        }

        private void ResetSun()
        {
            if (sun != null && globalTimeManager != null)
                sun.transform.rotation = Quaternion.Euler(new Vector3(globalTimeManager.Elevation, globalTimeManager.Azimuth, 0));
        }

        private bool IsApproved()
        {
            if
            (
                player == null ||
                sun == null ||
                sunLight == null ||
                stars == null ||
                starsRenderer == null ||
                skyMaterial == null
            )
                return false;

            return true;
        }

        public void UpdateAtmosphere(bool forced = false)
        {
            if (TTerraWorldManager.SceneSettingsGO1 == null) return;
            if (!IsApproved()) return;
            if (globalTimeManager != null) enableTimeOfDay = globalTimeManager.EnableTimeOfDay;
            if (!forced && runOnceToggle && !enableTimeOfDay) return;

            float tRange = 1 - minPoint;
            float sunPos = Mathf.Clamp01((Vector3.Dot(sun.transform.forward, Vector3.down) - minPoint) / tRange);
            float intensity = ((maxIntensity - minIntensity) * sunPos) + minIntensity;
            sunLight.intensity = intensity;

            tRange = 1 - minAmbientPoint;
            sunPosNormalized = Mathf.Clamp01((Vector3.Dot(sun.transform.forward, Vector3.down) - minAmbientPoint) / tRange);
            float intensityAmbient = ((maxAmbient - minAmbient) * sunPosNormalized) + minAmbient;
            RenderSettings.ambientIntensity = intensityAmbient;

            if (nightDayColor != null)
                sunLight.color = nightDayColor.Evaluate(sunPosNormalized);

            Color.RGBToHSV(cloudsEmissionColor, out hue, out saturation, out value);

            RenderSettings.ambientLight = sunLight.color;

            if (runOnceToggle)
                skippedFrames = Mathf.Clamp(updateIntervalInSeconds * 120f, 1, 100000); // Based on 120 frames per second

            if (sunPosNormalized > 0) // We are in Day
            {
                isDay = true;
                isNight = false;
                if (enableTimeOfDay) daySpeed = dayRotateSpeed * Time.deltaTime * skySpeed * skippedFrames;
            }
            else // We are at Night
            {
                isDay = false;
                isNight = true;
                if (enableTimeOfDay) nightSpeed = nightRotateSpeed * Time.deltaTime * skySpeed * skippedFrames;
            }

            if (!forced && runOnceToggle && enableTimeOfDay)
            {
                if (Time.realtimeSinceStartup <= elapsedTime + updateIntervalInSeconds) return;
                elapsedTime = Time.realtimeSinceStartup;
            }

#if UNITY_STANDALONE_WIN
            if (atmosphericScattering != null && atmosphericScattering.enabled)
            {
                Color.RGBToHSV(heightRayleighColor, out hue, out saturation, out value);
                atmosphericScattering.heightRayleighColor = Color.HSVToRGB(hue, saturation, value * sunPosNormalized);

                // Draw proper halo around the sun based on day time
                atmosphericScattering.worldMieColorIntensity = sunPosNormalized / 2f;
                atmosphericScattering.worldMiePhaseAnisotropy = Mathf.Clamp01(sunPosNormalized - 0.1f);

                atmosphericScattering.worldRayleighColorIntensity = worldRayleighColorIntensity * (1 - Mathf.Clamp(sunPosNormalized, 0.001f, 1f));
            }

            if (volumetricFog != null && volumetricFog.enabled)
            {
                Color.RGBToHSV(volumetricFogColor, out hue, out saturation, out value);
                volumetricFog.m_AmbientLightColor = Color.HSVToRGB(hue, saturation, value * sunPosNormalized);
            }
#endif

            if (sunPosNormalized > 0) // We are in Day
            {
                if (enableTimeOfDay)
                    sun.transform.Rotate(daySpeed);
                else
                    sun.transform.rotation = Quaternion.Euler(new Vector3(globalTimeManager.Elevation, globalTimeManager.Azimuth, 0));

                float intensityDay = ((dayAtmosphereThickness - nightAtmosphereThickness) * sunPosNormalized) + nightAtmosphereThickness;
                skyMaterial.SetFloat("_AtmosphereThickness", intensityDay);

                exposure = ((dayMaxExposureStrength - dayMinExposureStrength) * sunPosNormalized) + dayMinExposureStrength;
                skyMaterial.SetFloat("_Exposure", exposure);
                skyMaterial.SetColor("_SkyTint", Color.Lerp(daySkyTint, nightSkyTint, 1 - sunPosNormalized));
                skyMaterial.SetColor("_GroundColor", Color.Lerp(dayGroundColor, nightGroundColor, 1 - sunPosNormalized));

                if (cloudsManager != null)
                {
                    cloudsManager.cloudColor = cloudsColor;

                    if (cloudsManager.cloudsMaterial != null)
                        cloudsManager.cloudsMaterial.SetColor("_EmissionColor", Color.HSVToRGB(hue, saturation, value * Mathf.Clamp(sunPosNormalized, 0.5f, 1f)));
                }

#if UNITY_STANDALONE_WIN
                if (atmosphericScattering != null && atmosphericScattering.enabled)
                {
                    atmosphericScattering.worldRayleighColor = worldRayleighColorDay;
                    atmosphericScattering.UpdateStaticUniforms();
                }
#endif
            }
            else // We are at Night
            {
                if (enableTimeOfDay)
                    sun.transform.Rotate(nightSpeed);
                else
                    sun.transform.rotation = Quaternion.Euler(new Vector3(globalTimeManager.Elevation, globalTimeManager.Azimuth, 0));

                float dotNight = Mathf.Clamp01((Vector3.Dot(sun.transform.forward, Vector3.up) - minAmbientPoint) / tRange);
                float intensityNight = nightAtmosphereThickness;

                skyMaterial.SetFloat("_AtmosphereThickness", intensityNight);
                exposure = ((nightMaxExposureStrength - dayMinExposureStrength) * dotNight) + dayMinExposureStrength;
                skyMaterial.SetFloat("_Exposure", exposure);
                skyMaterial.SetColor("_SkyTint", Color.Lerp(nightSkyTint, daySkyTint, sunPosNormalized));
                skyMaterial.SetColor("_GroundColor", Color.Lerp(nightGroundColor, dayGroundColor, sunPosNormalized));

                if (cloudsManager != null)
                {
                    Color.RGBToHSV(cloudsColor, out hue, out saturation, out value);
                    cloudsManager.cloudColor = Color.HSVToRGB(hue, saturation, Mathf.Clamp(value * sunPosNormalized, minCloudBrightness, 1f));
                }

                cloudsManager.cloudsMaterial.SetColor("_EmissionColor", Color.HSVToRGB(hue, saturation, value * sunPosNormalized));

#if UNITY_STANDALONE_WIN
                if (atmosphericScattering != null && atmosphericScattering.enabled)
                {
                    atmosphericScattering.worldRayleighColor = Color.Lerp(worldRayleighColorDay, worldRayleighColorNight, Mathf.Clamp(dotNight, 0.5f, 1f));
                    atmosphericScattering.UpdateStaticUniforms();
                }
#endif
            }

            stars.transform.rotation = sun.transform.rotation;

            if (starsRenderer != null)
            {
                if (sunPosNormalized > starsRendererNormilizedSunAngle) // && sunPosNormalized < starsRendererEndAngle)
                    starsRenderer.enabled = false;
                else
                    starsRenderer.enabled = true;
            }

            // Update scene's Ambient Lighting and Reflections
            DynamicGI.UpdateEnvironment();

            runOnceToggle = true;
        }

        private void ResetSkyMaterial()
        {
            if (sun != null) sun.GetComponent<Light>().intensity = 1;

            if (skyMaterial != null)
            {
                skyMaterial?.SetFloat("_AtmosphereThickness", 1.2f);
                skyMaterial?.SetFloat("_Exposure", 1.56f);
            }
        }

        void OnDestroy()
        {
            ResetSkyMaterial();
        }
    }
}

