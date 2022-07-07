#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace TerraUnity.Runtime
{
    public class InteractiveTargets
    {
        public static List<GameObject> playerTargets;

        public static void GetPlayerTargets(bool reset = false)
        {
            playerTargets = new List<GameObject>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                {
                    if (go.activeSelf && go.GetComponent<PlayerInteractions>() != null)
                    {
                        //if (go.GetComponent<PlayerInteractions>().GPULayers == null || go.GetComponent<PlayerInteractions>().statesGPU == null)
                        go.GetComponent<PlayerInteractions>().ScanScatters(reset);
                        playerTargets.Add(go);
                    }
                }
            }
        }
    }
}
#endif

