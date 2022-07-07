#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System.Collections.Generic;

namespace TerraUnity.Edittime
{
    public class TLinearMeshLayer : TMeshLayer
    {
        public List<TLinearObject> Lines;

        public TLinearMeshLayer()
        {
            Lines = new List<TLinearObject>();
        }

        public TLinearMeshLayer(string name) : this()
        {
            LayerName = name;
        }
    }
}
#endif
#endif

