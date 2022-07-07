#if TERRAWORLD_XPRO
using System;
using UnityEngine;
using TerraUnity;
using System.Collections.Generic;
using UnityEditor;
using TerraUnity.Edittime;
using System.IO;
using System.Xml;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Generator/Water Generator")]
    public class TXWaterGeneratorNode : TXWaterModules
    {

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXLandcoverModules Input;



        public Material waterMaterial;
        public string layerName;
        public float lodCulling = 25f;
        public int AroundPointsDensity;
        public float AroundVariation;
        public string unityLayerName;
        public string XMLMaskData;
        public System.Numerics.Vector3 positionOffset = System.Numerics.Vector3.Zero;
        public int priority = 0;
        public bool GenerateLakes = true;
        public bool GenerateOceans = true;
        public bool GenerateRiver = true;
        public float RiverWidthInMeter = 200;
        public float Depth = 7;
        public float LakeMinSizeInM2 = 30000;
        public List<Vector2> boundingPoints;
        public bool smoothOperation = true;
        public float deformAngle = 10f;



        protected override void Init()
        {
            base.Init();
            SetName("Water Generator");
        }

        protected override void ModuleAction(TMap CurrentMap)
        {
            _progress = 0;
            XmlDocument xmlDocument = Input.GetProceededLandcover(CurrentMap._refTerrain);
            _lakeLayer = null;
            _riverLayer = null;
            _oceanLayer = null;
            Vector2 centerPointNormalized = new Vector2(0,0);
            float deformAngleRadian = deformAngle * (float)Math.PI / 180f;

            if (IsActive)
            {


                if (GenerateLakes)
                {
                    _lakeLayer = new TLakeLayer();
                    _lakeLayer.AroundPointsDensity = AroundPointsDensity;
                    _lakeLayer.AroundVariation = AroundVariation;
                    _lakeLayer.LayerName = NodeName;
                    _lakeLayer.xMaterial = waterMaterial;
                    _lakeLayer.Priority = priority;
                    _lakeLayer.UnityLayerName = unityLayerName;
                    _lakeLayer.useLayer = true;
                    _lakeLayer.Offset = positionOffset;
                    _lakeLayer.depth = Depth;
                    _lakeLayer.LODCulling = lodCulling;
                    _lakeLayer._minSizeInM2 = LakeMinSizeInM2;
                    List<T2DObject> lakes = _lakeLayer.LakesList;

                    OSMParser.ExtractLakes(xmlDocument, ref lakes, CurrentMap._area);
                    TLandcoverProccessor.FilterLakesBordersByBoundArea(CurrentMap, ref _lakeLayer, LakeMinSizeInM2);
                    _lakeLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, lakes, false, 1, 1f, false);

                    if (_lakeLayer?.WaterMasks?.Count > 0)
                    {

                        for (int i = 0; i < _lakeLayer.WaterMasks.Count; i++)
                        {
                            TMask mask = _lakeLayer.WaterMasks[i];
                            THeightmapProcessors.DeformWaterByMask(ref CurrentMap._refTerrain.Heightmap.heightsData, mask, false, null, 2,
                                                                   TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat,
                                                                   deformAngleRadian, out lakes[i].minHeight, out lakes[i].maxHeight, out lakes[i].avgHeight);

                        }
                    }
                }

                if (GenerateOceans)
                {
                    _oceanLayer = new TOceanLayer();
                    _oceanLayer.LayerName = NodeName;
                    _oceanLayer.xMaterial = waterMaterial;
                    //_oceanLayer.MaxElevation = areaBounds.maxElevation;
                    //_oceanLayer.MinElevation = areaBounds.minElevation;
                    _oceanLayer.Priority = priority;
                    _oceanLayer.UnityLayerName = unityLayerName;
                    _oceanLayer.useLayer = true;
                    _oceanLayer.Offset = positionOffset;
                    _oceanLayer.depth = Depth;
                    _oceanLayer.LODCulling = lodCulling;
                    List<T2DObject> Oceans = _oceanLayer.Coastlines;

                    OSMParser.ExtractOceans(xmlDocument, ref Oceans, CurrentMap._area);
                    _oceanLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, Oceans, false, 0, 1f, true);

                    if (_oceanLayer.WaterMasks.Count > 0)
                    {
                        TMask mask = _oceanLayer.WaterMasks[0];
                        T2DObject ocean = _oceanLayer.Coastlines[0];
                        THeightmapProcessors.DeformWaterByMask(ref CurrentMap._refTerrain.Heightmap.heightsData, mask, true, null, 2,
                                                               TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat,
                                                               deformAngleRadian, out ocean.minHeight, out ocean.maxHeight, out ocean.avgHeight);
                    }
                }

                if (GenerateRiver)
                {
                    _riverLayer = new TRiverLayer();
                    _riverLayer.LayerName = NodeName;
                    _riverLayer.xMaterial = waterMaterial;
                    //_riverLayer.MaxElevation = areaBounds.maxElevation;
                    //_riverLayer.MinElevation = areaBounds.minElevation;
                    _riverLayer.Priority = priority;
                    _riverLayer.UnityLayerName = unityLayerName;
                    _riverLayer.useLayer = true;
                    _riverLayer.Offset = positionOffset;
                    _riverLayer.depth = Depth;
                    _riverLayer.LODCulling = lodCulling;
                    _riverLayer._width = RiverWidthInMeter;

                    List<TLinearObject> rivers = _riverLayer.RiversList;


                    OSMParser.ExtractRivers(xmlDocument, ref rivers, CurrentMap._area);
                    _riverLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasksLinear(CurrentMap._refTerrain, rivers, false, 0, 1f, (RiverWidthInMeter) / 2f, true);

                    if (_riverLayer?.WaterMasks?.Count > 0)
                    {
                        for (int i = 0; i < _riverLayer.WaterMasks.Count; i++)
                        {
                            TMask mask = _riverLayer.WaterMasks[i];
                            THeightmapProcessors.DeformByMask(ref CurrentMap._refTerrain.Heightmap.heightsData, mask, _riverLayer.depth + 4,
                                                                                                                           false, null, 1);
                        }
                    }

                    if (smoothOperation)
                    {
                        CurrentMap._refTerrain.Heightmap.heightsData = THeightmapProcessors.SmoothHeightmap(
                                                CurrentMap._refTerrain.Heightmap.heightsData, 1, 0, THeightmapProcessors.Neighbourhood.Moore);
                    }
                }

                _progress = 1;
            }

        }


        public override void CheckEssentioalInputs()
        {
            Input = GetInputValue<TXLandcoverModules>("Input");
            if (Input == null) throw new Exception("Input" + " is missed for " + NodeName + " Node.");

            if (waterMaterial == null)
                throw new Exception("No Material Selected For " + NodeName + "\n\n Please Check The Node.");
        }

    }


}
#endif