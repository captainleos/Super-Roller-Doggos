#if TERRAWORLD_XPRO
using System.Collections.Generic;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    public abstract class TXAreaModules :TXNode , TXAreaInterface
    {
        [Output] public TXAreaModules Output;

        protected List<TArea> _outputAreas;

        public abstract List<TArea> GetAreas(TTerrain terrain);
    }
}
#endif
