#if TERRAWORLD_XPRO
using System.Collections.Generic;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    public class TXMaskModules : TXNode , TXMaskInterface
    {
        [Output] public TXMaskModules Output;

        protected List<TMask> OutMasks;

        public List<TMask> GetMasks(TTerrain terrain)
        {
            if (IsDone) return OutMasks;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return OutMasks;
        }
    }
}
#endif
