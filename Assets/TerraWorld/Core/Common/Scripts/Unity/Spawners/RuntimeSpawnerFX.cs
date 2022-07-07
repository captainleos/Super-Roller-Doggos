using UnityEngine;

namespace TerraUnity.Runtime
{
    //[ExecuteAlways]
    public class RuntimeSpawnerFX : MonoBehaviour
    {
        public GameObject player;
        public GameObject prefab;
        public int count = 3;
        public float lifeTime = 3f;
        public float spawnDelay = 3f;
        [Range(0, 4)] public float randomTimeRange = 2f;
        public float radius = 20f;
        public float startHeight = 0f;
        public float endHeight = 2000f;
        public LayerMask layerMask;
        public float heightOffset = 0.5f;
        public bool getGroundAngle = true;
        private float checkingHeight = 100000f;
        private GameObject[] effect;
        private GameObject effects;
        private ParticleSystem[] particleSystems;
        private ParticleSystem.EmissionModule[] emissionModules;

        void Start()
        {
            if (player == null) return;

            effects = new GameObject("Runtime FX");
            effect = new GameObject[count];
            particleSystems = new ParticleSystem[count];
            emissionModules = new ParticleSystem.EmissionModule[count];

            for (int i = 0; i < count; i++)
            {
                effect[i] = Instantiate(prefab, effects.transform);
                effect[i].name = "Effect_" + (i + 1).ToString();
                particleSystems[i] = effect[i].GetComponent<ParticleSystem>();
                emissionModules[i] = particleSystems[i].emission;

                emissionModules[i].enabled = false;
                particleSystems[i].Simulate(1);
                particleSystems[i].Play();
            }

            InvokeRepeating("SpawnEffects", 0, lifeTime + spawnDelay + randomTimeRange);
        }

        private void SpawnEffects()
        {
            if (player == null) return;

            if (player.transform.position.y >= startHeight && player.transform.position.y <= endHeight)
            {
                Physics.autoSimulation = false;
                Physics.Simulate(Time.fixedDeltaTime);

                for (int i = 0; i < count; i++)
                {
                    Vector3 origin = player.transform.position;
                    origin += Random.insideUnitSphere * radius;
                    origin.y = checkingHeight;
                    Ray ray = new Ray(origin, Vector3.down);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                    {
                        Vector3 position = hit.point;

                        if (position.y >= startHeight && position.y <= endHeight)
                        {
                            position.y += heightOffset;
                            effect[i].transform.position = position;

                            Vector3 normal = hit.normal;

                            if (getGroundAngle)
                                effect[i].transform.rotation = Quaternion.LookRotation(normal);
                        }
                    }
                }

                Physics.autoSimulation = true;

                Invoke("TurnOnEffects", Random.Range(0, randomTimeRange));
                Invoke("TurnOffEffects", lifeTime + Random.Range(0, randomTimeRange));
            }
        }

        private void TurnOnEffects()
        {
            for (int i = 0; i < count; i++)
                emissionModules[i].enabled = true;
        }

        private void TurnOffEffects()
        {
            for (int i = 0; i < count; i++)
                emissionModules[i].enabled = false;
        }
    }
}

