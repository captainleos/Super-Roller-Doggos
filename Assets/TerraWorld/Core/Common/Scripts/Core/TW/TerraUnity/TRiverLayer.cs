#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System.Collections.Generic;

namespace TerraUnity.Edittime
{
    public class TRiverLayer : TLinearMeshLayer
    {
        public List<TMask> WaterMasks;
        private float lodCulling;
        public float _width = 100;
        public float depth = 3;

        public List<TLinearObject> RiversList { get => Lines; set => Lines = value; }
        public float LODCulling { get => lodCulling; set => lodCulling = value; }

        public TRiverLayer() : base()
        {
            layerType = LayerType.River;
        }

        public TRiverLayer(string name) : this()
        {
            LayerName = name;
        }
    }
}
#endif
#endif

