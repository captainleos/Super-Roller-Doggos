#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TerraUnity.Edittime
{
    public enum HeightmapProcessors
    {
        Heightmap_Source,
        Smoothen_Terrain,
        Water_Erosion_Filter,
        Terrace_Filter,
        Thermal_Erosion_Filter,
        Hydraulic_Erosion_Filter,
        Voxel_Creator,
        Terrain_Deformer
    }

    public enum HeightmapMasks
    {
        Flow_Way_Filter,
        Slope_Filter,
        Area_Mixer
    }

    public enum HydraulicErosionMethod
    {
        Normal,
        Ultimate
    }

    // Entry
    //---------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IMaskPreModules 
    {
        TMask GetPreMask(TMap CurrentMap);
    }

    public abstract class THeightmapModules : TNode
    {
        
        [XmlIgnore] public float[,] _heightmapData;

        public THeightmapModules() : base() {}
    }

    // Master Node
    [XmlType("HeightmapMaster")]
    public class HeightmapMaster : THeightmapModules
    {
        public int _pixelError = 5;

        public HeightmapMaster() : base()
        {
          //  type = typeof(HeightmapMaster).FullName;
            Data.moduleType = ModuleType.Master;
            Data.name = "Global Heightmap";
            isRemovable = false;
            isSource = false;
            Data.nodePosition = NodePosition._1;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Node") };
            outputConnectionType = ConnectionDataType.HeightmapMaster;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
            _heightmapData = preNode._heightmapData;
        }
    }

    // Raw heightmap data from mapping servers
    [XmlType("HeightmapSource")]
    public class HeightmapSource : THeightmapModules
    {
        public TMapManager.mapElevationSourceEnum _source = TMapManager.mapElevationSourceEnum.ESRI;
        public int _resolution = 1024;
        public bool highestResolution = true;
        public float elevationExaggeration = 1;

        public HeightmapSource() : base()
        {
          //  type = typeof(HeightmapSource).FullName;
            Data.moduleType = ModuleType.Processor;
            Data.name = "Heightmap Source";
            isRemovable = true;
            isSource = true;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.Heightmap;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction (TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            if (!isActive)
                _heightmapData = new float[_resolution, _resolution];
            else
            {
                _heightmapData = currentMap.Heightmap.heightsData.Clone() as float[,];

                if (elevationExaggeration != 1)
                    _heightmapData = THeightmapProcessors.ExaggerateHeightmap(_heightmapData, elevationExaggeration);

                if (!highestResolution)
                {
                    _heightmapData = THeightmapProcessors.ResampleHeightmap(_heightmapData, THeightmapProcessors.ResampleMode.DOWN, _resolution + 1);

                    int smoothIterration = (int)((_resolution + 1) * 1.0f / _heightmapData.GetLength(0));
                    _heightmapData = THeightmapProcessors.SmoothHeightmap( _heightmapData, smoothIterration);
                }

                _progress = 1;
            }

            isDone = true;
        }

    }

    // Processors
    //---------------------------------------------------------------------------------------------------------------------------------------------------

    [XmlType("SmoothProcess")]
    public class SmoothProcess : THeightmapModules
    {
        public int _steps = 1;
        public float _blending = 0.5f;
        public THeightmapProcessors.Neighbourhood _smoothMode = THeightmapProcessors.Neighbourhood.VonNeumann;

        public SmoothProcess() : base()
        {
          //  type = typeof(SmoothProcess).FullName;
            Data.moduleType = ModuleType.Processor;
            Data.name = "Smoothen Terrain";
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Node") };
            outputConnectionType = ConnectionDataType.Heightmap;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction (TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

            if (isActive)
            {
                _heightmapData = THeightmapProcessors.SmoothHeightmap( preNode._heightmapData, _steps, _blending, _smoothMode);
                _progress = 1;
            }
            else
                _heightmapData = preNode._heightmapData;

            isDone = true;
        }
    }

    [XmlType("HydraulicErosionMainProcess")]
    public class HydraulicErosionMainProcess : THeightmapModules
    {
        public int _iterationsUltimate = 50000;
        public HydraulicErosionMethod hydraulicErosionMethod = HydraulicErosionMethod.Ultimate;
        public int _iterations = 40;
        public float _rainAmount = 0.75f;
        public float _sediment = 0.05f;
        public float _evaporation = 0.75f;

        public UnityEngine.ComputeShader erosionCompute;

        public HydraulicErosionMainProcess() : base()
        {
          //  type = typeof(HydraulicErosionMainProcess).FullName;
            Data.moduleType = ModuleType.Processor;
            Data.name = "Hydraulic Erosion";
            //lastNumber++;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Node") };
            outputConnectionType = ConnectionDataType.Heightmap;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

            if (isActive)
            {
                switch (hydraulicErosionMethod)
                {
                    case HydraulicErosionMethod.Normal:
                        _heightmapData = THeightmapProcessors.HydraulicErosion( preNode._heightmapData, _iterations, _rainAmount, _sediment, _evaporation);
                        break;
                    case HydraulicErosionMethod.Ultimate:
                        //_heightmapData = THeightmapProcessors.HydraulicErosionComputeShader(this, preNode._heightmapData, erosionCompute, _iterationsUltimate);
                        _heightmapData = THeightmapProcessors.HydraulicErosionUltimate(this, preNode._heightmapData, _iterationsUltimate);
                        break;
                    default:
                        _heightmapData = preNode._heightmapData;
                        break;
                }
                
                _progress = 1;
            }
            else
                _heightmapData = preNode._heightmapData;

            isDone = true;
        }
    }

    [XmlType("HydraulicErosionProcess")]
    public class HydraulicErosionProcess : HydraulicErosionMainProcess
    {
        public HydraulicErosionProcess() : base()
        {
           // type = typeof(HydraulicErosionProcess).FullName;
            hydraulicErosionMethod = HydraulicErosionMethod.Normal;
        }
    }

    [XmlType("WaterErosionProcess")]
    public class WaterErosionProcess : THeightmapModules
    {
        public int _iterations = 40;
        public float _shape = 1f;
        public float _rivers = 0.0025f;
        public float _vertical = 0.5f;
        public float _seaBedCarve = 0.5f;

        public WaterErosionProcess() : base()
        {
           // type = typeof(WaterErosionProcess).FullName;
            Data.moduleType = ModuleType.Processor;
            Data.name = "Water Erosion";
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Node") };
            outputConnectionType = ConnectionDataType.Heightmap;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

            if (isActive)
            {
                _heightmapData = THeightmapProcessors.WaterErosion(this, preNode._heightmapData, _iterations, _shape, _rivers, _vertical, _seaBedCarve);
                _progress = 1;
            }
            else
                _heightmapData = preNode._heightmapData;

            isDone = true;
        }
    }

    [XmlType("ThermalErosionProcess")]
    public class ThermalErosionProcess : THeightmapModules
    {
        public int _iterations = 1;

        public ThermalErosionProcess() : base()
        {
          //  type = typeof(ThermalErosionProcess).FullName;
            Data.moduleType = ModuleType.Processor;
            Data.name = "Thermal Erosion";
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Node") };
            outputConnectionType = ConnectionDataType.Heightmap;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

            if (isActive)
            {
                _heightmapData = THeightmapProcessors.ThermalErosion(this, preNode._heightmapData, _iterations);
                _progress = 1;
            }
            else
                _heightmapData = preNode._heightmapData;

            isDone = true;
        }
    }

    [XmlType("TerraceProcess")]
    public class TerraceProcess : THeightmapModules
    {
        public int _terraceCount = 7;
        public float _strength = 0.7f;
        public float _terraceVariation = 0f;
        public float[] controlPoints;

        public TerraceProcess() : base()
        {
          //  type = typeof(TerraceProcess).FullName;
            Data.moduleType = ModuleType.Processor;
            Data.name = "Terrace Filter";
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Node") };
            outputConnectionType = ConnectionDataType.Heightmap;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

            if (isActive)
            {
                _heightmapData = THeightmapProcessors.Terrace(this, preNode._heightmapData, ref controlPoints, _terraceCount, _strength, _terraceVariation);
                _progress = 1;
            }
            else
                _heightmapData = preNode._heightmapData;

            isDone = true;
        }
    }

    [XmlType("VoxelProcess")]
    public class VoxelProcess : THeightmapModules
    {
        public int voxelSize = 7;

        public VoxelProcess() : base()
        {
          //  type = typeof(VoxelProcess).FullName;
            Data.moduleType = ModuleType.Processor;
            Data.name = "Voxel Terrain";
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Node") };
            outputConnectionType = ConnectionDataType.Heightmap;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

            if (isActive)
            {
                _heightmapData = THeightmapProcessors.VoxelizeHeightmap(this, preNode._heightmapData, voxelSize);
                _progress = 1;
            }
            else
                _heightmapData = preNode._heightmapData;

            isDone = true;
        }
    }


    // Masks
    //---------------------------------------------------------------------------------------------------------------------------------------------------

    //[XmlType("Slopemap")]
    //public class Slopemap : THeightmapModules, IMaskPreModules
    //{
    //    public static int lastNumber = 1;
    //
    //    public float _strength = 1;
    //    public float _widthMultiplier = 1;
    //    public float _heightMultiplier = 1;
    //    [XmlIgnore] public TMask _preMask;
    //
    //    public Slopemap() : base()
    //    {
    //        type = typeof(Slopemap).FullName;
    //        Data.name = "Slopemap " + lastNumber;
    //        //lastNumber++;
    //    }
    //
    //    public override void InitConnections()
    //    {
    //        inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Source") };
    //        outputConnectionType = ConnectionDataType.Mask;
    //    }
    //
    //    public override void ModuleAction (TMap currentMap)
    //    {
    //        if (isDone) return;
    //        _progress = 0;
    //
    //        OutMasks.Clear();
    //
    //        if (isActive)
    //        {
    //            TMask _mask;
    //            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
    //            _mask = THeightmapProcessors.CreateSlopeMask(this, preNode._heightmapData, currentMap._area._areaSizeLon * 1000, currentMap._area._areaSizeLat * 1000, _widthMultiplier, _heightMultiplier, _strength);
    //            OutMasks.Add(_mask);
    //            _progress = 1;
    //        }
    //
    //        isDone = true;
    //    }
    //
    //    public TMask GetPreMask(TMap currentMap)
    //    {
    //        if (isDone)
    //            return OutMasks[0];
    //        else
    //        {
    //            _preMask = THeightmapProcessors.CreateSlopeMask(this, currentMap.Heightmap.heightsData, currentMap._area._areaSizeLon * 1000, currentMap._area._areaSizeLat * 1000, _widthMultiplier, _heightMultiplier, _strength);
    //            return _preMask;
    //        }
    //    }
    //}

    [XmlType("Flowmap")]
    public class Flowmap : THeightmapModules , IMaskPreModules
    {
        public int _iterations = 5;
        public float _widthMultiplier = 1;
        public float _heightMultiplier = 1;
        public float minRange = 0.3f;
        public float maxRange = 1f;
        [XmlIgnore] public TMask _preMask;

        public Flowmap() : base()
        {
          //  type = typeof(Flowmap).FullName;
            Data.moduleType = ModuleType.Mask;
            Data.name = "Flow Way Filter";
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Node") };
            outputConnectionType = ConnectionDataType.Mask;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            OutMasks.Clear();

            if (isActive)
            {
                TMask _mask;
                THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
                _mask = THeightmapProcessors.CreateFlowMask(this, preNode._heightmapData, _widthMultiplier, _heightMultiplier, _iterations);
                _mask = _mask.FilteredMask(minRange, maxRange);
                OutMasks.Add(_mask);
                _progress = 1;
            }

            isDone = true;
        }

        public TMask GetPreMask(TMap currentMap)
        {

                _preMask = THeightmapProcessors.CreateFlowMask(this, currentMap.Heightmap.heightsData, _widthMultiplier, _heightMultiplier, _iterations);
                return _preMask;

        }
    }

    //[XmlType("Curvaturemap")]
    //public class Curvaturemap : THeightmapModules , IMaskPreModules
    //{
    //    public static int lastNumber = 1;
    //
    //    public float _limit = 10000;
    //    public float _widthMultiplier = 1;
    //    public float _heightMultiplier = 1;
    //    [XmlIgnore] public TMask _preMask;
    //
    //    public Curvaturemap() : base()
    //    {
    //        type = typeof(Curvaturemap).FullName;
    //        Data.name = "Curvature " + lastNumber;
    //        //lastNumber++;
    //    }
    //
    //    public override void InitConnections()
    //    {
    //        inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Source") };
    //        outputConnectionType = ConnectionDataType.Mask;
    //    }
    //
    //    public override void ModuleAction(TMap currentMap)
    //    {
    //        if (isDone) return;
    //        _progress = 0;
    //
    //        OutMasks.Clear();
    //
    //        if (isActive)
    //        {
    //            TMask _mask;
    //            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
    //            _mask = THeightmapProcessors.CreateCurvatureMask(this, preNode._heightmapData, _widthMultiplier, _heightMultiplier, _limit, THeightmapProcessors.CURVATURE_TYPE.AVERAGE);
    //            OutMasks.Add(_mask);
    //            _progress = 1;
    //        }
    //
    //        isDone = true;
    //    }
    //
    //    public TMask GetPreMask(TMap currentMap)
    //    {
    //        if (isDone)
    //            return OutMasks[0];
    //        else
    //        {
    //            _preMask = THeightmapProcessors.CreateCurvatureMask(this, currentMap.Heightmap.heightsData, _widthMultiplier, _heightMultiplier, _limit, THeightmapProcessors.CURVATURE_TYPE.AVERAGE);
    //            return _preMask;
    //        }
    //    }
    //}

    //[XmlType("Normalmap")]
    //public class Normalmap : THeightmapModules , IMaskPreModules
    //{
    //    public static int lastNumber = 1;
    //
    //    public float _strength = 1;
    //    public float _widthMultiplier = 1;
    //    public float _heightMultiplier = 1;
    //    [XmlIgnore] public TMask _preMask;
    //
    //    public Normalmap() : base()
    //    {
    //        type = typeof(Normalmap).FullName;
    //        Data.name = "Normalmap " + lastNumber;
    //        //lastNumber++;
    //    }
    //
    //    public override void InitConnections()
    //    {
    //        inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Source") };
    //        outputConnectionType = ConnectionDataType.Mask;
    //    }
    //
    //    public override void ModuleAction(TMap currentMap)
    //    {
    //        if (isDone) return;
    //        _progress = 0;
    //
    //        OutMasks.Clear();
    //
    //        if (isActive)
    //        {
    //            TMask _mask;
    //            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
    //            _mask = THeightmapProcessors.CreateNormalMask(this, preNode._heightmapData, _widthMultiplier, _heightMultiplier, _strength);
    //            OutMasks.Add(_mask);
    //            _progress = 1;
    //        }
    //
    //        isDone = true;
    //    }
    //
    //    public TMask GetPreMask(TMap currentMap)
    //    {
    //        if (isDone)
    //            return OutMasks[0];
    //        else
    //        {
    //            _preMask = THeightmapProcessors.CreateNormalMask(this, currentMap.Heightmap.heightsData, _widthMultiplier, _heightMultiplier, _strength);
    //            return _preMask;
    //        }
    //    }
    //}

    //[XmlType("Aspectmap")]
    //public class Aspectmap : THeightmapModules , IMaskPreModules
    //{
    //    public static int lastNumber = 1;
    //
    //    public float _widthMultiplier = 1;
    //    public float _heightMultiplier = 1;
    //    public THeightmapProcessors.ASPECT_TYPE _aspectType = THeightmapProcessors.ASPECT_TYPE.ASPECT;
    //    [XmlIgnore] public TMask _preMask;
    //
    //    public Aspectmap() : base()
    //    {
    //        type = typeof(Aspectmap).FullName;
    //        Data.name = "Aspectmap " + lastNumber;
    //        //lastNumber++;
    //    }
    //
    //    public override void InitConnections()
    //    {
    //        inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Source") };
    //        outputConnectionType = ConnectionDataType.Mask;
    //    }
    //
    //    public override void ModuleAction(TMap currentMap)
    //    {
    //        if (isDone) return;
    //        _progress = 0;
    //
    //        OutMasks.Clear();
    //
    //        if (isActive)
    //        {
    //            THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
    //            TMask _mask = THeightmapProcessors.CreateAspectMask(this, preNode._heightmapData, _widthMultiplier, _heightMultiplier, _aspectType);
    //            OutMasks.Add(_mask);
    //            _progress = 1;
    //        }
    //
    //        isDone = true;
    //    }
    //
    //    public TMask GetPreMask(TMap currentMap)
    //    {
    //        if (isDone)
    //            return OutMasks[0];
    //        else
    //        {
    //            _preMask = THeightmapProcessors.CreateAspectMask(this, currentMap.Heightmap.heightsData, _widthMultiplier, _heightMultiplier, _aspectType);
    //            return _preMask;
    //        }
    //    }
    //}

    [XmlType("Slopemask")]
    public class Slopemask : THeightmapModules, IMaskPreModules
    {
        public Vector3 scaleMultiplier =Vector3.One;
        public float MinSlope = 0;
        public float MaxSlope = 90;
        [XmlIgnore] public TMask _preMask = null;

        public Slopemask() : base()
        {
          //  type = typeof(Slopemask).FullName;
            Data.moduleType = ModuleType.Mask;
            Data.name = "Slope Filter";
            //lastNumber++;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Node") };
            outputConnectionType = ConnectionDataType.Mask;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            OutMasks.Clear();

            if (isActive)
            {
                THeightmapModules preNode = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
                THeightmap heightmap = new THeightmap(preNode._heightmapData);
                TMask _mask = THeightmapProcessors.GetSlopeMap(currentMap, heightmap, MaxSlope, MinSlope);
                OutMasks.Add(_mask);
                _progress = 1;
            }

            isDone = true;
        }

        public TMask GetPreMask(TMap currentMap)
        {
            THeightmap heightmap = new THeightmap(currentMap.Heightmap.heightsData);
            _preMask = THeightmapProcessors.GetSlopeMap(currentMap, heightmap, MaxSlope, MinSlope);
            return _preMask;
        }
    }


    // Operators
    //---------------------------------------------------------------------------------------------------------------------------------------------------

    [XmlType("ApplyMask")]
    public class ApplyMask : THeightmapModules
    {
        public float _depth = 10f;
        public bool _flat = true;

        public ApplyMask() : base()
        {
          //  type = typeof(ApplyMask).FullName;
            Data.moduleType = ModuleType.Operator;
            Data.name = "Terrain Deformer";
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap, true, "Heightmap Node"), new TConnection(null, this, ConnectionDataType.Mask, true, "Area Node") };
            outputConnectionType = ConnectionDataType.Heightmap;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            THeightmapModules preNodeHeightmap = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

            if (isActive)
            {
                preNodeHeightmap = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
                TNode preNodeMask = parentGraph.worldGraph.GetNodeByID(inputConnections[1].previousNodeID);
                _heightmapData = preNodeHeightmap._heightmapData.Clone() as float[,];
                TMask _mask = TMask.MergeMasks(preNodeMask.OutMasks);
                THeightmapProcessors.DeformByMask(ref _heightmapData, _mask, _depth, _flat, this, 0);
                _progress = 1;
            }
            else
                _heightmapData = preNodeHeightmap._heightmapData;

            isDone = true;
        }
    }


    // Graph
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    public class THeightmapGraph : TGraph
    {
        private static HeightmapMaster heightmapMaster;

        public THeightmapGraph() : base(ConnectionDataType.Heightmap, "Heighmap Graph")
        {
        }

        public void InitGraph(TTerraWorldGraph terraWorldGraph)
        {

            worldGraph = terraWorldGraph;
            _title = "HEIGHTMAP";

            if (nodes.Count > 0) return;

            heightmapMaster = new HeightmapMaster();
            heightmapMaster.Init(this);
            nodes.Add(heightmapMaster);
        }

        public HeightmapSource HeightmapSource()
        {
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].GetType() == typeof(HeightmapSource)) return (nodes[i] as HeightmapSource);

            return null;
        }

        public HeightmapMaster HeightmapMaster()
        {
            if (nodes.Count > 0)
            {
                heightmapMaster = ((HeightmapMaster)nodes[0]);
            }
            else
                throw new Exception("Heightmap Master Node Not Found! (Internal Error)");

            return heightmapMaster;
        }

        public int AddNode(HeightmapProcessors heightmapProcessors)
        {
            int _id = -1;
            if (heightmapProcessors.Equals(HeightmapProcessors.Heightmap_Source))
            {
                HeightmapSource node = new HeightmapSource();
                node.Init(this);
                nodes.Add(node);
                _id = node.Data.ID;
            }

            else if (heightmapProcessors.Equals(HeightmapProcessors.Hydraulic_Erosion_Filter))
            {
                HydraulicErosionMainProcess node = new HydraulicErosionMainProcess();
                node.Init(this);
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Smoothen_Terrain))
            {
                SmoothProcess node = new SmoothProcess();
                node.Init(this);
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Terrace_Filter))
            {
                TerraceProcess node = new TerraceProcess();
                node.Init(this);
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Thermal_Erosion_Filter))
            {
                ThermalErosionProcess node = new ThermalErosionProcess();
                node.Init(this);
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Water_Erosion_Filter))
            {
                WaterErosionProcess node = new WaterErosionProcess();
                node.Init(this);
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Terrain_Deformer))
            {
                ApplyMask node = new ApplyMask();
                node.Init(this);
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Voxel_Creator))
            {
                VoxelProcess node = new VoxelProcess();
                node.Init(this);
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }

            nodes[0].AddInputConnection(nodes[nodes.Count - 1], 0, -1);

            UpdateConnections();

            return _id;
        }

        public void AddNode(HeightmapMasks heightmapMasks)
        {
            if (heightmapMasks.Equals(HeightmapMasks.Slope_Filter))
            {
                Slopemask node = new Slopemask();
                node.Init(this);
                if (nodes.Count > 0)
                    node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (heightmapMasks.Equals(HeightmapMasks.Flow_Way_Filter))
            {
                Flowmap node = new Flowmap();
                node.Init(this);
                if (nodes.Count > 0)
                    node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (heightmapMasks.Equals(HeightmapMasks.Area_Mixer))
            {
                MaskBlendOperator node = new MaskBlendOperator();
                node.Init(this);
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            UpdateConnections();
        }

        // For New Graph System
        public int NewAddNode(HeightmapProcessors heightmapProcessors)
        {
            int _id = -1;
            if (heightmapProcessors.Equals(HeightmapProcessors.Heightmap_Source))
            {
                HeightmapSource node = new HeightmapSource();
                node.Init(this);
                nodes.Add(node);
                _id = node.Data.ID;
            }

            else if (heightmapProcessors.Equals(HeightmapProcessors.Hydraulic_Erosion_Filter))
            {
                HydraulicErosionMainProcess node = new HydraulicErosionMainProcess();
                node.Init(this);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Smoothen_Terrain))
            {
                SmoothProcess node = new SmoothProcess();
                node.Init(this);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Terrace_Filter))
            {
                TerraceProcess node = new TerraceProcess();
                node.Init(this);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Thermal_Erosion_Filter))
            {
                ThermalErosionProcess node = new ThermalErosionProcess();
                node.Init(this);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Water_Erosion_Filter))
            {
                WaterErosionProcess node = new WaterErosionProcess();
                node.Init(this);
                nodes.Add(node);
                _id = node.Data.ID;
            }
            else if (heightmapProcessors.Equals(HeightmapProcessors.Terrain_Deformer))
            {
                ApplyMask node = new ApplyMask();
                node.Init(this);
                nodes.Add(node);
                _id = node.Data.ID;
            }

            return _id;
        }




        public override bool CheckConnections()
        {
            base.CheckConnections();

            // check for last node
            int lastNodeCount = GetLastNodes(_outputDataType).Count;
            if (lastNodeCount > 1) throw new Exception("More than one end node detected on heighmap graph. Please Remove one of them.");
            if (lastNodeCount < 1) throw new Exception("There isn't any end node for heightmap graph. Pleas check the heightmap graph");

            return true;
        }
    }


}
#endif
#endif

