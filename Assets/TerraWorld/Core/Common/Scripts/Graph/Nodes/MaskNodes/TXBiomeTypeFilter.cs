#if TERRAWORLD_XPRO
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    public enum BiomeTypes
    {
        Waters,
        Lakes,
        Sea,
        River,
        Trees,
        Wood,
        Meadow,
        Orchard,
        Grass,
        Greenfield,
        Beach,
        Wetland,
        Bay
    }

    [CreateNodeMenu("Masks/Landcover -> Mask")]
    public class TXBiomeTypeFilter : TXMaskModules
    {

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXLandcoverModules Input;

        public BiomeTypes biomeType = BiomeTypes.Lakes;
        public bool bordersOnly = false;
        public int edgeSize = 1;
        public float riverWidth = 100;
        public float scaleFactor = 1f;
        public string XMLMaskData;
        public float MinSize = 2000;
        public bool FixWithImage = false;


        protected override void Init()
        {
            base.Init();
            SetName("Bime Type Filter");
        }

        public override void CheckEssentioalInputs()
        {
            Input = GetInputValue<TXLandcoverModules>("Input");
            if (Input == null) throw new Exception("Input" + " is missed for " + name + " Node.");
        }

        protected override void ModuleAction(TMap CurrentMap)
        {
            _progress = 0;

            XmlDocument _landcoverXML = Input.GetProceededLandcover(CurrentMap._refTerrain);
            OutMasks = new List<TMask>();

            if (IsActive)
            {
                switch (biomeType)
                {
                    case BiomeTypes.Waters:
                        {
                            List<T2DObject> lakes = new List<T2DObject>();
                            OSMParser.ExtractLakes(_landcoverXML, ref lakes, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref lakes, MinSize);
                            List<TMask> lakesMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, lakes, bordersOnly, edgeSize, scaleFactor, true);
                            if (lakesMasks?.Count > 0) OutMasks.Add(lakesMasks[0]);
                            List<T2DObject> Oceans = new List<T2DObject>();
                            OSMParser.ExtractOceans(_landcoverXML, ref Oceans, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref Oceans, MinSize);
                            List<TMask> OceanMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, Oceans, bordersOnly, edgeSize, scaleFactor, true);
                            if (OceanMasks?.Count > 0) OutMasks.Add(OceanMasks[0]);
                            List<TLinearObject> rivers = new List<TLinearObject>();
                            OSMParser.ExtractRivers(_landcoverXML, ref rivers, CurrentMap._area);
                            List<TMask> RiverMasks = TLandcoverProccessor.GetBiomesMasksLinear(CurrentMap._refTerrain, rivers, bordersOnly, edgeSize, scaleFactor, riverWidth / 2f, true);
                            if (RiverMasks?.Count > 0) OutMasks.Add(RiverMasks[0]);
                            TMask allmasks = TMask.MergeMasks(OutMasks);
                            OutMasks.Clear();
                            OutMasks.Add(allmasks);
                        }
                        break;

                    case BiomeTypes.Lakes:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractLakes(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Sea:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractOceans(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Trees:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractForest(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Wood:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractWood(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Meadow:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractMeadow(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Orchard:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractOrchard(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Grass:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractGrass(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Greenfield:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractGreenField(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.River:
                        {
                            TLinearMeshLayer _Biomeslayer = new TLinearMeshLayer();
                            List<TLinearObject> rivers = _Biomeslayer.Lines;
                            OSMParser.ExtractRivers(_landcoverXML, ref rivers, CurrentMap._area);
                            //TLandcoverProccessor.FilterRiverBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer);
                            OutMasks = TLandcoverProccessor.GetBiomesMasksLinear(CurrentMap._refTerrain, rivers, bordersOnly, edgeSize, scaleFactor, riverWidth / 2f, true);
                        }
                        break;

                    case BiomeTypes.Wetland:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractWetland(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Beach:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractBeach(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Bay:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractBay(_landcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;
                }

                _progress = 1;
            }

        }
    }
}
#endif