#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System.Numerics;

namespace TerraUnity.Edittime
{
    public enum BuilderType
    {
        Quad = 0,
        FromMesh = 5,
    }

    public enum NormalType
    {
        KeepMesh = 0,
        Up = 1,
        Shading = 2,
    }

    public class TGrassScatterLayer : TPointLayer
    {
        public int maxParallelJobCount;
        public TMaterial material;
        public Vector2 scale;
        public string modelPath;
        public string meshName;
        public float radius = 1000f;
        public float gridSize = 50f;
        public float slant;
        public int amountPerBlock = 10000;
        public float alphaMapThreshold = 0.3f;
        public float densityFactor = 0.5f;
        public BuilderType builderType;
        public NormalType normalType = NormalType.Up;
        public float groundOffset = 0;
        public TShadowCastingMode shadowCastingMode = TShadowCastingMode.On;
        public int seedNo;
        public string unityLayerName = "Default";

        public float[,] maskData;
        //public TMask mask;

        public bool layerBasedPlacement;
        public bool bypassWater;
        public bool underWater;
        public bool onWater;
    }
}
#endif
#endif

