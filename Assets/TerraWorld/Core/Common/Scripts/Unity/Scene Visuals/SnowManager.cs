using UnityEngine;
using System;
using TerraUnity.Edittime;

namespace TerraUnity.Runtime
{
    [ExecuteInEditMode]
    public class SnowManager : MonoBehaviour
    {
        public bool isSnowEnabled = true;
        [Range(-1000f, 10000f)] public float snowHeight = 5000f;
        [Range(0f, 10000f)] public float snowFalloff = 1000f;
        [HideInInspector] [Range(0f, 1f)] public float snowThickness = 0f;
        [HideInInspector] [Range(0f, 1f)] public float snowDamping = 0f;

        public bool SnowState
        {
            get { return isSnowEnabled; }
            set
            {
                isSnowEnabled = value;
                ApplySettings();
            }
        }

        public float SnowHeight
        {
            get { return snowHeight; }
            set
            {
                snowHeight = value;
                ApplySettings();
            }
        }

        public float SnowFalloff
        {
            get { return snowFalloff; }
            set
            {
                snowFalloff = value;
                ApplySettings();
            }
        }

        public float SnowThickness
        {
            get { return snowThickness; }
            set
            {
                snowThickness = value;
                ApplySettings();
            }
        }

        public float SnowDamping
        {
            get { return snowDamping; }
            set
            {
                snowDamping = value;
                ApplySettings();
            }
        }

        //TODO: Needed for builds but should be removed after debugging
        private void Start()
        {
            if (Application.isPlaying)
                ApplySettings();
        }

        private void OnValidate()
        {
            // Only update if user is changing parameters in inspector and not through code calls
            if (Event.current == null) return;
            ApplySettings();
        }

        public void ApplySnow (bool _snowState, float _snowHeight, float _snowFalloff, float _snowThickness, float _snowDamping)
        {
            isSnowEnabled = _snowState;
            snowHeight = _snowHeight;
            snowFalloff = _snowFalloff;
            snowThickness = _snowThickness;
            snowDamping = _snowDamping;
            
            ApplySettings();
        }

        private void ApplySettings()
        {
            foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
            {
                if (m.hideFlags == HideFlags.NotEditable || m.hideFlags == HideFlags.HideAndDontSave) continue;

                if (m.IsKeywordEnabled("_PROCEDURALSNOW"))
                {
                    if (m.HasProperty("_SnowState"))
                    {
                        m.SetFloat("_SnowState", Convert.ToInt32(isSnowEnabled));

                        if (SnowState)
                        {
                            if (m.HasProperty("_SnowStartHeight")) m.SetFloat("_SnowStartHeight", snowHeight);
                            if (m.HasProperty("_HeightFalloff")) m.SetFloat("_HeightFalloff", snowFalloff);
                            if (m.HasProperty("_SnowThickness")) m.SetFloat("_SnowThickness", snowThickness);
                            if (m.HasProperty("_SnowDamping")) m.SetFloat("_SnowDamping", snowDamping);
                        }
                    }
                }
            }
        }

        public void RemoveSnow ()
        {
            foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
            {
                if (m.hideFlags == HideFlags.NotEditable || m.hideFlags == HideFlags.HideAndDontSave) continue;

                if (m.IsKeywordEnabled("_PROCEDURALSNOW"))
                    if (m.HasProperty("_SnowState")) m.SetFloat("_SnowState", 0);
            }
        }

        public void Init()
        {
#if TERRAWORLD_PRO
#if UNITY_EDITOR
            if (TerrainRenderingManager.TerrainMaterial != null)
            {
                isSnowEnabled = TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.proceduralSnow;
                snowHeight = TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.snowStartHeight;
                snowFalloff = TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.heightFalloff;
                snowThickness = TerrainRenderingManager.SnowThickness;
                snowDamping = TerrainRenderingManager.SnowDamping;
            }
            else
            {
                isSnowEnabled = true;
                snowHeight = 5000f;
                snowFalloff = 1000;
                snowThickness = 0;
                snowDamping = 0;
            }

            ApplySettings();

            //foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)) as Material[])
            //{
            //    if (m.hideFlags == HideFlags.NotEditable || m.hideFlags == HideFlags.HideAndDontSave) continue;
            //
            //    if (m.IsKeywordEnabled("_PROCEDURALSNOW"))
            //        if (m.HasProperty("_SnowState"))
            //        {
            //            if (m.GetFloat("_SnowState") == 0) isSnowEnabled = false;
            //            else isSnowEnabled = true;
            //            break;
            //        }
            //}
#endif
#endif
        }
    }
}

