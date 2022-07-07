#if TERRAWORLD_XPRO
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{

    public class TXWaterModules : TXNode
    {
        protected TLakeLayer _lakeLayer;
        protected TRiverLayer _riverLayer;
        protected TOceanLayer _oceanLayer;

        public TLakeLayer GetLakes(TTerrain terrain)
        {
            if (IsDone) return _lakeLayer;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return _lakeLayer;
        }
        public TRiverLayer GetRivers(TTerrain terrain)
        {
            if (IsDone) return _riverLayer;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return _riverLayer;
        }
        public TOceanLayer GetOceans(TTerrain terrain)
        {
            if (IsDone) return _oceanLayer;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return _oceanLayer;
        }
    }
}
#endif
