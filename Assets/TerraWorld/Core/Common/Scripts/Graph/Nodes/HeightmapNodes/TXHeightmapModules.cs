#if TERRAWORLD_XPRO
using System;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [NodeTint(254, 173, 209)]
    public class TXHeightmapModules : TXNode , TXHeightmapInterface
    {
        [Output] public TXHeightmapModules Output;
        protected  float[,] _heightmapData;

        public float[,] GetProceededHeightMap(TTerrain terrain)
        {
            if (IsDone) return _heightmapData;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return _heightmapData;
        }

    }
}
#endif
