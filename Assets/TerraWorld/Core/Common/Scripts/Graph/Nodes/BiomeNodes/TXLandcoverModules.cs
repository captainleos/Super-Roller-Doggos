#if TERRAWORLD_XPRO
using System.Xml;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    public class TXLandcoverModules : TXNode ,TXLandcoverInterface
    {
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
#endif