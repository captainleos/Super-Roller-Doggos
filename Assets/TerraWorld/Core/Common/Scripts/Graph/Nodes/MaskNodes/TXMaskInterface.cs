#if TERRAWORLD_XPRO
using System.Collections.Generic;
using TerraUnity;
using TerraUnity.Edittime;

public interface  TXMaskInterface 
{
    List<TMask> GetMasks(TTerrain terrain);
}
#endif
