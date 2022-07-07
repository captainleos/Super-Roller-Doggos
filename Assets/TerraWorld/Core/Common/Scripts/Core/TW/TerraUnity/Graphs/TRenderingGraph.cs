#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;

namespace TerraUnity.Edittime
{
    public enum RenderingModules
    {
        RenderingModule
    }

    public abstract class TRenderingModules : TNode
    {
        public RenderingParams renderingParams = new RenderingParams(true);

        public TRenderingModules() : base()
        {
        }
    }

    public struct RenderingParams
    {
        public string worldName;

        public bool modernRendering;
        //public bool instancedDrawing;
        public bool tessellation;
        public bool heightmapBlending;
        public bool colormapBlending;
        public bool proceduralSnow;
        public bool proceduralPuddles;
        public bool isFlatShading;
        
        // Surface Tint
        //-----------------------------------------------------------------------
        public Vector4 surfaceTintColorMAIN;
        public Vector4 surfaceTintColorBG;
        
        // Tessellation
        //-----------------------------------------------------------------------
        public float tessellationQuality;
        public float edgeSmoothness;
        public float displacement1;
        public float displacement2;
        public float displacement3;
        public float displacement4;
        public float displacement5;
        public float displacement6;
        public float displacement7;
        public float displacement8;
        public float heightOffset1;
        public float heightOffset2;
        public float heightOffset3;
        public float heightOffset4;
        public float heightOffset5;
        public float heightOffset6;
        public float heightOffset7;
        public float heightOffset8;
        
        // Heightmap Blending
        //-----------------------------------------------------------------------
        public float heightBlending;
                     
        // Tiling Remover
        //-----------------------------------------------------------------------
        public float tilingRemover1;
        public float tilingRemover2;
        public float tilingRemover3;
        public float tilingRemover4;
        public float tilingRemover5;
        public float tilingRemover6;
        public float tilingRemover7;
        public float tilingRemover8;
        
        public float noiseTiling1;
        public float noiseTiling2;
        public float noiseTiling3;
        public float noiseTiling4;
        public float noiseTiling5;
        public float noiseTiling6;
        public float noiseTiling7;
        public float noiseTiling8;
        
        // Colormap Blending
        //-----------------------------------------------------------------------
        public float colormapBlendingDistance;
        public float colormapBlendingRange;
        
        // Procedural Snow
        //-----------------------------------------------------------------------
        public float snowColorR;
        public float snowColorG;
        public float snowColorB;
        public float snowTiling;
        public float snowAmount;
        public float snowAngles;
        public float snowNormalInfluence;
        public float snowPower;
        //public float snowMetallic;
        public float snowSmoothness;
        //public float snowNormalPower;
        public float snowStartHeight;
        public float heightFalloff;
        
        // Procedural Puddles
        //-----------------------------------------------------------------------
        public float puddleColorR;
        public float puddleColorG;
        public float puddleColorB;
        public float puddleRefraction;
        public float puddleMetallic;
        public float puddleSmoothness;
        public float puddlewaterHeight;
        public float puddleSlope;
        public float puddleMinSlope;
        public float puddleNoiseTiling;
        public float puddleNoiseInfluence;
        public bool  puddleReflections;
        
        // Layer Properties
        //-----------------------------------------------------------------------
        public float layerColor1R;
        public float layerColor1G;
        public float layerColor1B;
        public float layerColor2R;
        public float layerColor2G;
        public float layerColor2B;
        public float layerColor3R;
        public float layerColor3G;
        public float layerColor3B;
        public float layerColor4R;
        public float layerColor4G;
        public float layerColor4B;
        public float layerColor5R;
        public float layerColor5G;
        public float layerColor5B;
        public float layerColor6R;
        public float layerColor6G;
        public float layerColor6B;
        public float layerColor7R;
        public float layerColor7G;
        public float layerColor7B;
        public float layerColor8R;
        public float layerColor8G;
        public float layerColor8B;
        public float layerAO1;
        public float layerAO2;
        public float layerAO3;
        public float layerAO4;
        public float layerAO5;
        public float layerAO6;
        public float layerAO7;
        public float layerAO8;

        // Splatmap Settings
        //-----------------------------------------------------------------------
        public bool splatmapResolutionBestFit;
        public int splatmapSmoothness;
        public int splatmapResolution;

        // Main Terrain Settings
        //-----------------------------------------------------------------------
        public int terrainPixelError;

        // Background Terrain Settings
        //-----------------------------------------------------------------------
        public bool BGMountains;
        public int BGTerrainScaleMultiplier;
        public int BGTerrainHeightmapResolution;
        public int BGTerrainSatelliteImageResolution;
        public int BGTerrainPixelError;
        public float BGTerrainOffset;


        public RenderingParams(bool newparameters = false)
        {
            if (!newparameters) throw new Exception("Undefined RenderingParams ");
            worldName = "";

            // Global Settings
            //-----------------------------------------------------------------------
            modernRendering = true;
            //renderingParams.instancedDrawing = false;
            tessellation = false;
            heightmapBlending = false;
            colormapBlending = false;
            proceduralSnow = false;
            proceduralPuddles = false;
            isFlatShading = false;

            // Surface Tint
            //-----------------------------------------------------------------------
            surfaceTintColorMAIN = new Vector4(0.85f, 0.85f, 0.85f, 1.0f);
            surfaceTintColorBG = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);

            // Tessellation
            //-----------------------------------------------------------------------
            tessellationQuality = 10f;
            edgeSmoothness = 0f;
            displacement1 = 1f;
            displacement2 = 1f;
            displacement3 = 1f;
            displacement4 = 1f;
            displacement5 = 1f;
            displacement6 = 1f;
            displacement7 = 1f;
            displacement8 = 1f;
            heightOffset1 = 0f;
            heightOffset2 = 0f;
            heightOffset3 = 0f;
            heightOffset4 = 0f;
            heightOffset5 = 0f;
            heightOffset6 = 0f;
            heightOffset7 = 0f;
            heightOffset8 = 0f;

            // Heightmap Blending
            //-----------------------------------------------------------------------
            heightBlending = 0.25f;

            // Tiling Remover
            //-----------------------------------------------------------------------
            tilingRemover1 = 0f;
            tilingRemover2 = 0f;
            tilingRemover3 = 0f;
            tilingRemover4 = 0f;
            tilingRemover5 = 0f;
            tilingRemover6 = 0f;
            tilingRemover7 = 0f;
            tilingRemover8 = 0f;
            noiseTiling1 = 100f;
            noiseTiling2 = 100f;
            noiseTiling3 = 100f;
            noiseTiling4 = 100f;
            noiseTiling5 = 100f;
            noiseTiling6 = 100f;
            noiseTiling7 = 100f;
            noiseTiling8 = 100f;

            // Colormap Blending
            //-----------------------------------------------------------------------
            colormapBlendingDistance = 2500f;
            colormapBlendingRange = 0.05f;

            // Procedural Snow
            //-----------------------------------------------------------------------
            snowColorR = 0.5f;
            snowColorG = 0.5f;
            snowColorB = 0.5f;
            snowTiling = 2000f;
            snowAmount = 0.75f;
            snowAngles = -0.4f;
            snowNormalInfluence = 0.5f;
            snowPower = 0.9f;
            snowSmoothness = 1f;
            snowStartHeight = 3500f;
            heightFalloff = 1000f;

            // Procedural Puddles
            //-----------------------------------------------------------------------
            puddleColorR = 0.85f;
            puddleColorG = 0.85f;
            puddleColorB = 0.85f;
            puddleRefraction = 0.07f;
            puddleMetallic = 0.925f;
            puddleSmoothness = 0.95f;
            puddlewaterHeight = 1f;
            puddleSlope = 0.004f;
            puddleMinSlope = 0.0025f;
            puddleNoiseTiling = 300f;
            puddleNoiseInfluence = 0.1f;
            puddleReflections = false;

            // Layer Properties
            //-----------------------------------------------------------------------
            layerColor1R = 1f;
            layerColor1G = 1f;
            layerColor1B = 1f;
            layerColor2R = 1f;
            layerColor2G = 1f;
            layerColor2B = 1f;
            layerColor3R = 1f;
            layerColor3G = 1f;
            layerColor3B = 1f;
            layerColor4R = 1f;
            layerColor4G = 1f;
            layerColor4B = 1f;
            layerColor5R = 1f;
            layerColor5G = 1f;
            layerColor5B = 1f;
            layerColor6R = 1f;
            layerColor6G = 1f;
            layerColor6B = 1f;
            layerColor7R = 1f;
            layerColor7G = 1f;
            layerColor7B = 1f;
            layerColor8R = 1f;
            layerColor8G = 1f;
            layerColor8B = 1f;
            layerAO1 = 1f;
            layerAO2 = 1f;
            layerAO3 = 1f;
            layerAO4 = 1f;
            layerAO5 = 1f;
            layerAO6 = 1f;
            layerAO7 = 1f;
            layerAO8 = 1f;

            splatmapResolutionBestFit = true;
            splatmapSmoothness = 1;
            splatmapResolution = 512;
            terrainPixelError = 5;
            BGMountains = true;
            BGTerrainScaleMultiplier = 8;
            BGTerrainHeightmapResolution = 512;
            BGTerrainSatelliteImageResolution = 2048;
            BGTerrainPixelError = 40;
            BGTerrainOffset = 0f;
        }
    }

    [XmlType("RenderingNode")]
    public class RenderingNode : TRenderingModules
    {
        public RenderingNode() : base()
        {
            Data.moduleType = ModuleType.Processor;
            //type = typeof(RenderingNode).FullName;
            Data.name = "Rendering";
            isRemovable = false;
            isRunnable = false;
            //ResetToDefaultSettings();
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.Area;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public void SetRenderingParams
        (
            RenderingParams newRenderingParams,
            bool Apply_RenderingGraph_surfaceTint,
            bool Apply_RenderingGraph_modernRendering,
            //bool Apply_RenderingGraph_instancedDrawing,
            bool Apply_RenderingGraph_tessellation,
            bool Apply_RenderingGraph_heightmapBlending,
            bool Apply_RenderingGraph_TillingRemover,
            bool Apply_RenderingGraph_colormapBlending,
            bool Apply_RenderingGraph_proceduralSnow,
            bool Apply_RenderingGraph_proceduralPuddles,
            bool Apply_RenderingGraph_LayerProperties,
            bool Apply_RenderingGraph_isFlatShading,
            bool Apply_RenderingGraph_SplatmapSettings,
            //bool Apply_RenderingGraph_MainTerrainSettings,
            bool Apply_RenderingGraph_BGTerrainSettings
        )
        {
            renderingParams.worldName = newRenderingParams.worldName;

            //if (Apply_RenderingGraph_surfaceTint)
            //{
            //    renderingParams.surfaceTintColorMAIN = newRenderingParams.surfaceTintColorMAIN;
            //    renderingParams.surfaceTintColorBG = newRenderingParams.surfaceTintColorBG;
            //}
            //
            //if (Apply_RenderingGraph_modernRendering)
            //{
            //    renderingParams.modernRendering = newRenderingParams.modernRendering;
            //}
            //
            ////if (Apply_RenderingGraph_instancedDrawing)
            ////{
            ////    renderingParams.instancedDrawing = newRenderingParams.instancedDrawing;
            ////}
            //
            //// Tessellation
            ////-----------------------------------------------------------------------
            //if (Apply_RenderingGraph_tessellation)
            //{
            //    renderingParams.tessellation = newRenderingParams.tessellation;
            //    renderingParams.tessellationQuality = newRenderingParams.tessellationQuality;
            //    renderingParams.edgeSmoothness = newRenderingParams.edgeSmoothness;
            //    renderingParams.displacement1 = newRenderingParams.displacement1;
            //    renderingParams.displacement2 = newRenderingParams.displacement2;
            //    renderingParams.displacement3 = newRenderingParams.displacement3;
            //    renderingParams.displacement4 = newRenderingParams.displacement4;
            //    renderingParams.displacement5 = newRenderingParams.displacement5;
            //    renderingParams.displacement6 = newRenderingParams.displacement6;
            //    renderingParams.displacement7 = newRenderingParams.displacement7;
            //    renderingParams.displacement8 = newRenderingParams.displacement8;
            //    renderingParams.heightOffset1 = newRenderingParams.heightOffset1;
            //    renderingParams.heightOffset2 = newRenderingParams.heightOffset2;
            //    renderingParams.heightOffset3 = newRenderingParams.heightOffset3;
            //    renderingParams.heightOffset4 = newRenderingParams.heightOffset4;
            //    renderingParams.heightOffset5 = newRenderingParams.heightOffset5;
            //    renderingParams.heightOffset6 = newRenderingParams.heightOffset6;
            //    renderingParams.heightOffset7 = newRenderingParams.heightOffset7;
            //    renderingParams.heightOffset8 = newRenderingParams.heightOffset8;
            //}
            //
            //// Heightmap Blending
            ////-----------------------------------------------------------------------
            //if (Apply_RenderingGraph_heightmapBlending)
            //{
            //    renderingParams.heightmapBlending = newRenderingParams.heightmapBlending;
            //    renderingParams.heightBlending = newRenderingParams.heightBlending;
            //}
            //
            //// Tiling Remover
            ////-----------------------------------------------------------------------
            //if (Apply_RenderingGraph_TillingRemover)
            //{
            //    renderingParams.tilingRemover1 = newRenderingParams.tilingRemover1;
            //    renderingParams.tilingRemover2 = newRenderingParams.tilingRemover2;
            //    renderingParams.tilingRemover3 = newRenderingParams.tilingRemover3;
            //    renderingParams.tilingRemover4 = newRenderingParams.tilingRemover4;
            //    renderingParams.tilingRemover5 = newRenderingParams.tilingRemover5;
            //    renderingParams.tilingRemover6 = newRenderingParams.tilingRemover6;
            //    renderingParams.tilingRemover7 = newRenderingParams.tilingRemover7;
            //    renderingParams.tilingRemover8 = newRenderingParams.tilingRemover8;
            //
            //    renderingParams.noiseTiling1 = newRenderingParams.noiseTiling1;
            //    renderingParams.noiseTiling2 = newRenderingParams.noiseTiling2;
            //    renderingParams.noiseTiling3 = newRenderingParams.noiseTiling3;
            //    renderingParams.noiseTiling4 = newRenderingParams.noiseTiling4;
            //    renderingParams.noiseTiling5 = newRenderingParams.noiseTiling5;
            //    renderingParams.noiseTiling6 = newRenderingParams.noiseTiling6;
            //    renderingParams.noiseTiling7 = newRenderingParams.noiseTiling7;
            //    renderingParams.noiseTiling8 = newRenderingParams.noiseTiling8;
            //}
            //
            //// Colormap Blending
            ////-----------------------------------------------------------------------
            //if (Apply_RenderingGraph_colormapBlending)
            //{
            //    renderingParams.colormapBlending = newRenderingParams.colormapBlending;
            //    renderingParams.colormapBlendingDistance = newRenderingParams.colormapBlendingDistance;
            //    renderingParams.colormapBlendingRange = newRenderingParams.colormapBlendingRange;
            //}
            //
            //// Procedural Snow
            ////-----------------------------------------------------------------------
            //if (Apply_RenderingGraph_proceduralSnow)
            //{
            //    renderingParams.proceduralSnow = newRenderingParams.proceduralSnow;
            //    renderingParams.snowColorR = newRenderingParams.snowColorR;
            //    renderingParams.snowColorG = newRenderingParams.snowColorG;
            //    renderingParams.snowColorB = newRenderingParams.snowColorB;
            //    renderingParams.snowTiling = newRenderingParams.snowTiling;
            //    renderingParams.snowAmount = newRenderingParams.snowAmount;
            //    renderingParams.snowAngles = newRenderingParams.snowAngles;
            //    renderingParams.snowNormalInfluence = newRenderingParams.snowNormalInfluence;
            //    renderingParams.snowPower = newRenderingParams.snowPower;
            //    renderingParams.snowSmoothness = newRenderingParams.snowSmoothness;
            //    renderingParams.snowStartHeight = newRenderingParams.snowStartHeight;
            //    renderingParams.heightFalloff = newRenderingParams.heightFalloff;
            //}
            //
            //// Procedural Puddles
            ////-----------------------------------------------------------------------
            //if (Apply_RenderingGraph_proceduralPuddles)
            //{
            //    renderingParams.proceduralPuddles = newRenderingParams.proceduralPuddles;
            //    renderingParams.puddleColorR = newRenderingParams.puddleColorR;
            //    renderingParams.puddleColorG = newRenderingParams.puddleColorG;
            //    renderingParams.puddleColorB = newRenderingParams.puddleColorB;
            //    renderingParams.puddleRefraction = newRenderingParams.puddleRefraction;
            //    renderingParams.puddleMetallic = newRenderingParams.puddleMetallic;
            //    renderingParams.puddleSmoothness = newRenderingParams.puddleSmoothness;
            //    renderingParams.puddlewaterHeight = newRenderingParams.puddlewaterHeight;
            //    renderingParams.puddleSlope = newRenderingParams.puddleSlope;
            //    renderingParams.puddleMinSlope = newRenderingParams.puddleMinSlope;
            //    renderingParams.puddleNoiseTiling = newRenderingParams.puddleNoiseTiling;
            //    renderingParams.puddleNoiseInfluence = newRenderingParams.puddleNoiseInfluence;
            //    renderingParams.puddleReflections = newRenderingParams.puddleReflections;
            //}
            //
            //if (Apply_RenderingGraph_LayerProperties)
            //{
            //    renderingParams.layerColor1R = newRenderingParams.layerColor1R;
            //    renderingParams.layerColor1G = newRenderingParams.layerColor1G;
            //    renderingParams.layerColor1B = newRenderingParams.layerColor1B;
            //    renderingParams.layerColor2R = newRenderingParams.layerColor2R;
            //    renderingParams.layerColor2G = newRenderingParams.layerColor2G;
            //    renderingParams.layerColor2B = newRenderingParams.layerColor2B;
            //    renderingParams.layerColor3R = newRenderingParams.layerColor3R;
            //    renderingParams.layerColor3G = newRenderingParams.layerColor3G;
            //    renderingParams.layerColor3B = newRenderingParams.layerColor3B;
            //    renderingParams.layerColor4R = newRenderingParams.layerColor4R;
            //    renderingParams.layerColor4G = newRenderingParams.layerColor4G;
            //    renderingParams.layerColor4B = newRenderingParams.layerColor4B;
            //    renderingParams.layerColor5R = newRenderingParams.layerColor5R;
            //    renderingParams.layerColor5G = newRenderingParams.layerColor5G;
            //    renderingParams.layerColor5B = newRenderingParams.layerColor5B;
            //    renderingParams.layerColor6R = newRenderingParams.layerColor6R;
            //    renderingParams.layerColor6G = newRenderingParams.layerColor6G;
            //    renderingParams.layerColor6B = newRenderingParams.layerColor6B;
            //    renderingParams.layerColor7R = newRenderingParams.layerColor7R;
            //    renderingParams.layerColor7G = newRenderingParams.layerColor7G;
            //    renderingParams.layerColor7B = newRenderingParams.layerColor7B;
            //    renderingParams.layerColor8R = newRenderingParams.layerColor8R;
            //    renderingParams.layerColor8G = newRenderingParams.layerColor8G;
            //    renderingParams.layerColor8B = newRenderingParams.layerColor8B;
            //    renderingParams.layerAO1 = newRenderingParams.layerAO1;
            //    renderingParams.layerAO2 = newRenderingParams.layerAO2;
            //    renderingParams.layerAO3 = newRenderingParams.layerAO3;
            //    renderingParams.layerAO4 = newRenderingParams.layerAO4;
            //    renderingParams.layerAO5 = newRenderingParams.layerAO5;
            //    renderingParams.layerAO6 = newRenderingParams.layerAO6;
            //    renderingParams.layerAO7 = newRenderingParams.layerAO7;
            //    renderingParams.layerAO8 = newRenderingParams.layerAO8;
            //}
            //
            //if (Apply_RenderingGraph_isFlatShading)
            //{
            //    renderingParams.isFlatShading = newRenderingParams.isFlatShading;
            //}

            if (Apply_RenderingGraph_SplatmapSettings)
            {
                renderingParams.splatmapResolutionBestFit = newRenderingParams.splatmapResolutionBestFit;
                renderingParams.splatmapSmoothness = newRenderingParams.splatmapSmoothness;
                renderingParams.splatmapResolution = newRenderingParams.splatmapResolution;
            }

           // if (Apply_RenderingGraph_MainTerrainSettings)
           // {
           //     renderingParams.terrainPixelError = newRenderingParams.terrainPixelError;
           // }

            if (Apply_RenderingGraph_BGTerrainSettings)
            {
                renderingParams.BGMountains = newRenderingParams.BGMountains;
                renderingParams.BGTerrainScaleMultiplier = newRenderingParams.BGTerrainScaleMultiplier;
                renderingParams.BGTerrainHeightmapResolution = newRenderingParams.BGTerrainHeightmapResolution;
                renderingParams.BGTerrainSatelliteImageResolution = newRenderingParams.BGTerrainSatelliteImageResolution;
                renderingParams.BGTerrainPixelError = newRenderingParams.BGTerrainPixelError;
                renderingParams.BGTerrainOffset = newRenderingParams.BGTerrainOffset;
            }
        }
    }

    public class TRenderingGraph : TGraph
    {
        public TRenderingGraph() : base(ConnectionDataType.Global, "RENDERING Graph")
        {
        }

        public void InitGraph(TTerraWorldGraph terraWorldGraph)
        {
            worldGraph = terraWorldGraph;
            _title = "RENDERING";
            if (nodes.Count > 0) return;
            RenderingNode node = new RenderingNode();
            node.Init(this);
            nodes.Add(node);
        }

        public RenderingNode GetEntryNode()
        {
            return (nodes[0] as RenderingNode);
        }
    }
}
#endif
#endif

