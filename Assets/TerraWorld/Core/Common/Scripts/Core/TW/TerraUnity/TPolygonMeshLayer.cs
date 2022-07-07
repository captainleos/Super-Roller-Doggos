#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Numerics;

namespace TerraUnity.Edittime
{
    public class TPolygonMeshLayer : TMeshLayer
    {
        public List<T2DObject> MeshArea;

        public TPolygonMeshLayer()
        {
            MeshArea = new List<T2DObject>();
        }

        public TPolygonMeshLayer(string name) : this()
        {
            LayerName = name;
        }
    }
}
#endif
#endif

