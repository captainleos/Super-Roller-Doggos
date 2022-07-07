#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace TerraUnity.Runtime
{
    public class WorldTools : WorldToolsParams
    {
        private Terrain _terrain;
        public List<TScatterParams> GPULayers;
        public List<GrassLayer> grassLayers;
        public List<bool> editableGPU;
        public List<bool> editableGrass;
        public List<TScatterLayer.MaskData[]> maskDataListGPU;
        public List<float[]> exclusionOpacitiesListGPU;
        public List<TScatterLayer.MaskData[]> maskDataListGrass;
        public List<float[]> exclusionOpacitiesListGrass;

        public Terrain Terrain
        {
            get
            {
                if (_terrain == null && TTerraWorldManager.MainTerrainGO !=null ) _terrain = TTerraWorldManager.MainTerrainGO.GetComponent<Terrain>();
                return _terrain;
            }
        }
    }
}
#endif

