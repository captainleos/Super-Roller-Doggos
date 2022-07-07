#if TERRAWORLD_XPRO


using XNodeEditor;
using UnityEditor;
using TerraUnity.Edittime;
using UnityEngine;
using UnityEngine.UI;

namespace TerraUnity.Graph.Editor
{
    [CustomNodeEditor(typeof(TXRealWorldSourceNode))]
    public class TXTerrainNodeEditor : TXNodeEditor
    {
        private TXTerrainNode module;

        public override void OnBodyGUI()
        {
            if (module == null) module = target as TXTerrainNode;
            base.OnBodyGUI();
        }

    }
}
#endif

