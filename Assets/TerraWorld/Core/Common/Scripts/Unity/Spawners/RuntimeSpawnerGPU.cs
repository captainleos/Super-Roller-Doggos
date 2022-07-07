using UnityEngine;
using System.Collections.Generic;
using TerraUnity.UI;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class RuntimeSpawnerGPU : MonoBehaviour
    {
        public GameObject player;
        public GameObject prefab;
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

        [HideInInspector] public List<Matrix4x4> matrices = null;

        private float startDistance;
        private float endDistance;
        private GameObject currentLOD;
        private Mesh renderingMesh;
        private Material[] renderingMaterials;
        private GameObject currentCam;

        public List<float> LODDistances;
        private float maxDistance;
        private List<string> LODs;
        private List<GameObject> LODsGO;
        private Mesh[] LODsMeshes;
        private Material[][] LODsMaterials;
        private GameObject lastPrefab = null;

        public UnityEngine.Rendering.ShadowCastingMode shadowCastMode = UnityEngine.Rendering.ShadowCastingMode.On;
        public bool receiveShadows = true;
        public string instanceLayer = "Default";
        int LODIndex = 0;

        public GameObject Prefab
        {
            get => prefab;
            set
            {
                prefab = value;
                GetPrefabLODs();
                InitLODs();
            }
        }

        public void InitLODs()
        {
            if (prefab == lastPrefab && LODsMaterials != null) return;
            if (prefab == null) return;
            if (LODsGO == null) GetPrefabLODs();

            LODsMeshes = new Mesh[LODsGO.Count];
            LODsMaterials = new Material[LODsGO.Count][];

            for (int i = 0; i < LODsGO.Count; i++)
            {
                if (LODsGO[i].GetComponent<MeshFilter>() != null)
                {
                    LODsMeshes[i] = LODsGO[i].GetComponent<MeshFilter>().sharedMesh;
                    Renderer renderer = LODsGO[i].GetComponent<Renderer>();
                    Material[] _LODsMaterials = renderer.sharedMaterials;
                    LODsMaterials[i] = new Material[_LODsMaterials.Length];

                    for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                        LODsMaterials[i][submeshIndex] = _LODsMaterials[submeshIndex];
                }
                else if (LODsGO[i].GetComponent<SkinnedMeshRenderer>() != null)
                {
                    SkinnedMeshRenderer renderer = LODsGO[i].GetComponent<SkinnedMeshRenderer>();
                    LODsMeshes[i] = renderer.sharedMesh;
                    Material[] _LODsMaterials = renderer.sharedMaterials;
                    LODsMaterials[i] = new Material[_LODsMaterials.Length];

                    for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                        LODsMaterials[i][submeshIndex] = _LODsMaterials[submeshIndex];
                }
            }

            lastPrefab = prefab;
        }

        private void GetPrefabLODs()
        {
            LODIndex = 0;
            if (LODDistances.Count == 0) LODDistances.Add(spawnRadius / 10f);
            List<float> LODDistancesTemp = new List<float>();
            LODsGO = new List<GameObject>();
            int index = 0;

            foreach (Transform t in prefab.GetComponentsInChildren(typeof(Transform), false))
            {
                if (t.GetComponent<MeshFilter>() != null && t.GetComponent<Renderer>() != null)
                {
                    LODsGO.Add(t.gameObject);

                    if (index < LODDistances.Count)
                        LODDistancesTemp.Add(LODDistances[index]);
                    else
                        LODDistancesTemp.Add(LODDistancesTemp[LODDistancesTemp.Count - 1] + 100);

                    index++;
                }
                else if (t.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    LODsGO.Add(t.gameObject);

                    if (index < LODDistances.Count)
                        LODDistancesTemp.Add(LODDistances[index]);
                    else
                        LODDistancesTemp.Add(LODDistancesTemp[LODDistancesTemp.Count - 1] + 100);

                    index++;
                }
            }

            LODDistances = LODDistancesTemp;
        }

        private void OnValidate()
        {
            InitObjects();

            if (prefab == null) return;

            if (prefab != lastPrefab)
                GetPrefabLODs();

            for (int i = 0; i < LODDistances.Count; i++)
            {
                LODDistances[i] = Mathf.Clamp(LODDistances[i], 0, 100000);

                if (i > 0)
                    if (LODDistances[i] <= LODDistances[i - 1])
                        LODDistances[i] = LODDistances[i - 1] + 1;
            }

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        private void Start()
        {
            //InitObjects();
        }

        void FixedUpdate()
        {
            if (Application.isPlaying)
                UpdateObjects();
        }

        void Update()
        {
            if (player == null || prefab == null) return;

            if (!Application.isPlaying)
                UpdateObjects();

#if UNITY_EDITOR
            //if (Application.isEditor)
            {
                if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null)
                {
                    Camera temp = UnityEditor.SceneView.lastActiveSceneView.camera;

                    if (temp.name.Equals("SceneCamera"))
                        currentCam = temp.gameObject;
                    else
                        currentCam = null;
                }
                else
                    currentCam = Camera.main.gameObject;
            }
#else
        //else
            currentCam = Camera.main.gameObject;
#endif

            float cameraDistance = (currentCam.transform.position - player.transform.position).sqrMagnitude;

            for (int i = 0; i < LODDistances.Count; i++)
            {
                if (i == 0) startDistance = 0;
                else startDistance = LODDistances[i - 1];
                endDistance = LODDistances[i];

                if (cameraDistance > startDistance * startDistance && cameraDistance <= endDistance * endDistance)
                {
                    LODIndex = i;
                    break;
                }
            }

            InitLODs();

            if (LODsMaterials == null || LODsMaterials.Length == 0) return;

            for (int submeshIndex = 0; submeshIndex < LODsMaterials[LODIndex].Length; submeshIndex++)
                Graphics.DrawMeshInstanced(LODsMeshes[LODIndex], submeshIndex, LODsMaterials[LODIndex][submeshIndex], matrices, null, shadowCastMode, receiveShadows, LayerMask.NameToLayer(instanceLayer));
        }

        void InitObjects()
        {
            if (player == null || prefab == null) return;
            SpawnObjects();
        }

        private void UpdateObjects()
        {
            if (player == null || prefab == null) return;
            if (matrices == null || matrices.Count == 0) InitObjects();

            //TODO: Only update if player changed position from last state

            //List<int> indices = new List<int>();
            //List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < matrices.Count; i++)
            {
                Matrix4x4 matrix = matrices[i];
                Vector3 matrixPosition = TMatrix.ExtractTranslationFromMatrix(ref matrix);
                float distance = (new Vector2(matrixPosition.x, matrixPosition.z) - new Vector2(player.transform.position.x, player.transform.position.z)).sqrMagnitude;

                if (distance > spawnRadius * spawnRadius)
                {
                    //Vector2 dir = -(new Vector2(matrixPosition.x, matrixPosition.z) - new Vector2(player.transform.position.x, player.transform.position.z)).normalized;
                    //SpawnObject(i, player.transform.position + (new Vector3(dir.x, 0, dir.y) * spawnRadius));

                    SpawnObject(i, RandomCircle(player.transform.position, spawnRadius * 0.85f));

                    //indices.Add(i);
                    //positions.Add(player.transform.position + (new Vector3(dir.x, 0, dir.y) * spawnRadius));
                }
            }

            //SpawnObject(indices, positions);
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

        private void SpawnObjects()
        {
            if (player == null || prefab == null) return;

            Random.InitState(seedNo);
            matrices = new List<Matrix4x4>();
            if (waterDetection == WaterDetection.bypassWater) isBypassWater = true;
            else isBypassWater = false;
            if (waterDetection == WaterDetection.underWater) isUnderwater = true;
            else isUnderwater = false;

            Physics.autoSimulation = false;
            Physics.Simulate(Time.fixedDeltaTime);

            for (int i = 0; i < instanceCount; i++)
            {
                Vector3 origin = player.transform.position;
                origin += Random.insideUnitSphere * spawnRadius;
                origin.y = checkingHeight;
                Ray ray = new Ray(origin, Vector3.down);
                RaycastHit hit;

                if (!Raycasts.RaycastNonAllocSorted(ray, isBypassWater, isUnderwater, out hit, layerMask))
                    continue;

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
                                float rotationY = Mathf.Round(Random.Range(0f, 4)) * 90;
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
                                rotationX += Mathf.Round(Random.Range(0f, 4)) * 90;
                                rotationY += Mathf.Round(Random.Range(0f, 4)) * 90;
                                rotationZ += Mathf.Round(Random.Range(0f, 4)) * 90;
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

                        matrices.Add(Matrix4x4.TRS(origin, rotation, lossyScale));
                    }
                }
            }

            Physics.autoSimulation = true;
        }

        private void SpawnObject(int i, Vector3 origin)
        {
            if (player == null || prefab == null) return;

            Physics.autoSimulation = false;
            Physics.Simulate(Time.fixedDeltaTime);
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
                            float rotationY = Mathf.Round(Random.Range(0f, 4)) * 90;
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
                            rotationX += Mathf.Round(Random.Range(0f, 4)) * 90;
                            rotationY += Mathf.Round(Random.Range(0f, 4)) * 90;
                            rotationZ += Mathf.Round(Random.Range(0f, 4)) * 90;
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

                    matrices[i] = Matrix4x4.TRS(origin, rotation, lossyScale);
                }
            }

            Physics.autoSimulation = true;
        }

        private void SpawnObject(List<int> indices, List<Vector3> origins)
        {
            if (player == null || prefab == null) return;

            Physics.autoSimulation = false;
            Physics.Simulate(Time.fixedDeltaTime);
            for (int i = 0; i < indices.Count; i++)
            {
                //origin = player.transform.position;
                //origin += Random.insideUnitSphere * spawnRadius;
                Vector3 origin = origins[i];
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
                                float rotationY = Mathf.Round(Random.Range(0f, 4)) * 90;
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
                                rotationX += Mathf.Round(Random.Range(0f, 4)) * 90;
                                rotationY += Mathf.Round(Random.Range(0f, 4)) * 90;
                                rotationZ += Mathf.Round(Random.Range(0f, 4)) * 90;
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

                        matrices[indices[i]] = Matrix4x4.TRS(origin, rotation, lossyScale);
                    }
                }
            }

            Physics.autoSimulation = true;
        }
    }
}

