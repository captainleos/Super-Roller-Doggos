#if TERRAWORLD_XPRO
using System.Xml;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    public interface TXLandcoverInterface
    {
        XmlDocument GetProceededLandcover(TTerrain terrain);
    }
}
#endif