#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;

namespace TerraUnity.Edittime
{
    public class SetStandardMaterialParams
    {
        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent, // Physically plausible transparency mode, implemented as alpha pre-multiply
            Additive,
            Subtractive,
            Modulate
        }

        public static void SwitchMaterialBlendingType (Material material, BlendMode blendMode)
        {
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));

            material.SetFloat("_Mode", (float)blendMode);
            MaterialChanged(material, blendMode);
        }

        private static void MaterialChanged(Material material, BlendMode blendMode)
        {
            SetupMaterialWithBlendMode(material, blendMode);
            SetMaterialKeywords(material);
        }

        private static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Additive:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Subtractive:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.ReverseSubtract);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Modulate:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.EnableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }

        private static void SetMaterialKeywords(Material material)
        {
            // Z write doesn't work with distortion/fading
            bool hasZWrite = (material.GetInt("_ZWrite") != 0);

            // Lit shader?
            bool useLighting = (material.GetFloat("_LightingEnabled") > 0.0f);

            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            bool useDistortion = !hasZWrite && (material.GetFloat("_DistortionEnabled") > 0.0f);
            SetKeyword(material, "_NORMALMAP", (useLighting || useDistortion) && material.GetTexture("_BumpMap"));
            SetKeyword(material, "_METALLICGLOSSMAP", useLighting && (material.GetTexture("_MetallicGlossMap") != null));

            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            SetKeyword(material, "_EMISSION", material.GetFloat("_EmissionEnabled") > 0.0f);

            // Set the define for flipbook blending
            bool useFlipbookBlending = (material.GetFloat("_FlipbookMode") > 0.0f);
            SetKeyword(material, "_REQUIRE_UV2", useFlipbookBlending);

            // Clamp fade distances
            bool useSoftParticles = (material.GetFloat("_SoftParticlesEnabled") > 0.0f);
            bool useCameraFading = (material.GetFloat("_CameraFadingEnabled") > 0.0f);
            float softParticlesNearFadeDistance = material.GetFloat("_SoftParticlesNearFadeDistance");
            float softParticlesFarFadeDistance = material.GetFloat("_SoftParticlesFarFadeDistance");
            float cameraNearFadeDistance = material.GetFloat("_CameraNearFadeDistance");
            float cameraFarFadeDistance = material.GetFloat("_CameraFarFadeDistance");

            if (softParticlesNearFadeDistance < 0.0f)
            {
                softParticlesNearFadeDistance = 0.0f;
                material.SetFloat("_SoftParticlesNearFadeDistance", 0.0f);
            }
            if (softParticlesFarFadeDistance < 0.0f)
            {
                softParticlesFarFadeDistance = 0.0f;
                material.SetFloat("_SoftParticlesFarFadeDistance", 0.0f);
            }
            if (cameraNearFadeDistance < 0.0f)
            {
                cameraNearFadeDistance = 0.0f;
                material.SetFloat("_CameraNearFadeDistance", 0.0f);
            }
            if (cameraFarFadeDistance < 0.0f)
            {
                cameraFarFadeDistance = 0.0f;
                material.SetFloat("_CameraFarFadeDistance", 0.0f);
            }

            // Set the define for fading
            bool useFading = (useSoftParticles || useCameraFading) && !hasZWrite;
            SetKeyword(material, "_FADING_ON", useFading);
            if (useSoftParticles)
                material.SetVector("_SoftParticleFadeParams", new Vector4(softParticlesNearFadeDistance, 1.0f / (softParticlesFarFadeDistance - softParticlesNearFadeDistance), 0.0f, 0.0f));
            else
                material.SetVector("_SoftParticleFadeParams", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            if (useCameraFading)
                material.SetVector("_CameraFadeParams", new Vector4(cameraNearFadeDistance, 1.0f / (cameraFarFadeDistance - cameraNearFadeDistance), 0.0f, 0.0f));
            else
                material.SetVector("_CameraFadeParams", new Vector4(0.0f, Mathf.Infinity, 0.0f, 0.0f));

            // Set the define for distortion + grabpass
            SetKeyword(material, "EFFECT_BUMP", useDistortion);
            material.SetShaderPassEnabled("Always", useDistortion);
            if (useDistortion)
                material.SetFloat("_DistortionStrengthScaled", material.GetFloat("_DistortionStrength") * 0.1f);   // more friendly number scale than 1 unit per size of the screen
        }

        private static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
}
#endif
#endif

