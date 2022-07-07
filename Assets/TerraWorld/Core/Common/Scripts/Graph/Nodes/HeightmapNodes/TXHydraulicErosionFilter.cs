#if TERRAWORLD_XPRO
using System;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Heightmap/Hydraulic Erosion Process")]
    public class TXHydraulicErosionFilter : TXHeightmapModules
    {
        public int _iterationsUltimate = 50000;
        public HydraulicErosionMethod hydraulicErosionMethod = HydraulicErosionMethod.Ultimate;
        public int _iterations = 40;
        public float _rainAmount = 0.75f;
        public float _sediment = 0.05f;
        public float _evaporation = 0.75f;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXHeightmapModules Input;

        protected override void Init()
        {
            base.Init();
            SetName("Hydraulic Erosion Filter");
        }

        protected override void ModuleAction(TMap currentMap)
        {
            _progress = 0;

            if (IsActive)
            {
                switch (hydraulicErosionMethod)
                {
                    case HydraulicErosionMethod.Normal:
                        _heightmapData = THeightmapProcessors.HydraulicErosion(Input.GetProceededHeightMap(currentMap._refTerrain), _iterations, _rainAmount, _sediment, _evaporation);
                        break;
                    case HydraulicErosionMethod.Ultimate:
                        _heightmapData = THeightmapProcessors.HydraulicErosionUltimate(null,Input.GetProceededHeightMap(currentMap._refTerrain), _iterationsUltimate);
                        break;
                }

                _progress = 1;
            }
            else
                _heightmapData = Input.GetProceededHeightMap(currentMap._refTerrain);

        }
        public override void CheckEssentioalInputs()
        {
            Input = GetInputValue<TXHeightmapModules>("Input");
            if (Input == null) throw new Exception("Input" + " is missed for " + this.name + " Node.");
        }
    }
}
#endif