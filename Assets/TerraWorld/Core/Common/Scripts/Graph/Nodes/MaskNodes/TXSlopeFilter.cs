#if TERRAWORLD_XPRO
using System;
using System.Collections.Generic;
using System.Numerics;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Masks/Slope Filter -> Mask")]
    public class TXSlopeFilter : TXMaskModules
    {

        public Vector3 scaleMultiplier = Vector3.One;
        public float MinSlope = 0;
        public float MaxSlope = 90;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXHeightmapModules Input;

        protected override void Init()
        {
            base.Init();
            SetName("Slope Filter");
        }

        protected override void ModuleAction(TMap currentMap)
        {
            _progress = 0;

            OutMasks = new List<TMask>();

            if (IsActive)
            {
                THeightmap heightmap = new THeightmap(Input.GetProceededHeightMap(currentMap._refTerrain));
                TMask _mask = THeightmapProcessors.GetSlopeMap(currentMap, heightmap, MaxSlope, MinSlope);
                OutMasks.Add(_mask);
                _progress = 1;
            }
        }

        public override void CheckEssentioalInputs()
        {
            Input = GetInputValue<TXHeightmapModules>("Input");
            if (Input == null) throw new Exception("Input" + " is missed for " + NodeName + " Node.");
        }
    }
}
#endif