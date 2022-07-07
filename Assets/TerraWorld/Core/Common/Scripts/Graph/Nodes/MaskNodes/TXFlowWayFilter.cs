#if TERRAWORLD_XPRO
using System;
using System.Collections.Generic;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Masks/Flow Way -> Mask")]
    public class TXFlowWayFilter : TXMaskModules
    {
        public int _iterations = 5;
        public float _widthMultiplier = 1;
        public float _heightMultiplier = 1;
        public float minRange = 0.3f;
        public float maxRange = 1f;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXHeightmapModules Input;

        protected override void Init()
        {
            base.Init();
            SetName("Flow Way Filter");
        }

        public override void CheckEssentioalInputs()
        {
            Input = GetInputValue<TXHeightmapModules>("Input");
            if (Input == null) throw new Exception("Input" + " is missed for " + name + " Node.");
        }

        protected override void ModuleAction(TMap currentMap)
        {
            _progress = 0;

            OutMasks = new List<TMask>();

            if (IsActive)
            {
                TMask _mask;
                _mask = THeightmapProcessors.CreateFlowMask(null, Input.GetProceededHeightMap(currentMap._refTerrain), _widthMultiplier, _heightMultiplier, _iterations);
                _mask = _mask.FilteredMask(minRange, maxRange);
                OutMasks.Add(_mask);
                _progress = 1;
            }
        }
    }
}
#endif