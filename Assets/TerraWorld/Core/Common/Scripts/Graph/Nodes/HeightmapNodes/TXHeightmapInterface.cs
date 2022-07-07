#if TERRAWORLD_XPRO
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    public interface TXHeightmapInterface 
    {
        float[,] GetProceededHeightMap(TTerrain terrain);
    }
}
#endif