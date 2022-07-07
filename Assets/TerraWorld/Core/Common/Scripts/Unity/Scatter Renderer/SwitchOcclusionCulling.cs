using UnityEngine;
using System.Collections.Generic;

namespace TerraUnity.Runtime
{
    public class SwitchOcclusionCulling : MonoBehaviour
    {
        public bool occlusionCulling = true;
        private List<TScatterParams> GPULayers = null;

        void Start()
        {
            GetGPULayers();
            SwitchOcclusion();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                occlusionCulling = !occlusionCulling;
                SwitchOcclusion();
            }
        }

        private void GetGPULayers()
        {
            GPULayers = new List<TScatterParams>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                    if (go.activeSelf && go.GetComponent<TScatterParams>() != null)
                        GPULayers.Add(go.GetComponent<TScatterParams>());
        }

        private void SwitchOcclusion()
        {
            for (int i = 0; i < GPULayers.Count; i++)
                GPULayers[i].occlusionCulling = occlusionCulling;

            if (occlusionCulling) Debug.Log("Occlusion Culling: ON");
            else Debug.Log("Occlusion Culling: OFF");
        }
    }
}

