#if TERRAWORLD_XPRO
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [NodeWidth(300), NodeTint(214, 50, 255)]

    public class TXImageModules : TXNode , TXImageInterface
    {
        [Output] public TXImageModules Image;

        protected TImage _outputImage;

        public TImage GetImage(TTerrain terrain)
        {
            if (IsDone) return _outputImage;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return _outputImage;
        }
    }
}
#endif
