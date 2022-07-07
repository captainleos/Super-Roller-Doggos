#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System.Collections.Generic;

namespace TerraUnity.Edittime
{
    public struct TLakeData
    {
        public int AroundPointsDensity;
        public float AroundVariation;
        public float LODCulling;
    }

    public class TLakeLayer : TPolygonMeshLayer
    {
        private TLakeData _lakeData;
        public List<TMask> WaterMasks;
        public float _minSizeInM2 = 5000;
        public float depth = 20f;

        public int AroundPointsDensity { get => _lakeData.AroundPointsDensity; set => _lakeData.AroundPointsDensity = value; }
        public float AroundVariation { get => _lakeData.AroundVariation; set => _lakeData.AroundVariation = value; }
        public float LODCulling { get => _lakeData.LODCulling; set => _lakeData.LODCulling = value; }
        public List<T2DObject> LakesList { get => MeshArea; set => MeshArea = value; }

        public TLakeLayer() : base()
        {
            layerType = LayerType.Lake;
        }

        public TLakeLayer(string name) :this()
        {
            LayerName = name;
        }
    }
}
#endif
#endif
