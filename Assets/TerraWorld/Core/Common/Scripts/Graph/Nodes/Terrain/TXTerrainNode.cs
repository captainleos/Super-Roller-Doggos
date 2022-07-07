#if TERRAWORLD_XPRO
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerraUnity.Edittime;
using UnityEngine;
using XNode;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Generators/Terrain")]
    public class TXTerrainNode : TXNode
    {
        [Range(1, 200)] public int PixelError = 5;
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXHeightmapModules Heightmap;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXImageModules ColorMap;

        protected override void Init()
        {
            base.Init();
            SetName("Terrain Generator");
            AddDynamicPorts();
        }


        /// <summary>
        /// Returns the "final" heightmap attached to this 
        /// node's input
        /// </summary>
        public float[,] GetProceededHeightMap(TTerrain terrain)
        {
            CheckEssentioalInputs();
            var iv = GetInputValue<TXHeightmapModules>("Heightmap");
            return iv == null ? null : iv.GetProceededHeightMap(terrain);
        }

        public TImage GetColorMap(TTerrain terrain)
        {
            TXImageModules iv = GetInputValue<TXImageModules>("ColorMap");
            if (iv != null) return iv.GetImage(terrain);
            return null;
        }

        public List<TDetailTexture> GetDetailedTextures(TTerrain terrain)
        {
            List<TDetailTexture> _result = new List<TDetailTexture>();
            foreach (NodePort port in DynamicInputs)
            {
                TXTerrainLayerModules TextureLayer = GetInputValue<TXTerrainLayerModules>(port.fieldName);
                if (TextureLayer != null)
                    _result.Add(TextureLayer.GetTerrainLayer(terrain));
            }
            return _result;
        }



        public override void CheckEssentioalInputs()
        {
            Heightmap = GetInputValue<TXHeightmapModules>("Heightmap");
            if (Heightmap == null) throw new Exception("Heightmap" + " is missed for " + NodeName + " Node.");
        }

        void AddDynamicPorts()
        {
            if (DynamicInputs.ToList().Count() >= 4) return;
            foreach (NodePort port in DynamicInputs)
            {
                if (!port.IsConnected ) return ;
            }

            AddDynamicInput(typeof(TXTerrainLayerModules), fieldName: "TerrainLayer" + (DynamicInputs.ToList().Count()+1).ToString());
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            AddDynamicPorts();
            base.OnCreateConnection(from, to);

            //if (to.GetType() == typeof(TXTerrainLayerModules) || from.GetType() == typeof(TXTerrainLayerModules)) AddDynamicPorts();
        }

    }


}
#endif