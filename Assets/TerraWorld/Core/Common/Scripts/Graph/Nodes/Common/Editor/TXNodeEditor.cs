#if TERRAWORLD_XPRO
using System;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace TerraUnity.Graph.Editor
{
    [CustomNodeEditor(typeof(TXNode))]
    public class TXNodeEditor : NodeEditor
    {
        private TXNode currentNode;
        private Texture2D moduleIcon, moduleState;
        private Color enabledColor = Color.white;
        private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
        public override void OnHeaderGUI()
        {
            if (currentNode == null) currentNode = target as TXNode;

            if (currentNode.IsActive)
                moduleState = TResourcesManager.onIcon;
            else
                moduleState = TResourcesManager.offIcon;

            GUIStyle style = new GUIStyle(EditorStyles.textField);
            style.fontSize = 8;
            style.alignment = TextAnchor.UpperCenter;

            // Enable/Disable module
            if (GUI.Button(new Rect(10, 8, 20, 20), moduleState))
            {
                currentNode.IsActive = !currentNode.IsActive;
            }
            base.OnHeaderGUI();
        }

        public override Color GetTint()
        {
            Color color = base.GetTint();
            if (((TXNode)target).IsDone) return EditorColors.TintDone;
            else return color;
        }

    }
}
#endif
