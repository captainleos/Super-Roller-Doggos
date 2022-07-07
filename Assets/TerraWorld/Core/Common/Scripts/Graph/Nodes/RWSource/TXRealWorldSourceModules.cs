/*
using System;
using System.Xml;
using TerraUnity.Edittime;
using XNode;

namespace TerraUnity.Graph
{
    public class TXRealWorldSourceModules : TXNode, TXImageInterface, TXLandcoverInterface
    {
        [Output] public TXHeightmapModules Output;

        protected float[,] _heightmapData;

        public float[,] GetProceededHeightMap(TTerrain terrain)
        {
            if (IsDone) return _heightmapData;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return _heightmapData;
        }

        [Output] public TXImageModules Image;

        protected TImage _outputImage;

        public TImage GetImage(TTerrain terrain)
        {
            if (IsDone) return _outputImage;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return _outputImage;
        }

        [Output] public TXLandcoverModules Landcover_Data;

        protected XmlDocument _outputLandcover;
        public XmlDocument GetProceededLandcover(TTerrain terrain)
        {
            if (IsDone) return _outputLandcover;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return _outputLandcover;
        }

    }
}
*/
