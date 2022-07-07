#if TERRAWORLD_XPRO
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [NodeWidth(300), NodeTint(214, 255, 255)]

    public class TXTerrainLayerModules : TXNode , TXTerrainLayerInterface
    {
        [Output] public TXTerrainLayerModules TerrainLayer;

        protected TDetailTexture _outputDetailTexture;

        public TDetailTexture GetTerrainLayer(TTerrain terrain)
        {
            if (IsDone) return _outputDetailTexture;
            CheckEssentioalInputs();
            ModuleAction(terrain.Map);
            IsDone = true;
            return _outputDetailTexture;
        }
    }
}
#endif
