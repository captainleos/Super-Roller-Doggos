using UnityEngine;

namespace TerraUnity.Runtime
{
    public class TimedDestructions : MonoBehaviour
    {
        public float destructionTimeInSeconds = 30f;

        void Start()
        {
            Destroy(gameObject, destructionTimeInSeconds);
        }
    }
}

