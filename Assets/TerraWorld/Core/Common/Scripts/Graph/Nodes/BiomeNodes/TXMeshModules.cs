#if TERRAWORLD_XPRO
using System.Collections.Generic;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{

    public abstract class TXMeshModules : TXNode
    {
        protected TGridLayer _gridLayer;

        public TGridLayer GetMeshLayer(TTerrain terrain)
        {
            if (IsDone) return _gridLayer;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return _gridLayer;
        }
    }
}
#endif
