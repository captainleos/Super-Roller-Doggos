using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class DynamicSpawner : MonoBehaviour
    {
        public GameObject center;
        public Vector2 simulationAreaSize;
        private Vector4 areaBounds = new Vector4();

        void Update()
        {
            if (center == null || simulationAreaSize == Vector2.zero) return;

            areaBounds.x = center.transform.position.x + simulationAreaSize.x;
            areaBounds.y = center.transform.position.x - simulationAreaSize.x;
            areaBounds.z = center.transform.position.z + simulationAreaSize.y;
            areaBounds.w = center.transform.position.z - simulationAreaSize.y;

            if (transform.position.x > areaBounds.x)
                transform.position = new Vector3(areaBounds.y, transform.position.y, transform.position.z);
            else if (transform.position.x < areaBounds.y)
                transform.position = new Vector3(areaBounds.x, transform.position.y, transform.position.z);
            else if (transform.position.z > areaBounds.z)
                transform.position = new Vector3(transform.position.x, transform.position.y, areaBounds.w);
            else if (transform.position.z < areaBounds.w)
                transform.position = new Vector3(transform.position.x, transform.position.y, areaBounds.z);
        }
    }
}

