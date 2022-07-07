#if TERRAWORLD_XPRO
using System;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Heightmap/Smoothen Terrain Process")]
    public class TXSmoothenTerrain : TXHeightmapModules
    {
        public int _steps = 1;
        public float _blending = 0.5f;
        public THeightmapProcessors.Neighbourhood _smoothMode = THeightmapProcessors.Neighbourhood.VonNeumann;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXHeightmapModules Input;

        protected override void Init()
        {
            base.Init();
            SetName("Smoothen Terrain");
        }

        public override void CheckEssentioalInputs()
        {
            Input = GetInputValue<TXHeightmapModules>("Input");
            if (Input == null) throw new Exception("Input" + " is missed for " + this.name + " Node.");
        }

        protected override void ModuleAction(TMap currentMap)
        {
            _progress = 0;

            if (IsActive)
            {
                _heightmapData = THeightmapProcessors.SmoothHeightmap(Input.GetProceededHeightMap(currentMap._refTerrain), _steps, _blending, _smoothMode);
                _progress = 1;
            }
            else
                _heightmapData = Input.GetProceededHeightMap(currentMap._refTerrain);
        }
    }
}
#endif