#if TERRAWORLD_XPRO
using System.Collections.Generic;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    public interface TXAreaInterface
   {
        List<TArea> GetAreas(TTerrain terrain);
    }
}
#endif