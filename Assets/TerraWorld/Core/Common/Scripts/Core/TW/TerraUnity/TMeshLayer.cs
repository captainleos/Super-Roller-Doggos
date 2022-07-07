#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;

namespace TerraUnity.Edittime
{
    public struct TMeshData
    {
        public string meshFileAddress;
        public string materialFileAddress;
    }

    public class TMeshLayer : TObjectiveLayer
    {
        //public TMesh mesh;
        public TMaterial material;
        public Material  xMaterial;
    }
}
#endif
#endif

