#if TERRAWORLD_XPRO
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    public interface TXImageInterface
    {
        TImage GetImage(TTerrain terrain);
    }
}
#endif