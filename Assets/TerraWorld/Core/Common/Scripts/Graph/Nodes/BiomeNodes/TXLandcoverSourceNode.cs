#if TERRAWORLD_XPRO
using System;
using System.Xml;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    public class TXLandcoverSourceNode : TXLandcoverModules
    {

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXAreaModules Area;

        protected override void Init()
        {
            base.Init();
            SetName("Landcover Data Extractor");
        }

        protected override void ModuleAction(TMap currentMap)
        {
            _outputLandcover = currentMap.LandcoverXML;
        }


        public override void CheckEssentioalInputs()
        {
        }

    }
}
#endif
