#if TERRAWORLD_XPRO
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TerraUnity.Edittime;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

namespace TerraUnity.Graph
{
    [Serializable]
    public abstract class TXNode : Node
    {
        //public Texture2D PreviewTexture;
        //public int PreviewTextureSize = 100;
        private int _id;
        public int ID { get => _id; set => _id = value; }
        private bool isDone = false;
        public bool IsDone { get => isDone; set => isDone = value; }
        private bool isActive = true;
        public bool IsActive { get => isActive; set => isActive = value; }
        private int progress;
        public int Progress { get => progress; set => progress = value; }

        [HideInInspector] public int nodeTypeIndex;
        public int NodeTypeIndex { get => nodeTypeIndex; set => nodeTypeIndex = value; }

        private string nodeName;
        public string NodeName { get => nodeName; }

        protected float _progress;

     //   protected TAreaBounds areaBounds;
        //[SerializeField] public bool IsPreviewDropdown;

        public TXNode():base()
        {
            _id = TTerraWorldGraph.GetNewID();
        }

        public override object GetValue(NodePort port)
        {
            return this;
        }

        public virtual void CheckEssentioalInputs()
        {
            throw new Exception("CheckEssentioalInputs() Should Impliment for " + nodeName);
        }

        protected virtual void ModuleAction(TMap currentMap)
        {
            throw new Exception("ModuleAction not implemented for " + nodeName);
        }

        protected override void Init()
        {
            base.Init();
        }
        public void SetName( string _name)
        {
          //  if (nodeTypeIndex != 0) return;
          //  if (graphHotfix == null) return;
          //  nodeTypeIndex = ((TXGraph)graph).getlastTypeIndex((Node)this) + 1;
          //  this.name = name + " ("+ nodeTypeIndex+")";
            nodeName = _name;
        }

        public static string[] FieldNames
        {
            get
            {
                return new[] { "PreviewTexture", "PreviewTextureSize", "IsPreviewDropdown" };
            }
        }

        public void ResetNodesStatus()
        {
            isDone = false;
            progress = 0;
        }


    }
}
#endif