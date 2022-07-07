#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Windows;
using System;
using System.Collections.Generic;
using TerraUnity.Runtime;
using UnityEditor;
#if TERRAWORLD_XPRO
using TerraUnity.Graph;
using XNode;
#endif

namespace TerraUnity.Edittime
{
    public enum EventCategory
    {
        UX,
        Params,
        Nodes,
        Templates,
        SystemInfo,
        SoftwareInfo
    }

    public enum EventAction
    {
        Click,
        Version,
        Uses,
        OperatingSystem,
        DeviceType,
        SystemMemorySize,
        graphicsMemorySize,
        UnityVersion,
        Platform
    }

    public class TTerraWorld
    {
        //private static TTerraWorldGraph _worldGraph;
        public List<TTerrain> _terrains;
        private Action<TTerraWorld> _lastActions;
        private int _terrainsDone = 0;
        public static string _ID;
        private static int Counter;
        public static string TemplateName;
        public static bool FeedbackSystem = TProjectSettings.FeedbackSystem;

        //public static TTerraWorldGraph WorldGraph { get => GetWorldGraph(); }
        public static TTerraWorldGraph WorldGraph { get => TTerraWorldManager.WorldGraph; }

        public static GlobalTimeManager globalTimeManager { get => TTerraWorldManager.GlobalTimeManagerScript; }
        public static CloudsManager cloudsManager { get => TTerraWorldManager.CloudsManagerScript; }
        public static Crepuscular godRaysManager { get => TTerraWorldManager.GodRaysManagerScript; }
        public static TimeOfDay timeOfDayManager { get => TTerraWorldManager.TimeOfDayManagerScript; }
        public static HorizonFog horizonFogManager { get => TTerraWorldManager.HorizonFogManagerScript; }
        public static WaterManager waterManager { get => TTerraWorldManager.WaterManagerScript; }
        public static PostProcessVolume postProcessVolumeManager { get => TTerraWorldManager.PostProcessVolumeManagerScript; }
        public static SnowManager snowManager { get => TTerraWorldManager.SnowManagerScript; }
        public static WindManager windManager { get => TTerraWorldManager.WindManagerScript; }
        public static FlatShadingManager flatShadingManager { get => TTerraWorldManager.FlatShadingManagerScript; }

   //     public int SplatmapSmoothness { get => WorldGraph.renderingGraph.GetEntryNode().renderingParams.splatmapSmoothness; }
   //     public int TerrainPixelError { get => WorldGraph.renderingGraph.GetEntryNode().renderingParams.terrainPixelError; }
   //     public int BgTerrainPixelError { get => WorldGraph.renderingGraph.GetEntryNode().renderingParams.BGTerrainPixelError; }
        public static TArea Area { get => WorldGraph.areaGraph.WorldArea.Area; }
 
        public static string GraphPath { 
            get 
            {
                string path = TTerraWorldManager.TerraWorldGraphPath;
                if (!string.IsNullOrEmpty(path)) 
                    return path; 
                else
                    return WorkDirectoryFullPath + "graph.xml"; 
            } 
        }


        private static int _messageCounter = 0;
        public static int MessageCounter { get => _messageCounter; }

        private static string _newVersionWebAddress = "";
        public static string NewVersionWebAddress { get => _newVersionWebAddress; set => _newVersionWebAddress = value; }

        private TMapManager.mapImagerySourceEnum _imagerySource = TMapManager.mapImagerySourceEnum.ESRI;
        public TMapManager.mapImagerySourceEnum ImagerySource { get => _imagerySource; set => _imagerySource = value; }

        private TMapManager.mapElevationSourceEnum _elevationSource = TMapManager.mapElevationSourceEnum.ESRI;
        public TMapManager.mapElevationSourceEnum ElevationSource { get => _elevationSource; set => _elevationSource = value; }

        private TMapManager.mapLandcoverSourceEnum _landcoverSource = TMapManager.mapLandcoverSourceEnum.OSM;
        public TMapManager.mapLandcoverSourceEnum LandcoverSource { get => _landcoverSource; set => _landcoverSource = value; }

        public static void FeedbackSystemInfo()
        {
            FeedbackEvent(EventCategory.SystemInfo, EventAction.OperatingSystem, UnityEngine.SystemInfo.operatingSystem.ToString());
            FeedbackEvent(EventCategory.SystemInfo, EventAction.DeviceType, UnityEngine.SystemInfo.deviceType.ToString());
            FeedbackEvent(EventCategory.SystemInfo, EventAction.SystemMemorySize, UnityEngine.SystemInfo.systemMemorySize.ToString());
            FeedbackEvent(EventCategory.SystemInfo, EventAction.graphicsMemorySize, UnityEngine.SystemInfo.graphicsMemorySize.ToString());
            FeedbackEvent(EventCategory.SystemInfo, EventAction.UnityVersion, UnityEngine.Application.unityVersion.ToString());
            FeedbackEvent(EventCategory.SystemInfo, EventAction.Platform, UnityEngine.Application.platform.ToString());
        }

        public static void FeedbackEvent(EventCategory EventCategory, EventAction EventAction, string label, int? value = null)
        {
            if (FeedbackSystem)
            {
                if (value != null)
                    TFeedback.FeedbackEvent(EventCategory.ToString(), EventAction.ToString(), label + "_" + value.ToString());
                else
                    TFeedback.FeedbackEvent(EventCategory.ToString(), EventAction.ToString(), label.ToString());
            }
        }

        public static void FeedbackEvent(EventCategory EventCategory, string EventAction, int value)
        {
            if (FeedbackSystem)
                TFeedback.FeedbackEvent(EventCategory.ToString(), EventAction, value.ToString());
        }

        public static void FeedbackEvent(EventCategory EventCategory, string EventAction, string value)
        {
            if (FeedbackSystem)
                TFeedback.FeedbackEvent(EventCategory.ToString(), EventAction, value);
        }

        public static string WorkDirectoryFullPath
        {
            get { return TAddresses.projectPath + TTerraWorldManager.WorkDirectoryLocalPath; }
        }

        public static string WorkDirectoryLocalPath
        {
            get { return TTerraWorldManager.WorkDirectoryLocalPath; }
        }

       // private static TTerraWorldGraph GetWorldGraph()
       // {
       //     if (_worldGraph == null)
       //     {
       //         string oldPath = "Assets/TerraWorld/Core/Presets/Graph.xml";
       //         
       //         if (File.Exists(TTerraWorldManager.TerraWorldGraphPath))
       //         {
       //             LoadWorldGraph(TTerraWorldManager.TerraWorldGraphPath, false, out Exception exception, out bool reGenerate);
       //             if (exception != null) TDebug.LogErrorToUnityUI(exception);
       //         }
       //         else if (File.Exists(WorkDirectoryFullPath + "graph.xml"))
       //         {
       //             LoadWorldGraph(WorkDirectoryFullPath + "graph.xml", false, out Exception exception, out bool reGenerate);
       //             if (exception != null) TDebug.LogErrorToUnityUI(exception);
       //         }
       //         else if (File.Exists(oldPath))
       //         {
       //             LoadWorldGraph(oldPath, false, out Exception exception, out bool reGenerate);
       //             if (exception != null) TDebug.LogErrorToUnityUI(exception);
       //         }
       //         else
       //             ResetWorldGraph();
       //     }
       //
       //     return _worldGraph;
       // }

       // public static void ResetWorldGraph()
       // {
       //     TAreaGraph OldareaGraphArea = null;
       //
       //     if (_worldGraph != null)
       //         OldareaGraphArea = _worldGraph.areaGraph;
       //
       //     _worldGraph = TTerraWorldGraph.GetNewWorldGraph(TVersionController.MajorVersion, TVersionController.MinorVersion);
       //
       //     if (OldareaGraphArea != null)
       //         _worldGraph.areaGraph = OldareaGraphArea;
       //
       //     SaveWorldGraphAsItIs();
       // }

     //   public static void SaveGraphAsTemplate(string path)
     //   {
     //       TDebug.TraceMessage();
     //       if (WorldGraph == null) return;
     //       WorldGraph.templateIgnoreList.Ignore_AreaGraph = true;
     //       SaveWorldGraphFromScene(path);
     //   }

        public static void SaveGraphAsTemplate(string path)
        {
            TDebug.TraceMessage();
            if (WorldGraph == null) return;
            WorldGraph.templateIgnoreList.Ignore_AreaGraph = true;
            WorldGraph.SaveGraph(path);
        }

        //  public static void SaveWorldGraphFromScene(string path = "")
        //  {
        //      if (_worldGraph == null) return;
        //
        //      if (TTerraWorldManager.TimeOfDayManagerScript != null)
        //      {
        //          _worldGraph.TimeOfDayParams = TTerraWorldManager.TimeOfDayManagerScript.GetParams();
        //          _worldGraph._timeOfDayParamsSaved = true;
        //      }
        //      else
        //          _worldGraph._timeOfDayParamsSaved = false;
        //
        //      _worldGraph.FXGraph.GetEntryNode().fxParams = SceneSettingsManager.FXParameters;
        //      _worldGraph.renderingGraph.GetEntryNode().renderingParams = TerrainRenderingManager.RenderingParams;
        //
        //      SaveWorldGraphAsItIs(path);
        //  }

      //  public static void UpdateWorldGraphFromScene(string path = "")
      //  {
      //
      //      if (TTerraWorldManager.TimeOfDayManagerScript != null)
      //      {
      //          WorldGraph.TimeOfDayParams = TTerraWorldManager.TimeOfDayManagerScript.GetParams();
      //          WorldGraph._timeOfDayParamsSaved = true;
      //      }
      //      else
      //          WorldGraph._timeOfDayParamsSaved = false;
      //
      //      if (TTerraWorldManager.SceneSettingsGO1 != null)
      //          WorldGraph.FXGraph.GetEntryNode().fxParams = SceneSettingsManager.FXParameters;
      //
      //      if (TTerraWorldManager.SceneSettingsGO1 != null)
      //          WorldGraph.FXGraph.GetEntryNode().fxParams = SceneSettingsManager.FXParameters;
      //
      //      _worldGraph.FXGraph.GetEntryNode().fxParams = SceneSettingsManager.FXParameters;
      //      _worldGraph.renderingGraph.GetEntryNode().renderingParams = TerrainRenderingManager.RenderingParams;
      //
      //      SaveWorldGraphAsItIs(path);
      //  }

     //   public static void SaveWorldGraphAsItIs(string path = "")
     //   {
     //       if (_worldGraph == null) return;
     //
     //       if (!string.IsNullOrEmpty(path))
     //           _worldGraph.SaveGraph(path);
     //
     //       if (string.IsNullOrEmpty(TTerraWorldManager.TerraWorldGraphPath))
     //       {
     //           SaveWorldGraphAsItIsToWorkDirectory();
     //       }
     //       else
     //           _worldGraph.SaveGraph(TTerraWorldManager.TerraWorldGraphPath);
     //   }

     //   public static void SaveWorldGraphAsItIsToWorkDirectory()
     //   {
     //       if (_worldGraph == null) return;
     //
     //       string path = WorkDirectoryFullPath + "graph.xml";
     //       _worldGraph.SaveGraph(path);
     //       TTerraWorldManager.TerraWorldGraphPath = path;
     //   }

        public static void LoadWorldGraph(string path, bool Template, out Exception exception, out bool reGenerate)
        {
            exception = null;
            reGenerate = false;

            try
            {
                if (TTerraWorldGraph.CheckGraph(path))
                {
                    //if (WorldGraph == null) _worldGraph = new TTerraWorldGraph();
                    reGenerate = WorldGraph.LoadGraph(path, Template);
                    //SaveWorldGraphAsItIsToWorkDirectory();
                    TTerraWorldManager.SaveGraph();
                    
                }
                else
                    throw new Exception("Corrupt File!");
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        public static bool LoadTemplate(string path, out Exception exception)
        {
            LoadWorldGraph(path, true, out exception, out bool reGenerate);
            return reGenerate;
        }



        public string HeightmapPath
        {
            get { return WorkDirectoryLocalPath + "Terrain Data.asset"; }
        }

        public string HeightmapPathBackground
        {
            get { return WorkDirectoryLocalPath + "Terrain Data BG.asset"; }
        }

        private int _ElevationzoomLevel = 10;
        public int ElevationZoomLevel
        {
            get { return _ElevationzoomLevel; }
            set
            {
                _ElevationzoomLevel = value;
            }
        }

        private int _ImagerzoomLevel = 10;
        public int ImageZoomLevel
        {
            get { return _ImagerzoomLevel; }
            set
            {
                _ImagerzoomLevel = value;
            }
        }

        public static bool CacheData
        {
            get { return TProjectSettings.CacheData; }
        }

        private float _progressPersentage;
        public float ProgressPersentage
        {
            get { UpdateProgressPersentage(); return _progressPersentage; }
        }

        public void UpdateProgressPersentage()
        {
            _progressPersentage = 0;

            for (int i = 0; i < _terrains.Count; i++)
                _progressPersentage += _terrains[i].Progress;

            _progressPersentage = (_progressPersentage * 1.0f / _terrains.Count);
        }

        public TTerraWorld()
        {
            TDebug.TraceMessage();
            Random rand = new Random((int)DateTime.Now.Ticks);
            _ID = (rand.Next() + Counter++).ToString();
        }

        public void GenerateTerrains(TArea area)
        {
            TDebug.TraceMessage();
            _terrains.Clear();
            _terrainsDone = 0;
            TTerrain tempTerrain = new TTerrain(area._top, area._left, area._bottom, area._right, this);
            _terrains.Add(tempTerrain);
        }

        public void UpdateTerraWorld(Action<TTerraWorld> act = null)
        {
            TDebug.TraceMessage();
            if (act != null) _lastActions = act;
            _terrains = new List<TTerrain>();
            GenerateTerrains(Area);

            foreach (TTerrain t in _terrains)
                t.UpdateTerrain();
        }

        public void EachTerrainDone(TTerrain CurrentTerrain)
        {
            TDebug.TraceMessage();
            _terrainsDone++;

            if (_terrainsDone == _terrains.Count)
                WhenAllDone();
        }

        private void WhenAllDone()
        {
            TDebug.TraceMessage();
            if (_lastActions != null)
                _lastActions.Invoke(this);
        }

        public System.Numerics.Vector3 GetWorldPosition(TGlobalPoint gpoint) => _terrains[0].GetWorldPositionWithHeight(gpoint);
        public System.Numerics.Vector3 GetAngle(TGlobalPoint gpoint) => _terrains[0].GetAngle(gpoint);
        public float GetSteepness(TGlobalPoint gpoint) => _terrains[0].GetSteepness(gpoint);
        public System.Numerics.Vector2 GetNormalPositionN(TGlobalPoint gpoint) => _terrains[0].GetNormalPositionN(gpoint);
    }
}
#endif
#endif

