#if TERRAWORLD_PRO
#if UNITY_EDITOR

namespace TerraUnity.Edittime
{
    public class TTerraMeshLayer : TMeshLayer
    {
        public MeshResolution meshResolution;
        public float offsetFalloff = 0.001f;
        public float endCurve = 0.001f;
    }
}
#endif
#endif

