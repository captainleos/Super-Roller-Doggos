#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;


namespace TerraUnity.Edittime
{
    public enum BiomeMasks
    {
        Biome_Type_Filter,
        Area_Mixer
    }

    public enum BiomeScatters
    {
        Terrain_Tree_Scatter,
        Object_Scatter,
        GPU_Instance_Scatter,
        Grass_Scatter
    }

    public enum BiomeMeshGenerators
    {
        Water_Generator,
        Terrain_Mesh_Generator
    }

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


    // Entry
    //---------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TWaterModules : TNode
    {
        [XmlIgnore] public TLakeLayer _lakeLayer;
        [XmlIgnore] public TRiverLayer _riverLayer;
        [XmlIgnore] public TOceanLayer _oceanLayer;

        public TWaterModules() : base()
        {
        }
    }

    public abstract class TGridModules : TNode
    {
        [XmlIgnore] public TGridLayer _gridLayer;

        public TGridModules() : base()
        {
        }
    }

    public abstract class TScatteredObjectModules : TNode
    {
        [XmlIgnore] public TObjectScatterLayer _objectScatterLayer;

        public TScatteredObjectModules() : base()
        {
        }
    }

    public abstract  class TScatteredInstanceModules : TNode
    {
        [XmlIgnore] public TInstanceScatterLayer _instanceScatterLayer;

        public TScatteredInstanceModules() : base()
        {
        }
    }

    public abstract class TScatteredGrassModules : TNode
    {
        [XmlIgnore] public TGrassScatterLayer _grassScatterLayer;

        public TScatteredGrassModules() : base()
        {
        }
    }


    // Extractors
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    [XmlType("BiomeExtractor")]
    //public class BiomeExtractor : TMultiMaskModules
    public class BiomeExtractor : TNode
    {
        public BiomeTypes biomeType = BiomeTypes.Lakes;
        public bool bordersOnly = false;
        public int edgeSize = 1;
        public float riverWidth = 100;
        public float scaleFactor = 1f;
        public string XMLMaskData;
        public float MinSize = 2000;
        public bool FixWithImage = false;
        

        [XmlIgnore] private TMap _currentMap;

        public BiomeExtractor() : base()
        {
         //   type = typeof(BiomeExtractor).FullName;
            Data.moduleType = ModuleType.Extractor;
            Data.name = "Biome Filter ";
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.Mask;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap CurrentMap)
        {
            if (isDone) return;
            _progress = 0;

            _currentMap = CurrentMap;
            OutMasks.Clear();

            if (isActive)
            {
                //switch (biomeType)
                //{
                //    case BiomeTypes.Waters:
                //        {
                //            List<T2DObject> lakes = new List<T2DObject>();
                //            OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref lakes, areaBounds, CurrentMap._area);
                //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref lakes, MinSize);
                //            List<TMask> lakesMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, lakes, bordersOnly, edgeSize, scaleFactor, true);
                //            if (lakesMasks?.Count > 0)  OutMasks.Add(lakesMasks[0]);
                //            List<T2DObject> Oceans = new List<T2DObject>();
                //            OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref Oceans, areaBounds, CurrentMap._area);
                //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref Oceans, MinSize);
                //            List<TMask> OceanMasks =TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, Oceans, bordersOnly, edgeSize, scaleFactor, true);
                //            if (OceanMasks?.Count > 0) OutMasks.Add(OceanMasks[0]);
                //            List<TLinearObject> rivers = new List<TLinearObject>();
                //            OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, areaBounds, CurrentMap._area);
                //            List<TMask> RiverMasks = TLandcoverProccessor.GetBiomesMasksLinear(_currentMap._refTerrain, rivers, bordersOnly, edgeSize, scaleFactor, riverWidth / 2f, true);
                //            if (RiverMasks?.Count > 0) OutMasks.Add(RiverMasks[0]);
                //            TMask allmasks = TMask.MergeMasks(OutMasks);
                //            OutMasks.Clear();
                //            OutMasks.Add(allmasks);
                //        }
                //        break;
                //
                //
                //    case BiomeTypes.Lakes:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.Sea:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.Trees:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractForest(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.Wood:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractWood(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.Meadow:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractMeadow(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.Orchard:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractOrchard(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.Grass:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractGrass(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.Greenfield:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractGreenField(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.River:
                //        {
                //            TLinearMeshLayer _Biomeslayer = new TLinearMeshLayer();
                //            List<TLinearObject> rivers = _Biomeslayer.Lines;
                //            OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, areaBounds, CurrentMap._area);
                //            //TLandcoverProccessor.FilterRiverBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer);
                //            TLandcoverProccessor.FilterRiverBordersByBoundArea(CurrentMap, ref _Biomeslayer);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasksLinear(_currentMap._refTerrain, rivers, bordersOnly, edgeSize, scaleFactor, riverWidth / 2f, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.Wetland:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractWetland(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.Beach:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractBeach(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //
                //    case BiomeTypes.Bay:
                //        {
                //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                //            OSMParser.ExtractBay(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
                //            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                //        }
                //        break;
                //}

                switch (biomeType)
                {
                    case BiomeTypes.Waters:
                        {
                            List<T2DObject> lakes = new List<T2DObject>();
                            OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref lakes, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref lakes, MinSize);
                            List<TMask> lakesMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, lakes, bordersOnly, edgeSize, scaleFactor, true);
                            if (lakesMasks?.Count > 0) OutMasks.Add(lakesMasks[0]);
                            List<T2DObject> Oceans = new List<T2DObject>();
                            OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref Oceans, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref Oceans, MinSize);
                            List<TMask> OceanMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, Oceans, bordersOnly, edgeSize, scaleFactor, true);
                            if (OceanMasks?.Count > 0) OutMasks.Add(OceanMasks[0]);
                            List<TLinearObject> rivers = new List<TLinearObject>();
                            OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, CurrentMap._area);
                            List<TMask> RiverMasks = TLandcoverProccessor.GetBiomesMasksLinear(_currentMap._refTerrain, rivers, bordersOnly, edgeSize, scaleFactor, riverWidth / 2f, true);
                            if (RiverMasks?.Count > 0) OutMasks.Add(RiverMasks[0]);
                            TMask allmasks = TMask.MergeMasks(OutMasks);
                            OutMasks.Clear();
                            OutMasks.Add(allmasks);
                        }
                        break;

                    case BiomeTypes.Lakes:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Sea:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Trees:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractForest(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Wood:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractWood(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Meadow:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractMeadow(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Orchard:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractOrchard(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Grass:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractGrass(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Greenfield:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractGreenField(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.River:
                        {
                            TLinearMeshLayer _Biomeslayer = new TLinearMeshLayer();
                            List<TLinearObject> rivers = _Biomeslayer.Lines;
                            OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, CurrentMap._area);
                            //TLandcoverProccessor.FilterRiverBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer);
                            OutMasks = TLandcoverProccessor.GetBiomesMasksLinear(_currentMap._refTerrain, rivers, bordersOnly, edgeSize, scaleFactor, riverWidth / 2f, true);
                        }
                        break;

                    case BiomeTypes.Wetland:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractWetland(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Beach:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractBeach(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;

                    case BiomeTypes.Bay:
                        {
                            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                            OSMParser.ExtractBay(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                            OutMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                        }
                        break;
                }

                _progress = 1;
            }

            isDone = true;
        }
    }


    // Scatters
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    [XmlType("TreeScatter")]
    public class TreeScatter : TScatteredObjectModules
    {
        public string prefabName;
        public int seedNo;
        //public int densityResolutionPerKilometer = 500;
        public bool bypassLakes = true;
        public bool underLakes = false;
        public bool underLakesMask = false;
        public bool onLakes = false;
        public float minRotationRange = 0f;
        public float maxRotationRange = 359f;
        public float positionVariation = 100f;
        public Vector3 scaleMultiplier = Vector3.One;
        public float minScale = 0.8f;
        public float maxScale = 1.5f;
        public float minSlope = 0;
        public float maxSlope = 90;
        public int priority = 0;
        public Vector3 objectScale = Vector3.One;
        public float minRange = 0;
        public float maxRange = 1;
        //public string unityLayerName = "Default";
        public int maskLayer = ~0;
        //public string layerName;
        public bool isWorldOffset = true;

        public float averageDistance = 10f;
        public bool checkBoundingBox = false;
        public float maxElevation = 100000;
        public float minElevation = -100000;

        public TreeScatter() : base()
        {
           // type = typeof(TreeScatter).FullName;
            Data.moduleType = ModuleType.Scatter;
            Data.name = "Terrain Tree Scatter" ;
            //layerName = Data.name;
            seedNo = Data.ID;
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.ObjectScatter;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(prefabName) && File.Exists(Path.GetFullPath(prefabName))) result.Add(prefabName);
            else result.Add(null);

            return result;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            TMask _mask = null;
            float[,] _maskData = null;
            _objectScatterLayer = null;

            if (isActive)
            {
                if (string.IsNullOrEmpty(prefabName) || !File.Exists(Path.GetFullPath(prefabName))) throw new Exception("Missing Prefab Selected For " + Data.name + "\n\n Please Check The Node.");
                
                if (inputConnections[0].previousNodeID != -1)
                {
                    TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

                    if (preNode != null)
                    {
                        _mask = TMask.MergeMasks(preNode.OutMasks);
                        _mask = _mask?.FilteredMask(minRange, maxRange);
                    }

                    if (_mask == null ) return;
                    _maskData = _mask.MaskData;
                }

                _objectScatterLayer = new TObjectScatterLayer(true);
                _objectScatterLayer.SeedNo = seedNo;
                //_objectScatterLayer.DensityResolutionPerKilometer = densityResolutionPerKilometer;
                _objectScatterLayer.bypassLake = bypassLakes;
                _objectScatterLayer.underLake = underLakes;
                _objectScatterLayer.underLakeMask = underLakesMask;
                _objectScatterLayer.onLake = onLakes;
                _objectScatterLayer.MaxRotationRange = (int)maxRotationRange;
                _objectScatterLayer.MinRotationRange = (int)minRotationRange;
                _objectScatterLayer.PositionVariation = positionVariation;
                _objectScatterLayer.ScaleMultiplier = scaleMultiplier;
                _objectScatterLayer.MaxScale = maxScale;
                _objectScatterLayer.MinScale = minScale;
                _objectScatterLayer.layerName = Data.name;
                //_objectScatterLayer.MaxElevation = areaBounds.maxElevation;
                //_objectScatterLayer.MinElevation = areaBounds.minElevation;
                _objectScatterLayer.MaxSlope = maxSlope;
                _objectScatterLayer.MinSlope = minSlope;
                _objectScatterLayer.Priority = priority;
                //_objectScatterLayer.UnityLayerName = unityLayerName;
                _objectScatterLayer.UnityLayerMask = maskLayer;
                _objectScatterLayer.useLayer = true;
                _objectScatterLayer.prefabNames.Add(prefabName);

                _objectScatterLayer.maskData = _maskData;
                _objectScatterLayer.averageDistance = averageDistance;
                _objectScatterLayer.checkBoundingBox = checkBoundingBox;
                _objectScatterLayer.MaxElevation = maxElevation;
                _objectScatterLayer.MinElevation = minElevation;

                //TLandcoverProccessor.ObjectScatter(currentMap._refTerrain, _mask, ref _objectScatterLayer, areaBounds);
                //TLandcoverProccessor.ObjectScatter(currentMap._refTerrain, _mask, ref _objectScatterLayer);

                _progress = 1;
            }

            isDone = true;
        }
    }


    [XmlType("ObjectScatter")]
    public class ObjectScatter : TScatteredObjectModules
    {
        public List<string> prefabNames;
        public int seedNo;
        //public int densityResolutionPerKilometer = 100;
        public bool rotation90Degrees = false;
        public bool bypassLakes = true;
        public bool underLakes = false;
        public bool underLakesMask = false;
        public bool onLakes = false;
        public bool lockYRotation = false;
        public bool getSurfaceAngle = false;
        public float minRotationRange = 0f;
        public float maxRotationRange = 359f;
        public float positionVariation = 100f;
        public Vector3 scaleMultiplier = Vector3.One;
        public float minScale = 0.8f;
        public float maxScale = 1.5f;
        public bool hasCollider = true;
        public bool hasPhysics = false;
        public string unityLayerName = "Default";
        public int maskLayer = ~0;
        public string layerName;
        public float minSlope = 0;
        public float maxSlope = 90;
        public Vector3 positionOffset = Vector3.Zero;
        public Vector3 rotationOffset = Vector3.Zero;
        public int priority = 0;
        public List<ObjectBounds> bounds;
        public List<Vector3> objectScales;
        public float minRange = 0;
        public float maxRange = 1;
        public bool isWorldOffset = true;

        public float averageDistance = 10f;
        public bool checkBoundingBox = false;
        public float maxElevation = 100000;
        public float minElevation = -100000;

        public ObjectScatter() : base()
        {
           // type = typeof(ObjectScatter).FullName;
            Data.name = "Object Scatter";
            prefabNames = new List<string>();
            //prefabNames.Add("");
            layerName = Data.name;
            seedNo = Data.ID;
            Data.moduleType = ModuleType.Scatter;
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.ObjectScatter;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();

            if (prefabNames.Count == 0)
                result.Add(null);
            else
            {
                for (int i = 0; i < prefabNames.Count; i++)
                {
                    if (!string.IsNullOrEmpty(prefabNames[i]) && !string.IsNullOrEmpty(prefabNames[i]) && File.Exists(Path.GetFullPath(prefabNames[i]))) result.Add(prefabNames[i]);
                    else result.Add(null);
                }
            }

            return result;
        }

        public override void ModuleAction (TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            TMask _mask = null;
            float[,] _maskData = null;
            _objectScatterLayer = null;

            if (isActive)
            {
                if (prefabNames.Count < 1) throw new Exception("No Prefab selected for " + Data.name + "\n\n Please Check The Node.");

                for (int i = 0; i < prefabNames.Count; i++)
                    if (string.IsNullOrEmpty(prefabNames[i]) || !File.Exists(Path.GetFullPath(prefabNames[i]))) throw new Exception("Missing Prefab Selected For " + Data.name + "\n\n Please Check The Node.");

                if (inputConnections[0].previousNodeID != -1)
                {
                    TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

                    if (preNode != null)
                    {
                        _mask = TMask.MergeMasks(preNode.OutMasks);
                        _mask = _mask?.FilteredMask(minRange, maxRange);
                    }

                    if (_mask == null) return;
                    _maskData = _mask._maskData;
                }

                _objectScatterLayer = new TObjectScatterLayer(false);
                _objectScatterLayer.SeedNo = seedNo;
                //_objectScatterLayer.DensityResolutionPerKilometer = densityResolutionPerKilometer;
                _objectScatterLayer.rotation90Degrees = rotation90Degrees;
                _objectScatterLayer.bypassLake = bypassLakes;
                _objectScatterLayer.underLake = underLakes;
                _objectScatterLayer.underLakeMask = underLakesMask;
                _objectScatterLayer.onLake = onLakes;
                _objectScatterLayer.lockYRotation = lockYRotation;                                         
                _objectScatterLayer.getSurfaceAngle = getSurfaceAngle;
                _objectScatterLayer.MaxRotationRange = (int)maxRotationRange;
                _objectScatterLayer.MinRotationRange = (int)minRotationRange;                               
                _objectScatterLayer.PositionVariation = positionVariation;                                  
                _objectScatterLayer.ScaleMultiplier = scaleMultiplier;
                _objectScatterLayer.MaxScale = maxScale;
                _objectScatterLayer.MinScale = minScale;
                _objectScatterLayer.layerName = Data.name;
                _objectScatterLayer.HasCollider = hasCollider;
                _objectScatterLayer.HasPhysics = hasPhysics;
                //_objectScatterLayer.MaxElevation = areaBounds.maxElevation;
                //_objectScatterLayer.MinElevation = areaBounds.minElevation;
                _objectScatterLayer.MaxSlope = maxSlope;
                _objectScatterLayer.MinSlope = minSlope;
                _objectScatterLayer.Priority = priority;
                _objectScatterLayer.UnityLayerName = unityLayerName;
                _objectScatterLayer.UnityLayerMask = maskLayer;
                _objectScatterLayer.useLayer = true;
                _objectScatterLayer.Offset = positionOffset;
                _objectScatterLayer.RotationOffset = rotationOffset;
                _objectScatterLayer.prefabNames = prefabNames;

                _objectScatterLayer.maskData = _maskData;
                _objectScatterLayer.averageDistance = averageDistance;
                _objectScatterLayer.checkBoundingBox = checkBoundingBox;
                _objectScatterLayer.MaxElevation = maxElevation;
                _objectScatterLayer.MinElevation = minElevation;

                //TLandcoverProccessor.ObjectScatter(currentMap._refTerrain, _mask, ref _objectScatterLayer, areaBounds);
                //TLandcoverProccessor.ObjectScatter(currentMap._refTerrain, _mask, ref _objectScatterLayer);

                _progress = 1;
            }

            isDone = true;
        }
    }

    [XmlType("InstanceScatter")]
    public class InstanceScatter : TScatteredInstanceModules
    {
        public string prefabName;
        public int seedNo;
        //public List<string> LODNames;
        //public List<float> LODDistances = new List<float>();
        public float averageDistance = 10f;
        public int gridResolution = 100;
        public bool rotation90Degrees = false;
        public bool lockYRotation = false;
        public bool getSurfaceAngle = false;
        public float minRotationRange = 0f;
        public float maxRotationRange = 359f;
        public float positionVariation = 100f;
        public Vector3 scaleMultiplier = Vector3.One;
        public float minScale = 0.8f;
        public float maxScale = 1.5f;
        public string unityLayerName = "Default";
        public int maskLayer = ~0;
        public string layerName;
        public float minSlope = 0;
        public float maxSlope = 90;
        public Vector3 positionOffset = Vector3.Zero;
        public Vector3 rotationOffset = Vector3.Zero;
        public int priority = 0;
        public List<ObjectBounds> bounds;
        public float minRange = 0;
        public float maxRange = 1;
        public bool receiveShadows = true;
        public bool bypassLakes = true;
        public bool underLakes = false;
        public bool underLakesMask = false;
        public bool onLakes = false;
        public TShadowCastingMode shadowCastingMode = TShadowCastingMode.On;
        public float LODMultiplier = 1.0f;
        [XmlIgnore] private List<TMask> _masks = new List<TMask>();
        public bool isWorldOffset = true; //TODO: Ali should implement this
        public bool prefabHasCollider = false;
        public float maxDistance = 2000f;
        public float frustumMultiplier = 1.1f;
        public bool checkBoundingBox = false;
        public float maxElevation = 100000;
        public float minElevation = -100000;
        public bool occlusionCulling = true;

        public InstanceScatter() : base()
        {
          //  type = typeof(InstanceScatter).FullName;
            Data.moduleType = ModuleType.Scatter;
            Data.name = "GPU Instance Scatter";
            layerName = Data.name;
            seedNo = Data.ID;
            //lastNumber++;
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.InstanceScatter;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(prefabName) && File.Exists(Path.GetFullPath(prefabName))) result.Add(prefabName);
            else result.Add(null);

            return result;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            TMask _mask = null;
            float[,] _maskData = null;
            _instanceScatterLayer = null;

            if (isActive )
            {
                if (string.IsNullOrEmpty(prefabName) || !File.Exists(Path.GetFullPath(prefabName))) throw new Exception("Missing Prefab Selected For " + Data.name + "\n\n Please Check The Node.");

                if (inputConnections[0].previousNodeID != -1)
                {
                    TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

                    if (preNode != null)
                    {
                        _mask = TMask.MergeMasks(preNode.OutMasks);
                        _mask = _mask?.FilteredMask(minRange, maxRange);
                    }

                    if (_mask == null) return;
                    _maskData = _mask._maskData;
                }
                else
                    throw new Exception("Missing input for " + Data.name + "\n\n Please Check The Node.");

                _instanceScatterLayer = new TInstanceScatterLayer();
                _instanceScatterLayer.SeedNo = seedNo;
                _instanceScatterLayer.averageDistance = averageDistance;
                _instanceScatterLayer.rotation90Degrees = rotation90Degrees;
                _instanceScatterLayer.lockYRotation = lockYRotation;
                _instanceScatterLayer.getSurfaceAngle = getSurfaceAngle;
                _instanceScatterLayer.MaxRotationRange = (int)maxRotationRange;
                _instanceScatterLayer.MinRotationRange = (int)minRotationRange;
                _instanceScatterLayer.PositionVariation = positionVariation;
                _instanceScatterLayer.ScaleMultiplier = scaleMultiplier;
                _instanceScatterLayer.MinScale = minScale;
                _instanceScatterLayer.MaxScale = maxScale;
                _instanceScatterLayer.layerName = Data.name;
                //_instanceScatterLayer.MaxElevation = areaBounds.maxElevation;
                //_instanceScatterLayer.MinElevation = areaBounds.minElevation;
                _instanceScatterLayer.MaxElevation = maxElevation;
                _instanceScatterLayer.MinElevation = minElevation;
                _instanceScatterLayer.MaxSlope = maxSlope;
                _instanceScatterLayer.MinSlope = minSlope;
                _instanceScatterLayer.Priority = priority;
                _instanceScatterLayer.UnityLayerName = unityLayerName;
                _instanceScatterLayer.UnityLayerMask = maskLayer;
                _instanceScatterLayer.useLayer = true;
                _instanceScatterLayer.Offset = positionOffset;
                _instanceScatterLayer.RotationOffset = rotationOffset;
                _instanceScatterLayer.shadowCastingMode = shadowCastingMode;
                _instanceScatterLayer.receiveShadows = receiveShadows;
                _instanceScatterLayer.bypassLake = bypassLakes;
                _instanceScatterLayer.underLake = underLakes;
                _instanceScatterLayer.underLakeMask = underLakesMask;
                _instanceScatterLayer.onLake = onLakes;
                _instanceScatterLayer.prefabName = prefabName;
                //_instanceScatterLayer.LODNames = LODNames;
                //_instanceScatterLayer.LODDistances = LODDistances;
                // _instanceScatterLayer.ActiveAreaLeft = (areaBounds.left > currentMap._area._left ? areaBounds.left : currentMap._area._left);
                // _instanceScatterLayer.ActiveAreaRight = (areaBounds.right < currentMap._area._right ? areaBounds.right : currentMap._area._right);
                // _instanceScatterLayer.ActiveAreaTop = (areaBounds.top < currentMap._area._top ? areaBounds.top : currentMap._area._top);
                // _instanceScatterLayer.ActiveAreaBottom = (areaBounds.bottom > currentMap._area._bottom ? areaBounds.bottom : currentMap._area._bottom);

                _instanceScatterLayer.maskData = _maskData;
                //_instanceScatterLayer.mask = _mask;

                _instanceScatterLayer.HasCollider = prefabHasCollider;
                _instanceScatterLayer.maxDistance = maxDistance;
                _instanceScatterLayer.LODMultiplier = LODMultiplier;
                _instanceScatterLayer.gridResolution = gridResolution;
                _instanceScatterLayer.frustumMultiplier = frustumMultiplier;
                _instanceScatterLayer.checkBoundingBox = checkBoundingBox;
                _instanceScatterLayer.occlusionCulling = occlusionCulling;

                //TLandcoverProccessor.InstancePatchScatter(currentMap._refTerrain, ref _instanceScatterLayer, areaBounds);

                _progress = 1;
            }

            isDone = true;
        }
    }

    [XmlType("GrassScatter")]
    public class GrassScatter : TScatteredGrassModules
    {
        [XmlIgnore] private TMaterial material;
        //[XmlIgnore] private TMesh mesh;
        public int maxParallelJobCount = 50;
        public Vector2 scale = Vector2.One;
        public float radius = 300f;
        public float gridSize = 50f;
        public float slant = 0.2f;
        public float groundOffset = 0;
        public int amountPerBlock = 2000;
        public string unityLayerName = "Default";
        public string layerName;
        public float alphaMapThreshold = 0.2f;
        public float densityFactor = 0.2f;
        public BuilderType builderType = BuilderType.Quad;
        public NormalType normalType = NormalType.Up;
        public TShadowCastingMode shadowCastingMode = TShadowCastingMode.On;
        public int seedNo;
        public float minRange = 0;
        public float maxRange = 1;

        public string Materialpath { get => material.ObjectPath; set => material.ObjectPath = value; }
        public string Modelpath;
        public string MeshName;

        public bool layerBasedPlacement = false;
        public bool bypassWater = true;
        public bool underWater = false;
        public bool onWater = false;
        public int maskLayer = ~0;

        public GrassScatter() : base()
        {
          //  type = typeof(GrassScatter).FullName;
            Data.moduleType = ModuleType.Scatter;
            Data.name = "Grass Scatter";
            layerName = Data.name;
            seedNo = Data.ID;
            material = new TMaterial();
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.GrassScatter;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();
            
            if (!string.IsNullOrEmpty(Materialpath) && File.Exists(Path.GetFullPath(Materialpath))) result.Add(Materialpath);
            else result.Add(null);

            if (!string.IsNullOrEmpty(Modelpath) && File.Exists(Path.GetFullPath(Modelpath))) result.Add(Modelpath);
            else result.Add(null);

            return result;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            TMask _mask = null;
            float[,] _maskData = null;
            _grassScatterLayer = null;

            if (isActive)
            {
                if (string.IsNullOrEmpty(Materialpath) || !File.Exists(Path.GetFullPath(Materialpath)))
                    throw new Exception("No Material Selected For " + Data.name + "\n\n Please Check The Node.");

                if (inputConnections[0].previousNodeID != -1)
                {
                    TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

                    if (preNode != null)
                    {
                        _mask = TMask.MergeMasks(preNode.OutMasks);
                        _mask = _mask?.FilteredMask(minRange, maxRange);
                    }

                    if (_mask == null) return;

                    _maskData = _mask._maskData;
                }
                else
                    throw new Exception("Missing input for " + Data.name + "\n\n Please Check The Node.");

                _grassScatterLayer = new TGrassScatterLayer();
                _grassScatterLayer.maxParallelJobCount = maxParallelJobCount;
                _grassScatterLayer.material = material;
                _grassScatterLayer.meshName = MeshName;
                _grassScatterLayer.modelPath = Modelpath;
                _grassScatterLayer.scale = scale;
                _grassScatterLayer.radius = radius;
                _grassScatterLayer.gridSize = gridSize;
                _grassScatterLayer.slant = slant;
                _grassScatterLayer.amountPerBlock = amountPerBlock;
                _grassScatterLayer.alphaMapThreshold = alphaMapThreshold;
                _grassScatterLayer.densityFactor = densityFactor;
                _grassScatterLayer.builderType = builderType;
                _grassScatterLayer.normalType = normalType;
                _grassScatterLayer.groundOffset = groundOffset;
                _grassScatterLayer.shadowCastingMode = shadowCastingMode;
                _grassScatterLayer.seedNo = seedNo;
                _grassScatterLayer.unityLayerName = unityLayerName;
                _grassScatterLayer.layerName = Data.name;

                _grassScatterLayer.maskData = _maskData;
                //_grassScatterLayer.mask = _mask;

                _grassScatterLayer.layerBasedPlacement = layerBasedPlacement;
                _grassScatterLayer.UnityLayerMask = maskLayer;
                _grassScatterLayer.bypassWater = bypassWater;
                _grassScatterLayer.underWater = underWater;
                _grassScatterLayer.onWater = onWater;

                _progress = 1;
            }

            isDone = true;
        }

        private int FirstNthDigits(int input, int digits)
        {
            int length = (int)Math.Floor(Math.Log10(input) + 1);
            if (digits > length) digits = length;
            char[] chars = new char[digits];
            for (int i = 0; i < digits; i++) chars[i] = input.ToString()[i];
            string result = new string(chars);
            return int.Parse(result);
        }
    }

    // Mesh Generators
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    [XmlType("WaterGenerator")]
    public class WaterGenerator : TWaterModules
    {
        [XmlIgnore] private TMaterial material;
        public string Materialpath { get => material.ObjectPath; set => material.ObjectPath = value; }

        public string layerName;
        public float lodCulling = 25f;
        public int AroundPointsDensity;
        public float AroundVariation;
        public string unityLayerName;
        public string XMLMaskData;
        public Vector3 positionOffset = Vector3.Zero;
        public int priority = 0;
        public bool GenerateLakes = true;
        public bool GenerateOceans = true;
        public bool GenerateRiver = true;
        public float RiverWidthInMeter = 150;
        public float Depth = 0.5f;
        public float LakeMinSizeInM2 = 20000;
        public List<Vector2> boundingPoints;
        public bool smoothOperation = true;
        public float deformAngle = 10f;
        
        [XmlIgnore] private TMap _currentMap;
        [XmlIgnore] public Bitmap lakeAroundMask;
        [XmlIgnore] public Bitmap maskImage;

        public WaterGenerator() : base()
        {
            //type = typeof(WaterGenerator).FullName;
            Data.moduleType = ModuleType.TerraMesh;
            Data.name = "Water Generator";
            layerName = "Water";
            unityLayerName = "Water";
            material = new TMaterial();
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.Lakes;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(Materialpath) && File.Exists(Path.GetFullPath(Materialpath))) result.Add(Materialpath);
            else result.Add(null);
            return result;
        }

        public override void ModuleAction(TMap CurrentMap)
        {
            if (isDone) return;
            _progress = 0;

            _currentMap = CurrentMap;
            _lakeLayer = null;
            _riverLayer = null;
            _oceanLayer = null;
            Vector2 centerPointNormalized = Vector2.Zero;
            float deformAngleRadian = deformAngle * (float)Math.PI / 180f;

            if (isActive)
            {
                if (string.IsNullOrEmpty(Materialpath) || !File.Exists(Path.GetFullPath(Materialpath)))
                    throw new Exception("No Material Selected For " + Data.name + "\n\n Please Check The Node.");

                if (GenerateLakes)
                {
                    _lakeLayer = new TLakeLayer();
                    _lakeLayer.AroundPointsDensity = AroundPointsDensity;
                    _lakeLayer.AroundVariation = AroundVariation;
                    _lakeLayer.LayerName = Data.name;
                    _lakeLayer.material = material;
                    //_lakeLayer.MaxElevation = areaBounds.maxElevation;
                    //_lakeLayer.MinElevation = areaBounds.minElevation;
                    _lakeLayer.Priority = priority;
                    _lakeLayer.UnityLayerName = unityLayerName;
                    _lakeLayer.useLayer = true;
                    _lakeLayer.Offset = positionOffset;
                    _lakeLayer.depth = Depth;
                    _lakeLayer.LODCulling = lodCulling;
                    _lakeLayer._minSizeInM2 = LakeMinSizeInM2;

                    //CurrentMap.LandCoverImage.Image.GetPixel();

                    List<T2DObject> lakes = _lakeLayer.LakesList;

                    //OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref lakes, areaBounds, CurrentMap._area);
                    //TLandcoverProccessor.FilterLakesBordersByBoundArea(areaBounds, CurrentMap, ref _lakeLayer, LakeMinSizeInM2);
                    //_lakeLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, lakes, false, 1, 1f, false);

                    OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref lakes, CurrentMap._area);
                    TLandcoverProccessor.FilterLakesBordersByBoundArea(CurrentMap, ref _lakeLayer, LakeMinSizeInM2);
                    _lakeLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, lakes, false, 1, 1f, false);

                    if (_lakeLayer?.WaterMasks?.Count > 0)
                    {

                        for(int i = 0; i < _lakeLayer.WaterMasks.Count; i++)
                        {
                            TMask mask = _lakeLayer.WaterMasks[i];
                            THeightmapProcessors.DeformWaterByMask(ref _currentMap._refTerrain.Heightmap.heightsData, mask, false, null, 2,
                                                                   TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat,
                                                                   deformAngleRadian, out lakes[i].minHeight,out lakes[i].maxHeight, out lakes[i].avgHeight);

                            //   TDebug.LogInfoToUnityUI("THeightmapProcessors.GetminValue");
                            //    THeightmapProcessors.GetminValue(ref _currentMap._refTerrain.Heightmap.heightsData, mask, out lakes[i].minHeight, 
                            //                                                                    out lakes[i].maxHeight, out lakes[i].avgHeight);
                            //mask.GetBWImage().Save("../Masks/" + lakes[i].name + ".jpg");
                        }
                    }

                 //  if (smoothOperation)
                 //   {
                 //       List<TMask> masks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, lakes, false, 1, 1f, true);
                 //       if (masks.Count > 0)
                 //           THeightmapProcessors.SmoothWaterByMask(ref _currentMap._refTerrain.Heightmap.heightsData, masks[0],
                 //                                              TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat, 20);
                 //   }
                }

                if (GenerateOceans)
                {
                    _oceanLayer = new TOceanLayer();
                    _oceanLayer.LayerName = Data.name;
                    _oceanLayer.material = material;
                    //_oceanLayer.MaxElevation = areaBounds.maxElevation;
                    //_oceanLayer.MinElevation = areaBounds.minElevation;
                    _oceanLayer.Priority = priority;
                    _oceanLayer.UnityLayerName = unityLayerName;
                    _oceanLayer.useLayer = true;
                    _oceanLayer.Offset = positionOffset;
                    _oceanLayer.depth = Depth;
                    _oceanLayer.LODCulling = lodCulling;
                    List<T2DObject> Oceans = _oceanLayer.Coastlines;

                    //OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref Oceans, areaBounds, CurrentMap._area);
                    //TLandcoverProccessor.FilterOceansBordersByBoundArea(areaBounds, CurrentMap, ref _oceanLayer);
                    //_oceanLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, Oceans, false, 0, 1f, true);

                    OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref Oceans, CurrentMap._area);
                    _oceanLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, Oceans, false, 0, 1f, true);

                    if (_oceanLayer.WaterMasks.Count > 0)
                    {
                        TMask mask = _oceanLayer.WaterMasks[0];
                        T2DObject ocean = _oceanLayer.Coastlines[0];
                        THeightmapProcessors.DeformWaterByMask(ref _currentMap._refTerrain.Heightmap.heightsData, mask, true, null, 2,
                                                               TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat,
                                                               deformAngleRadian, out ocean.minHeight, out ocean.maxHeight, out ocean.avgHeight);
                    //    if (smoothOperation)
                    //    {
                    //        THeightmapProcessors.SmoothWaterByMask(ref _currentMap._refTerrain.Heightmap.heightsData, mask,
                    //                                                          TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat);
                    //    }

                      //  THeightmapProcessors.GetminValue(ref _currentMap._refTerrain.Heightmap.heightsData, mask, out ocean.minHeight, 
                       //                                                                         out ocean.maxHeight, out ocean.avgHeight );
                        //mask.GetBWImage().Save("../Masks/" + ocean.name + ".jpg");
                    }
                }

                if (GenerateRiver)
                {
                    _riverLayer = new TRiverLayer();
                    _riverLayer.LayerName = Data.name;
                    _riverLayer.material = material;
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

                    //OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, areaBounds, CurrentMap._area);
                    //TLinearMeshLayer river = (TLinearMeshLayer)_riverLayer;
                    //TLandcoverProccessor.FilterRiverBordersByBoundArea(areaBounds, CurrentMap, ref river);

                    OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, CurrentMap._area);
                    //TLinearMeshLayer river = (TLinearMeshLayer)_riverLayer;
                    //TLandcoverProccessor.FilterRiverBordersByBoundArea(CurrentMap, ref river);

                    _riverLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasksLinear(_currentMap._refTerrain, rivers, false, 0, 1f, (RiverWidthInMeter) / 2f, true);

                    if (_riverLayer?.WaterMasks?.Count > 0)
                    {
                        for(int i = 0; i < _riverLayer.WaterMasks.Count; i++)
                        {
                            TMask mask = _riverLayer.WaterMasks[i];
                            THeightmapProcessors.DeformByMask(ref _currentMap._refTerrain.Heightmap.heightsData, mask, _riverLayer.depth + 4, 
                                                                                                                            false, null, 1);
                            //mask.GetBWImage().Save("../Masks/" + river[i].name + ".jpg");
                        }
                    }

                    if (smoothOperation)
                    {
                    //   List<TMask> masks = TLandcoverProccessor.GetBiomesMasksLinear(_currentMap._refTerrain, rivers, false, 0, 1f, (RiverWidthInMeter) / 2f, true);
                    //   
                    //   if (masks.Count > 0)
                        _currentMap._refTerrain.Heightmap.heightsData = THeightmapProcessors.SmoothHeightmap(
                                                _currentMap._refTerrain.Heightmap.heightsData, 1, 0, THeightmapProcessors.Neighbourhood.Moore);
                    }
                }

                /*
                if (_riverLayer?.WaterMasks?.Count > 0 || _lakeLayer?.WaterMasks?.Count > 0 || _oceanLayer?.WaterMasks?.Count > 0)
                {
                    //  THeightmapProcessors.DeformByMask(ref _currentMap._refTerrain.Heightmap.heightsData, Deformmask, Depth * 1.5f, false, null);
                    if (smoothOperation)
                    {
                        _currentMap._refTerrain.Heightmap.heightsData = THeightmapProcessors.SmoothHeightmap(null, 
                                                _currentMap._refTerrain.Heightmap.heightsData, 1, 0, THeightmapProcessors.Neighbourhood.Moore);
                    }
                }*/

                _progress = 1;
            }

            isDone = true;
        }
    }


    [XmlType("MeshGenerator")]
    public class MeshGenerator : TGridModules
    {
        //private string materialpath;
        public string Materialpath { get => material.ObjectPath; set => material.ObjectPath = value; }
        public int densityResolutionPerKilometer = 5;
        public int vertexCount = 10000;
        public int density = 90;
        public float edgeCurve = -1;
        public int gridCount = 16;
        public float lodCulling = 25f;
        public Vector3 scale = Vector3.One;
        public string unityLayerName = "Default";
        public bool hasCollider = true;
        public bool hasPhysics = false;
        public bool SeperatedObject = false;
        public string layerName;
        public Vector3 positionOffset = Vector3.Zero;
        public int priority = 0;
        [XmlIgnore] private TMaterial material;
        [XmlIgnore] public List<TMask> _masks;

        public MeshGenerator() : base()
        {
          //  type = typeof(MeshGenerator).FullName;
            Data.moduleType = ModuleType.TerraMesh;
            Data.name = "Terrain Mesh Generator";
            _masks = new List<TMask>();
            layerName = Data.name;
            material = new TMaterial();
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.Mesh;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(Materialpath) && File.Exists(Path.GetFullPath(Materialpath))) result.Add(Materialpath);
            else result.Add(null);

            return result;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;
            _gridLayer = null;

            if (isActive)
            {
                if (string.IsNullOrEmpty(Materialpath) || !File.Exists(Path.GetFullPath(Materialpath)))
                    throw new Exception("No Material Selected For " + Data.name);

                if (inputConnections[0].previousNodeID != -1)
                {
                    TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

                    if (preNode != null)
                        _masks = preNode.OutMasks;

                    if (_masks.Count < 1) return;
                }

                _gridLayer = new TGridLayer();
                _gridLayer.HasCollider = hasCollider;
                _gridLayer.HasPhysics = hasPhysics;
                _gridLayer.KM2Resulotion = densityResolutionPerKilometer;
                _gridLayer.LayerName = Data.name;
                _gridLayer.material = material;
                _gridLayer.MinElevation = -100000;
                _gridLayer.MaxElevation = 100000;
                _gridLayer.MinSlope = 0;
                _gridLayer.MaxSlope = 90;
                _gridLayer.Priority = priority;
                _gridLayer.UnityLayerName = unityLayerName;
                _gridLayer.Offset = positionOffset;
                _gridLayer.Scale = scale;
                _gridLayer.layerName = layerName;
                _gridLayer.EdgeCurve = edgeCurve;
                _gridLayer.density = density;
                _gridLayer.LODCulling = lodCulling;

                //if (_masks == null)
                //    TLandcoverProccessor.GenerateGrid(currentMap._refTerrain, null, ref _gridLayer, areaBounds, gridCount, SeperatedObject);
                //else if (_masks.Count > 0)
                //    for (int i = 0; i < _masks.Count; i++)
                //        TLandcoverProccessor.GenerateGrid(currentMap._refTerrain, _masks[i], ref _gridLayer, areaBounds, gridCount, SeperatedObject);

                if (_masks == null)
                    TLandcoverProccessor.GenerateGrid(currentMap._refTerrain, null, ref _gridLayer, gridCount, SeperatedObject);
                else if (_masks.Count > 0)
                    for (int i = 0; i < _masks.Count; i++)
                        TLandcoverProccessor.GenerateGrid(currentMap._refTerrain, _masks[i], ref _gridLayer, gridCount, SeperatedObject);

                _progress = 1;
            }

            isDone = true;
        }
    }


    // Graph
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    public class TBiomesGraph : TGraph
    {
        //TODO: Check ConnectionDataType
        public TBiomesGraph() : base(ConnectionDataType.Lakes, "BIOMES Graph")
        {
        }

        public void InitGraph(TTerraWorldGraph terraWorldGraph)
        {

            worldGraph = terraWorldGraph;
            _title = "BIOMES";
        }

        public void AddNode(BiomeMasks biomeMasks)
        {
            if (biomeMasks.Equals(BiomeMasks.Biome_Type_Filter))
            {
                BiomeExtractor node = new BiomeExtractor();
                node.Init(this);
                node.Data.moduleType = ModuleType.Extractor;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            if (biomeMasks.Equals(BiomeMasks.Area_Mixer))
            {
                MaskBlendOperator node = new MaskBlendOperator();
                node.Init(this);
                node.Data.moduleType = ModuleType.Operator;
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }

            UpdateConnections();
        }

        public void AddNode(BiomeScatters biomeScatters)
        {
            if (biomeScatters.Equals(BiomeScatters.Object_Scatter))
            {
                ObjectScatter node = new ObjectScatter();
                node.Init(this);
                node.Data.moduleType = ModuleType.Scatter;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (biomeScatters.Equals(BiomeScatters.Terrain_Tree_Scatter))
            {
                TreeScatter node = new TreeScatter();
                node.Init(this);
                node.Data.moduleType = ModuleType.Scatter;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (biomeScatters.Equals(BiomeScatters.GPU_Instance_Scatter))
            {
                InstanceScatter node = new InstanceScatter();
                node.Init(this);
                node.Data.moduleType = ModuleType.Scatter;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (biomeScatters.Equals(BiomeScatters.Grass_Scatter))
            {
                GrassScatter node = new GrassScatter();
                node.Init(this);
                node.Data.moduleType = ModuleType.Scatter;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
        }

        public void AddNode(BiomeMeshGenerators biomeMeshGenerators)
        {
            if (biomeMeshGenerators.Equals(BiomeMeshGenerators.Water_Generator))
            {
                WaterGenerator node = new WaterGenerator();
                node.Init(this);
                node.Data.moduleType = ModuleType.TerraMesh;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (biomeMeshGenerators.Equals(BiomeMeshGenerators.Terrain_Mesh_Generator))
            {
                MeshGenerator node = new MeshGenerator();
                node.Init(this);
                node.Data.moduleType = ModuleType.TerraMesh;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
        }

        public bool AnyLandCoverDataNode ()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].GetType() == typeof(WaterGenerator)) return true;
                if (nodes[i].GetType() == typeof(BiomeExtractor)) return true;
            }

            return false;
        }
    }
}
#endif
#endif
