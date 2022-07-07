using UnityEngine;
using System;
using TerraUnity.Edittime;

namespace TerraUnity.Runtime
{
    public class FlatShadingManager : MonoBehaviour
    {
#if TERRAWORLD_PRO
#if UNITY_EDITOR
        public bool isFlatShadingObjects = false;

        public bool FlatShadingStateObjects
        {
            get { return isFlatShadingObjects; }
            set
            {
                isFlatShadingObjects = value;
                ApplySettings();
            }
        }

        public void OnValidate()
        {
            ApplySettings();
        }

        public void ApplyFlatShading(bool _isFlatShadingObjects)
        {
            isFlatShadingObjects = _isFlatShadingObjects;
            ApplySettings();
        }

        private void ApplySettings()
        {
            foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
            {
                if (m.hideFlags == HideFlags.NotEditable || m.hideFlags == HideFlags.HideAndDontSave) continue;
                if (m == TerrainRenderingManager.TerrainMaterial) continue;
                if (m == TerrainRenderingManager.TerrainMaterialBG) continue;
                if (TTerraWorldManager.CloudsManagerScript!= null  && m == TTerraWorldManager.CloudsManagerScript.cloudsMaterial) continue;

                if (m.IsKeywordEnabled("_FLATSHADING"))
                    if (m.HasProperty("_FlatShadingState"))
                        m.SetFloat("_FlatShadingState", Convert.ToInt32(isFlatShadingObjects));
            }
        }

        public void RemoveFlatShading()
        {
            foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
            {
                if (m.hideFlags == HideFlags.NotEditable || m.hideFlags == HideFlags.HideAndDontSave) continue;

                if (m.IsKeywordEnabled("_FLATSHADING"))
                    if (m.HasProperty("_FlatShadingState")) m.SetFloat("_FlatShadingState", 0);
            }
        }

        public void Init ()
        {
            if (TerrainRenderingManager.TerrainMaterial != null)
                FlatShadingStateObjects = TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.isFlatShading;
            else
                FlatShadingStateObjects = false;
        }
#endif
#endif
    }
}

