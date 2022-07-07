/*
using System.Collections.Generic;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Areas/Real World Area")]
    public class TXAreaSourceNode : TXAreaModules
    {

        protected override void Init()
        {
            base.Init();
            SetName("Real World Area");
        }

        public override List<TArea> GetAreas(TTerrain terrain)
        {
            if (!IsActive) return new List<TArea>();
            if (IsDone) return _outputAreas ;

            _outputAreas = new List<TArea>();
            _outputAreas.Add(terrain.Map._area);

            IsDone = true;
            return _outputAreas;

        }

        public override void CheckEssentioalInputs()
        {
        }

    }
}
*/