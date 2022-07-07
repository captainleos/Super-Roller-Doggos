#if TERRAWORLD_XPRO
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{

    public abstract class TXInstanceScatterModules : TXNode
    {
        protected TInstanceScatterLayer _instanceScatterLayer;
        public abstract TInstanceScatterLayer GetInstancesLayer(TTerrain terrain);
    }
}
#endif
