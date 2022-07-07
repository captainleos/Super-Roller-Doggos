using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class WaterManager : MonoBehaviour
    {
        private TimeOfDay timeOfDay { get => TTerraWorldManager.TimeOfDayManagerScript; }

        //Resources
        public Material waterMaterial;

        public Color waterBaseColor = new Color(0.153709f, 0.2681301f, 0.2985074f, 0.8196079f);
        public Color waterReflectionColor = new Color(0.5794164f, 0.6849238f, 0.761194f, 0.4313726f);
        private float hue, saturation, value;
        private Color finalColor;

        void Update()
        {
            if (waterMaterial == null || timeOfDay == null || timeOfDay.sun == null) return;

            Color.RGBToHSV(waterBaseColor, out hue, out saturation, out value);
            finalColor = Color.HSVToRGB(hue, saturation, value * timeOfDay.sunPosNormalized);
            finalColor.a = waterBaseColor.a;
            waterMaterial.SetColor("_BaseColor", finalColor);

            Color.RGBToHSV(waterReflectionColor, out hue, out saturation, out value);
            finalColor = Color.HSVToRGB(hue, saturation, value * timeOfDay.sunPosNormalized);
            finalColor.a = waterReflectionColor.a;
            waterMaterial.SetColor("_ReflectionColor", finalColor);

            if (timeOfDay.sunLight != null) waterMaterial.SetColor("_SpecularColor", timeOfDay.sunLight.color);
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += OnEditorUpdate;
#endif
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
    }
}

