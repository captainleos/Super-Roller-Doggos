#if UNITY_EDITOR
using System.Collections.Generic;

namespace TerraUnity.Edittime
{
    public class TObjectScatterLayer : TPointLayer
    {
        public bool bypassLake;
        public bool underLake;
        public bool underLakeMask;
        public bool onLake;
        public List<string> prefabNames;
        public float[,] maskData;
        public float averageDistance;
        public bool checkBoundingBox;

        //private int densityResolutionPerKilometer = 1000;
        //public int DensityResolutionPerKilometer { get => densityResolutionPerKilometer; set => densityResolutionPerKilometer = value; }

        public TObjectScatterLayer(bool TreeType) : base()
        {
            if (TreeType) layerType = LayerType.ScatteredTrees; else layerType = LayerType.ScatteredObject;
            prefabNames = new List<string>();
        }
    }
}
#endif

