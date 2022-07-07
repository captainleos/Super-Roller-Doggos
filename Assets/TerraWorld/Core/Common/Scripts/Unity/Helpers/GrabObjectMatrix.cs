using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class GrabObjectMatrix : MonoBehaviour
    {
        public GameObject target;
        public bool grabPosition = true;
        public bool grabRotation = true;

        void Update()
        {
            if (target == null) return;
            if (grabPosition) transform.position = target.transform.position;
            if (grabRotation) transform.rotation = target.transform.rotation;
        }
    }
}

