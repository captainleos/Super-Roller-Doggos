#if TERRAWORLD_PRO
#if UNITY_EDITOR

namespace TerraUnity.Edittime
{
    public enum GridSize
    {
        _1 = 1,
        _2x2 = 2,
        _3x3 = 3,
        _4x4 = 4,
        _5x5 = 5,
        _6x6 = 6,
        _7x7 = 7,
        _8x8 = 8,
        _9x9 = 9,
        _10x10 = 10,
        _11x11 = 11,
        _12x12 = 12,
        _13x13 = 13,
        _14x14 = 14,
        _15x15 = 15,
        _16x16 = 16,
        _32x32 = 32,
        _64x64 = 64,
        _100x100 = 100,
        _128x128 = 128,
        _200x200 = 200,
        _256x256 = 256
    }

    public class TInstanceScatterLayer : TPointLayer
    {
        public float averageDistance = 1;
        public int gridResolution = 100;
        public float maxDistance = 100;
        public float LODMultiplier = 1;
        public TPatch[,] patches;
        public string prefabName;
        //public List<string> LODNames;
        //public List<float> LODDistances;
        public TShadowCastingMode shadowCastingMode;
        public bool receiveShadows;
        public bool bypassLake;
        public bool underLake;
        public bool underLakeMask;
        public bool onLake;

        public float[,] maskData;
        //public TMask mask;

        public float frustumMultiplier = 1.1f;
        public bool checkBoundingBox = false;
        public bool occlusionCulling = true;
    }
}
#endif
#endif

