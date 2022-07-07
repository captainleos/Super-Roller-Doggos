#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System.Collections.Generic;

namespace TerraUnity.Edittime
{
    public struct TTreeData
    {
    }

    public class TTreesLayer : TPolygonMeshLayer
    {
        private TTreeData _TreesData;
 
        public List<T2DObject> TreesAreaList { get => MeshArea; set => MeshArea = value; }

        public TTreesLayer() : base()
        {

        }

        public TTreesLayer(string name) : this()
        {
            LayerName = name;
        }
    }
}
#endif
#endif

