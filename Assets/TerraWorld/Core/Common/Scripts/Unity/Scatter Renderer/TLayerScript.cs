using UnityEngine;
using System;

namespace TerraUnity.Runtime
{
    [Serializable]
    public class TLayerScript : MonoBehaviour
    {
        [HideInInspector] public string unityLayerName;
        [HideInInspector] public int unityLayerIndex;
        protected Terrain _terrain;
        public bool updatePlacement = false;

        public Terrain terrain
        {
            get
            {
                if (_terrain == null) SetParentTerrain();
                return _terrain;
            }
        }

        private void SetParentTerrain()
        {
            Transform parent = transform;
            
            while (parent != null)
            {
                if (parent.GetComponent<Terrain>() != null)
                {
                    _terrain = parent.GetComponent<Terrain>();
                    break;
                }
                else
                    parent = parent.transform.parent;
            }
        }

        protected void Validate()
        {
            if (_terrain == null) SetParentTerrain();
            unityLayerIndex = LayerMask.NameToLayer(unityLayerName);

            if (updatePlacement)
            {
                UpdateLayer();
                updatePlacement = false;
            }
        }

        public virtual void UpdateLayer()
        {
            Debug.Log("UpdateLayer function not implemented!");
        }
    }
}

