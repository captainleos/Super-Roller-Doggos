#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System.Collections.Generic;

namespace TerraUnity.Edittime
{
    public struct TOceanData
    {
        public float LODCulling;
    }

    public class TOceanLayer : TPolygonMeshLayer
    {
        private TOceanData _oceanData;
        public List<TMask> WaterMasks;
        public float depth = 20f;

        public float LODCulling { get => _oceanData.LODCulling; set => _oceanData.LODCulling = value; }
        public List<T2DObject> Coastlines { get => MeshArea; set => MeshArea = value; }

        public TOceanLayer() : base()
        {
            layerType = LayerType.Oceans;
        }

        public TOceanLayer(string name) : this()
        {
            LayerName = name;
        }
    }
}
#endif
#endif

