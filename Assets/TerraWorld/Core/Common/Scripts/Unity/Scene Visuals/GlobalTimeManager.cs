using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class GlobalTimeManager : MonoBehaviour
    {
        private TimeOfDay timeOfDay { get => TTerraWorldManager.TimeOfDayManagerScript; }
        private GameObject sun { get => TTerraWorldManager.TimeOfDayManagerScript.sun; }
        private CloudsManager cloudsManager { get => TTerraWorldManager.CloudsManagerScript; }

        //private bool _enableTimeOfDay;
        public bool EnableTimeOfDay = false;
        //{
        //    get { return _enableTimeOfDay; }
        //    set
        //    {
        //        _enableTimeOfDay = value;
        //        //OnValidate();
        //    }
        //}

        [Range(0f, 359f)] //private float _elevation;
        public float Elevation = 45f;
        //{
        //    get { return _elevation; }
        //    set
        //    {
        //        _elevation = value;
        //        //OnValidate();
        //    }
        //}

        [Range(0f, 359f)] //private float _azimuth;
        public float Azimuth = 60f;
        //{
        //    get { return _azimuth; }
        //    set
        //    {
        //        _azimuth = value;
        //        //OnValidate();
        //    }
        //}

        [Range(0.001f, 3000f)] //private float _globalSpeedX = 1f;
        public float GlobalSpeedX = 3f;
        //{
        //    get { return _globalSpeedX; }
        //    set
        //    {
        //        _globalSpeedX = value;
        //        //OnValidate();
        //    }
        //}

        private const float defaultDayRotateSpeed = 0.00005f;
        private const float defaultNightRotateSpeed = 0.0001f;
        private const float defaultCloudsSpeed = 4;
        private float blendingSpeed = 1;
        [HideInInspector] public Quaternion sunRotation;
        [HideInInspector] public float sunIntensity;

        private void OnDestroy()
        {
            //SetDefaults();
        }

        private void Update()
        {
            UpdateDayNightCycle();
        }

        private void UpdateDayNightCycle()
        {
            if (timeOfDay == null || sun == null) return;

            timeOfDay.enableTimeOfDay = EnableTimeOfDay;

            if (EnableTimeOfDay)
            {
                if (GlobalSpeedX == 0)
                    SetDefaultsSmooth();
                else
                {
                    timeOfDay.dayRotateSpeed.x = defaultDayRotateSpeed * GlobalSpeedX;
                    timeOfDay.nightRotateSpeed.x = defaultNightRotateSpeed * GlobalSpeedX;
                }
            }
            //else
            //sun.transform.rotation = Quaternion.Euler(new Vector3(Elevation, Azimuth, 0));

            cloudsManager.windSpeed = defaultCloudsSpeed * GlobalSpeedX;
        }

        private void SetDefaultsSmooth()
        {
            if (timeOfDay == null) return;
            timeOfDay.dayRotateSpeed.x = Mathf.Lerp(timeOfDay.dayRotateSpeed.x, defaultDayRotateSpeed, Time.deltaTime * blendingSpeed);
            timeOfDay.nightRotateSpeed.x = Mathf.Lerp(timeOfDay.nightRotateSpeed.x, defaultNightRotateSpeed, Time.deltaTime * blendingSpeed);

            cloudsManager.windSpeed = Mathf.Lerp(cloudsManager.windSpeed, defaultCloudsSpeed, Time.deltaTime * blendingSpeed);

            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Lerp(sun.transform.rotation, sunRotation, Time.deltaTime * blendingSpeed);
                sun.GetComponent<Light>().intensity = Mathf.Lerp(sun.GetComponent<Light>().intensity, sunIntensity, Time.deltaTime * blendingSpeed);
            }
        }

        public void SetDefaults()
        {
            timeOfDay.dayRotateSpeed.x = defaultDayRotateSpeed;
            timeOfDay.nightRotateSpeed.x = defaultNightRotateSpeed;

            cloudsManager.windSpeed = defaultCloudsSpeed;

            if (sun != null)
            {
                sun.transform.rotation = sunRotation;
                sun.GetComponent<Light>().intensity = sunIntensity;
            }
        }

        public void OnValidate()
        {
            if (timeOfDay != null)
                timeOfDay.OnValidate();
        }
    }
}

