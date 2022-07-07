/// <summary>
/// In Game: Full CullingGroup API implementation to cull and LOD patches hense much better performance.
/// In Editor: Uses traditional distance and AABB Plane checks for LOD & Culling.
/// </summary>

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using TerraUnity.UI;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class TScatterParams : TScatterLayer
    {
        [HideInInspector] public Patch[] _patches;

        [Serializable]
        public struct LODMaterials
        {
            public Material[] subMaterials;
        }
        [HideInInspector] public LODMaterials[] LODsMaterials;


        //private float densityTemp = -1f;

        //TODO: Uncomment [HideInInspector] after debugging
        //[HideInInspector]
        public List<float> LODDistances;
        [Range(0.1f, 5f)] public float LODMultiplier = 1;

        [HideInInspector] public bool LODGroupNotDetected;
        [ConditionalHide("LODGroupNotDetected")] public float maxDistance;
        //[HideInInspector] public List<string> LODs;
        [HideInInspector] public List<GameObject> LODsGO;
        [HideInInspector] public GameObject LODWithCollider;
        [HideInInspector] public bool colliderDetected;
        [HideInInspector] public Mesh[] LODsMeshes;

        // [HideInInspector] public float gridSizeValue;

        [HideInInspector] public float renderDistance;
        public UnityEngine.Rendering.ShadowCastingMode shadowCastMode;
        public bool receiveShadows;

        [HideInInspector] public bool reDrawInstances;
        [HideInInspector] public int gridResolution = 10;

        //[HideInInspector] public TAreaBounds activeAreaBounds;

        private Camera renderingCamera;
        private CullingGroup localCullingGroup;
        private BoundingSphere[] cullingPoints;

        [HideInInspector] public int[] cullingPoints2Patch;

        private Vector3 lastPositionTransform;
        private Vector3 lastPositionCamera;
        private Quaternion lastRotationCamera;
        [HideInInspector] public int patchesRowCount;
        [HideInInspector] public int patchesColCount;
        [HideInInspector] public float patchScale;

        private static int instanceCount = 0;
        private static int patchCount = 0;

        //private int[] activeIndices = null;
        private int[] activePatchesCount = null;
        [HideInInspector] public int[] activeIndices;
        private List<Matrix4x4>[,] matricesList = null;

        [HideInInspector] public bool runtimeMode;

        //[ConditionalHide("colliderDetected")] public bool hasCollision;
        private Matrix4x4 lastColliderMatrix;

        [Tooltip("Increase this value to prevent offscreen elements disappearing while their shadow is still in screen. Value of 1.1 means 10% bigger offscreen switching to keep elements outside camera frustum for 10% of patch size.")]
        [Range(1f, 3f)] public float frustumMultiplier = 1.1f;

        private bool randomColors = false;

        [HideInInspector] public Color[] randomColorsList = null;
        [HideInInspector] public bool isCompoundModel = false;

        private int progressID = -1;
        //private float deadZoneMeters = 1;
        public List<int> validIndices;

        private float cameraHeight;
        private float occlusionCheckHeight = 150f;
        //private float occlusionDistance;

        //private GameObject patchBox;
        //private Material patchBoxMaterial;
        //private Bounds patchBoxBounds;
        ////private Mesh patchBoxMesh;


        // Prefab Analysis
        //-------------------------------------------------------------------------------------------------------

        TScatterParams()
        {
#if UNITY_EDITOR
            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdate;
#endif
        }

        public GameObject Prefab
        {
            get => prefab;
            set
            {
                prefab = value;
                GetPrefabLODs();
            }
        }

        public void SetPrefabWithoutUpdatePatches(GameObject Prefab)
        {
            prefab = Prefab;
        }

        private void GetPrefabLODs()
        {
            if (prefab == null) return;
            GetPrefabCollider();
            SetLODDistances();
            GetBiggestFaceLength();
            GeneratePatches();
            SyncLayer(null, true);
        }

        public void SyncLayer (List<int> indices = null, bool showProgress = false)
        {
            UpdatePatches(indices, showProgress);
            InitcullingPoints2Patch();
            SetRandomColors();
            SetCullingLODEditor(true);
        }

        public void SetRandomColors()
        {
            if (!randomColors) return;
            randomColorsList = new Color[cullingPoints2Patch.Length];

            for (int i = 0; i < randomColorsList.Length; i++)
            {
                float grayscale = UnityEngine.Random.Range(0.5f, 1f);
                randomColorsList[i] = new Color(grayscale, grayscale, grayscale, 1);
            }
        }

        public bool GetPrefabCollider()
        {
            bool result = false;

            foreach (Transform t in prefab.GetComponentsInChildren(typeof(Transform), false))
            {
                if (!result && t.GetComponent<Collider>() != null && t.GetComponent<Collider>().enabled) // && hasCollision
                {
                    LODWithCollider = t.gameObject;
                    result = true;
                }
            }

            if (!result)
            {
                foreach (Transform t in prefab.GetComponentsInChildren(typeof(Transform), false))
                {
                    if (!result && t.GetComponent<MeshCollider>() != null && t.GetComponent<MeshCollider>().enabled) // && hasCollision
                    {
                        LODWithCollider = t.gameObject;
                        result = true;
                    }
                }
            }

            colliderDetected = result;
            return result;
        }

        private void SetLODDistances()
        {
            SetRenderingCamera();
            LODsGO = new List<GameObject>();
            LODDistances = new List<float>();
            LODGroup lODGroup = prefab.GetComponent<LODGroup>();

            if (lODGroup != null)
            {
                LODGroupNotDetected = false;
                LOD[] lods = lODGroup.GetLODs();
                int index = 0;

                for (int i = 0; i < lods.Length; i++)
                {
                    Renderer[] renderers = lods[i].renderers;
                    if (renderers == null || renderers.Length == 0) continue;
                    Renderer renderer = renderers[0];
                    if (renderer == null) continue;
                    if (renderer.sharedMaterial == null) continue;
                    if (renderer.gameObject.GetComponent<MeshFilter>().sharedMesh == null) continue;
                    LODsGO.Add(renderer.gameObject);
                    float LODDistance = GetDistanceToCamera(LODsGO[index], lods[i].screenRelativeTransitionHeight);
                    LODDistances.Add(LODDistance);
                    index++;
                }

                LODsMeshes = new Mesh[LODsGO.Count];
                LODsMaterials = new LODMaterials[LODsGO.Count];

                for (int i = 0; i < LODsGO.Count; i++)
                {
                    LODsMeshes[i] = LODsGO[i].GetComponent<MeshFilter>().sharedMesh;
                    Material[] _LODsMaterials = LODsGO[i].GetComponent<Renderer>().sharedMaterials;
                    LODsMaterials[i].subMaterials = new Material[_LODsMaterials.Length];

                    for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                        LODsMaterials[i].subMaterials[submeshIndex] = LODsGO[i].GetComponent<Renderer>().sharedMaterials[submeshIndex];
                }

                if (LODDistances.Count > 0)
                    LODDistances[LODDistances.Count - 1] = GetDistanceToCamera(LODsGO[LODsGO.Count - 1], lods[lods.Length - 1].screenRelativeTransitionHeight);
            }
            else
            {
                LODGroupNotDetected = true;

                foreach (Transform t in prefab.GetComponentsInChildren(typeof(Transform), false))
                    if (t.GetComponent<MeshFilter>() != null && t.GetComponent<Renderer>() != null)
                        LODsGO.Add(t.gameObject);

                if (LODsGO != null && LODsGO.Count > 0)
                {
                    if (LODsGO.Count == 1)
                        isCompoundModel = false;
                    else
                        isCompoundModel = true;

                    if (!isCompoundModel)
                    {
                        LODDistances.Add(maxDistance);
                        LODsMeshes = new Mesh[1];
                        LODsMaterials = new LODMaterials[1];
                        LODsMeshes[0] = LODsGO[0].GetComponent<MeshFilter>().sharedMesh;
                        Material[] _LODsMaterials = LODsGO[0].GetComponent<Renderer>().sharedMaterials;
                        LODsMaterials[0].subMaterials = new Material[_LODsMaterials.Length];

                        for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                            LODsMaterials[0].subMaterials[submeshIndex] = LODsGO[0].GetComponent<Renderer>().sharedMaterials[submeshIndex];
                    }
                    else
                    {
                        LODDistances.Add(maxDistance);
                        LODsMeshes = new Mesh[LODsGO.Count];
                        LODsMaterials = new LODMaterials[LODsGO.Count];

                        for (int i = 0; i < LODsGO.Count; i++)
                        {
                            LODsMeshes[i] = LODsGO[i].GetComponent<MeshFilter>().sharedMesh;
                            Material[] _LODsMaterials = LODsGO[i].GetComponent<Renderer>().sharedMaterials;
                            LODsMaterials[i].subMaterials = new Material[_LODsMaterials.Length];

                            for (int submeshIndex = 0; submeshIndex < _LODsMaterials.Length; submeshIndex++)
                                LODsMaterials[i].subMaterials[submeshIndex] = LODsGO[i].GetComponent<Renderer>().sharedMaterials[submeshIndex];
                        }
                    }
                }
            }
        }


        // Patches Generation
        //-------------------------------------------------------------------------------------------------------

        public override void UpdateLayer()
        {
            if (!CheckMask()) return;
            GetPrefabLODs();
        }

        private static float GCD(float a, float b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a == 0 ? b : a;
        }

        private void GeneratePatches()
        {
            float worldSizeMetersX = terrain.terrainData.size.x;
            float worldSizeMetersZ = terrain.terrainData.size.z;
            float maxRenderingDistance = LODDistances[LODDistances.Count - 1];

            //float BMM100 = 2;
            //
            //if (LODDistances.Count > 1)
            //{
            //    float A1 = Mathf.Round(LODDistances[0] / averageDistance);
            //    float A2 = Mathf.Round((LODDistances[1] - LODDistances[0]) / averageDistance);
            //    BMM100 = Mathf.Clamp(GCD(A1, A2), 2, 10);
            //}

            //   patchWidth = (int)(BMM100 * averageDistance);
            //patchWidth = (int)(worldSizeMetersX / gridResolution);
            //if (patchWidth > (worldSizeMetersX / 4)) patchWidth = (int)(worldSizeMetersX / 4);
            //if (patchWidth < 2) patchWidth = 2;
            //if (patchWidth < averageDistance) patchWidth = (int)averageDistance + 1;
            //if (patchWidth > (averageDistance * 30)) patchWidth = (int)averageDistance * 30;

            // Higher value reduces polygons around camera, but creates larger lists to render
            gridResolution = (int)Mathf.Clamp(Mathf.InverseLerp(16000, 2000, worldSizeMetersX) * 16, 8, 16);
            patchScale = (int)Mathf.Clamp(maxRenderingDistance / gridResolution, 2f, maxRenderingDistance / 4f);
            patchScale = (int)Mathf.Clamp(patchScale, averageDistance + 1f, averageDistance * 30f);
            //int maxInstancesPerPatch = 128; // Max. allowed -> 1024
            //patchWidth = Mathf.CeilToInt(Mathf.Clamp(Mathf.Pow(LODDistances[LODDistances.Count - 1] / averageDistance, 2f) / maxInstancesPerPatch, averageDistance, averageDistance * 30f));

            patchesRowCount = (int)(worldSizeMetersZ / patchScale);
            patchesColCount = (int)(worldSizeMetersX / patchScale);
            _patches = new Patch[patchesColCount * patchesRowCount];

            for (int t = 0; t < patchesRowCount; t++)
            {
                for (int u = 0; u < patchesColCount; u++)
                {
                    double xNormal = ((u + 0.5d) * 1.0d / patchesColCount);
                    double zNormal = ((t + 0.5d) * 1.0d / patchesRowCount);
                    float height = terrain.terrainData.GetInterpolatedHeight((float)xNormal, (float)zNormal);

                    Vector3 patchCenterWorldPosition = new Vector3((float)(xNormal * terrain.terrainData.size.x), height, (float)(zNormal * terrain.terrainData.size.z));
                    patchCenterWorldPosition.x += terrain.transform.position.x;
                    patchCenterWorldPosition.z += terrain.transform.position.z;

                    _patches[t * patchesColCount + u].position = patchCenterWorldPosition;
                    _patches[t * patchesColCount + u].scale = patchScale;

                    //GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //dummy.name = "Sper - " + t + "-" + u + "-" + ((int)(t * patchesColCount + u)).ToString();
                    //dummy.transform.position = _patches[t * patchesColCount + u].position;
                    //dummy.transform.localScale = new Vector3(patchWidth, patchWidth, patchWidth);
                }
            }

            //int indexpatch;
            //int row;
            //int col;
            //GetPachesRowCol(new Vector2 (-100,-774),out indexpatch, out row, out col);
            //GameObject dummy1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //dummy1.name = "Index - " + row  + " - "+ col  + " - "+ indexpatch;
            //dummy1.transform.position = new Vector3(-100, _patches[indexpatch].position.y, -774);
            //dummy1.transform.localScale = new Vector3(20, 20, 20);
        }

        private void UpdatePatches(List<int> indices = null, bool showProgress = false)
        {
            if (_patches == null || _patches.Length == 0) return;
            lastPositionTransform = Vector3.zero;

            try
            {

#if UNITY_EDITOR
                if (showProgress)
                {
                    progressID = TProgressBar.StartProgressBar("TERRAWORLD", "Updating Objects & Instances Placement for: " + gameObject.name, TProgressBar.ProgressOptionsList.Indefinite, false);
                    TProgressBar.DisplayProgressBar("TERRAWORLD", "Updating Objects & Instances Placement for: " + gameObject.name, 0.5f, progressID);
                }
#endif

                int terrainLayersCount = _terrain.terrainData.terrainLayers.Length;
                if (exclusionOpacities == null || exclusionOpacities.Length == 0 || exclusionOpacities.Length != terrainLayersCount)
                {
                    exclusionOpacities = new float[terrainLayersCount];
                    for (int i = 0; i < exclusionOpacities.Length; i++) exclusionOpacities[i] = 1f;
                }

                for (int i = 0; i < _patches.Length; i++)
                {
                    if (indices != null && !indices.Contains(i)) continue;

                    _patches[i].matrices = DataToMatrix.GenerateMatrices
                    (
                        maskData,
                        terrain,
                        _patches[i],
                        averageDistance,
                        positionVariation,
                        density,
                        bypassLake,
                        underLake,
                        unityLayerMask,
                        minAllowedAngle,
                        maxAllowedAngle,
                        minAllowedHeight,
                        maxAllowedHeight,
                        positionOffset,
                        getSurfaceAngle,
                        lock90DegreeRotation,
                        minRotationRange,
                        maxRotationRange,
                        rotationOffset,
                        lockYRotation,
                        scale,
                        minScale,
                        maxScale,
                        checkBoundingBox,
                        biggestFaceLength,
                        exclusionOpacities,
                        seedNo + i
                    );
                }

                lastPositionTransform = transform.position;
            }
            finally
            {
#if UNITY_EDITOR
                TProgressBar.RemoveProgressBar(progressID);
#endif
            }
        }


        // Events
        //-------------------------------------------------------------------------------------------------------

        private void OnValidate()
        {
            //base.Validate();
            if (prefab == null) return;
          
            for (int i = 0; i < LODDistances.Count; i++)
            {
                LODDistances[i] = Mathf.Clamp(LODDistances[i], 0, 100000);
          
                if (i > 0)
                    if (LODDistances[i] <= LODDistances[i - 1])
                        LODDistances[i] = LODDistances[i - 1] + 1;
            }
          
            if (LODGroupNotDetected && LODDistances != null)
                LODDistances[0] = maxDistance;
          
            SetCullingLODEditor();
        }

        private void OnEnable()
        {
            ConvertMaskFromTexture2D();
            if (renderingCamera != null && lastPositionCamera == renderingCamera.transform.position) lastPositionCamera += Vector3.one;
        }

        void OnDisable()
        {
            DisposeCullingGroup();
        }

        void Start()
        {
            Initialize();
        }

        private void Update()
        {
            if (cullingPoints2Patch == null || cullingPoints2Patch.Length < 1) return;
            TranslatePatchesIfNeeded();
            RenderPatches();
        }

        private void TranslatePatchesIfNeeded()
        {
            if (lastPositionTransform != transform.position)
            {
                Vector3 delta = transform.position - lastPositionTransform;

                for (int i = 0; i < _patches.Length; i++)
                {
                    _patches[i].position += delta;

                    if (_patches[i].matrices != null)
                    {
                        for (int j = 0; j < _patches[i].matrices.Count; j++)
                        {
                            Matrix4x4 matrix = _patches[i].matrices[j];
                            matrix.m03 = _patches[i].matrices[j].m03 + delta.x;
                            matrix.m13 = _patches[i].matrices[j].m13 + delta.y;
                            matrix.m23 = _patches[i].matrices[j].m23 + delta.z;
                            _patches[i].matrices[j] = matrix;
                        }
                    }
                }

                Initialize();
                lastPositionTransform = transform.position;
            }
        }

        void OnPrefabInstanceUpdate(GameObject instance)
        {
#if UNITY_EDITOR
            if (PrefabUtility.GetCorrespondingObjectFromSource(instance) == prefab)
                GetPrefabLODs();
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            PrefabUtility.prefabInstanceUpdated -= OnPrefabInstanceUpdate;
#endif
        }


        // Initialization
        //-------------------------------------------------------------------------------------------------------

        public void Initialize()
        {
            SetRenderingCamera();
            if (Application.isPlaying) InitCullingGroup();
        }

        private void SetRenderingCamera()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                if (!Application.isPlaying && SceneView.lastActiveSceneView != null)
                {
                    Camera temp = SceneView.lastActiveSceneView.camera;
                    if (temp.name.Equals("SceneCamera")) renderingCamera = temp;
                    else renderingCamera = null;
                }
                else
                    renderingCamera = Camera.main;
            }
            else
#endif
                renderingCamera = Camera.main;

            if (renderingCamera == null) return;
        }


        // CullingGroup API
        //-------------------------------------------------------------------------------------------------------

        public void InitcullingPoints2Patch()
        {
            int activePatches = 0;

            for (int i = 0; i < _patches.Length; i++)
                if (_patches[i].matrices != null && _patches[i].matrices.Count > 0)
                    activePatches++;

            cullingPoints2Patch = new int[activePatches];
            int counter = 0;

            for (int i = 0; i < _patches.Length; i++)
            {
                if (_patches[i].matrices != null && _patches[i].matrices.Count > 0)
                {
                    cullingPoints2Patch[counter] = i;
                    counter++;

                    instanceCount += _patches[i].matrices.Count;
                }
            }

            patchCount += _patches.Length;
            //Debug.Log(gameObject.name + " patches count: " + _patches.Length + " Active Patches: " + activePatches + " Total Patches: " + patchCount + " Instances: " + instanceCount);
        }

        private void InitCullingGroup()
        {
            cullingPoints = new BoundingSphere[cullingPoints2Patch.Length];

            for (int i = 0; i < cullingPoints2Patch.Length; i++)
            {
                cullingPoints[i].position = _patches[cullingPoints2Patch[i]].position;

                patchScale = _patches[cullingPoints2Patch[i]].scale;
                float hypotenuseFromCenter = Mathf.Sqrt(patchScale * patchScale + patchScale * patchScale) / 2f * frustumMultiplier;
                cullingPoints[i].radius = hypotenuseFromCenter;
            }

            activePatchesCount = new int[LODDistances.Count];
            matricesList = new List<Matrix4x4>[LODDistances.Count, cullingPoints2Patch.Length];
            activeIndices = new int[cullingPoints2Patch.Length];

            localCullingGroup = new CullingGroup();
            localCullingGroup.onStateChanged = CullingEvent;
            localCullingGroup.SetBoundingSpheres(cullingPoints);

            float[] tempDistances = new float[LODDistances.Count];

            for (int i = 0; i < LODDistances.Count; i++)
                tempDistances[i] = LODDistances[i] * QualitySettings.lodBias * LODMultiplier;

            localCullingGroup.SetBoundingDistances(tempDistances);
            localCullingGroup.SetDistanceReferencePoint(renderingCamera.transform.position);
            localCullingGroup.targetCamera = renderingCamera;

            //patchBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //patchBox.transform.localScale = Vector3.one * patchScale * frustumMultiplier;
            //
            //
            //patchBox.GetComponent<MeshRenderer>().enabled = false;
            //patchBox.GetComponent<BoxCollider>().isTrigger = true;
            //
            ////patchBoxMaterial = new Material(Shader.Find("TerraUnity/Occlusion Detector"));
            ////patchBox.GetComponent<MeshRenderer>().material = patchBoxMaterial;
            ////patchBox.GetComponent<BoxCollider>().enabled = false;
            //
            //
            //patchBox.hideFlags = HideFlags.HideInHierarchy;
            ////patchBoxMesh = patchBox.GetComponent<MeshFilter>().sharedMesh;
            //patchBoxBounds = patchBox.GetComponent<BoxCollider>().bounds;

            //float startDistance = 0;
            //float endDistance = LODDistances[LODDistances.Count - 1];
            //Vector3 patchSize = new Vector3(patchScale * frustumMultiplier, patchScale * frustumMultiplier, patchScale * frustumMultiplier);
            //float maxDistance = LODDistances[LODDistances.Count - 1] * LODDistances[LODDistances.Count - 1];
        }

        private void DisposeCullingGroup()
        {
            if (localCullingGroup == null) return;
            localCullingGroup.onStateChanged -= CullingEvent;
            localCullingGroup.Dispose();
            localCullingGroup = null;
        }

        private void SetReferencePoint()
        {
            if (renderingCamera == null) SetRenderingCamera();
            if (localCullingGroup == null) InitCullingGroup();
            if (localCullingGroup != null) localCullingGroup.SetDistanceReferencePoint(renderingCamera.transform.position);
        }

        void CullingEvent(CullingGroupEvent sphere)
        {
            if (!Application.isPlaying) return;
            //ManageCulling(sphere.index, sphere.isVisible);
            ManageLODs();
        }


        // Rendering
        //-------------------------------------------------------------------------------------------------------

        private void RenderPatches()
        {
            if (Application.isPlaying) SetReferencePoint();
            else SetCullingLODEditor();

            if (activePatchesCount == null) return;
            if (LODDistances == null) return;
            if (LODsMaterials == null) return;
            if (LODsMeshes == null) return;
            if (matricesList == null) return;

            int counter = 0;

            if (!isCompoundModel)
            {
                for (int LODIndex = 0; LODIndex < LODDistances.Count; LODIndex++)
                {
                    for (int i = 0; i < activePatchesCount[LODIndex]; i++)
                    {
                        for (int submeshIndex = 0; submeshIndex < LODsMaterials[LODIndex].subMaterials.Length; submeshIndex++)
                        {
                            if (matricesList[LODIndex, i] == null) continue;

                            if (randomColors)
                            {
                                MaterialPropertyBlock MPB = new MaterialPropertyBlock();
                                MPB.SetColor("_Color", randomColorsList[counter++]);

                                Graphics.DrawMeshInstanced
                                (
                                    LODsMeshes[LODIndex],
                                    submeshIndex,
                                    LODsMaterials[LODIndex].subMaterials[submeshIndex],
                                    matricesList[LODIndex, i],
                                    MPB,
                                    shadowCastMode,
                                    receiveShadows,
                                    unityLayerIndex
                                );
                            }
                            else
                            {
                                Graphics.DrawMeshInstanced
                                (
                                    LODsMeshes[LODIndex],
                                    submeshIndex,
                                    LODsMaterials[LODIndex].subMaterials[submeshIndex],
                                    matricesList[LODIndex, i],
                                    null,
                                    shadowCastMode,
                                    receiveShadows,
                                    unityLayerIndex
                                );
                            }
                        }
                    }
                }
            }
            else
            {
                for (int LODIndex = 0; LODIndex < LODsMeshes.Length; LODIndex++)
                {
                    for (int i = 0; i < activePatchesCount[0]; i++)
                    {
                        for (int submeshIndex = 0; submeshIndex < LODsMaterials[LODIndex].subMaterials.Length; submeshIndex++)
                        {
                            if (matricesList[0, i] == null) continue;

                            Graphics.DrawMeshInstanced
                            (
                                LODsMeshes[LODIndex],
                                submeshIndex,
                                LODsMaterials[LODIndex].subMaterials[submeshIndex],
                                matricesList[0, i],
                                null,
                                shadowCastMode,
                                receiveShadows,
                                unityLayerIndex
                            );
                        }
                    }
                }
            }
        }


        // LOD & Culling Runtime
        //-------------------------------------------------------------------------------------------------------

        //private bool IsInView(Vector3[] targetPoints)
        //{
        //    int hits = 0;
        //
        //    for (int i = 0; i < targetPoints.Length; i++)
        //    {
        //        RaycastHit hit;
        //
        //        // Only check against colliders with Static flag of "Occluder Static" such as Terrains in scene
        //        if (Physics.Linecast(renderingCamera.transform.position, targetPoints[i], out hit))
        //        {
        //            if (GameObjectUtility.AreStaticEditorFlagsSet(hit.transform.gameObject, StaticEditorFlags.OccluderStatic))
        //                hits++;
        //            else
        //                return true;
        //        }
        //    }
        //
        //    //for (int i = 0; i < targetPoints.Length; i++)
        //    //{
        //    //    Vector3 heading = targetPoints[i] - renderingCamera.transform.position;
        //    //    Vector3 direction = heading.normalized;// / heading.magnitude;
        //    //    Ray ray = new Ray(renderingCamera.transform.position, direction);
        //    //    RaycastHit hit;
        //    //
        //    //    // Only check against colliders with Static flag of "Occluder Static" such as Terrains in scene
        //    //    if (Raycasts.RaycastNonAlloc(ray, out hit, ~0, LODDistances[LODDistances.Count - 1]))
        //    //    {
        //    //        if (GameObjectUtility.AreStaticEditorFlagsSet(hit.transform.gameObject, StaticEditorFlags.OccluderStatic))
        //    //            hits++;
        //    //        else
        //    //            return true;
        //    //    }
        //    //}
        //
        //    if (hits == targetPoints.Length) return false;
        //    else return true;
        //}

        //private bool IsInView(Vector3 targetPoint)
        //{
        //    bool result = true;
        //    Vector3 pointOnScreen = renderingCamera.WorldToScreenPoint(targetPoint);
        //    RaycastHit hit;
        //
        //    // Only check against colliders in front of camera with Static flag of "Occluder Static" such as Terrains in scene
        //    if (pointOnScreen.z > patchScale && Physics.Linecast(renderingCamera.transform.position, targetPoint, out hit))
        //    {
        //        Renderer[] renderers = hit.transform.GetComponentsInChildren<Renderer>();
        //        
        //        for (int i = 0; i < renderers.Length; i++)
        //        {
        //            if (renderers[i].isPartOfStaticBatch)
        //            {
        //                result = false;
        //                break;
        //            }
        //        }
        //    
        //        //if (GameObjectUtility.AreStaticEditorFlagsSet(hit.transform.gameObject, StaticEditorFlags.OccluderStatic))
        //        //    result = false;
        //        //else
        //        //    result = true;
        //    }
        //}

        private bool IsInView(Vector3 targetPoint)
        {
            if (Physics.Linecast(renderingCamera.transform.position, targetPoint))
                return false;

            return true;
        }

        private void ManageLODs()
        {
            if (lastRotationCamera == renderingCamera.transform.rotation && lastPositionCamera == renderingCamera.transform.position) return;

            //if (lastRotationCamera == renderingCamera.transform.rotation && lastPositionCamera != renderingCamera.transform.position)
            //{
            //    float lastPosDistance = (lastPositionCamera - renderingCamera.transform.position).sqrMagnitude;
            //    if (lastPosDistance <= deadZoneMeters * deadZoneMeters) return;
            //}

            if (occlusionCulling)
            {
                cameraHeight = renderingCamera.transform.position.y - terrain.SampleHeight(renderingCamera.transform.position);
                //occlusionDistance = Mathf.Pow(LODDistances[0] * QualitySettings.lodBias * LODMultiplier * 2, 2f);
            }

            // Find all visible spheres in different LODs
            for (int i = 0; i < LODDistances.Count; i++)
            {
                activePatchesCount[i] = localCullingGroup.QueryIndices(true, i, activeIndices, 0);

                //int patchesNotInView = 0;
                //int index = 0;

                for (int j = 0; j < activePatchesCount[i]; j++)
                {
                    int patchIndex = cullingPoints2Patch[activeIndices[j]];
                    Patch patch = _patches[patchIndex];

                    if (occlusionCulling)
                    {
                        //float patchDistance = (patch.position - renderingCamera.transform.position).sqrMagnitude;

                        if (cameraHeight < occlusionCheckHeight)
                        //if (patchDistance <= occlusionDistance && cameraHeight < occlusionCheckHeight)
                        {
                            //Vector3[] targetPoints = new Vector3[1];
                            //targetPoints[0] = patch.position + (Vector3.up * patchScale);
                            //targetPoints[1] = patch.position + (Vector3.up * patchScale) + (Vector3.left * patchScale);
                            //targetPoints[2] = patch.position + (Vector3.up * patchScale) + (Vector3.right * patchScale);
                            //targetPoints[3] = patch.position + (Vector3.left * patchScale);
                            //targetPoints[4] = patch.position + (Vector3.right * patchScale);

                            // Ray against top-center point on patch's bounding box to check if any collisions are through
                            if (IsInView(patch.position + (Vector3.up * patchScale * 0.666f)))
                                matricesList[i, j] = patch.matrices;
                            else
                                matricesList[i, j] = null;
                        }
                        else
                            matricesList[i, j] = patch.matrices;
                    }
                    else
                        matricesList[i, j] = patch.matrices;

                    //patchBox.transform.position = patch.position;
                    //Vector3 heading = patchBox.transform.position - renderingCamera.transform.position;
                    //Vector3 direction = heading.normalized;// / heading.magnitude;
                    //
                    //Ray ray = new Ray(renderingCamera.transform.position, renderingCamera.transform.forward * Vector3.Angle(renderingCamera.transform.position, patchBox.transform.position));
                    //
                    //if (!patchBoxBounds.IntersectRay(ray))
                    //{
                    //    patchesNotInView++;
                    //    Debug.Log("Skipped " + patchIndex);
                    //    continue;
                    //}

                    //Graphics.Blit(null, renderTexture, patchBoxMaterial);
                    //RenderTexture.active = renderTexture;
                    //outputTex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0, false);
                    //Color[] colors = outputTex.GetPixels();
                    //bool occluded = false;
                    //
                    //for (int k = 0; k < colors.Length; k++)
                    //{
                    //    if (colors[k] == Color.white)
                    //    {
                    //        occluded = false;
                    //        break;
                    //    }
                    //
                    //    if (k == colors.Length - 1)
                    //    {
                    //        occluded = true;
                    //        Debug.Log("Skipped " + patchIndex);
                    //    }
                    //}
                    //
                    //if (occluded)
                    //{
                    //    patchesNotInView++;
                    //    continue;
                    //}
                    //
                    //matricesList[i, index] = patch.matrices;
                    //index++;

                    //bool notInView = false;
                    //
                    //for (int k = 0; k < 8; k++)
                    //{
                    //    Ray ray = new Ray(renderingCamera.transform.position, renderingCamera.transform.forward * Vector3.Angle(renderingCamera.transform.position, patchBoxMesh.vertices[k]));
                    //    
                    //    if (!Physics.Raycast(ray))
                    //    {
                    //        notInView = true;
                    //        break;
                    //    }
                    //}
                    //
                    //if (notInView)
                    //{
                    //    patchesNotInView++;
                    //    continue;
                    //}

                    //matricesList[i, index] = patch.matrices;
                    //index++;
                }

                //activePatchesCount[i] -= patchesNotInView;
            }

            lastRotationCamera = renderingCamera.transform.rotation;
            lastPositionCamera = renderingCamera.transform.position;
        }


        // LOD Editor
        //-------------------------------------------------------------------------------------------------------

        public void SetCullingLODEditor(bool forced = false)
        {
            if (Application.isPlaying) return;
            if (cullingPoints2Patch == null || cullingPoints2Patch.Length == 0 || LODDistances == null || LODDistances.Count == 0) return;
            if (renderingCamera == null || renderingCamera.name != "SceneCamera") SetRenderingCamera();
            if (renderingCamera == null) return;

            if
            (
                lastPositionCamera != renderingCamera.transform.position ||
                lastRotationCamera != renderingCamera.transform.rotation ||
                forced
            )
            {
                Vector3 cameraPosition = renderingCamera.transform.position;
                if (activePatchesCount == null || activePatchesCount.Length != LODDistances.Count) activePatchesCount = new int[LODDistances.Count];
                if (matricesList == null || matricesList.GetLength(0) != LODDistances.Count || matricesList.GetLength(1) != _patches.Length) matricesList = new List<Matrix4x4>[LODDistances.Count, _patches.Length];

                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(renderingCamera);
                float startDistance = 0;
                float endDistance = LODDistances[LODDistances.Count - 1];
                Vector3 patchSize = new Vector3(patchScale * frustumMultiplier, patchScale * frustumMultiplier, patchScale * frustumMultiplier);
                float maxDistance = LODDistances[LODDistances.Count - 1] * LODDistances[LODDistances.Count - 1];

                for (int i = 0; i < LODDistances.Count; i++)
                {
                    activePatchesCount[i] = 0;

                    for (int j = 0; j < cullingPoints2Patch.Length; j++)
                    {
                        if (cullingPoints2Patch[j] >= _patches.Length) return;
                        if (_patches[cullingPoints2Patch[j]].matrices == null) continue;
                        Vector3 patchPosition = _patches[cullingPoints2Patch[j]].position;
                        float cameraDistance = (cameraPosition - patchPosition).sqrMagnitude;
                        if (cameraDistance > maxDistance) continue;
                        Bounds patchBounds = new Bounds(patchPosition, patchSize);
                        if (!GeometryUtility.TestPlanesAABB(planes, patchBounds)) continue;
                        if (i == 0) startDistance = 0;
                        else startDistance = LODDistances[i - 1] * QualitySettings.lodBias * LODMultiplier;
                        endDistance = LODDistances[i] * QualitySettings.lodBias * LODMultiplier;

                        if (cameraDistance > startDistance * startDistance && cameraDistance <= endDistance * endDistance)
                        {
                            matricesList[i, activePatchesCount[i]] = _patches[cullingPoints2Patch[j]].matrices;
                            activePatchesCount[i]++;
                        }
                    }
                }

#if UNITY_EDITOR
                SceneView.RepaintAll();
#endif

                lastPositionCamera = renderingCamera.transform.position;
                lastRotationCamera = renderingCamera.transform.rotation;
            }
        }


        // Player Interactions
        //-------------------------------------------------------------------------------------------------------

        public Matrix4x4 GetColliderMatrix(Vector3 playerPos)
        {
            if (localCullingGroup == null) return lastColliderMatrix;
            Matrix4x4 result = new Matrix4x4();
            Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.z);
            int index = 0;
            int row = 0;
            int col = 0;

            if (GetPatchesRowCol(playerPos2D, out index, out row, out col))
            {
                float lastDistance = float.MaxValue;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if (GetPatchesIndex((row + i), (col + j), out index))
                        {
                            List<Matrix4x4> matrixList = _patches[index].matrices;

                            for (int k = 0; k < matrixList.Count; k++)
                            {
                                Matrix4x4 matrix = matrixList[k];
                                Vector2 instancePos2D = TMatrix.ExtractTranslationFromMatrix2D(ref matrix);
                                float distance = (instancePos2D - playerPos2D).sqrMagnitude;

                                if (distance < lastDistance)
                                {
                                    result = matrix;
                                    lastDistance = distance;
                                }
                            }
                        }
                    }
                }
            }

            lastColliderMatrix = result;
            return result;
        }

        public List<Matrix4x4> GetInstanceMatrices3D(Vector3 playerPos, float checkDistance, int neighborPatchesCount)
        {
            Vector2 playerPos2D = new Vector2(playerPos.x, playerPos.z);
            List<Matrix4x4> result = new List<Matrix4x4>();
            int index, row, col;
            int indexStart = -neighborPatchesCount;
            int indexEnd = neighborPatchesCount + 1;
            //int patchesCount = (indexStart * -1) + indexEnd;

            if (GetPatchesRowCol(playerPos2D, out index, out row, out col))
            {
                for (int i = indexStart; i < indexEnd; i++)
                {
                    for (int j = indexStart; j < indexEnd; j++)
                    {
                        if (GetPatchesIndex((row + i), (col + j), out index))
                        {
                            validIndices = new List<int>();
                            List<Matrix4x4> matrixList = _patches[index].matrices;

                            for (int k = 0; k < matrixList.Count; k++)
                            {
                                Matrix4x4 matrix = matrixList[k];
                                Vector3 instancePos = TMatrix.ExtractTranslationFromMatrix(ref matrix);
                                float distance = (instancePos - playerPos).sqrMagnitude;

                                if (distance < checkDistance * checkDistance)
                                {
                                    validIndices.Add(k);
                                    result.Add(matrix);
                                }
                            }

                            GPURemove(index, validIndices);
                        }
                    }
                }
            }

            return result;
        }

        public List<Matrix4x4> GetInstanceMatrices2D(Vector2 playerPos2D, float checkDistance, int neighborPatchesCount)
        {
            List<Matrix4x4> result = new List<Matrix4x4>();
            int index, row, col;
            int indexStart = -neighborPatchesCount;
            int indexEnd = neighborPatchesCount + 1;
            //int patchesCount = (indexStart * -1) + indexEnd;

            if (GetPatchesRowCol(playerPos2D, out index, out row, out col))
            {
                for (int i = indexStart; i < indexEnd; i++)
                {
                    for (int j = indexStart; j < indexEnd; j++)
                    {
                        if (GetPatchesIndex((row + i), (col + j), out index))
                        {
                            validIndices = new List<int>();
                            List<Matrix4x4> matrixList = _patches[index].matrices;

                            for (int k = 0; k < matrixList.Count; k++)
                            {
                                Matrix4x4 matrix = matrixList[k];
                                Vector2 instancePos2D = TMatrix.ExtractTranslationFromMatrix2D(ref matrix);
                                float distance = (instancePos2D - playerPos2D).sqrMagnitude;

                                if (distance < checkDistance * checkDistance)
                                {
                                    validIndices.Add(k);
                                    result.Add(matrix);
                                }
                            }

                            GPURemove(index, validIndices);
                        }
                    }
                }
            }

            return result;
        }

        private void GPURemove(int index, List<int> indices)
        {
            for (int k = 0; k < indices.Count; k++)
                _patches[index].matrices.RemoveAt(indices[k] - k);
        }


        // Helpers
        //-------------------------------------------------------------------------------------------------------

        public bool GetPatchesRowCol(Vector2 GlobalPosition, out int Index, out int Row, out int Col)
        {
            Index = Row = Col = 0;
            if (GlobalPosition.x < terrain.transform.position.x) return false;
            if (GlobalPosition.x > terrain.transform.position.x + terrain.terrainData.size.x) return false;
            if (GlobalPosition.y < terrain.transform.position.z) return false;
            if (GlobalPosition.y > terrain.transform.position.z + terrain.terrainData.size.z) return false;
            double xNormal = (GlobalPosition.x - terrain.transform.position.x) * 1.0d / terrain.terrainData.size.x;
            double zNormal = (GlobalPosition.y - terrain.transform.position.z) * 1.0d / terrain.terrainData.size.z;
            Col = (int)(xNormal * patchesColCount);
            Row = (int)(zNormal * patchesRowCount);
            if (GetPatchesIndex(Row, Col, out Index)) return true;

            return false;
        }

        public bool GetPatchesIndex(int Row, int Col, out int Index)
        {
            Index = 0;
            if (Col < 0) return false;
            if (Col >= patchesColCount) return false;
            if (Row < 0) return false;
            if (Row >= patchesRowCount) return false;
            Index = Row * patchesColCount + Col;

            return true;
        }

        private void GetBiggestFaceLength(GameObject go = null)
        {
            biggestFaceLength = float.MinValue;

            if (go == null)
            {
                List<float> lengths = new List<float>();

                for (int i = 0; i < LODsGO.Count; i++)
                {
                    Bounds bounds = LODsGO[i].GetComponent<Renderer>().bounds;

                    if (biggestFaceLength < bounds.extents.x)
                        biggestFaceLength = bounds.extents.x;
                    if (biggestFaceLength < bounds.extents.y)
                        biggestFaceLength = bounds.extents.y;
                    if (biggestFaceLength < bounds.extents.z)
                        biggestFaceLength = bounds.extents.z;

                    lengths.Add(biggestFaceLength);
                }

                biggestFaceLength = lengths.Max();
            }
            else
            {
                Bounds bounds = go.GetComponent<Renderer>().bounds;

                if (biggestFaceLength < bounds.extents.x)
                    biggestFaceLength = bounds.extents.x;
                if (biggestFaceLength < bounds.extents.y)
                    biggestFaceLength = bounds.extents.y;
                if (biggestFaceLength < bounds.extents.z)
                    biggestFaceLength = bounds.extents.z;
            }

            if (scale.x >= scale.z)
                biggestFaceLength *= scale.x * maxScale;
            else
                biggestFaceLength *= scale.z * maxScale;
        }

        private float GetDistanceToCamera(GameObject go, float NormalizedPersentage)
        {
            GetBiggestFaceLength(go);
            return biggestFaceLength * maxScale / (((renderingCamera.pixelRect.height * NormalizedPersentage) / renderingCamera.pixelHeight) * (2 * Mathf.Tan(renderingCamera.fieldOfView / 2 * Mathf.Deg2Rad)));
        }

        //private void ManageCulling (int index, bool visible)
        //{
        //    patchRenderers[index].enabled = visible;
        //}
    }
}

