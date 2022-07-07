using UnityEngine;
using System.Linq;
using TerraUnity.UI;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class RuntimeSpawnerGO : MonoBehaviour
    {
        public GameObject player;
        public GameObject prefab;
        public bool resetInstances = true;
        [Range(1, 1000)] public int instanceCount = 100;
        public float spawnRadius = 100f;
        [MinMax(-10000, 10000, ShowEditRange = true)] public Vector2 heightRange = new Vector2(-10000, 10000);
        [MinMax(0, 90, ShowEditRange = true)] public Vector2 slopeRange = new Vector2(0, 90);
        public LayerMask layerMask = ~0;
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;
        [MinMax(0, 359, ShowEditRange = true)] public Vector2 rotationRange = new Vector2(0, 359);
        public bool lock90DegreeRotation = false;
        public bool lockYRotation = false;
        public bool getGroundAngle = true;
        public Vector3 scale = Vector3.one;
        [MinMax(0, 10, ShowEditRange = true)] public Vector2 scaleRange = new Vector2(0.8f, 1.5f);
        public int seedNo = 12345;

        private float checkingHeight = 100000f;
        private Quaternion rotation;

        public enum WaterDetection
        {
            bypassWater,
            underWater,
            onWater
        }
        public WaterDetection waterDetection = WaterDetection.bypassWater;
        private bool isBypassWater;
        private bool isUnderwater;

        [HideInInspector] public GameObject[] instance;
        private GameObject instances;
        private bool updateSettings = false;

        private bool hasRigidbody = false;
        private bool isKinematic = false;

        private void OnValidate()
        {
            if (Time.realtimeSinceStartup > 10f)
                updateSettings = true;

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private void OnEnable()
        {
            if (Time.realtimeSinceStartup > 10f)
                InitObjects();
        }

        private void OnDisable()
        {
            RemoveInstances();
        }

        private void CreateInstancesParent()
        {
            if (GameObject.Find("GameObject Instances") == null)
                instances = new GameObject("GameObject Instances");
            else
                instances = GameObject.Find("GameObject Instances");
        }

        private void RemoveInstancesParent()
        {
            if (GameObject.Find("GameObject Instances") != null)
                DestroyImmediate(GameObject.Find("GameObject Instances"));
        }

        private void RemoveInstances()
        {
            if (instance != null && instance.Length > 0)
                for (int i = 0; i < instance.Length; i++)
                    DestroyImmediate(instance[i]);
        }

        private void Awake()
        {
            if (Application.isPlaying)
                RemoveInstancesParent();
        }

        private void Start()
        {
            if (Application.isPlaying)
                InitObjects();
        }

        void Update()
        {
            if (player == null || prefab == null) return;
            UpdateObjects();
        }

        void InitObjects()
        {
            if (player == null || prefab == null) return;

            RemoveInstances();
            CreateInstancesParent();
            CreateInstances();
            SpawnObjects();
            CheckRigidbody();
        }

        private void CheckRigidbody()
        {
            if (prefab.GetComponent<Rigidbody>() != null)
            {
                hasRigidbody = true;

                if (prefab.GetComponent<Rigidbody>().isKinematic)
                    isKinematic = true;
            }
        }

        private void CreateInstances()
        {
            instance = new GameObject[instanceCount];

            for (int i = 0; i < instanceCount; i++)
            {
                instance[i] = Instantiate(prefab, instances.transform);
                instance[i].name = prefab.name + "_" + (i + 1).ToString();
            }
        }

        private void SpawnObjects()
        {
            if (player == null || prefab == null) return;

            Random.InitState(seedNo);
            if (waterDetection == WaterDetection.bypassWater) isBypassWater = true;
            else isBypassWater = false;
            if (waterDetection == WaterDetection.underWater) isUnderwater = true;
            else isUnderwater = false;

            if (!Application.isPlaying)
            {
                Physics.autoSimulation = false;
                Physics.Simulate(Time.fixedDeltaTime);
            }

            for (int i = 0; i < instanceCount; i++)
            {
                Vector3 origin = player.transform.position;
                origin += Random.insideUnitSphere * spawnRadius;
                origin.y = checkingHeight;
                Ray ray = new Ray(origin, Vector3.down);
                RaycastHit hit;

                if (!Raycasts.RaycastNonAllocSorted(ray, isBypassWater, isUnderwater, out hit, layerMask))
                {
                    RemoveInstance(i);
                    continue;
                }

                Vector3 normal = hit.normal;
                origin = hit.point;

                if (Vector3.Angle(normal, Vector3.up) >= slopeRange.x && Vector3.Angle(normal, Vector3.up) <= slopeRange.y)
                {
                    if (origin.y >= heightRange.x && origin.y <= heightRange.y)
                    {
                        // --- position offset
                        origin += positionOffset;

                        // --- rotation
                        if (getGroundAngle)
                        {
                            Vector3 finalRotation = Quaternion.FromToRotation(Vector3.up, normal).eulerAngles;
                            Quaternion surfaceRotation = Quaternion.Euler(finalRotation);

                            if (!lock90DegreeRotation)
                            {
                                float rotationY = Random.Range(rotationRange.x, rotationRange.y);
                                surfaceRotation *= Quaternion.AngleAxis(rotationY, Vector3.up);
                            }
                            else
                            {
                                float rotationY = Mathf.Round(Random.Range(0, 4f)) * 90;
                                surfaceRotation *= Quaternion.AngleAxis(rotationY, Vector3.up);
                                surfaceRotation.eulerAngles = new Vector3(surfaceRotation.eulerAngles.x, rotationY, surfaceRotation.eulerAngles.z);
                            }

                            rotation = surfaceRotation * Quaternion.Euler(rotationOffset);
                        }
                        else
                        {
                            float rotationX = rotationOffset.x;
                            float rotationY = rotationOffset.y;
                            float rotationZ = rotationOffset.z;

                            if (!lock90DegreeRotation)
                            {
                                rotationX += Random.Range(rotationRange.x, rotationRange.y);
                                rotationY += Random.Range(rotationRange.x, rotationRange.y);
                                rotationZ += Random.Range(rotationRange.x, rotationRange.y);
                            }
                            else
                            {
                                rotationX += Mathf.Round(Random.Range(0, 4f)) * 90;
                                rotationY += Mathf.Round(Random.Range(0, 4f)) * 90;
                                rotationZ += Mathf.Round(Random.Range(0, 4f)) * 90;
                            }

                            if (lockYRotation)
                            {
                                rotationX = rotationOffset.x;
                                rotationZ = rotationOffset.z;
                            }

                            rotation = Quaternion.Euler(new Vector3(rotationX, rotationY, rotationZ));
                        }

                        // --- scaling
                        Vector3 lossyScale = scale;
                        float randomScale = Random.Range(scaleRange.x, scaleRange.y);
                        lossyScale.x *= randomScale;
                        lossyScale.y *= randomScale;
                        lossyScale.z *= randomScale;

                        instance[i].transform.position = origin;
                        instance[i].transform.rotation = rotation;
                        instance[i].transform.localScale = lossyScale;
                    }
                    else
                        RemoveInstance(i);
                }
                else
                    RemoveInstance(i);
            }

            if (!Application.isPlaying)
                Physics.autoSimulation = true;
        }

        private void ResetRigidbody(int index)
        {
            if (!hasRigidbody) return;

            Rigidbody rigidbody = instance[index].GetComponent<Rigidbody>();
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.isKinematic = isKinematic;
        }

        private void SpawnObject(int i, Vector3 origin)
        {
            if (player == null || prefab == null) return;

            ResetRigidbody(i);

            if (!Application.isPlaying)
            {
                Physics.autoSimulation = false;
                Physics.Simulate(Time.fixedDeltaTime);
            }

            //origin = player.transform.position;
            //origin += Random.insideUnitSphere * spawnRadius;
            origin.y = checkingHeight;
            Ray ray = new Ray(origin, Vector3.down);
            RaycastHit hit;

            if (!Raycasts.RaycastNonAllocSorted(ray, isBypassWater, isUnderwater, out hit, layerMask))
                return;

            Vector3 normal = hit.normal;
            origin = hit.point;

            if (Vector3.Angle(normal, Vector3.up) >= slopeRange.x && Vector3.Angle(normal, Vector3.up) <= slopeRange.y)
            {
                if (origin.y >= heightRange.x && origin.y <= heightRange.y)
                {
                    // --- position offset
                    origin += positionOffset;

                    // --- rotation
                    if (getGroundAngle)
                    {
                        Vector3 finalRotation = Quaternion.FromToRotation(Vector3.up, normal).eulerAngles;
                        Quaternion surfaceRotation = Quaternion.Euler(finalRotation);

                        if (!lock90DegreeRotation)
                        {
                            float rotationY = Random.Range(rotationRange.x, rotationRange.y);
                            surfaceRotation *= Quaternion.AngleAxis(rotationY, Vector3.up);
                        }
                        else
                        {
                            float rotationY = Mathf.Round(Random.Range(0, 4)) * 90;
                            surfaceRotation *= Quaternion.AngleAxis(rotationY, Vector3.up);
                            surfaceRotation.eulerAngles = new Vector3(surfaceRotation.eulerAngles.x, rotationY, surfaceRotation.eulerAngles.z);
                        }

                        rotation = surfaceRotation * Quaternion.Euler(rotationOffset);
                    }
                    else
                    {
                        float rotationX = rotationOffset.x;
                        float rotationY = rotationOffset.y;
                        float rotationZ = rotationOffset.z;

                        if (!lock90DegreeRotation)
                        {
                            rotationX += Random.Range(rotationRange.x, rotationRange.y);
                            rotationY += Random.Range(rotationRange.x, rotationRange.y);
                            rotationZ += Random.Range(rotationRange.x, rotationRange.y);
                        }
                        else
                        {
                            rotationX += Mathf.Round(Random.Range(0, 4)) * 90;
                            rotationY += Mathf.Round(Random.Range(0, 4)) * 90;
                            rotationZ += Mathf.Round(Random.Range(0, 4)) * 90;
                        }

                        if (lockYRotation)
                        {
                            rotationX = rotationOffset.x;
                            rotationZ = rotationOffset.z;
                        }

                        rotation = Quaternion.Euler(new Vector3(rotationX, rotationY, rotationZ));
                    }

                    // --- scaling
                    Vector3 lossyScale = scale;
                    float randomScale = Random.Range(scaleRange.x, scaleRange.y);
                    lossyScale.x *= randomScale;
                    lossyScale.y *= randomScale;
                    lossyScale.z *= randomScale;

                    instance[i].transform.position = origin;
                    instance[i].transform.rotation = rotation;
                    instance[i].transform.localScale = lossyScale;
                }
            }

            if (!Application.isPlaying)
                Physics.autoSimulation = true;
        }

        private void RemoveInstance(int i)
        {
            if (Application.isPlaying)
                Destroy(instance[i]);
            else
                DestroyImmediate(instance[i]);

            instance.ToList().RemoveAt(i);

            //instance[i].SetActive(false);
        }

        private void UpdateObjects()
        {
            if (!Application.isPlaying && updateSettings)
            {
                InitObjects();
                updateSettings = false;
            }

            if (!resetInstances) return;

            for (int i = 0; i < instance.Length; i++)
            {
                if (instance[i] == null) continue;

                float distance = (new Vector2(instance[i].transform.position.x, instance[i].transform.position.z) - new Vector2(player.transform.position.x, player.transform.position.z)).sqrMagnitude;

                if (distance > spawnRadius * spawnRadius)
                {
                    instance[i].SetActive(false);

                    //Vector2 dir = -(new Vector2(instance[i].transform.position.x, instance[i].transform.position.z) - new Vector2(player.transform.position.x, player.transform.position.z)).normalized;
                    //SpawnObject(i, player.transform.position + (new Vector3(dir.x, 0, dir.y) * spawnRadius));

                    SpawnObject(i, RandomCircle(player.transform.position, spawnRadius * 0.85f));
                }
                else
                    instance[i].SetActive(true);
            }
        }

        private Vector3 RandomCircle(Vector3 center, float radius)
        {
            float ang = Random.value * 360;
            Vector3 pos;
            pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
            pos.y = center.y;
            pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
            return pos;
        }
    }
}

