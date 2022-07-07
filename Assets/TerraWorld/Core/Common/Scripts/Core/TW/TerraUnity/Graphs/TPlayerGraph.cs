#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TerraUnity.Edittime
{
    public class TPlayerModules : TNode
    {
        public TPlayerModules() : base()
        {
            isSource = true;
        }

        public override List<string> GetResourcePaths()
        {
            throw new System.Exception("GetResourcePaths");
        }
    }

    [XmlType("PlayerNode")]
    public class PlayerNode : TPlayerModules
    {
        public PlayerNode() : base()
        {
         //   type = typeof(PlayerNode).FullName;
            Data.name = "Player Manager";
            Data.moduleType = ModuleType.Processor;
            isRemovable = false;
            isRunnable = false;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.Player;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }
    }

    public class TPlayerGraph : TGraph
    {
        private static PlayerNode playerNode;

        public TPlayerGraph() : base(ConnectionDataType.Player, "Player Graph")
        {
        }

        // -----------------------------------------------------------------------------------------------------------------------------------

        public void InitGraph(TTerraWorldGraph terraWorldGraph)
        {
            worldGraph = terraWorldGraph;

            _title = "PLAYER";
            if (nodes.Count > 0) return;
            playerNode = new PlayerNode();
            playerNode.Init(this);
            nodes.Add(playerNode);
        }

        public PlayerNode PlayerNode
        {
            get
            {
                return nodes[0] as PlayerNode;
            }
        }
    }
}
#endif
#endif

