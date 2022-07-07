/*
using System.Collections.Generic;
using TerraUnity.Edittime;
using UnityEngine;


namespace TerraUnity.Graph
{
    [CreateNodeMenu("End Nodes/TerrainLayers")]
    [NodeWidth(300), NodeTint(214, 255, 255)]

    public class TXTerrainLayerEndNode : TXNode
    {
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXImageModules ColorMap;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXTerrainLayerModules TextureLayer1;
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXTerrainLayerModules TextureLayer2;
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXTerrainLayerModules TextureLayer3;
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXTerrainLayerModules TextureLayer4;

        protected override void Init()
        {
            base.Init();
            SetName("Texture Layers EndNode");
        }


        /// <summary>
        /// Returns the "final" heightmap attached to this 
        /// node's input
        /// </summary>
        public TImage GetColorMap(TTerrain terrain)
        {
            TXImageModules iv = GetInputValue<TXImageModules>("ColorMap");
            if (iv != null) return iv.GetImage(terrain);
            return null;
        }


        public override void CheckEssentioalInputs()
        {
        }


    }


}
*/