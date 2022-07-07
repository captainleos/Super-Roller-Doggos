using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class HorizonFog : VolumetricObjectBase
    {
        public Light sun;
        public Material material = null;
        public float coneHeight = 2f;
        public float coneAngle = 30f;
        public float startOffset = 0f;
        public float endOffset = 0f;
        public bool autoColor = true;

        private float previousConeHeight = 0f;
        private float previousConeAngle = 0f;
        private float previousStartOffset = 0f;
        private static float epsilon = Mathf.Epsilon;

#if UNITY_EDITOR
        static public void CreateVolume()
        {
            GameObject newObject = new GameObject("Horizon Fog");
            if (UnityEditor.SceneView.currentDrawingSceneView) UnityEditor.SceneView.currentDrawingSceneView.MoveToView(newObject.transform);
            HorizonFog horizonFog = (HorizonFog)newObject.AddComponent<HorizonFog>();
            horizonFog.enabled = false;
            horizonFog.enabled = true;
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            material = volumetricMaterial;
        }

        public override bool HasChanged()
        {
            if (coneHeight != previousConeHeight ||
                coneAngle != previousConeAngle ||
                startOffset != previousStartOffset ||
                base.HasChanged())
            {
                return true;
            }

            return false;
        }

        protected override void SetChangedValues()
        {
            if (coneHeight < 0f) coneHeight = 0f;
            if (coneAngle >= 89f) coneAngle = 89f;
            previousConeHeight = coneHeight;
            previousConeAngle = coneAngle;
            previousStartOffset = startOffset;
            base.SetChangedValues();
        }

        public override void UpdateVolume()
        {
            if (sun == null)
            {
                foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                {
                    if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                        if (go.GetComponent<Light>() != null && go.GetComponent<Light>().type == LightType.Directional)
                        {
                            sun = go.GetComponent<Light>();
                            break;
                        }
                }
            }

            if (sun != null)
            {
                //coneHeight = (Camera.main.farClipPlane / 2);
                coneHeight = 100000f;
                transform.position = new Vector3(transform.position.x, -coneHeight + endOffset - (coneHeight / 2), transform.position.z);

                if (autoColor)
                {
                    float sunPosNormalized = Mathf.Clamp(Vector3.Dot(sun.transform.forward, Vector3.down), epsilon, 0.55f);

                    if (RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_GroundColor"))
                        volumeColor = RenderSettings.skybox.GetColor("_GroundColor");

                    if (sun.transform.eulerAngles.x == 0 || sun.transform.eulerAngles.x >= 350 || sunPosNormalized > 0)
                    {
                        volumeColor *= sunPosNormalized + (strength * sunPosNormalized) + 0.25f;
                        //volumeColor += Color.Lerp(sun.color, volumeColor, Mathf.Clamp01(sunPosNormalized));
                        volumeColor += Color.Lerp(RenderSettings.ambientGroundColor * 3, volumeColor, Mathf.Clamp01(sunPosNormalized));
                    }
                    else
                        volumeColor *= 0.5f;
                }
                else
                    volumeColor.a = strength;
            }

            float angleRads = coneAngle * Mathf.Deg2Rad;
            float bottomRadius = Mathf.Tan(angleRads) * coneHeight;
            float bottomRadiusHalf = bottomRadius * 0.5f;

            Vector3 halfBoxSize = new Vector3(bottomRadius, coneHeight, bottomRadius);

            if (meshInstance)
            {
                ScaleMesh(meshInstance, halfBoxSize, -Vector3.up * coneHeight * 0.5f);

                // Set bounding volume so modified vertices don't get culled
                Bounds bounds = new Bounds();
                bounds.SetMinMax(-halfBoxSize, halfBoxSize);
                meshInstance.bounds = bounds;
            }

            if (materialInstance)
            {
                materialInstance.SetVector("_ConeData", new Vector4(bottomRadiusHalf, coneHeight, startOffset, Mathf.Cos(angleRads)));
                materialInstance.SetVector("_TextureData", new Vector4(-textureMovement.x, -textureMovement.y, -textureMovement.z, (1f / textureScale)));
                materialInstance.SetFloat("_Visibility", visibility);
                materialInstance.SetColor("_Color", volumeColor);
                materialInstance.SetTexture("_MainTex", texture);
            }
        }
    }
}

