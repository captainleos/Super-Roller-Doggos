#if TERRAWORLD_XPRO
using System;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Heightmap/Terrace Process")]
    public class TXTerraceFilter : TXHeightmapModules
    {
        public int _terraceCount = 7;
        public float _strength = 0.7f;
        public float _terraceVariation = 0f;
        public float[] controlPoints;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXHeightmapModules Input;

        protected override void Init()
        {
            base.Init();
            SetName("Terrace Filter");
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
                _heightmapData = THeightmapProcessors.Terrace(null, Input.GetProceededHeightMap(currentMap._refTerrain), ref controlPoints, _terraceCount, _strength, _terraceVariation);
                _progress = 1;
            }
            else
                _heightmapData = Input.GetProceededHeightMap(currentMap._refTerrain);
        }
    }
}
#endif