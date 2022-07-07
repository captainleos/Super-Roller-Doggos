using System;
/*
using UnityEngine;


namespace TerraUnity.Graph
{
    [CreateNodeMenu("End Nodes/Heightmap EndNode")]
    public class TXHeightMapEndNode : TXNode
    {
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]

        public TXHeightmapModules Input;
        // [Output] public float[,] b;

        protected override void Init()
        {
            base.Init();
            SetName("Heightmap EndNode");
        }


        /// <summary>
        /// Returns the "final" heightmap attached to this 
        /// node's input
        /// </summary>
        public float[,] GetProceededHeightMap(TTerrain terrain)
        {
            CheckEssentioalInputs();
            var iv = GetInputValue<TXHeightmapModules>("Input");
            return iv == null ? null : iv.GetProceededHeightMap(terrain);
        }

        public override void CheckEssentioalInputs()
        {
            var iv = GetInputValue<TXNode>("Input");
            if (iv == null) throw new Exception("Input" + " is missed for " + NodeName + " Node.");
        }

        //   private void Reset()
        //   {
        //       name = _heightmapMaster.Data.name;
        //   }

        //  public float[,] GetFinalHeightmap()
        //  {
        //      var iv = GetInputValue<THeightmap>("HeightmapRawData");
        //      return null ;
        //  }

        //  public override object GetValue(XNode.NodePort port)
        //
        //  {
        //      if (port.fieldName == "b") return GetInputValue<float[,]>("HeightmapRawData", HeightmapRawData);
        //      else return null;
        //  }

    }

 
}
*/