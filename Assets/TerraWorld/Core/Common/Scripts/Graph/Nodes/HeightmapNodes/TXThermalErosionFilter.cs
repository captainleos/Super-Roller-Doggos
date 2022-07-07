#if TERRAWORLD_XPRO
using System;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Heightmap/Thermal Erosion Process")]
    public class TXThermalErosionFilter : TXHeightmapModules
    {
        public int _iterations = 1;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXHeightmapModules Input;

        protected override void Init()
        {
            base.Init();
            SetName("Thermal Erosion Filter");
        }

        public override void CheckEssentioalInputs()
        {
            Input = GetInputValue<TXHeightmapModules>("Input");
            if (Input == null) throw new Exception("Input" + " is missed for " + name + " Node.");
        }

        protected override void ModuleAction(TMap currentMap)
        {
            _progress = 0;

            if (IsActive)
            {
                _heightmapData = THeightmapProcessors.ThermalErosion(null, Input.GetProceededHeightMap(currentMap._refTerrain), _iterations);
                _progress = 1;
            }
            else
                _heightmapData = Input.GetProceededHeightMap(currentMap._refTerrain);
        }
    }
}
#endif