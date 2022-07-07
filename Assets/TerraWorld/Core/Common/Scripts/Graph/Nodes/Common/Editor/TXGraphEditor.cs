#if TERRAWORLD_XPRO
using System;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace TerraUnity.Graph.Editor
{
    [CustomNodeGraphEditor(typeof(TXGraph))]
    public class TXGraphEditor : NodeGraphEditor
    {
        public override void OnGUI()
        {
            base.OnGUI();

            //Debug.Log(NodeEditorWindow.current.name);


            if (Selection.objects.Length == 1 && Selection.objects[0] is XNode.Node)
            {
                TXNode node = Selection.objects[0] as TXNode;
                if (node != TXGraph.xSelectedNode)
                {
                    //       TXGlobalGraph.xSelectedNode = node;
                    //       THeightmapGraph activeGraph = TTerraWorld.WorldGraph.heightmapGraph;
                    //       TerraWorld.activeGraph = activeGraph;
                    //       THelpersUI.ActiveNode = activeGraph.GetNodeByID(node.ID); 
                }
            }

        }

        /// <summary> Override to display custom tooltips </summary>
        public override string GetPortTooltip(XNode.NodePort port)
        {
            string tooltip = base.GetPortTooltip(port);
            Type portType = port.ValueType;
            if (typeof(TXHeightmapModules) == portType) tooltip = "Heightmap Data";
            if (typeof(TXImageModules) == portType) tooltip = "Image Data";
            if (typeof(TXLandcoverModules) == portType) tooltip = "Landcover Data";
            if (typeof(TXMaskModules) == portType) tooltip = "Mask Data";
            if (typeof(TXTerrainLayerModules) == portType) tooltip = "Terrain Layer Data";

            //
            //  tooltip = portType.PrettyName();
            //  if (port.IsOutput)
            //  {
            //      object obj = port.node.GetValue(port);
            //      tooltip += " = " + (obj != null ? obj.ToString() : "null");
            //  }
            return tooltip;
        }

        /// <summary> 
        /// Overriding GetNodeMenuName lets you control if and how nodes are categorized.
        /// In this example we are sorting out all node types that are not in the XNode.Examples namespace.
        /// </summary>
      //  public override string GetNodeMenuName(System.Type type)
      //  {
      //      if (type.Namespace == "XNode.Examples.StateGraph")
      //      {
      //          return base.GetNodeMenuName(type).Replace("X Node/Examples/State Graph/", "");
      //      }
      //      else return base.GetNodeMenuName(type);
      //  }

        public override void AddContextMenuItems(GenericMenu menu)
        {

            TXGraph tXGraph = (TXGraph)target;


            if (tXGraph.GetRealWorldSourceNode() == null)
            {
                Vector2 pos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);
                {
                    Type type = typeof(TXRealWorldSourceNode);

                    //Get node context menu path
                    string path = GetNodeMenuName(type);
                    menu.AddItem(new GUIContent(path), false, () =>
                    {
                        XNode.Node node = CreateNode(type, pos);
                        NodeEditorWindow.current.AutoConnect(node);
                    });
                }
            }
            else if (tXGraph.GetTerrainNode() == null)
            {
                Vector2 pos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);
                {
                    Type type = typeof(TXTerrainNode);

                    //Get node context menu path
                    string path = GetNodeMenuName(type);
                    menu.AddItem(new GUIContent(path), false, () =>
                    {
                        XNode.Node node = CreateNode(type, pos);
                        NodeEditorWindow.current.AutoConnect(node);
                    });
                }
            }
            else
            {
                Vector2 pos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);
                for (int i = 0; i < NodeEditorReflection.nodeTypes.Length; i++)
                {
                    Type type = NodeEditorReflection.nodeTypes[i];
                    if (type == typeof(TXTerrainNode)) continue;
                    if (type == typeof(TXRealWorldSourceNode)) continue;
                    //Get node context menu path
                    string path = GetNodeMenuName(type);
                    if (string.IsNullOrEmpty(path)) continue;
                    if (path.Contains("Terra Unity")) continue;

                    menu.AddItem(new GUIContent(path), false, () => {
                        XNode.Node node = CreateNode(type, pos);
                        NodeEditorWindow.current.AutoConnect(node);
                    });
                }
                menu.AddSeparator("");
                if (NodeEditorWindow.copyBuffer != null && NodeEditorWindow.copyBuffer.Length > 0) menu.AddItem(new GUIContent("Paste"), false, () => NodeEditorWindow.current.PasteNodes(pos));
                else menu.AddDisabledItem(new GUIContent("Paste"));
                menu.AddItem(new GUIContent("Preferences"), false, () => NodeEditorReflection.OpenPreferences());
                menu.AddCustomContextMenuItems(target);
            }

            


        }
    }
}
#endif
