#if TERRAWORLD_XPRO
using System;
using UnityEngine;
using XNode;
using XNodeEditor;
using UnityEditor;
using TerraUnity.Edittime;
using System.Collections.Generic;

namespace TerraUnity.Graph
{
    public enum TXHeightmapConnectionType { Heightmap }
    /// <summary> Defines a noise graph that can be created as an asset using the Terra dropdown.</summary>
    [Serializable, CreateAssetMenu(fileName = "New Graph", menuName = "TerraWorld/Graph")]
    public class TXGraph : NodeGraph
    {
        public static TXNode xSelectedNode;

        /// <summary>
        /// Returns the Terrain generator Node if 
        /// exists.
        /// </summary>
        public TXTerrainNode GetTerrainNode()
        {
            foreach (Node n in nodes)
            {
                if (n is TXTerrainNode)
                {
                    return ((TXTerrainNode)n);
                }
            }
            return null;
        }

        public List<TXWaterGeneratorNode> GetWaterGeneratorNodes()
        {
            List<TXWaterGeneratorNode> result = new List<TXWaterGeneratorNode>();
            foreach (Node n in nodes)
            {
                if (n is TXWaterGeneratorNode)
                {
                    result.Add((TXWaterGeneratorNode)n);
                }
            }

            return result;
        }

        public List<TXMeshGeneratorNode> GetMeshGeneratorNodes()
        {
            List<TXMeshGeneratorNode> result = new List<TXMeshGeneratorNode>();
            foreach (Node n in nodes)
            {
                if (n is TXMeshGeneratorNode)
                {
                    result.Add((TXMeshGeneratorNode)n);
                }
            }

            return result;
        }


        public List<TXTreeScatterNode> GetTreeScatterNodes()
        {
            List<TXTreeScatterNode> result = new List<TXTreeScatterNode>();
            foreach (Node n in nodes)
            {
                if (n is TXTreeScatterNode)
                {
                    result.Add((TXTreeScatterNode)n);
                }
            }

            return result;
        }


        public List<TXObjectScatterNode> GetObjectScatterNodes()
        {
            List<TXObjectScatterNode> result = new List<TXObjectScatterNode>();
            foreach (Node n in nodes)
            {
                if (n is TXObjectScatterNode)
                {
                    result.Add((TXObjectScatterNode)n);
                }
            }

            return result;
        }

        public List<TXInstanceScatterNode> GetInstanceScatterNodes()
        {
            List<TXInstanceScatterNode> result = new List<TXInstanceScatterNode>();
            foreach (Node n in nodes)
            {
                if (n is TXInstanceScatterNode)
                {
                    result.Add((TXInstanceScatterNode)n);
                }
            }

            return result;
        }

        public TXRealWorldSourceNode GetRealWorldSourceNode()
        {
            foreach (Node n in nodes)
            {
                if (n is TXRealWorldSourceNode)
                {
                    return ((TXRealWorldSourceNode)n);
                }
            }

            return null;
        }

        public void ResetAllNodesStatus()
        {
            foreach (Node n in nodes)
            {
                if (n is TXNode)
                {
                    ((TXNode)n).ResetNodesStatus();
                }
            }
        }

        public int getlastTypeIndex(TXNode node)
        {
            int lastIndex = 0;
            foreach (Node n in nodes)
            {
                if (n?.GetType() == node?.GetType())
                {
                    if (((TXNode)n).NodeTypeIndex > lastIndex) lastIndex = ((TXNode)n).NodeTypeIndex;
                }
            }

            return lastIndex;
        }

        public void CheckNodesEssenssialInputs()
        {
            foreach (Node n in nodes)
            {
                if (n is TXNode)
                {
                    ((TXNode)n).CheckEssentioalInputs();
                }
            }
        }

        public override Node AddNode(Type type)
        {
            if (type == typeof(TXRealWorldSourceNode))
            {
                if (GetRealWorldSourceNode() != null)
                {
                    EditorUtility.DisplayDialog("TERRAWORLD", "Just one area node can be placed on the graph.", "Ok");
                    return null;
                }
            }


            if (type == typeof(TXTerrainNode))
            {
                if (GetTerrainNode() != null)
                {
                    EditorUtility.DisplayDialog("TERRAWORLD", "Just one terrain generator node can be placed on the graph.", "Ok");
                    return null;
                }
            }

            Node node = base.AddNode(type);

            if (node is TXNode)
            {
                TXNode n = (TXNode)node;
                n.NodeTypeIndex = getlastTypeIndex(n) + 1;
                n.name = n.NodeName + " (" + n.NodeTypeIndex + ")";
                n.SetName(n.name);
            }

            return node;
        }

        public int GetNodesCount()
        {
            return nodes.Count;
        }

        public void CheckConnections()
        {
            foreach (Node n in nodes)
            {
                if (n is TXNode)
                {
                    ((TXNode)n).CheckEssentioalInputs();
                }
            }
        }

        public void ResetNodesStatus()
        {
            foreach (Node n in nodes)
            {
                if (n is TXNode)
                {
                    ((TXNode)n).IsDone = false;
                }
            }
        }


        //   public TXGraph( bool addSourceNode)
        //   {
        //       TXSatImageSourceNode sn = (TXSatImageSourceNode)AddNode(typeof(TXSatImageSourceNode));
        //       sn.position = new Vector2(-500, 10);
        //       EditorUtility.SetDirty(sn);
        //       AssetDatabase.SaveAssets();
        //   }




    }



}
#endif