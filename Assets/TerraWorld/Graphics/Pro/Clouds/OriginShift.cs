using UnityEngine;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class OriginShift : MonoBehaviour
    {
        public float distance = 10000.0f;
        private float distanceSqr;
        private Object[] clouds;
        public CloudsManager cloudsManager;

        private void OnEnable()
        {
            Initialize();
        }

        void Start()
        {
            Initialize();
        }

        void Update()
        {
            if (cloudsManager != null) cloudsManager.MoveCloudsWithWind();
        }

        void LateUpdate()
        {
            ManageFloatingOrigin();
        }

        private void Initialize()
        {
            distanceSqr = Mathf.Pow(distance, 2f);
            clouds = GetComponentsInChildren(typeof(Transform));

            foreach (Transform cloud in clouds)
            {
                ParticleSystem cloudSystem = cloud.GetComponent<ParticleSystem>();

                if (cloudSystem != null)
                {
                    cloudSystem.Simulate(1);
                    cloudSystem.Play(true);
                }
            }

            if (cloudsManager != null) cloudsManager.clouds = this.gameObject;
        }

        private void ManageFloatingOrigin()
        {
            if (clouds == null || clouds.Length == 0) Initialize();
            Vector3 offsetPosition = transform.position;
            offsetPosition.y = 0;

            if (offsetPosition.sqrMagnitude > distanceSqr)
            {
                foreach (Object o in clouds)
                {
                    Transform t = (Transform)o;

                    if (t == transform)
                        t.position -= offsetPosition;
                    else if (t != null)
                        t.position += offsetPosition;
                }
            }
        }
    }
}

