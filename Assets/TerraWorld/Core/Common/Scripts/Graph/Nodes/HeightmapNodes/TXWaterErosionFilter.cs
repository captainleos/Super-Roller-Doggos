#if TERRAWORLD_XPRO
using System;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Heightmap/Water Erosion Process")]
    public class TXWaterErosionFilter : TXHeightmapModules
    {
        public int _iterations = 40;
        public float _shape = 1f;
        public float _rivers = 0.0025f;
        public float _vertical = 0.5f;
        public float _seaBedCarve = 0.5f;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXHeightmapModules Input;

        protected override void Init()
        {
            base.Init();
            SetName("Water Erosion Filter");
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
                _heightmapData = THeightmapProcessors.WaterErosion(null, Input.GetProceededHeightMap(currentMap._refTerrain), _iterations, _shape, _rivers, _vertical, _seaBedCarve);
                _progress = 1;
            }
            else
                _heightmapData = Input.GetProceededHeightMap(currentMap._refTerrain); 
        }
    }
}
#endif