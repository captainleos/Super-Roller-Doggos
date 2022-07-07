using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class SunPosition : MonoBehaviour
    {
        public GameObject player;
        public float distance = 10000f;
        public bool mainLight = true;
        public Light sun;

        void Start()
        {
            if (mainLight) sun = transform.GetComponent<Light>();
            SetSunPosition();
        }

        void LateUpdate()
        {
            SetSunPosition();
        }

        private void SetSunPosition()
        {
            if (player == null) player = Camera.main.gameObject;
            if (!mainLight) transform.rotation = sun.transform.rotation;
            transform.position = player.transform.position + (-transform.forward * distance);
        }
    }
}

