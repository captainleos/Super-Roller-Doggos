#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TerraUnity.Edittime
{
    public enum ModuleType
    {
        Processor,
        Operator,
        Mask,
        Extractor,
        Scatter,
        TerraMesh,
        Master
    }

    public enum DataType
    {
        Heightmap,
        Image
    }

    public enum ConnectionDataType
    {
        Area,
        Heightmap,
        Mask,
        Image,
        OSM,
        Lakes,
        DetailTexture,
        ObjectScatter,
        InstanceScatter,
        GrassScatter,
        Global,
        Mesh,
        FXParameters,
        HeightmapMaster,
        ColormapMaster,
        DetailTextureMaster,
        Player
    }

    public struct UIToggle
    {
        public bool BoundingBox;
        public bool Outputs;
        public bool Inputs;
        public bool Settings1;
        public bool Settings2;
        public bool Settings3;
        public bool Settings4;
        public bool Settings5;
        public bool Settings6;
        public bool Settings7;
        public bool Settings8;
        public bool Settings9;
        public bool Settings10;
        public bool Settings11;
        public bool Settings12;
        public bool Settings13;
    }

    public struct SectionToggle
    {
        public bool Settings1;
        public bool Settings2;
        public bool Settings3;
        public bool Settings4;
        public bool Settings5;
    }

    public class TConnection
    {
        public string _title;
        public int _ID;
        public int ID { get => _ID; }
        public int previousNodeID = -1;
        public int nextNodeID = -1;
        public ConnectionDataType _connectionDataType;
        public ConnectionDataType connectionDataType { get => _connectionDataType; }
        public string Title { get => _title; }
        public bool required = true;

        public TConnection(TNode previousNode, TNode nextNode, ConnectionDataType connectionDataType, bool required, string title)
        {
            if (previousNode != null) this.previousNodeID = previousNode.Data.ID;
            if (nextNode != null) this.nextNodeID = nextNode.Data.ID;
            _connectionDataType = connectionDataType;

            _ID = TTerraWorldGraph.GetNewID();
            if (!string.IsNullOrEmpty(title)) _title = title;
            this.required = required;
        }

        // Parameterless constructor for serialization
        public TConnection ()
        {
            _ID = TTerraWorldGraph.GetNewID();
        }
    }

    public enum NodePosition
    {
        _1,
        _2,
        _3,
        _4,
        _5,
        _Float
    };

    public struct NodeData
    {
        public int ID;
        public string name;
        public Vector4 position;
        public ModuleType moduleType;
        public NodePosition nodePosition;
    }

    public class TGraph
    {
        public int _id;
        public bool isActive = true;
        public string _title;

        public List<TNode> nodes = new List<TNode>();
        [XmlIgnore] public List<TConnection> connections = new List<TConnection>();
        [XmlIgnore] public bool isInProgress = false;
        [XmlIgnore] public bool isSyncing = false;
        [XmlIgnore] public float progress;
        [XmlIgnore] public TTerraWorldGraph worldGraph ;
        [XmlIgnore] private List<TSequence> _actionSequences = new List<TSequence>();
        [XmlIgnore] public ConnectionDataType _outputDataType;

        public TGraph(ConnectionDataType outputDataType , string title)
        {

            _id = TTerraWorldGraph.GetNewID();
            _title = title;
            _outputDataType = outputDataType;
        }

        public TNode GetNodeByID(int ID)
        {
            return worldGraph.GetNodeByID(ID);
        }

        public List<TNode> GetOutputNodes(int ID)
        {
            return worldGraph.GetOutputNodes(ID);
        }

        public void UpdateConnections ()
        {
            connections.Clear();

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].inputConnections != null)
                {
                    for (int j = 0; j < nodes[i].inputConnections.Count; j++)
                    {
                        TConnection connection = nodes[i].inputConnections[j];

                        if (connection.previousNodeID != -1)
                            connections.Add(connection);
                    }
                }
            }
        }

        public virtual bool CheckConnections ()
        {
            // Checks for nodes consequentiality
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!nodes[i].CheckConnections())
                    return false;
            }

            List<TNode> lastnodes = GetLastNodes(_outputDataType);
            List<TNode> chainNodes = new List<TNode>();

            for (int i = 0; i < lastnodes.Count; i++)
            {
                if (lastnodes[i].LoopConnections(chainNodes))
                    throw new Exception("Loop detected in graph : " + _title + "\n\n Pleas Check The Graph");

            }

            return true;
        }

        public virtual List<TNode> GetLastNodes(ConnectionDataType connectionDataType)
        {
            List<TNode>  result = nodes.GetRange(0, nodes.Count);

            if (result.Count == 0)
                return result;

            for (int u = 0; u < result.Count; u++)
            {
                if (result[u].outputConnectionType != connectionDataType)
                    result.Remove(result[u--]);
            }

            if (result.Count == 0)
                return result;

            for (int i = 0; i < nodes.Count; i++)
            for (int j = 0; j < nodes[i].inputConnections.Count; j++)
                for (int u = 0; u < result.Count; u++)
                {
                    if (result[u].Data.ID == nodes[i].inputConnections[j].previousNodeID && nodes[i].outputConnectionType == connectionDataType)
                    result.Remove(result[u--]);
                }

            if (result.Count < 2)
                return result;

            for (int i = 0; i < nodes.Count; i++)
                for (int j = 0; j < nodes[i].inputConnections.Count; j++)
                    for (int u = 0; u < result.Count; u++)
                    {
                        if (result[u].Data.ID == nodes[i].inputConnections[j].previousNodeID)
                            result.Remove(result[u--]);
                    }

            return result;
        }

        public void RemoveNode (int ID)
        {
            worldGraph.RemoveNodeByID(ID);
            TNode detectedNode = null;
            int nodeIndex = -1;
       
            for (int i = 0; i < nodes.Count; i++)
            {
                if(nodes[i].Data.ID == ID)
                {
                    detectedNode = nodes[i];
                    nodeIndex = i;
                }
            }
        }

        public void RemoveConnection (TNode node1, TNode node2)
        {
            for (int i = 0; i < node1.inputConnections.Count; i++)
            {
                if (node1.inputConnections[i].previousNodeID == node2.Data.ID)
                    node1.inputConnections[i].previousNodeID = -1;
            }

            for (int i = 0; i < node2.inputConnections.Count; i++)
            {
                if (node2.inputConnections[i].previousNodeID == node1.Data.ID)
                    node2.inputConnections[i].previousNodeID = -1;
            }
        }

        public int GetActiveNodesCount ()
        {
            int activeNodes = 0;

            for (int i = 1; i < nodes.Count; i++)
                if (nodes[i].isActive)
                {
                    if(!isSyncing)
                        activeNodes++;
                    else if (!nodes[i].isDone)
                        activeNodes++;
                }   

            return activeNodes;
        }

        public int initSequences(ConnectionDataType outputDataType)
        {
            List<TNode> result = new List<TNode>();
            List<TSequence> actionSequences = new List<TSequence>();
            if (TTerrainGenerator.WorldInProgress) 
            worldGraph.GetSequences(ref actionSequences, outputDataType);

            int nodeCount = 0;

            for (int u = 0; u < actionSequences.Count; u++)
            {
                nodeCount += actionSequences[u].Nodes.Count;
            }

            return nodeCount;
        }

        public List<TNode> RunGraph(TTerrain CurrentTerrain, ConnectionDataType outputDataType)
        {
            TDebug.TraceMessage();
            List<TNode> result = new List<TNode>();
            List<TSequence> actionSequences = new List<TSequence>();
            if (TTerrainGenerator.WorldInProgress) 
            worldGraph.GetSequences(ref actionSequences, outputDataType);

            for (int u = 0; u < actionSequences.Count; u++)
            {
                TNode currentNode = null;
                List<TNode> sequence = actionSequences[u].Nodes;
                bool reRun = false;

                if (sequence.Count > 0)
                {
                    for (int i = 0; i < sequence.Count; i++)
                    {
                        if (TTerrainGenerator.WorldInProgress) 
                        {
                            currentNode = sequence[i];

                            if (currentNode.isDone == false) reRun = true;

                            if (reRun)
                            {
                                if (currentNode.isActive) TDebug.TraceMessage(currentNode.Data.name);
                                if (currentNode.isActive && currentNode.isDone == false && String.IsNullOrEmpty(TTerraWorld.TemplateName))
                                    TTerraWorld.FeedbackEvent(EventCategory.Nodes, currentNode.GetType().ToString(), currentNode.GetMainResource());
                                currentNode.ModuleAction(CurrentTerrain.Map);
                            }
                        }
                    }
                }

                result.Add(currentNode);
            }

            return result;
        }

        public void ResetNodesStatus ()
        {
            TDebug.TraceMessage();

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Progress = 0;
                nodes[i].isDone = false;
            }
        }

        public void RunAllDone()
        {

        }
    }

    [XmlType("TNode")]
    [
        XmlInclude(typeof(WorldArea)),
        XmlInclude(typeof(HeightmapSource)),
        XmlInclude(typeof(SmoothProcess)),
        XmlInclude(typeof(HydraulicErosionMainProcess)),
        XmlInclude(typeof(HydraulicErosionProcess)),
        XmlInclude(typeof(WaterErosionProcess)),
        XmlInclude(typeof(ThermalErosionProcess)),
        XmlInclude(typeof(TerraceProcess)),
        XmlInclude(typeof(SatelliteImage)),
        XmlInclude(typeof(ShadowRemover)),
        //XmlInclude(typeof(ColormapFromSlope)),
        XmlInclude(typeof(ApplyMask)),
        XmlInclude(typeof(MaskBlendOperator)),
        XmlInclude(typeof(WaterGenerator)),
        XmlInclude(typeof(MaskBlendOperator)),
        //XmlInclude(typeof(Slopemap)),
        XmlInclude(typeof(Flowmap)),
        XmlInclude(typeof(Slopemask)),
        XmlInclude(typeof(Image2Mask)),
        //XmlInclude(typeof(Curvaturemap)),
        //XmlInclude(typeof(Normalmap)),
        //XmlInclude(typeof(Aspectmap)),
        XmlInclude(typeof(Mask2DetailTexture)),
        XmlInclude(typeof(Mask2ColorMap)),
        XmlInclude(typeof(ObjectScatter)),
        XmlInclude(typeof(TreeScatter)),
        XmlInclude(typeof(InstanceScatter)),
        XmlInclude(typeof(RenderingNode)),
        XmlInclude(typeof(FXNode)),
        XmlInclude(typeof(MeshGenerator)),
        XmlInclude(typeof(BiomeExtractor)),
        XmlInclude(typeof(HeightmapMaster)),
        XmlInclude(typeof(ColormapMaster)),
        XmlInclude(typeof(TerrainLayerMaster1)),
        XmlInclude(typeof(TerrainLayerMaster2)),
        XmlInclude(typeof(TerrainLayerMaster3)),
        XmlInclude(typeof(TerrainLayerMaster4)),
        XmlInclude(typeof(PlayerNode)),
        XmlInclude(typeof(GrassScatter)),
        XmlInclude(typeof(VoxelProcess))
    ]
    public abstract class TNode
    {
        [XmlArray("inputConnections")]
        [XmlArrayItem("TConnection")]

        [XmlIgnore] public TGraph parentGraph;

        [XmlIgnore] public bool isDirty = true;
        [XmlIgnore] public bool isDone = false;

        public bool isActive = true;
        public bool isRemovable = true;
        public bool isRunnable = true;
        public bool isSource = false;
        public bool isSwitchable = true;

        public List<TConnection> inputConnections;
        public UIToggle uIToggles;
        public SectionToggle sectionToggles;
        public ConnectionDataType outputConnectionType;
        public NodeData Data;
        //public TAreaBounds areaBounds;
        public int NodeTypeIndex;
        public string parentGraphName;

        internal float _progress = 0;
        public float Progress { get => _progress; set => _progress = value; }

        //  private Type _type;
        //  public string type { get => _type.FullName; set => _type = Type.GetType(value); }
        //  public string Type { get => GetType().ToString(); }

        private List<TMask> _masks;
        [XmlIgnore] public List<TMask> OutMasks { get => _masks; set => _masks = value; }

        public TNode ()
        {
            OutMasks = new List<TMask>();
            Data.ID = TTerraWorldGraph.GetNewID();

            //areaBounds.top = 90;
            //areaBounds.left = -180;
            //areaBounds.bottom = -90;
            //areaBounds.right = 180;
            //
            //if (TTerraWorld.WorldGraph != null && TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource() != null)
            //{
            //    areaBounds.maxElevation = 8848f * TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration;
            //    areaBounds.minElevation = -500f * TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration;
            //}
            //else
            //{
            //    areaBounds.maxElevation = 8848f;
            //    areaBounds.minElevation = -500f;
            //}

            Data.nodePosition = NodePosition._Float;
        }

        public TNode(int ID):this()
        {
            Data.ID = ID;
        }

        public TConnection GetInputConnection (ConnectionDataType connectionDataType)
        {
            TConnection result = null;

            for (int i = 0; i < inputConnections.Count; i++)
                if (inputConnections[i].connectionDataType == connectionDataType)
                    result = inputConnections[i];

            return result;
        }

        public TConnection GetInputConnection (TNode otherNode)
        {
            TConnection result = null;

            for (int i = 0; i < inputConnections.Count; i++)
                if (inputConnections[i].previousNodeID == otherNode.Data.ID)
                    result = inputConnections[i];

            return result;
        }

        public void AddInputConnection (TNode newInputNode, int inputConnectionIndex, int newInputNodeOutputConnectionIndex = -1 )
        {
            if (newInputNode == null)
                return;

            if (newInputNode == this)
                return;

            if (inputConnections.Count < 1)
                return;

            if (newInputNode.Data.ID != -1 && inputConnections[inputConnectionIndex].connectionDataType != newInputNode.outputConnectionType) return;

            ConnectionDataType connectionDataType = inputConnections[inputConnectionIndex].connectionDataType;
            inputConnections[inputConnectionIndex].previousNodeID = newInputNode.Data.ID;

            if (parentGraph != null)
                parentGraph.UpdateConnections();
        }

        public bool CheckConnections ()
        {
            if (!CheckInputConnections()) return false;

            return true;
        }

        public bool CheckInputConnections()
        {
            if (inputConnections == null || inputConnections.Count <= 0 || isSource)
                return true;

            for (int i = 0; i < inputConnections.Count; i++)
            {
                if (outputConnectionType == ConnectionDataType.ColormapMaster) return true;
                if (outputConnectionType == ConnectionDataType.DetailTextureMaster) return true;

                if (inputConnections[i] != null && inputConnections[i].previousNodeID == -1)
                    throw new Exception("Input Connection Error - Node : " + Data.name + " \n \n Check Input Parameter For Node");
            }

            return true;
        }

        public bool LoopConnections(List<TNode> chainList)
        {
            if (inputConnections == null || inputConnections.Count <= 0 || isSource)
                return false;

            for (int i = 0; i < chainList.Count; i++)
                if (this == chainList[i])
                    return true;

            chainList.Add(this);

            for (int i = 0; i < inputConnections.Count; i++)
            {
                if (inputConnections[i] != null && inputConnections[i].previousNodeID != -1)
                {
                    TNode inputNode = parentGraph.worldGraph.GetNodeByID(inputConnections[i].previousNodeID);
                    if (inputNode.LoopConnections(chainList)) return true;
                }
            }

            return false;
        }

        public virtual void ModuleAction(TMap CurrentMap)
        {

        }

        public virtual void InitConnections()
        {

        }

        public abstract List<string> GetResourcePaths();

        public string GetMainResource()
        {
            string result;

            try
            {
                List<string> vs = GetResourcePaths();

                if (vs == null || vs.Count == 0) 
                    result = "-";
                else
                    result = Path.GetFileNameWithoutExtension(vs[0]);
            }
            catch (Exception)
            {
                result = ".";
            }

            return result;
        }

        public void Init(TGraph ParentGraph)
        {
            if (ParentGraph == null)
                throw new Exception("Internal Error: Null Parent Graph.");
            else
            {
                this.parentGraph = ParentGraph;
                parentGraphName = ParentGraph._title;
            }

            if (parentGraph.worldGraph == null)
            {
                throw new Exception("Internal Error: Null World Graph.");
            }
            NodeTypeIndex = parentGraph.worldGraph.GetLastTypeIndex(this) + 1;
            Data.name = Data.name + " " + NodeTypeIndex;
            InitConnections();
        }

        public Vector2 center()
        {
            Vector2 result = Vector2.Zero;
            result.X = Data.position.X + (Data.position.Z / 2f);
            result.Y = Data.position.Y + (Data.position.W / 2f);
            return result;
        }

        public float xMax()
        {
            return Data.position.X + Data.position.Z;
        }

        public float xMin ()
        {
            return Data.position.X;
        }

        public float yMax ()
        {
            return Data.position.Y;
        }

        public float yMin ()
        {
            return Data.position.Y + Data.position.W;
        }

        //public void ResetAreaBound ()
        //{
        //    areaBounds.top = 90;
        //    areaBounds.bottom = -90;
        //    areaBounds.left = -180;
        //    areaBounds.right = 180;
        //    areaBounds.minElevation = -500;
        //    areaBounds.maxElevation = 100000;
        //}
    }

    public class TNullNode : TNode
    {
        public TNullNode():base()
        {
            Data.ID = -1;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }
    }
}
#endif
#endif
