#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
#if TERRAWORLD_XPRO
using TerraUnity.Graph;
#endif
using TerraUnity.Runtime;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
    public enum Presets
    {
        mobile,
        tablet,
        pc,
        console,
        farm,
        custom
    }

    public enum VersionMode
    {
        Pro,
        Lite
    }

    public class TSequence
    {
        private List<TNode> _sequence;

        public TSequence()
        {
            _sequence = new List<TNode>();
        }

        public List<TNode> Nodes { get => _sequence; set => _sequence = value; }
    }

    public struct TemplateIgnoreList
    {
        public bool Ignore_AreaGraph;
        public bool Ignore_HeightmapGraph;
        public bool Ignore_ColormapGraph;
        public bool Ignore_BiomesGraph;

        //Rendering Graph Section
        public bool Ignore_RenderingGraph_surfaceTint;
        public bool Ignore_RenderingGraph_modernRendering;
        public bool Ignore_RenderingGraph_instancedDrawing;
        public bool Ignore_RenderingGraph_tessellation;
        public bool Ignore_RenderingGraph_heightmapBlending;
        public bool Ignore_RenderingGraph_TillingRemover;
        public bool Ignore_RenderingGraph_colormapBlending;
        public bool Ignore_RenderingGraph_proceduralSnow;
        public bool Ignore_RenderingGraph_proceduralPuddles;
        public bool Ignore_RenderingGraph_LayerProperties;
        public bool Ignore_RenderingGraph_isFlatShading;
        public bool Ignore_RenderingGraph_SplatmapSettings;
        public bool Ignore_RenderingGraph_MainTerrainSettings;
        public bool Ignore_RenderingGraph_BGTerrainSettings;

        public bool Ignore_FXGraph_selectionIndexVFX;
        public bool Ignore_FXGraph_TimeOfDay;
        public bool Ignore_FXGraph_CrepuscularRay;
        public bool Ignore_FXGraph_CloudsSettings;
        public bool Ignore_FXGraph_AtmosphericScatteringSettings;
        public bool Ignore_FXGraph_VolumetricFogSettings;
        public bool Ignore_FXGraph_WindSettings;
        public bool Ignore_FXGraph_WeatherSettings;
        public bool Ignore_FXGraph_ReflectionSettings;
        public bool Ignore_FXGraph_WaterSettings;
        public bool Ignore_FXGraph_PostProcessSettings;
        public bool Ignore_FXGraph_HorizonFogSettings;
        public bool Ignore_FXGraph_FlatShadingSettings;
        public bool Ignore_PlayerGraph;
    }

    [Serializable]
    public struct FileData
    {
        public byte[] data;
    }

    [Serializable]
    public class TTerraWorldGraph
    {
#if TERRAWORLD_PRO
        public VersionMode versionMode = VersionMode.Pro;
#else
        public VersionMode versionMode = VersionMode.Lite;
#endif
        public TemplateIgnoreList templateIgnoreList;

       // public RenderingParams RenderingParams {get> renderingGraph.GetEntryNode().renderingParams()};
       // public FXParams _VFXParams;

        public bool _timeOfDayParamsSaved = false;
        private TimeOfDayParams _timeOfDayParams;
        [XmlElement(Namespace = "TimeOfDayParams")]
        public TimeOfDayParams TimeOfDayParams { get => _timeOfDayParams; set => _timeOfDayParams = value; }

        private static Random rand = new Random();
        public delegate void AreaChangedHandler();

        // UI Settings
        public static float scaleFactor = 1;
        public static TMapManager.mapImagerySourceEnum _interactiveMapImagerySource = TMapManager.mapImagerySourceEnum.ESRI;
        public static TMapManager.mapTypeOSMEnum _OSMImagerySource = TMapManager.mapTypeOSMEnum.Standard;
        // -----------------------------------------------------------------------------------------------------------------------------------

        public TAreaGraph areaGraph = new TAreaGraph();
        public THeightmapGraph heightmapGraph = new THeightmapGraph();
        public TColormapGraph colormapGraph = new TColormapGraph();
        public TBiomesGraph biomesGraph = new TBiomesGraph();
        public TRenderingGraph renderingGraph = new TRenderingGraph();
        public TFXGraph FXGraph = new TFXGraph();
        public TPlayerGraph playerGraph = new TPlayerGraph();

        [XmlIgnore] public List<TGraph> graphList = new List<TGraph>();
        public int GraphMajorVersion = TVersionController.MajorVersion;
        public int GraphMinorVersion = TVersionController.MinorVersion;

        //[XmlElement("NaughtyXmlCharacters")]
        //public string NaughtyXmlCharactersAsString
        //{
        //    get
        //    {
        //        if (NaughtyXmlCharacters == null) return string.Empty;
        //        return BitConverter.ToString(NaughtyXmlCharacters);
        //    }
        //    set
        //    {
        //        // without this, the property is not serialized.
        //        String[] arr = value.Split('-');
        //        byte[] array = new byte[arr.Length];
        //        for (int i = 0; i < arr.Length; i++) array[i] = Convert.ToByte(arr[i], 16);
        //        NaughtyXmlCharacters = array;
        //    }
        //}
        //
        //[XmlIgnore]
        //public byte[] NaughtyXmlCharacters
        //{
        //    get;
        //    set;
        //}

        public static int GetNewID()
        {
            return rand.Next();
        }

        public static TTerraWorldGraph GetNewWorldGraph(int MajorVersion, int MinorVersion)
        {
            TTerraWorldGraph _result = new TTerraWorldGraph();
            _result.GraphMajorVersion = MajorVersion;
            _result.GraphMinorVersion = MinorVersion;
            return _result;
        }

#if TERRAWORLD_XPRO
        public static TXGraph GetNewXGraph(int MajorVersion, int MinorVersion)
        {
            TXGraph _result = new TXGraph();
            return _result;
        }
#endif

        public TTerraWorldGraph()
        {
            InitWorldGraph();
        }

        public void InitWorldGraph()
        {
            graphList = new List<TGraph>();

            graphList.Add(areaGraph);
            areaGraph.InitGraph(this);

            graphList.Add(heightmapGraph);
            heightmapGraph.InitGraph(this);

            graphList.Add(colormapGraph);
            colormapGraph.InitGraph(this);

            graphList.Add(biomesGraph);
            biomesGraph.InitGraph(this);

            graphList.Add(renderingGraph);
            renderingGraph.InitGraph(this);

            graphList.Add(FXGraph);
            FXGraph.InitGraph(this);

            graphList.Add(playerGraph);
            playerGraph.InitGraph(this);

            for (int i = 0; i < graphList.Count; i++)
            {
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                    graphList[i].nodes[j].parentGraph = graphList[i];

                graphList[i].UpdateConnections();
            }
        }



        public void SaveGraph(string path)
        {
            TDebug.TraceMessage();

            //string filename = path + "SceneSettings.prefab";
            //
            //SceneSettingsManager.SaveSceneSettingsPreFab(filename);
            //
            //using (FileStream fs1 = new FileStream(filename, FileMode.Open, FileAccess.Read))
            //{
            //    // Create a byte array of file stream length
            //    byte[] bytes = System.IO.File.ReadAllBytes(filename);
            //    //Read block of bytes from stream into the byte array
            //    fs1.Read(bytes, 0, System.Convert.ToInt32(fs1.Length));
            //    //Close the File Stream
            //    fs1.Close();
            //    NaughtyXmlCharacters = bytes; //return the byte data
            //}

            GraphMajorVersion = TVersionController.MajorVersion;
            GraphMinorVersion = TVersionController.MinorVersion;

            XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
            FileStream fs = new FileStream(path, FileMode.Create);
            serializer.Serialize(fs, this);
            fs.Close();
        }

        public List<TGraph> LoadGraphListCurrent()
        {
            List<TGraph> result = new List<TGraph>();
            result.Add(areaGraph);
            result.Add(heightmapGraph);
            result.Add(colormapGraph);
            result.Add(biomesGraph);
            result.Add(renderingGraph);
            result.Add(FXGraph);
            result.Add(playerGraph);

            return result;
        }

        public List<TGraph> LoadGraphList(string path)
        {
            TDebug.TraceMessage();
            XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
            FileStream fs = new FileStream(path, FileMode.Open);
            TTerraWorldGraph graph = (TTerraWorldGraph)serializer.Deserialize(fs);
            fs.Close();

            List<TGraph> result = new List<TGraph>();
            result.Add(graph.areaGraph);
            result.Add(graph.heightmapGraph);
            result.Add(graph.colormapGraph);
            result.Add(graph.biomesGraph);
            result.Add(graph.renderingGraph);
            result.Add(graph.FXGraph);
            result.Add(graph.playerGraph);

            return result;
        }

        public void NewGraph()
        {
            TDebug.TraceMessage();

            heightmapGraph = new THeightmapGraph();
            colormapGraph = new TColormapGraph();
            biomesGraph = new TBiomesGraph();

#if TERRAWORLD_PRO
            renderingGraph = new TRenderingGraph();
            FXGraph = new TFXGraph();
#else
#endif

            playerGraph = new TPlayerGraph();

            InitWorldGraph();
        }

        public static bool CheckGraph(string graphPath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
                FileStream fs = new FileStream(graphPath, FileMode.Open);
                TTerraWorldGraph newLoadedGraph = (TTerraWorldGraph)serializer.Deserialize(fs);
                fs.Close();
                return true;
            }
            catch (Exception e)
            {
                throw e;
                //return false;
            }
        }

        public static FXParams GetSavedVFXParams(string graphPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
            FileStream fs = new FileStream(graphPath, FileMode.Open);
            TTerraWorldGraph newLoadedGraph = (TTerraWorldGraph)serializer.Deserialize(fs);
            fs.Close();
            return newLoadedGraph.FXGraph.GetEntryNode().fxParams;
        }

        public bool LoadGraph(string graphPath, bool isTemplate)
        {
            TDebug.TraceMessage();
            bool needRegenerate = false;

            XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
            FileStream fs = new FileStream(graphPath, FileMode.Open);
            TTerraWorldGraph newLoadedGraph = (TTerraWorldGraph)serializer.Deserialize(fs);
            fs.Close();

            //File.WriteAllBytes(graphPath + "SceneSettings2.prefab", newLoadedGraph.NaughtyXmlCharacters);

            if ((newLoadedGraph.GraphMajorVersion > TVersionController.MajorVersion)) throw new Exception("Unable to load graph. It has been created by " + newLoadedGraph.GraphMajorVersion + "." + newLoadedGraph.GraphMajorVersion + " version!");
            else if ((newLoadedGraph.GraphMajorVersion == TVersionController.MajorVersion) && (newLoadedGraph.GraphMinorVersion > TVersionController.MinorVersion)) throw new Exception("Unable to load graph. It has been created by " + newLoadedGraph.GraphMajorVersion + "." + newLoadedGraph.GraphMajorVersion + " version!");

            //TODO: Remove the following lines later when all templates are synced with this new feature and terrain layers have proper colors
            int graphVersion = newLoadedGraph.GraphMajorVersion * 1000 + newLoadedGraph.GraphMinorVersion;

            templateIgnoreList = newLoadedGraph.templateIgnoreList;

            if (isTemplate) templateIgnoreList.Ignore_AreaGraph = true;

            if (!templateIgnoreList.Ignore_AreaGraph || !isTemplate)
            {
                areaGraph = newLoadedGraph.areaGraph;
                needRegenerate = true;
            }

            if (!templateIgnoreList.Ignore_HeightmapGraph || !isTemplate)
            {
                heightmapGraph = newLoadedGraph.heightmapGraph;
                needRegenerate = true;
            }

            if (!templateIgnoreList.Ignore_ColormapGraph || !isTemplate)
            {
                colormapGraph = newLoadedGraph.colormapGraph;
                needRegenerate = true;
            }

            if (!templateIgnoreList.Ignore_BiomesGraph || !isTemplate)
            {
                biomesGraph = newLoadedGraph.biomesGraph;
                needRegenerate = true;
            }

#if TERRAWORLD_PRO
            if (newLoadedGraph.versionMode != VersionMode.Lite)
            {
                //if (!isTemplate)
                //    renderingGraph = newLoadedGraph.renderingGraph;
                //else
                //{
                //    renderingGraph.GetEntryNode().SetRenderingParams(newLoadedGraph.renderingGraph.GetEntryNode().renderingParams,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_surfaceTint,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_modernRendering,
                //                                                     //!templateIgnoreList.Ignore_RenderingGraph_instancedDrawing,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_tessellation,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_heightmapBlending,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_TillingRemover,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_colormapBlending,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_proceduralSnow,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_proceduralPuddles,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_LayerProperties,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_isFlatShading,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_SplatmapSettings,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_MainTerrainSettings,
                //                                                     !templateIgnoreList.Ignore_RenderingGraph_BGTerrainSettings
                //                                                    );
                //}

                renderingGraph = newLoadedGraph.renderingGraph;

             //   if (graphVersion < 2228)
             //   {
             //       _renderingParams1 = newLoadedGraph.renderingGraph.GetEntryNode().renderingParams;
             //   }
             //   else
             //       _renderingParams1 = newLoadedGraph._renderingParams1;


                if (graphVersion <= 2130)
                {
                    renderingGraph.GetEntryNode().renderingParams.surfaceTintColorMAIN = TUtils.UnityColorToVector4(new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1.0f));
                    renderingGraph.GetEntryNode().renderingParams.surfaceTintColorBG = TUtils.UnityColorToVector4(new UnityEngine.Color(0.5f, 0.5f, 0.5f, 1.0f));
                }

                if (graphVersion < 2301)
                {
                    heightmapGraph.HeightmapMaster()._pixelError = renderingGraph.GetEntryNode().renderingParams.terrainPixelError;
                }
               // TerrainRenderingManager.ApplyRenderingParams(renderingGraph.GetEntryNode().renderingParams);



                //  TerrainRenderingManager.RenderingParams = _renderingParams1;
                //  renderingGraph.GetEntryNode().renderingParams = _renderingParams1;

                //renderingGraph.GetEntryNode().renderingParams = _renderingParams;
                //TerrainRenderingManager.RenderingParams = _renderingParams;

                //if (!isTemplate)
                //    FXGraph = newLoadedGraph.FXGraph;
                //else
                //{
                //    FXGraph.GetEntryNode().SetFXParams(newLoadedGraph.FXGraph.GetEntryNode().fxParams,
                //                                    !templateIgnoreList.Ignore_FXGraph_selectionIndexVFX,
                //                                    !templateIgnoreList.Ignore_FXGraph_TimeOfDay,
                //                                    !templateIgnoreList.Ignore_FXGraph_CrepuscularRay,
                //                                    !templateIgnoreList.Ignore_FXGraph_CloudsSettings,
                //                                    !templateIgnoreList.Ignore_FXGraph_AtmosphericScatteringSettings,
                //                                    !templateIgnoreList.Ignore_FXGraph_VolumetricFogSettings,
                //                                    !templateIgnoreList.Ignore_FXGraph_WindSettings,
                //                                    !templateIgnoreList.Ignore_FXGraph_WeatherSettings,
                //                                    !templateIgnoreList.Ignore_FXGraph_ReflectionSettings,
                //                                    !templateIgnoreList.Ignore_FXGraph_WaterSettings,
                //                                    !templateIgnoreList.Ignore_FXGraph_PostProcessSettings,
                //                                    !templateIgnoreList.Ignore_FXGraph_HorizonFogSettings,
                //                                    !templateIgnoreList.Ignore_FXGraph_FlatShadingSettings
                //                                    );
                //}

                FXGraph = newLoadedGraph.FXGraph;

             //   if (graphVersion < 2228)
             //   {
             //       _VFXParams = newLoadedGraph.FXGraph.GetEntryNode().fxParams;
             //   }
             //   else
             //       _VFXParams = newLoadedGraph._VFXParams;
             //
             //   FXGraph.GetEntryNode().fxParams = _VFXParams;
             //   SceneSettingsManager.FXParameters = FXGraph.GetEntryNode().fxParams;

                if (FXGraph.GetEntryNode().fxParams.selectionIndexVFX == 0)
                {
                    TTerraWorldManager.CreateSceneSettingsGameObject();
                    if (TTerraWorldManager.TimeOfDayManagerScript != null)
                        if (newLoadedGraph._timeOfDayParamsSaved == true && !templateIgnoreList.Ignore_FXGraph_TimeOfDay)
                            TTerraWorldManager.TimeOfDayManagerScript.SetParams(newLoadedGraph._timeOfDayParams);
                }
            }
#else
            if (newLoadedGraph.versionMode  != VersionMode.Lite ) throw new Exception("Unable to load graph. Graph has been created by Pro version of TerraWorld!");
#endif

            if (!templateIgnoreList.Ignore_PlayerGraph || !isTemplate)
            {
                playerGraph = newLoadedGraph.playerGraph;
                needRegenerate = true;
            }

            InitWorldGraph();
            //HandleAreaWorldChange();

            return needRegenerate;
        }

        /*
                public void LoadNewGraph (string path)
                {
                    TDebug.TraceMessage();

                    XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
                    FileStream fs = new FileStream(path, FileMode.Open);
                    TTerraWorldGraph graph = (TTerraWorldGraph)serializer.Deserialize(fs);
                    fs.Close();

                    areaGraph = graph.areaGraph;
                    heightmapGraph = new THeightmapGraph();
                    colormapGraph = new TColormapGraph();
                    biomesGraph = new TBiomesGraph();
                    if (graph.versionMode != VersionMode.Lite) renderingGraph = new TRenderingGraph();
                    if (graph.versionMode != VersionMode.Lite) FXGraph = new TFXGraph();
                    globalGraph = graph.globalGraph;
                    runtimeGraph = new TRuntimeGraph();

                    InitWorldGraph();

                    for (int i = 0; i < graphList.Count; i++)
                    {
                        for (int j = 0; j < graphList[i].nodes.Count; j++)
                        {
                            graphList[i].nodes[j].parentGraph = graphList[i];
                            graphList[i].worldGraph = this;
                            graphList[i].UpdateConnections();
                        }
                    }
                }

                public void LoadGraph (string path)
                {
                    TDebug.TraceMessage();

                    XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
                    FileStream fs = new FileStream(path, FileMode.Open);
                    TTerraWorldGraph graph = (TTerraWorldGraph)serializer.Deserialize(fs);
                    fs.Close();

#if TERRAWORLD_PRO
                    if (graph.versionMode  != VersionMode.Pro ) graph.versionMode = VersionMode.Pro;
#else
                    if (graph.versionMode  != VersionMode.Lite ) throw new Exception("Unable to load graph. Graph has been created by Pro version of TerraWorld!");
#endif

                    if (graph.GraphMajorVersion > TTerraWorld.MajorVersion) throw new Exception("Unable to load graph. Graph has been created by newer version!");
                    else if (graph.GraphMajorVersion > TTerraWorld.MajorVersion) throw new Exception("Unable to load graph. Graph has been created by newer version!");

                    areaGraph = graph.areaGraph;
                    heightmapGraph = graph.heightmapGraph;
                    colormapGraph = graph.colormapGraph;
                    biomesGraph = graph.biomesGraph;
                    if (graph.versionMode != VersionMode.Lite) renderingGraph = graph.renderingGraph;
                    if (graph.versionMode != VersionMode.Lite) FXGraph = graph.FXGraph;
                    globalGraph = graph.globalGraph;
                    runtimeGraph = graph.runtimeGraph;

                    InitWorldGraph();

                    for (int i = 0; i < graphList.Count; i++)
                    {
                        for (int j = 0; j < graphList[i].nodes.Count; j++)
                        {
                            graphList[i].nodes[j].parentGraph = graphList[i];
                            graphList[i].worldGraph = this;
                            graphList[i].UpdateConnections();
                        }
                    }
                }

                public void LoadTemplate (string currentGraphPath, string templatePath)
                {
                    TDebug.TraceMessage();

                    XmlSerializer serializer = new XmlSerializer(typeof(TTerraWorldGraph));
                    FileStream fs = new FileStream(templatePath, FileMode.Open);
                    TTerraWorldGraph template = (TTerraWorldGraph)serializer.Deserialize(fs);
                    serializer = new XmlSerializer(typeof(TTerraWorldGraph));
                    fs.Close();
                    fs = new FileStream(currentGraphPath, FileMode.Open);
                    TTerraWorldGraph currentGraph = (TTerraWorldGraph)serializer.Deserialize(fs);
                    fs.Close();

                    if (template.GraphMajorVersion > TTerraWorld.MajorVersion) throw new Exception("Unable to load template. template has been created for newer version!");
                    else if (template.GraphMajorVersion > TTerraWorld.MajorVersion) throw new Exception("Unable to load template. template has been created for newer version!");

                    areaGraph = currentGraph.areaGraph;
                    heightmapGraph = template.heightmapGraph;
                    colormapGraph = template.colormapGraph;
                    biomesGraph = template.biomesGraph;
                    if (template.versionMode != VersionMode.Lite) renderingGraph = template.renderingGraph;
                    if (template.versionMode != VersionMode.Lite) FXGraph = template.FXGraph;
                    globalGraph = currentGraph.globalGraph;
                    runtimeGraph = template.runtimeGraph;

                    InitWorldGraph();

                    for (int i = 0; i < graphList.Count; i++)
                    {
                        graphList[i].worldGraph = this;

                        for (int j = 0; j < graphList[i].nodes.Count; j++)
                            graphList[i].nodes[j].parentGraph = graphList[i];

                        graphList[i].UpdateConnections();
                    }

                    HandleAreaWorldChange();
                }
        */

        public bool CheckConnections()
        {
            TDebug.TraceMessage();

            for (int i = 0; i < graphList.Count; i++)
            {
                graphList[i].UpdateConnections();
                if (!graphList[i].CheckConnections()) return false;
            }

            return true;
        }

        public void RemoveNodeByID(int ID)
        {
            TDebug.TraceMessage();
            if (ID == -1) return;
            List<TNode> outputNodes = GetOutputNodes(ID);

            foreach (TNode node in outputNodes)
                for (int i = 0; i < node.inputConnections.Count; i++)
                    if (node.inputConnections[i].previousNodeID == ID)
                        node.inputConnections[i].previousNodeID = -1;

            for (int i = 0; i < graphList.Count; i++)
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                    if (graphList[i].nodes[j].Data.ID.Equals(ID))
                        graphList[i].nodes.Remove(graphList[i].nodes[j--]);
        }

        public TNode GetNodeByID(int ID)
        {
            TNode result = null;
            if (ID == -1) return null;

            for (int i = 0; i < graphList.Count; i++)
            {
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                {
                    if (graphList[i].nodes[j].Data.ID.Equals(ID))
                    {
                        result = graphList[i].nodes[j];
                        break;
                    }
                }

                if (result != null)
                    return result;
            }

            return result;
        }

        public int GetNodeCounts()
        {
            int result = 0;

            for (int i = 0; i < graphList.Count; i++)
                result += graphList[i].nodes.Count;

            return (result - 10);
        }

        public List<TNode> GetOutputNodes(int ID)
        {
            List<TNode> result = new List<TNode>();
            if (ID == -1) return result;

            for (int i = 0; i < graphList.Count; i++)
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                    for (int k = 0; k < graphList[i].nodes[j].inputConnections.Count; k++)
                        if (graphList[i].nodes[j].inputConnections[k].previousNodeID.Equals(ID))
                            result.Add(graphList[i].nodes[j]);

            return result;
        }

        public List<TNode> GetLastNodes(ConnectionDataType connectionDataType)
        {
            TDebug.TraceMessage();
            List<TNode> result = new List<TNode>();

            for (int i = 0; i < graphList.Count; i++)
            {
                result = graphList[i].GetLastNodes(connectionDataType);
                if (result.Count != 0) return result;
            }

            return result;
        }

        public void MakeChain(ref List<TNode> nodesList, TNode node)
        {
            TDebug.TraceMessage();

            for (int i = 0; i < nodesList.Count; i++)
            {
                TNode n = nodesList[i];

                if (n == node)
                {
                    nodesList.Remove(n);
                    i--;
                }
            }

            nodesList.Add(node);

            for (int i = 0; i < node.inputConnections.Count; i++)
            {
                if (node.inputConnections[i].previousNodeID != -1)
                {
                    TNode currentNode = GetNodeByID(node.inputConnections[i].previousNodeID);
                    MakeChain(ref nodesList, currentNode);
                }
            }
        }

        // Proper sequence of nodes
        public bool GetSequences(ref List<TSequence> sequences, ConnectionDataType DataType)
        {
            TDebug.TraceMessage();
            sequences.Clear();
            List<TNode> lastnodes = GetLastNodes(DataType);
            if (lastnodes.Count == 0) return false;

            for (int u = 0; u < lastnodes.Count; u++)
            {
                TSequence sequence = new TSequence();
                List<TNode> nodesList = new List<TNode>();
                MakeChain(ref nodesList, lastnodes[u]);

                for (int i = nodesList.Count - 1; i >= 0; i--)
                    sequence.Nodes.Add(nodesList[i]);

                sequences.Add(sequence);
            }

            return true;
        }

        public int UpdateInputList(ref Dictionary<TNode, string> InputList, TNode module, int inputIndex)
        {
            ConnectionDataType dataType = module.inputConnections[inputIndex].connectionDataType;
            InputList.Clear();
            InputList.Add(new TNullNode(), "No Filter");
            int result = 0;
            TNode node = null;

            for (int i = 0; i < graphList.Count; i++)
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                {
                    node = graphList[i].nodes[j];

                    if (node != module && node.outputConnectionType == dataType)
                        InputList.Add(node, node.Data.name);
                }

            node = GetNodeByID(module.inputConnections[inputIndex].previousNodeID);

            for (int j = 0; j < InputList.Count; j++)
                if (node == InputList.Keys.ToList()[j])
                    result = j;

            return result;
        }

        public void ResetGraphsStatus()
        {
            TDebug.TraceMessage();

            for (int i = 0; i < graphList.Count; i++)
                graphList[i].ResetNodesStatus();
        }

        //public void ResetBoundingBoxesArea()
        //{
        //    TDebug.TraceMessage();
        //
        //    for (int i = 0; i < graphList.Count; i++)
        //        for (int j = 0; j < graphList[i].nodes.Count; j++)
        //            graphList[i].nodes[j].ResetAreaBound();
        //}

        //public void HandleAreaWorldChange()
        //{
        //    TDebug.TraceMessage();
        //    ResetBoundingBoxesArea();
        //}

        public int GetLastTypeIndex(TNode node)
        {
            int result = 0;

            for (int i = 0; i < graphList.Count; i++)
            {
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                {
                    if (graphList[i].nodes[j].GetType() == node.GetType())
                    {
                        if (result < graphList[i].nodes[j].NodeTypeIndex)
                            result = graphList[i].nodes[j].NodeTypeIndex;
                    }
                }
            }

            return result;
        }
    }
}
#endif
#endif

