#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using TerraUnity.Runtime;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
    public class TerrainRenderingManager
    {
        //private static RenderingParams _renderingParams = new RenderingParams(true);
        private static bool SplatmapResolutionBestFit { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.splatmapResolutionBestFit; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.splatmapResolutionBestFit = value; } }
        private static int SplatmapSmoothness { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.splatmapSmoothness; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.splatmapSmoothness = value; } }
        private static int SplatmapResolution { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.splatmapResolution; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.splatmapResolution = value; } }
        //public static int TerrainPixelError { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.terrainPixelError; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.terrainPixelError = value; } }
        private static bool BGMountains { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGMountains; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGMountains = value; } }
        private static int BGTerrainScaleMultiplier { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainScaleMultiplier; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainScaleMultiplier = value; } }
        private static int BGTerrainHeightmapResolution { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainHeightmapResolution; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainHeightmapResolution = value; } }
        private static int BGTerrainSatelliteImageResolution { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainSatelliteImageResolution; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainSatelliteImageResolution = value; } }
        private static int BGTerrainPixelError { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainPixelError; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainPixelError = value; } }
        private static float BGTerrainOffset { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainOffset; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainOffset = value; } }
        private static string WorldName { get => TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.worldName; set { TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.worldName = value; } }

        //public static RenderingParams RenderingParams { get => GetRenderingParams(); }

        public static Material TerrainMaterial { get => GetTerrainMaterial(); }

        public static Material TerrainMaterialBG { get => GetBGTerrainMaterial(); }

        public static Terrain WorldTerrain { get => GetMainTerrain(); }

        private static Texture2D colormapTexture;
        public static Texture2D ColormapTexture { get => GetColormapTexture(); set => SetColormapTexture(value); }
        private static Texture2D waterMaskTexture;
        public static Texture2D WaterMaskTexture { get => GetWaterMaskTexture(); set => SetWaterMaskTexture(value); }

        private static Texture2D snowTexture;
        public static Texture2D SnowTexture { get => GetSnowTexture(); }

        //private static Texture2D noiseTexture;
        //public static Texture2D NoiseTexture { get => GetNoiseTexture(); }

        private static int terrainLayersCount;
        public static int TerrainLayersCount { get => GetTerrainLayersCount(); }

        private static bool isModernRendering { get => IsModernRendering(); set { SetModernRendering(value); } }
        private static bool isTessellated { get => IsTerrainTessellation(); set { SetTerrainTessellation(value); } }
        private static bool isHeightmapBlending { get => IsTerrainHeightmapBlending(); set { SetTerrainHeightmapBlending(value); } }
        private static bool isColormapBlending { get => IsTerrainColormapBlending(); set { SetTerrainColormapBlending(value); } }
        private static bool isProceduralSnow { get => IsTerrainProceduralSnow(); set { SetTerrainProceduralSnow(value); } }
        private static bool isProceduralSnowBG { get => IsTerrainProceduralSnowBG(); set { SetTerrainProceduralSnowBG(value); } }
        private static bool isFlatShading { get => IsTerrainFlatShading(); set { SetTerrainFlatShading(value); } }
        private static bool isProceduralPuddles { get => IsTerrainProceduralPuddles(); set { SetTerrainProceduralPuddles(value); } }

        private static Color LightingColor { get => GetColor("_LightingColor"); set { SetColor("_LightingColor", value); } }
        private static Color LightingColorBG { get => GetColorBG("_LightingColor"); set { SetColorBG("_LightingColor", value); } }

        private static Color SnowColor { get => GetColor("_SnowColor"); set { SetColor("_SnowColor", value); } }
        private static float SnowStartHeight { get => GetFloat("_SnowStartHeight"); set { SetFloat("_SnowStartHeight", value); SetFloatBG("_SnowStartHeight", value); } }
        private static float HeightFalloff { get => GetFloat("_HeightFalloff"); set { SetFloat("_HeightFalloff", value); SetFloatBG("_HeightFalloff", value); } }
        public static float SnowThickness { get => GetFloat("_SnowThickness"); set { SetFloat("_SnowThickness", value); SetFloatBG("_SnowThickness", value); } }
        public static float SnowDamping { get => GetFloat("_SnowDamping"); set { SetFloat("_SnowDamping", value); SetFloatBG("_SnowDamping", value); } }
        private static float SnowTile { get => GetFloat("_SnowTile"); set { SetFloat("_SnowTile", value); } }
        private static float SnowAmount { get => GetFloat("_SnowAmount"); set { SetFloat("_SnowAmount", value); } }
        private static float SnowAngle { get => GetFloat("_SnowAngle"); set { SetFloat("_SnowAngle", value); } }
        private static float SnowNormalInfluence { get => GetFloat("_NormalInfluence"); set { SetFloat("_NormalInfluence", value); } }
        private static float SnowPower { get => GetFloat("_SnowPower"); set { SetFloat("_SnowPower", value); } }
        private static float SnowSmoothness { get => GetFloat("_SnowSmoothness"); set { SetFloat("_SnowSmoothness", value); } }

        private static Color PuddleColor { get => GetColor("_PuddleColor"); set { SetColor("_PuddleColor", value); } }
        private static float PuddleRefraction { get => GetFloat("_Refraction"); set { SetFloat("_Refraction", value); } }
        private static float PuddleMetallic { get => GetFloat("_PuddleMetallic"); set { SetFloat("_PuddleMetallic", value); } }
        private static float PuddleSmoothness { get => GetFloat("_PuddleSmoothness"); set { SetFloat("_PuddleSmoothness", value); } }
        private static float PuddleSlope { get => GetFloat("_Slope"); set { SetFloat("_Slope", value); } }
        private static float PuddleSlopeMin { get => GetFloat("_SlopeMin"); set { SetFloat("_SlopeMin", value); } }
        private static float PuddleNoiseTiling { get => GetFloat("_NoiseTiling"); set { SetFloat("_NoiseTiling", value); } }
        private static float PuddlewaterHeight { get => GetFloat("_WaterHeight"); set { SetFloat("_WaterHeight", value); } }
        //public static float PuddleNoiseInfluence { get => GetFloat("_NoiseIntensity"); set { SetFloat("_NoiseIntensity", value); } }

        private static float HeightmapBlending { get => GetFloat("_HeightmapBlending"); set { SetFloat("_HeightmapBlending", value); } }
        private static float BlendingDistance { get => GetFloat("_BlendingDistance"); set { SetFloat("_BlendingDistance", value); } }

        private static Color GetColorBG(string variable)
        {
            if (TerrainMaterialBG != null && TerrainMaterialBG.HasProperty(variable))
                return TerrainMaterialBG.GetColor(variable);
            else
                return new Color(1, 1, 1);
        }

        private static void SetColorBG(string variable, Color value)
        {
            if (!isModernRendering) return;

            if (TerrainMaterialBG != null && TerrainMaterialBG.HasProperty(variable))
                TerrainMaterialBG.SetColor(variable, value);
        }

        private static float TessellationQuality
        {
            get
            {
                if (!isTessellated) return 0;
                return GetFloat("_EdgeLength");
            }
            set
            {
                if (isTessellated)
                    SetFloat("_EdgeLength", value);
            }
        }

        private static float EdgeSmoothness
        {
            get
            {
                if (!isTessellated) return 0;
                return GetFloat("_Phong");
            }
            set
            {
                if (isTessellated)
                    SetFloat("_Phong", value);
            }
        }

        private static float GetFloat(string variable)
        {
            if (isModernRendering && TerrainMaterial.HasProperty(variable))
                return TerrainMaterial.GetFloat(variable);
            else
                return 0;
        }

        private static void SetFloat(string variable, float value)
        {
            if (isModernRendering)
            {
                if (TerrainMaterial.HasProperty(variable))
                    TerrainMaterial.SetFloat(variable, value);
            }
        }

        private static void SetFloatBG(string variable, float value)
        {
            if (isModernRendering)
            {
                if (TerrainMaterialBG != null && TerrainMaterialBG.HasProperty(variable))
                    TerrainMaterialBG.SetFloat(variable, value);
            }
        }

        private static Color GetColor(string variable)
        {
            if (isModernRendering && TerrainMaterial.HasProperty(variable))
                return TerrainMaterial.GetColor(variable);
            else
                return new Color(1, 1, 1);
        }

        private static void SetColor(string variable, Color value)
        {
            if (isModernRendering && TerrainMaterial.HasProperty(variable))
                TerrainMaterial.SetColor(variable, value);
        }

        private static float GetDisplacement(int index)
        {
            if (!isTessellated) return 0;
            return GetFloat("_Displacement" + (index + 1).ToString());
        }

        private static void SetDisplacement(int index, float value)
        {
            if (isTessellated)
                SetFloat("_Displacement" + (index + 1).ToString(), value);
        }

        public static void SetDisplacement(ref RenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.displacement1 = value;
            if (index == 1) renderingParams.displacement2 = value;
            if (index == 2) renderingParams.displacement3 = value;
            if (index == 3) renderingParams.displacement4 = value;
            if (index == 4) renderingParams.displacement5 = value;
            if (index == 5) renderingParams.displacement6 = value;
            if (index == 6) renderingParams.displacement7 = value;
            if (index == 7) renderingParams.displacement8 = value;
        }

        public static float GetDisplacement(ref RenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.displacement1;
            if (index == 1) return renderingParams.displacement2;
            if (index == 2) return renderingParams.displacement3;
            if (index == 3) return renderingParams.displacement4;
            if (index == 4) return renderingParams.displacement5;
            if (index == 5) return renderingParams.displacement6;
            if (index == 6) return renderingParams.displacement7;
            if (index == 7) return renderingParams.displacement8;

            return 0;
        }

        private static float GetHeightOffset(int index)
        {
            if (!isTessellated) return 0;
            return GetFloat("_HeightShift" + (index + 1).ToString());
        }

        private static void SetHeightOffset(int index, float value)
        {
            if (isTessellated)
                SetFloat("_HeightShift" + (index + 1).ToString(), value);
        }

        public static void SetHeightOffset(ref RenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.heightOffset1 = value;
            if (index == 1) renderingParams.heightOffset2 = value;
            if (index == 2) renderingParams.heightOffset3 = value;
            if (index == 3) renderingParams.heightOffset4 = value;
            if (index == 4) renderingParams.heightOffset5 = value;
            if (index == 5) renderingParams.heightOffset6 = value;
            if (index == 6) renderingParams.heightOffset7 = value;
            if (index == 7) renderingParams.heightOffset8 = value;
        }

        public static float GetHeightOffset(ref RenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.heightOffset1;
            if (index == 1) return renderingParams.heightOffset2;
            if (index == 2) return renderingParams.heightOffset3;
            if (index == 3) return renderingParams.heightOffset4;
            if (index == 4) return renderingParams.heightOffset5;
            if (index == 5) return renderingParams.heightOffset6;
            if (index == 6) return renderingParams.heightOffset7;
            if (index == 7) return renderingParams.heightOffset8;

            return 0;
        }

        private static float GetTileRemover(int index)
        {
            return GetFloat("_TilingRemover" + (index + 1).ToString());
        }

        private static void SetTileRemover(int index, float value)
        {

            SetFloat("_TilingRemover" + (index + 1).ToString(), value);
        }

        public static void SetTileRemover(ref RenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.tilingRemover1 = value;
            if (index == 1) renderingParams.tilingRemover2 = value;
            if (index == 2) renderingParams.tilingRemover3 = value;
            if (index == 3) renderingParams.tilingRemover4 = value;
            if (index == 4) renderingParams.tilingRemover5 = value;
            if (index == 5) renderingParams.tilingRemover6 = value;
            if (index == 6) renderingParams.tilingRemover7 = value;
            if (index == 7) renderingParams.tilingRemover8 = value;
        }

        public static float GetTileRemover(ref RenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.tilingRemover1;
            if (index == 1) return renderingParams.tilingRemover2;
            if (index == 2) return renderingParams.tilingRemover3;
            if (index == 3) return renderingParams.tilingRemover4;
            if (index == 4) return renderingParams.tilingRemover5;
            if (index == 5) return renderingParams.tilingRemover6;
            if (index == 6) return renderingParams.tilingRemover7;
            if (index == 7) return renderingParams.tilingRemover8;

            return 0;
        }

        private static float GetNoiseTiling(int index)
        {
            return GetFloat("_NoiseTiling" + (index + 1).ToString());
        }

        private static void SetNoseTiling(int index, float value)
        {
            SetFloat("_NoiseTiling" + (index + 1).ToString(), value);
        }

        public static void SetNoseTiling(ref RenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.noiseTiling1 = value;
            if (index == 1) renderingParams.noiseTiling2 = value;
            if (index == 2) renderingParams.noiseTiling3 = value;
            if (index == 3) renderingParams.noiseTiling4 = value;
            if (index == 4) renderingParams.noiseTiling5 = value;
            if (index == 5) renderingParams.noiseTiling6 = value;
            if (index == 6) renderingParams.noiseTiling7 = value;
            if (index == 7) renderingParams.noiseTiling8 = value;
        }

        public static float GetNoiseTiling(ref RenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.noiseTiling1;
            if (index == 1) return renderingParams.noiseTiling2;
            if (index == 2) return renderingParams.noiseTiling3;
            if (index == 3) return renderingParams.noiseTiling4;
            if (index == 4) return renderingParams.noiseTiling5;
            if (index == 5) return renderingParams.noiseTiling6;
            if (index == 6) return renderingParams.noiseTiling7;
            if (index == 7) return renderingParams.noiseTiling8;

            return 0;
        }

        private static Color GetLayerColor(int index)
        {
            return GetColor("_LayerColor" + (index + 1).ToString());
        }

        private static void SetLayerColor(int index, Color value)
        {
            SetColor("_LayerColor" + (index + 1).ToString(), value);
        }

        public static void SetLayerColor(ref RenderingParams renderingParams, int index, Color value)
        {
            if (index == 0)
            {
                renderingParams.layerColor1R = value.r;
                renderingParams.layerColor1G = value.g;
                renderingParams.layerColor1B = value.b;
            }
            if (index == 1)
            {
                renderingParams.layerColor2R = value.r;
                renderingParams.layerColor2G = value.g;
                renderingParams.layerColor3B = value.b;
            }
            if (index == 2)
            {
                renderingParams.layerColor3R = value.r;
                renderingParams.layerColor3G = value.g;
                renderingParams.layerColor3B = value.b;
            }
            if (index == 3)
            {
                renderingParams.layerColor4R = value.r;
                renderingParams.layerColor4G = value.g;
                renderingParams.layerColor4B = value.b;
            }
            if (index == 4)
            {
                renderingParams.layerColor5R = value.r;
                renderingParams.layerColor5G = value.g;
                renderingParams.layerColor5B = value.b;
            }
            if (index == 5)
            {
                renderingParams.layerColor6R = value.r;
                renderingParams.layerColor6G = value.g;
                renderingParams.layerColor6B = value.b;
            }
            if (index == 6)
            {
                renderingParams.layerColor7R = value.r;
                renderingParams.layerColor7G = value.g;
                renderingParams.layerColor7B = value.b;
            }
            if (index == 7)
            {
                renderingParams.layerColor8R = value.r;
                renderingParams.layerColor8G = value.g;
                renderingParams.layerColor8B = value.b;
            }
        }

        public static Color GetLayerColor(ref RenderingParams renderingParams, int index)
        {
            Color result = new Color(0, 0, 0);
            if (index == 0)
            {
                result.r = renderingParams.layerColor1R;
                result.g = renderingParams.layerColor1G;
                result.b = renderingParams.layerColor1B;
            }
            if (index == 1)
            {
                result.r = renderingParams.layerColor2R;
                result.g = renderingParams.layerColor2G;
                result.b = renderingParams.layerColor2B;
            }
            if (index == 2)
            {
                result.r = renderingParams.layerColor3R;
                result.g = renderingParams.layerColor3G;
                result.b = renderingParams.layerColor3B;
            }
            if (index == 3)
            {
                result.r = renderingParams.layerColor4R;
                result.g = renderingParams.layerColor4G;
                result.b = renderingParams.layerColor4B;
            }
            if (index == 4)
            {
                result.r = renderingParams.layerColor5R;
                result.g = renderingParams.layerColor5G;
                result.b = renderingParams.layerColor5B;
            }
            if (index == 5)
            {
                result.r = renderingParams.layerColor6R;
                result.g = renderingParams.layerColor6G;
                result.b = renderingParams.layerColor6B;
            }
            if (index == 6)
            {
                result.r = renderingParams.layerColor7R;
                result.g = renderingParams.layerColor7G;
                result.b = renderingParams.layerColor7B;
            }
            if (index == 7)
            {
                result.r = renderingParams.layerColor8R;
                result.g = renderingParams.layerColor8G;
                result.b = renderingParams.layerColor8B;
            }

            return result;
        }

        private static float GetLayerAO(int index)
        {
            return GetFloat("_LayerAO" + (index + 1).ToString());
        }

        private static void SetLayerAO(int index, float value)
        {
            SetFloat("_LayerAO" + (index + 1).ToString(), value);
        }

        public static void SetLayerAO(ref RenderingParams renderingParams, int index, float value)
        {
            if (index == 0) renderingParams.layerAO1 = value;
            if (index == 1) renderingParams.layerAO2 = value;
            if (index == 2) renderingParams.layerAO3 = value;
            if (index == 3) renderingParams.layerAO4 = value;
            if (index == 4) renderingParams.layerAO5 = value;
            if (index == 5) renderingParams.layerAO6 = value;
            if (index == 6) renderingParams.layerAO7 = value;
            if (index == 7) renderingParams.layerAO8 = value;
        }

        public static float GetLayerAO(ref RenderingParams renderingParams, int index)
        {
            if (index == 0) return renderingParams.layerAO1;
            if (index == 1) return renderingParams.layerAO2;
            if (index == 2) return renderingParams.layerAO3;
            if (index == 3) return renderingParams.layerAO4;
            if (index == 4) return renderingParams.layerAO5;
            if (index == 5) return renderingParams.layerAO6;
            if (index == 6) return renderingParams.layerAO7;
            if (index == 7) return renderingParams.layerAO8;

            return 0;
        }


        public static void Reset()
        {
           ApplyRenderingParams(new RenderingParams(true));
        }

        public static RenderingParams GetRenderingParams()
        {
            RenderingParams renderingParams = TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams;
            if (TerrainMaterial != null)
            {
                renderingParams.modernRendering = isModernRendering;

                if (isModernRendering)
                {
                    renderingParams.worldName = WorldName;

                    renderingParams.tessellation = isTessellated;
                    renderingParams.heightmapBlending = isHeightmapBlending;
                    renderingParams.colormapBlending = isColormapBlending;
                    renderingParams.proceduralSnow = isProceduralSnow;
                    renderingParams.proceduralPuddles = isProceduralPuddles;
                    renderingParams.isFlatShading = isFlatShading;

                    renderingParams.surfaceTintColorMAIN = TUtils.UnityColorToVector4(LightingColor);
                    renderingParams.surfaceTintColorBG = TUtils.UnityColorToVector4(LightingColorBG);

                    renderingParams.tessellationQuality = TessellationQuality;
                    renderingParams.edgeSmoothness = EdgeSmoothness;

                    renderingParams.displacement1 = GetDisplacement(0);
                    renderingParams.displacement2 = GetDisplacement(1);
                    renderingParams.displacement3 = GetDisplacement(2);
                    renderingParams.displacement4 = GetDisplacement(3);
                    renderingParams.displacement5 = GetDisplacement(4);
                    renderingParams.displacement6 = GetDisplacement(5);
                    renderingParams.displacement7 = GetDisplacement(6);
                    renderingParams.displacement8 = GetDisplacement(7);

                    renderingParams.heightOffset1 = GetHeightOffset(0);
                    renderingParams.heightOffset2 = GetHeightOffset(1);
                    renderingParams.heightOffset3 = GetHeightOffset(2);
                    renderingParams.heightOffset4 = GetHeightOffset(3);
                    renderingParams.heightOffset5 = GetHeightOffset(4);
                    renderingParams.heightOffset6 = GetHeightOffset(5);
                    renderingParams.heightOffset7 = GetHeightOffset(6);
                    renderingParams.heightOffset8 = GetHeightOffset(7);

                    renderingParams.heightBlending = HeightmapBlending;

                    renderingParams.tilingRemover1 = GetTileRemover(0);
                    renderingParams.tilingRemover2 = GetTileRemover(1);
                    renderingParams.tilingRemover3 = GetTileRemover(2);
                    renderingParams.tilingRemover4 = GetTileRemover(3);
                    renderingParams.tilingRemover5 = GetTileRemover(4);
                    renderingParams.tilingRemover6 = GetTileRemover(5);
                    renderingParams.tilingRemover7 = GetTileRemover(6);
                    renderingParams.tilingRemover8 = GetTileRemover(7);

                    renderingParams.noiseTiling1 = GetNoiseTiling(0);
                    renderingParams.noiseTiling2 = GetNoiseTiling(1);
                    renderingParams.noiseTiling3 = GetNoiseTiling(2);
                    renderingParams.noiseTiling4 = GetNoiseTiling(3);
                    renderingParams.noiseTiling5 = GetNoiseTiling(4);
                    renderingParams.noiseTiling6 = GetNoiseTiling(5);
                    renderingParams.noiseTiling7 = GetNoiseTiling(6);
                    renderingParams.noiseTiling8 = GetNoiseTiling(7);

                    renderingParams.colormapBlendingDistance = BlendingDistance;
                    renderingParams.colormapBlendingRange = 0.0f;

                    renderingParams.snowColorR = SnowColor.r;
                    renderingParams.snowColorG = SnowColor.g;
                    renderingParams.snowColorB = SnowColor.b;
                    renderingParams.snowTiling = SnowTile;
                    renderingParams.snowAmount = SnowAmount;
                    renderingParams.snowAngles = SnowAngle;
                    renderingParams.snowNormalInfluence = SnowNormalInfluence;
                    renderingParams.snowPower = SnowPower;
                    renderingParams.snowSmoothness = SnowSmoothness;
                    renderingParams.snowStartHeight = SnowStartHeight;
                    renderingParams.heightFalloff = HeightFalloff;

                    renderingParams.puddleColorR = PuddleColor.r;
                    renderingParams.puddleColorG = PuddleColor.g;
                    renderingParams.puddleColorB = PuddleColor.b;
                    renderingParams.puddleRefraction = PuddleRefraction;
                    renderingParams.puddleMetallic = PuddleMetallic;
                    renderingParams.puddleSmoothness = PuddleSmoothness;
                    renderingParams.puddlewaterHeight = PuddlewaterHeight;
                    renderingParams.puddleSlope = PuddleSlope;
                    renderingParams.puddleMinSlope = PuddleSlopeMin;
                    renderingParams.puddleNoiseTiling = PuddleNoiseTiling;
                    //renderingParams.puddleNoiseInfluence = PuddleNoiseInfluence;
                    //renderingParams.puddleReflections = PuddleReflections;

                    renderingParams.layerColor1R = GetLayerColor(0).r;
                    renderingParams.layerColor1G = GetLayerColor(0).g;
                    renderingParams.layerColor1B = GetLayerColor(0).b;
                    renderingParams.layerColor2R = GetLayerColor(1).r;
                    renderingParams.layerColor2G = GetLayerColor(1).g;
                    renderingParams.layerColor2B = GetLayerColor(1).b;
                    renderingParams.layerColor3R = GetLayerColor(2).r;
                    renderingParams.layerColor3G = GetLayerColor(2).g;
                    renderingParams.layerColor3B = GetLayerColor(2).b;
                    renderingParams.layerColor4R = GetLayerColor(3).r;
                    renderingParams.layerColor4G = GetLayerColor(3).g;
                    renderingParams.layerColor4B = GetLayerColor(3).b;
                    renderingParams.layerColor5R = GetLayerColor(4).r;
                    renderingParams.layerColor5G = GetLayerColor(4).g;
                    renderingParams.layerColor5B = GetLayerColor(4).b;
                    renderingParams.layerColor6R = GetLayerColor(5).r;
                    renderingParams.layerColor6G = GetLayerColor(5).g;
                    renderingParams.layerColor6B = GetLayerColor(5).b;
                    renderingParams.layerColor7R = GetLayerColor(6).r;
                    renderingParams.layerColor7G = GetLayerColor(6).g;
                    renderingParams.layerColor7B = GetLayerColor(6).b;
                    renderingParams.layerColor8R = GetLayerColor(7).r;
                    renderingParams.layerColor8G = GetLayerColor(7).g;
                    renderingParams.layerColor8B = GetLayerColor(7).b;
                    renderingParams.layerAO1 = GetLayerAO(0);
                    renderingParams.layerAO2 = GetLayerAO(1);
                    renderingParams.layerAO3 = GetLayerAO(2);
                    renderingParams.layerAO4 = GetLayerAO(3);
                    renderingParams.layerAO5 = GetLayerAO(4);
                    renderingParams.layerAO6 = GetLayerAO(5);
                    renderingParams.layerAO7 = GetLayerAO(6);
                    renderingParams.layerAO8 = GetLayerAO(7);
                }

                renderingParams.splatmapResolutionBestFit = SplatmapResolutionBestFit;
                renderingParams.splatmapSmoothness = SplatmapSmoothness;
                renderingParams.splatmapResolution = SplatmapResolution;
                //renderingParams.terrainPixelError = TerrainPixelError;
                renderingParams.BGMountains = BGMountains;
                renderingParams.BGTerrainScaleMultiplier = BGTerrainScaleMultiplier;
                renderingParams.BGTerrainHeightmapResolution = BGTerrainHeightmapResolution;
                renderingParams.BGTerrainSatelliteImageResolution = BGTerrainSatelliteImageResolution;
                renderingParams.BGTerrainPixelError = BGTerrainPixelError;
                renderingParams.BGTerrainOffset = BGTerrainOffset;
            }

            TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams = renderingParams;

            return renderingParams;

        }

        // private static void SetRenderingParams(RenderingParams renderingParams)
        // {
        //     TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams = renderingParams;
        //     ApplyRenderingParams();
        // }

        // public static void ApplyRenderingParams()
        // {
        //     if (TerrainMaterial == null)
        //         return;
        //
        //     RenderingParams _renderingParams = TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams;
        //
        //     isModernRendering = _renderingParams.modernRendering;
        //
        //     if (_renderingParams.modernRendering)
        //     {
        //         WorldName = _renderingParams.worldName;
        //
        //         isTessellated = _renderingParams.tessellation;
        //         isHeightmapBlending = _renderingParams.heightmapBlending;
        //         isColormapBlending = _renderingParams.colormapBlending;
        //         isProceduralSnow = _renderingParams.proceduralSnow;
        //         isProceduralPuddles = _renderingParams.proceduralPuddles;
        //         isFlatShading = _renderingParams.isFlatShading;
        //
        //         LightingColor = TUtils.Vector4ToUnityColor(_renderingParams.surfaceTintColorMAIN);
        //         if (TTerraWorldManager.BackgroundTerrainGO != null)
        //             LightingColorBG = TUtils.Vector4ToUnityColor(_renderingParams.surfaceTintColorBG);
        //
        //         TessellationQuality = _renderingParams.tessellationQuality;
        //         EdgeSmoothness = _renderingParams.edgeSmoothness;
        //
        //         SetDisplacement(0, _renderingParams.displacement1);
        //         SetDisplacement(1, _renderingParams.displacement2);
        //         SetDisplacement(2, _renderingParams.displacement3);
        //         SetDisplacement(3, _renderingParams.displacement4);
        //         SetDisplacement(4, _renderingParams.displacement5);
        //         SetDisplacement(5, _renderingParams.displacement6);
        //         SetDisplacement(6, _renderingParams.displacement7);
        //         SetDisplacement(7, _renderingParams.displacement8);
        //
        //         SetHeightOffset(0, _renderingParams.heightOffset1);
        //         SetHeightOffset(1, _renderingParams.heightOffset2);
        //         SetHeightOffset(2, _renderingParams.heightOffset3);
        //         SetHeightOffset(3, _renderingParams.heightOffset4);
        //         SetHeightOffset(4, _renderingParams.heightOffset5);
        //         SetHeightOffset(5, _renderingParams.heightOffset6);
        //         SetHeightOffset(6, _renderingParams.heightOffset7);
        //         SetHeightOffset(7, _renderingParams.heightOffset8);
        //
        //         HeightmapBlending = _renderingParams.heightBlending;
        //
        //         SetTileRemover(0, _renderingParams.tilingRemover1);
        //         SetTileRemover(1, _renderingParams.tilingRemover2);
        //         SetTileRemover(2, _renderingParams.tilingRemover3);
        //         SetTileRemover(3, _renderingParams.tilingRemover4);
        //         SetTileRemover(4, _renderingParams.tilingRemover5);
        //         SetTileRemover(5, _renderingParams.tilingRemover6);
        //         SetTileRemover(6, _renderingParams.tilingRemover7);
        //         SetTileRemover(7, _renderingParams.tilingRemover8);
        //
        //         SetNoseTiling(0, _renderingParams.noiseTiling1);
        //         SetNoseTiling(1, _renderingParams.noiseTiling2);
        //         SetNoseTiling(2, _renderingParams.noiseTiling3);
        //         SetNoseTiling(3, _renderingParams.noiseTiling4);
        //         SetNoseTiling(4, _renderingParams.noiseTiling5);
        //         SetNoseTiling(5, _renderingParams.noiseTiling6);
        //         SetNoseTiling(6, _renderingParams.noiseTiling7);
        //         SetNoseTiling(7, _renderingParams.noiseTiling8);
        //
        //         BlendingDistance = _renderingParams.colormapBlendingDistance;
        //
        //         SnowColor = new Color(_renderingParams.snowColorR, _renderingParams.snowColorG, _renderingParams.snowColorB);
        //
        //         SnowTile = _renderingParams.snowTiling;
        //         SnowAmount = _renderingParams.snowAmount;
        //         SnowAngle = _renderingParams.snowAngles;
        //         SnowNormalInfluence = _renderingParams.snowNormalInfluence;
        //         SnowPower = _renderingParams.snowPower;
        //         SnowSmoothness = _renderingParams.snowSmoothness;
        //         SnowStartHeight = _renderingParams.snowStartHeight;
        //         HeightFalloff = _renderingParams.heightFalloff;
        //
        //         PuddleColor = new Color(_renderingParams.puddleColorR, _renderingParams.puddleColorG, _renderingParams.puddleColorB);
        //
        //         PuddleRefraction = _renderingParams.puddleRefraction;
        //         PuddleMetallic = _renderingParams.puddleMetallic;
        //         PuddleSmoothness = _renderingParams.puddleSmoothness;
        //         PuddlewaterHeight = _renderingParams.puddlewaterHeight;
        //         PuddleSlope = _renderingParams.puddleSlope;
        //         PuddleSlopeMin = _renderingParams.puddleMinSlope;
        //         PuddleNoiseTiling = _renderingParams.puddleNoiseTiling;
        //         //PuddleNoiseInfluence = _renderingParams.puddleNoiseInfluence;
        //         //PuddleReflections = _renderingParams.puddleReflections;
        //
        //         SetLayerColor(0, new Color(_renderingParams.layerColor1R, _renderingParams.layerColor1G, _renderingParams.layerColor1B));
        //         SetLayerColor(1, new Color(_renderingParams.layerColor2R, _renderingParams.layerColor2G, _renderingParams.layerColor2B));
        //         SetLayerColor(2, new Color(_renderingParams.layerColor3R, _renderingParams.layerColor3G, _renderingParams.layerColor3B));
        //         SetLayerColor(3, new Color(_renderingParams.layerColor4R, _renderingParams.layerColor4G, _renderingParams.layerColor4B));
        //         SetLayerColor(4, new Color(_renderingParams.layerColor5R, _renderingParams.layerColor5G, _renderingParams.layerColor5B));
        //         SetLayerColor(5, new Color(_renderingParams.layerColor6R, _renderingParams.layerColor6G, _renderingParams.layerColor6B));
        //         SetLayerColor(6, new Color(_renderingParams.layerColor7R, _renderingParams.layerColor7G, _renderingParams.layerColor7B));
        //         SetLayerColor(7, new Color(_renderingParams.layerColor8R, _renderingParams.layerColor8G, _renderingParams.layerColor8B));
        //
        //         SetLayerAO(0, _renderingParams.layerAO1);
        //         SetLayerAO(1, _renderingParams.layerAO2);
        //         SetLayerAO(2, _renderingParams.layerAO3);
        //         SetLayerAO(3, _renderingParams.layerAO4);
        //         SetLayerAO(4, _renderingParams.layerAO5);
        //         SetLayerAO(5, _renderingParams.layerAO6);
        //         SetLayerAO(6, _renderingParams.layerAO7);
        //         SetLayerAO(7, _renderingParams.layerAO8);
        //     }
        //
        //     SplatmapResolutionBestFit = _renderingParams.splatmapResolutionBestFit;
        //     SplatmapSmoothness = _renderingParams.splatmapSmoothness;
        //     SplatmapResolution = _renderingParams.splatmapResolution;
        //
        //     //TerrainPixelError = _renderingParams.terrainPixelError;
        //
        //     BGMountains = _renderingParams.BGMountains;
        //     BGTerrainScaleMultiplier = _renderingParams.BGTerrainScaleMultiplier;
        //     BGTerrainHeightmapResolution = _renderingParams.BGTerrainHeightmapResolution;
        //     BGTerrainSatelliteImageResolution = _renderingParams.BGTerrainSatelliteImageResolution;
        //     BGTerrainPixelError = _renderingParams.BGTerrainPixelError;
        //     BGTerrainOffset = _renderingParams.BGTerrainOffset;
        // }

        public static void ApplyRenderingParams(RenderingParams _renderingParams)
        {

            if (TerrainMaterial != null)
                isModernRendering = _renderingParams.modernRendering;
            {

                if (_renderingParams.modernRendering)
                {
                    WorldName = _renderingParams.worldName;

                    isTessellated = _renderingParams.tessellation;
                    isHeightmapBlending = _renderingParams.heightmapBlending;
                    isColormapBlending = _renderingParams.colormapBlending;
                    isProceduralSnow = _renderingParams.proceduralSnow;
                    isProceduralPuddles = _renderingParams.proceduralPuddles;
                    isFlatShading = _renderingParams.isFlatShading;

                    LightingColor = TUtils.Vector4ToUnityColor(_renderingParams.surfaceTintColorMAIN);
                    if (TTerraWorldManager.BackgroundTerrainGO != null)
                        LightingColorBG = TUtils.Vector4ToUnityColor(_renderingParams.surfaceTintColorBG);

                    TessellationQuality = _renderingParams.tessellationQuality;
                    EdgeSmoothness = _renderingParams.edgeSmoothness;

                    SetDisplacement(0, _renderingParams.displacement1);
                    SetDisplacement(1, _renderingParams.displacement2);
                    SetDisplacement(2, _renderingParams.displacement3);
                    SetDisplacement(3, _renderingParams.displacement4);
                    SetDisplacement(4, _renderingParams.displacement5);
                    SetDisplacement(5, _renderingParams.displacement6);
                    SetDisplacement(6, _renderingParams.displacement7);
                    SetDisplacement(7, _renderingParams.displacement8);

                    SetHeightOffset(0, _renderingParams.heightOffset1);
                    SetHeightOffset(1, _renderingParams.heightOffset2);
                    SetHeightOffset(2, _renderingParams.heightOffset3);
                    SetHeightOffset(3, _renderingParams.heightOffset4);
                    SetHeightOffset(4, _renderingParams.heightOffset5);
                    SetHeightOffset(5, _renderingParams.heightOffset6);
                    SetHeightOffset(6, _renderingParams.heightOffset7);
                    SetHeightOffset(7, _renderingParams.heightOffset8);

                    HeightmapBlending = _renderingParams.heightBlending;

                    SetTileRemover(0, _renderingParams.tilingRemover1);
                    SetTileRemover(1, _renderingParams.tilingRemover2);
                    SetTileRemover(2, _renderingParams.tilingRemover3);
                    SetTileRemover(3, _renderingParams.tilingRemover4);
                    SetTileRemover(4, _renderingParams.tilingRemover5);
                    SetTileRemover(5, _renderingParams.tilingRemover6);
                    SetTileRemover(6, _renderingParams.tilingRemover7);
                    SetTileRemover(7, _renderingParams.tilingRemover8);

                    SetNoseTiling(0, _renderingParams.noiseTiling1);
                    SetNoseTiling(1, _renderingParams.noiseTiling2);
                    SetNoseTiling(2, _renderingParams.noiseTiling3);
                    SetNoseTiling(3, _renderingParams.noiseTiling4);
                    SetNoseTiling(4, _renderingParams.noiseTiling5);
                    SetNoseTiling(5, _renderingParams.noiseTiling6);
                    SetNoseTiling(6, _renderingParams.noiseTiling7);
                    SetNoseTiling(7, _renderingParams.noiseTiling8);

                    BlendingDistance = _renderingParams.colormapBlendingDistance;

                    SnowColor = new Color(_renderingParams.snowColorR, _renderingParams.snowColorG, _renderingParams.snowColorB);

                    SnowTile = _renderingParams.snowTiling;
                    SnowAmount = _renderingParams.snowAmount;
                    SnowAngle = _renderingParams.snowAngles;
                    SnowNormalInfluence = _renderingParams.snowNormalInfluence;
                    SnowPower = _renderingParams.snowPower;
                    SnowSmoothness = _renderingParams.snowSmoothness;
                    SnowStartHeight = _renderingParams.snowStartHeight;
                    HeightFalloff = _renderingParams.heightFalloff;

                    PuddleColor = new Color(_renderingParams.puddleColorR, _renderingParams.puddleColorG, _renderingParams.puddleColorB);

                    PuddleRefraction = _renderingParams.puddleRefraction;
                    PuddleMetallic = _renderingParams.puddleMetallic;
                    PuddleSmoothness = _renderingParams.puddleSmoothness;
                    PuddlewaterHeight = _renderingParams.puddlewaterHeight;
                    PuddleSlope = _renderingParams.puddleSlope;
                    PuddleSlopeMin = _renderingParams.puddleMinSlope;
                    PuddleNoiseTiling = _renderingParams.puddleNoiseTiling;
                    //PuddleNoiseInfluence = _renderingParams.puddleNoiseInfluence;
                    //PuddleReflections = _renderingParams.puddleReflections;

                    SetLayerColor(0, new Color(_renderingParams.layerColor1R, _renderingParams.layerColor1G, _renderingParams.layerColor1B));
                    SetLayerColor(1, new Color(_renderingParams.layerColor2R, _renderingParams.layerColor2G, _renderingParams.layerColor2B));
                    SetLayerColor(2, new Color(_renderingParams.layerColor3R, _renderingParams.layerColor3G, _renderingParams.layerColor3B));
                    SetLayerColor(3, new Color(_renderingParams.layerColor4R, _renderingParams.layerColor4G, _renderingParams.layerColor4B));
                    SetLayerColor(4, new Color(_renderingParams.layerColor5R, _renderingParams.layerColor5G, _renderingParams.layerColor5B));
                    SetLayerColor(5, new Color(_renderingParams.layerColor6R, _renderingParams.layerColor6G, _renderingParams.layerColor6B));
                    SetLayerColor(6, new Color(_renderingParams.layerColor7R, _renderingParams.layerColor7G, _renderingParams.layerColor7B));
                    SetLayerColor(7, new Color(_renderingParams.layerColor8R, _renderingParams.layerColor8G, _renderingParams.layerColor8B));

                    SetLayerAO(0, _renderingParams.layerAO1);
                    SetLayerAO(1, _renderingParams.layerAO2);
                    SetLayerAO(2, _renderingParams.layerAO3);
                    SetLayerAO(3, _renderingParams.layerAO4);
                    SetLayerAO(4, _renderingParams.layerAO5);
                    SetLayerAO(5, _renderingParams.layerAO6);
                    SetLayerAO(6, _renderingParams.layerAO7);
                    SetLayerAO(7, _renderingParams.layerAO8);
                }

            }

            SplatmapResolutionBestFit = _renderingParams.splatmapResolutionBestFit;
            SplatmapSmoothness = _renderingParams.splatmapSmoothness;
            SplatmapResolution = _renderingParams.splatmapResolution;

            //TerrainPixelError = _renderingParams.terrainPixelError;

            BGMountains = _renderingParams.BGMountains;
            BGTerrainScaleMultiplier = _renderingParams.BGTerrainScaleMultiplier;
            BGTerrainHeightmapResolution = _renderingParams.BGTerrainHeightmapResolution;
            BGTerrainSatelliteImageResolution = _renderingParams.BGTerrainSatelliteImageResolution;
            BGTerrainPixelError = _renderingParams.BGTerrainPixelError;
            BGTerrainOffset = _renderingParams.BGTerrainOffset;

            TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams = _renderingParams;
            TTerraWorldManager.SaveGraph();
        }

        private static Terrain GetMainTerrain()
        {
            if (TTerraWorldManager.MainTerrainGO == null)
                throw new Exception("No Terrains Found!");

            return TTerraWorldManager.TerrainParamsScript.MainTerrain;
        }

        private static Material GetTerrainMaterial()
        {
            if (TTerraWorldManager.MainTerrainGO != null)
                return TTerraWorldManager.TerrainParamsScript.TerrainMaterial;
            else
                return null;
        }

        private static Material GetBGTerrainMaterial()
        {
            if (TTerraWorldManager.BackgroundTerrainGO != null)
                return TTerraWorldManager.TerrainParamsScript.TerrainMaterialBG;
            else
                return null;
        }

        private static bool IsModernRendering()
        {
            if (TerrainMaterial == null || !TerrainMaterial.HasProperty("_LightingColor"))
                return false;
            else if (TerrainMaterial.shader == Shader.Find("TerraUnity/TerraFormer") || TerrainMaterial.shader == Shader.Find("TerraUnity/TerraFormer Instanced"))
                return true;
            else
                return false;
        }

        private static void SetModernRendering(bool isModernRendering)
        {
            if (TerrainMaterial == null) return;

            if (isModernRendering)
            {
                TerrainMaterial.shader = Shader.Find("TerraUnity/TerraFormer Instanced");
                //SetTerrainMaterialTerraFormer();
            }
            else
            {
                //SetTerrainMaterialStandard();
                TerrainMaterial.shader = Shader.Find("Nature/Terrain/Standard");
            }

            if (TerrainMaterialBG == null) return;

            if (isModernRendering)
            {
                TerrainMaterialBG.shader = Shader.Find("TerraUnity/TerraFormer Instanced");
                //SetTerrainMaterialTerraFormer();
            }
            else
            {
                //SetTerrainMaterialStandard();
                TerrainMaterialBG.shader = Shader.Find("Nature/Terrain/Standard");
            }
        }

        private static bool IsTerrainTessellation()
        {
            if (!isModernRendering) return false;
            SetTerrainTessellation(TTerraWorldManager.IsTessalation);
            return TTerraWorldManager.IsTessalation;
        }

        private static void SetTerrainTessellation(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled)
            {
                TerrainMaterial.shader = Shader.Find("TerraUnity/TerraFormer");
                TerrainMaterial.EnableKeyword("_TESSELLATION");
                WorldTerrain.drawInstanced = false;
            }
            else
            {
                TerrainMaterial.shader = Shader.Find("TerraUnity/TerraFormer Instanced");
                WorldTerrain.drawInstanced = true;
            }

            TTerraWorldManager.IsTessalation = enabled;

        }

        private static bool IsTerrainHeightmapBlending()
        {
            if (!isModernRendering) return false;

            if (TerrainMaterial.IsKeywordEnabled("_HEIGHTMAPBLENDING"))
                return true;
            else
                return false;
        }

        private static void SetTerrainHeightmapBlending(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled)
                TerrainMaterial.EnableKeyword("_HEIGHTMAPBLENDING");
            else
                TerrainMaterial.DisableKeyword("_HEIGHTMAPBLENDING");
        }

        private static bool IsTerrainColormapBlending()
        {
            if (!isModernRendering) return false;

            if (TerrainMaterial.IsKeywordEnabled("_COLORMAPBLENDING"))
                return true;
            else
                return false;
        }

        private static void SetTerrainColormapBlending(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled)
                TerrainMaterial.EnableKeyword("_COLORMAPBLENDING");
            else
                TerrainMaterial.DisableKeyword("_COLORMAPBLENDING");
        }

        private static bool IsTerrainProceduralSnow()
        {
            if (!isModernRendering) return false;

            if (TerrainMaterial.IsKeywordEnabled("_PROCEDURALSNOW"))
                return true;
            else
                return false;
        }

        private static void SetTerrainProceduralSnow(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled)
            {
                TerrainMaterial.EnableKeyword("_PROCEDURALSNOW");
                TerrainMaterial.SetFloat("_SnowState", 1);
            }
            else
            {
                TerrainMaterial.DisableKeyword("_PROCEDURALSNOW");
                TerrainMaterial.SetFloat("_SnowState", 0);
            }

            SetTerrainProceduralSnowBG(enabled);
        }

        private static bool IsTerrainProceduralSnowBG()
        {
            if (TerrainMaterialBG == null) return false;

            if (TerrainMaterialBG.IsKeywordEnabled("_PROCEDURALSNOW"))
                return true;
            else
                return false;
        }

        private static void SetTerrainProceduralSnowBG(bool enabled)
        {
            if (TerrainMaterialBG == null || !isModernRendering) return;

            if (enabled)
            {
                TerrainMaterialBG.EnableKeyword("_PROCEDURALSNOW");
                TerrainMaterialBG.SetFloat("_SnowState", 1);

                TerrainMaterialBG.SetColor("_SnowColor", TerrainMaterial.GetColor("_SnowColor"));
                TerrainMaterialBG.SetFloat("_SnowTile", TerrainMaterial.GetFloat("_SnowTile"));
                TerrainMaterialBG.SetFloat("_SnowAmount", TerrainMaterial.GetFloat("_SnowAmount"));
                TerrainMaterialBG.SetFloat("_SnowAngle", TerrainMaterial.GetFloat("_SnowAngle"));
                TerrainMaterialBG.SetFloat("_NormalInfluence", TerrainMaterial.GetFloat("_NormalInfluence"));
                TerrainMaterialBG.SetFloat("_SnowPower", TerrainMaterial.GetFloat("_SnowPower"));
                TerrainMaterialBG.SetFloat("_SnowSmoothness", TerrainMaterial.GetFloat("_SnowSmoothness"));
                //TerrainMaterialBG.SetFloat("_SnowMetallic", TerrainMaterial.GetFloat("_SnowMetallic"));
            }
            else
            {
                TerrainMaterialBG.DisableKeyword("_PROCEDURALSNOW");
                TerrainMaterialBG.SetFloat("_SnowState", 0);
            }
        }

        private static bool IsTerrainFlatShading()
        {
            if (!isModernRendering) return false;

            if (TerrainMaterial.IsKeywordEnabled("_FLATSHADING"))
                return true;
            else
                return false;
        }

        private static void SetTerrainFlatShading(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled)
            {
                TerrainMaterial.EnableKeyword("_FLATSHADING");
                TerrainMaterial.SetFloat("_FlatShadingState", 1);
            }
            else
            {
                TerrainMaterial.DisableKeyword("_FLATSHADING");
                TerrainMaterial.SetFloat("_FlatShadingState", 0);
            }

            SetTerrainFlatShadingBG(enabled);
        }

        private static void SetTerrainFlatShadingBG(bool enabled)
        {
            if (TerrainMaterialBG == null) return;

            if (enabled)
            {
                TerrainMaterialBG.EnableKeyword("_FLATSHADING");
                TerrainMaterialBG.SetFloat("_FlatShadingState", 1);
            }
            else
            {
                TerrainMaterialBG.DisableKeyword("_FLATSHADING");
                TerrainMaterialBG.SetFloat("_FlatShadingState", 0);
            }
        }

        private static bool IsTerrainProceduralPuddles()
        {
            if (!isModernRendering) return false;

            if (TerrainMaterial.IsKeywordEnabled("_PROCEDURALPUDDLES"))
                return true;
            else
                return false;
        }

        private static void SetTerrainProceduralPuddles(bool enabled)
        {
            if (!isModernRendering) return;

            if (enabled)
                TerrainMaterial.EnableKeyword("_PROCEDURALPUDDLES");
            else
                TerrainMaterial.DisableKeyword("_PROCEDURALPUDDLES");
        }

        private static Texture2D GetColormapTexture()
        {
            if (!isModernRendering) colormapTexture = null;

            if (colormapTexture == null)
                colormapTexture = TerrainMaterial.GetTexture("_ColorMap") as Texture2D;

            if (colormapTexture == null)
                TerrainMaterial.DisableKeyword("_COLORMAPBLENDING");

            return colormapTexture;
        }

        private static void SetColormapTexture(Texture2D texture2D)
        {
            colormapTexture = texture2D;
            TerrainMaterial.SetTexture("_ColorMap", texture2D);

            if (colormapTexture == null)
                TerrainMaterial.DisableKeyword("_COLORMAPBLENDING");
        }

        private static Texture2D GetWaterMaskTexture()
        {
            if (!isModernRendering) waterMaskTexture = null;

            //if (waterMaskTexture == null)
            waterMaskTexture = TerrainMaterial.GetTexture("_WaterMask") as Texture2D;

            return waterMaskTexture;
        }

        private static void SetWaterMaskTexture(Texture2D _waterMask)
        {
            waterMaskTexture = _waterMask;
            TerrainMaterial.SetTexture("_WaterMask", _waterMask);
        }

        private static Texture2D GetSnowTexture()
        {
            if (!isModernRendering) snowTexture = null;
            else snowTexture = TerrainMaterial.GetTexture("_SnowDiffuse") as Texture2D;
            return snowTexture;
        }

        //private static Texture2D GetNoiseTexture()
        //{
        //    if (!isModernRendering) noiseTexture = null;
        //    else noiseTexture = TerrainMaterial.GetTexture("_Noise") as Texture2D;
        //    return noiseTexture;
        //}

        private static int GetTerrainLayersCount()
        {
            if (WorldTerrain != null && WorldTerrain.terrainData != null && WorldTerrain.terrainData.terrainLayers != null)
                terrainLayersCount = WorldTerrain.terrainData.terrainLayers.Length;
            else
                terrainLayersCount = 0;

            return terrainLayersCount;
        }

        public static void SwitchTerrainLayer(Terrain _terrain, int replaceIndex, TerrainLayer terrainLayer)
        {
            if (_terrain != null && _terrain.terrainData != null && _terrain.terrainData.terrainLayers != null)
            {
                TerrainLayer[] layers = _terrain.terrainData.terrainLayers;
                if (replaceIndex < layers.Length)
                {
                    layers[replaceIndex] = terrainLayer;
                    _terrain.terrainData.terrainLayers = layers;
                    _terrain.Flush();
                }
            }
        }

        public static void SetTerrainMaterialMAIN(Terrain _terrain)
        {
            TDebug.TraceMessage();

            if (_terrain == null)
                throw new Exception("No Terrain Component Found! (SetTerrainMaterialMAIN)");

#if !UNITY_2019_1_OR_NEWER
            _terrain.materialType = Terrain.MaterialType.Custom;
#endif
            Material mat = _terrain.materialTemplate;

            //if (mat != null) return;

            string materialPath = TTerraWorld.WorkDirectoryLocalPath + "Terrain.mat";

            if (!File.Exists(materialPath))
            {
                TResourcesManager.LoadAllResources();
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.TerraFormerMaterial), materialPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            mat = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            mat.shader = Shader.Find("TerraUnity/TerraFormer Instanced");
            mat.SetTexture("_SnowDiffuse", TResourcesManager.snowAlbedo);
            mat.SetTexture("_SnowNormalmap", TResourcesManager.snowNormalmap);
            mat.SetTexture("_SnowMaskmap", TResourcesManager.snowMaskmap);
            //mat.SetTexture("_Noise", TResourcesManager.noise);

            //Texture2D colormap = AssetDatabase.LoadAssetAtPath(TTerraWorld.WorkDirectoryLocalPath + "ColorMap.jpg", typeof(Texture2D)) as Texture2D;
            //
            //if (colormap != null)
            //    mat.SetTexture("_ColorMap", colormap);

            TTerraWorldTerrainManager.SetTerrainMaterial(_terrain, mat);
        }

        public static void SetTerrainMaterialBG(Terrain _terrain)
        {
            TDebug.TraceMessage();

            if (_terrain == null)
                throw new Exception("No Terrain Component Found! (SetTerrainMaterialBG)");

#if !UNITY_2019_1_OR_NEWER
            _terrain.materialType = Terrain.MaterialType.Custom;
#endif
            Material mat = _terrain.materialTemplate;

            //if (mat != null) return;

            string materialPath = TTerraWorld.WorkDirectoryLocalPath + "BGTerrain.mat";

            if (!File.Exists(materialPath))
            {
                TResourcesManager.LoadAllResources();
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(TResourcesManager.TerraFormerMaterialBG), materialPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

            mat = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            mat.shader = Shader.Find("TerraUnity/TerraFormer Instanced");
            mat.SetTexture("_SnowDiffuse", TResourcesManager.snowAlbedo);
            mat.SetTexture("_SnowNormalmap", TResourcesManager.snowNormalmap);
            mat.SetTexture("_SnowMaskmap", TResourcesManager.snowMaskmap);
            //mat.SetTexture("_Noise", TResourcesManager.noise);

            TTerraWorldTerrainManager.SetTerrainMaterialBG(_terrain, mat);
        }

     
    }
}
#endif
#endif

