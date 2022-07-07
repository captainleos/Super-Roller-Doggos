#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Numerics;

namespace TerraUnity.Edittime
{
    public struct TGridData
    {
    }

    public class TGridLayer : TPolygonMeshLayer
    {
        private TGridData _GridData;
        public List<T2DGrid> GridsList;
        public Vector3 Scale;
        public int KM2Resulotion;
        public int density;
        public float EdgeCurve;
        public float LODCulling;

        public TGridLayer()
        {
            layerType = LayerType.Mesh;
            GridsList = new List<T2DGrid>();
        }

        public TGridLayer(string name) : this()
        {
            LayerName = name;
        }

        public int GetGridofPoint(TGlobalPoint P1)
        {
            for (int i = 0; i < GridsList.Count; i ++)
                if (GridsList[i].IsPointInGrid(P1) > -1)
                    return i;

            return -1;
        }

        public int GetClosedGridToPoint(TGlobalPoint P1, double minDistance)
        {
            for (int i = 0; i < GridsList.Count; i++)
                if (GridsList[i].IsPointCloseToGrid(P1, minDistance) > -1)
                    return i;

            return -1;
        }

    }
}

#endif
#endif