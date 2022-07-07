#if TERRAWORLD_XPRO
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{

    public abstract class TXObjectScatterModules : TXNode
    {
        protected TObjectScatterLayer _objectScatterLayer;
        public abstract TObjectScatterLayer GetObjectsLayer(TTerrain terrain);
    }
}
#endif
