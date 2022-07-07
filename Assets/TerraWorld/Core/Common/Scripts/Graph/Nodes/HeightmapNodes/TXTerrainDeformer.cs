#if TERRAWORLD_XPRO
using System;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Heightmap/Deformation Process")]
    public class TXTerrainDeformer : TXHeightmapModules
    {

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXHeightmapModules Heitmap;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXMaskModules Mask;

        public float _depth = 10f;
        public bool _flat = true;

        protected override void Init()
        {
            base.Init();
            SetName("Terrain Deformer");
        }

        public override void CheckEssentioalInputs()
        {
            Heitmap = GetInputValue<TXHeightmapModules>("Heitmap");
            if (Heitmap == null) throw new Exception("Heitmap Input" + " is missed for " + name + " Node.");
            Mask = GetInputValue<TXMaskModules>("Mask");
            if (Mask == null) throw new Exception("Mask Input" + " is missed for " + name + " Node.");
        }

        protected override void ModuleAction(TMap currentMap)
        {
            _progress = 0;

            if (IsActive)
            {
                _heightmapData = Heitmap.GetProceededHeightMap(currentMap._refTerrain).Clone() as float[,];
                TMask _mask = TMask.MergeMasks(Mask.GetMasks(currentMap._refTerrain));
                THeightmapProcessors.DeformByMask(ref _heightmapData, _mask, _depth, _flat, null, 0);
                _progress = 1;
            }
            else
                _heightmapData = Heitmap.GetProceededHeightMap(currentMap._refTerrain); 
        }
    }
}
#endif