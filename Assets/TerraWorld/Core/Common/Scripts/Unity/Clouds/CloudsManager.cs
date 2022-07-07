using UnityEngine;
using UnityEngine.Rendering;

namespace TerraUnity.Runtime
{
    [ExecuteAlways]
    public class CloudsManager : MonoBehaviour
    {
        //Resources
        public Material cloudsMaterial;
        public GameObject cloudPrefab;
        public Mesh cloudMesh;
        public Texture2D cloudMeshTexture;

        public GameObject player;
        public GameObject center;
        public GameObject clouds;
        private Texture cloudTexture;
        private Texture cloudTextureNormal;

        public bool update = false;
        public bool createAtLevelStart = false;
        public bool cloudsAroundPlayer = false;

        public int seed = 12345;
        public float areaSize = 1000f;

        public int cloudCount = 40;
        [Range(0.01f, 100f)] public float density = 1;
        public int maxParticles = 40;

        public Vector3 cloudSize = new Vector3(250f, 250f, 250f);
        //public float meshNormalOffest = 30f;
        public float particleSize = 1750f;

        [Range(1f, 20f)] public float sizeMultiplier = 5f;
        [Range(0f, 1f)] public float randomParticleSize = 0.66f;

        public float altitude = 1500f;
        public float altitudeRange = 100f;
        //public bool updateAltitude = false;

        public bool randomRotation = true;
        [Range(0f, 360f)] public float rotationVariation = 30f;

        public float visibilityDistance = 20000;
        [Range(0f, 1f)] public float visibilityFalloff = 1;
        [Range(0.001f, 3f)] public float visibilityFalloffPower = 1;

        public Color cloudColor = new Color(0.922f, 0.922f, 0.922f, 1f);
        public Color emisssionColor = new Color(0.922f, 0.922f, 0.922f, 1f);
        [Range(0f, 1f)] public float cloudOpacity = 1f;

        [Range(0f, 3f)] public float textureMipMapBias = 0f;

        public bool castShadows = true;

        public float minWorldHeight = 700f;
        public float maxWorldHeight = 1500f;

        //public bool simulateWind = true;
        public bool windMovement = true;
        [Range(0.1f, 2000f)] public float windSpeed = 12f;
        public float windMultiplier = 1f;
        [Range(0f, 359f)] public float windDirection = 225f;

        [HideInInspector] public bool isFlatShading = false;

        public float cloudMeshNormalOffset;
        public int meshMode = 0;

        //TODO: Check the following params later and make them public if still need this feature.
        // Better solution for this, is Runtime FX Spawners around player as we already did.
        //private float subEmitterCheckDistance = 0f;
        //private int subEmitterMaxParticles = 0;
        //[Range(0, 100)] private int emitProbability = 0;
        //private static string subEmitterStr = "_SubEmitter";
        //private float checkDistance2X;
        //private Transform[] particles;
        //private Vector3 lastPosition;

        //private int renderingLayer = 15; //Insert snow/rain layer number
        //private float renderingDistance;

        void Start()
        {
            InitVariables();

            if (Application.isPlaying && createAtLevelStart)
                //update = true;
                Update();
        }

        public void InitVariables()
        {
            if (player == null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null)
                {
                    Camera temp = UnityEditor.SceneView.lastActiveSceneView.camera;

                    if (temp.name.Equals("SceneCamera"))
                        player = temp.gameObject;
                    else
                        player = null;
                }
                else
                    player = Camera.main.gameObject;
#else
                player = Camera.main.gameObject;
#endif
            }

            TResourcesManager.LoadCloudsResources();
        }

        void Update()
        {
            if (player == null || cloudPrefab == null) InitVariables();
            if (cloudsMaterial == null || cloudPrefab == null) return;

            if (cloudsAroundPlayer) center = player;
            else center = this.gameObject;

            if (update)
            {
                if (clouds != null)
                {
                    if (Application.isPlaying)
                        Destroy(clouds);
                    else
                        DestroyImmediate(clouds);
                }

                Initialize();
                update = false;
            }

            //if (TResourcesManager.subEmitterPrefab != null && lastPosition != player.transform.position && particles != null && player != null && clouds != null)
            //{
            //    DeactivateDistantParticles();
            //    lastPosition = player.transform.position;
            //}

            UpdateCloudsAltitude();
            GetCloudsMaterial();
            SetVisibility();
            SetTextureQuality();
        }

        private void Initialize()
        {
            if (cloudPrefab == null) return;

            //checkDistance2X = subEmitterCheckDistance * 2f;
            //SetCloudCullDistance();

            GetCloudsMaterial();
            clouds = new GameObject("Clouds");
            //clouds.transform.parent = TerraUnity.TTerrainGenerator.MainTerraworldGameObject.transform;
            clouds.transform.parent = GameObject.Find("Scene Settings").transform;
            Random.InitState(seed);

            for (int i = 0; i < cloudCount * (density * 0.5f); i++)
            {
                GameObject cloud = Instantiate(cloudPrefab);
                cloud.name = "Cloud " + (i + 1).ToString();
                cloud.transform.parent = clouds.transform;

                cloud.transform.position = new Vector3
                    (
                        Random.Range(center.transform.position.x - areaSize, center.transform.position.x + areaSize),
                        Random.Range(-(altitudeRange / 2f), (altitudeRange / 2f)),
                        Random.Range(center.transform.position.z - areaSize, center.transform.position.z + areaSize)
                    );

                cloud.transform.eulerAngles = new Vector3
                    (
                        0,
                        Random.Range(-rotationVariation, rotationVariation),
                        0
                    );

                float randomSizeMultiplier = Random.Range(1f / sizeMultiplier, sizeMultiplier);

                ParticleSystem cloudSystem = cloud.GetComponent<ParticleSystem>();
                ParticleSystem.ShapeModule cloudShape = cloudSystem.shape;
                float scaleX = cloudSize.x * randomSizeMultiplier * 2;
                float scaleY = cloudSize.y * randomSizeMultiplier * 2;
                float scaleZ = cloudSize.z * randomSizeMultiplier * 2;

                if (cloudMesh != null)
                {
                    cloudShape.shapeType = ParticleSystemShapeType.Mesh;

                    if (meshMode == 0)
                    {
                        cloudShape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
                        cloudShape.normalOffset = 1;
                        cloudShape.texture = null;
                    }
                    else if (meshMode == 1)
                    {
                        cloudShape.meshShapeType = ParticleSystemMeshShapeType.Vertex;
                        cloudShape.meshSpawnMode = ParticleSystemShapeMultiModeValue.Loop;
                        cloudShape.normalOffset = cloudMeshNormalOffset;
                        cloudShape.texture = cloudMeshTexture;
                    }

                    cloudShape.mesh = cloudMesh;
                }
                else
                {
                    cloudShape.shapeType = ParticleSystemShapeType.Sphere;
                    cloudShape.mesh = cloudMesh;
                }

                cloudShape.scale = new Vector3(scaleX, scaleY, scaleZ);

                ParticleSystem.MainModule cloudMain = cloudSystem.main;
                cloudMain.startSize = particleSize * randomSizeMultiplier * Random.Range(randomParticleSize, 1f);

                if (meshMode == 0)
                    cloudMain.maxParticles = maxParticles;
                else if (meshMode == 1)
                    cloudMain.maxParticles = 1;

                //if (TResourcesManager.subEmitterPrefab != null && emitProbability > 0)
                //{
                //    int probability = Random.Range(0, 101);
                //
                //    if (probability <= emitProbability)
                //    {
                //        GameObject subEmitter = Instantiate(TResourcesManager.subEmitterPrefab);
                //        subEmitter.name = cloud.name + subEmitterStr;
                //        subEmitter.transform.SetParent(cloud.transform);
                //        subEmitter.transform.localPosition = Vector3.zero;
                //
                //        ParticleSystem subParticleSystem = subEmitter.GetComponent<ParticleSystem>();
                //
                //        ParticleSystem.MainModule subMainModule = subParticleSystem.main;
                //        subMainModule.maxParticles = subEmitterMaxParticles;
                //        ParticleSystem.EmissionModule emissionModule = subParticleSystem.emission;
                //        emissionModule.rateOverTime = subEmitterMaxParticles;
                //        ParticleSystem.ShapeModule subShapeModule = subParticleSystem.shape;
                //        subShapeModule.shapeType = cloudShape.shapeType;
                //        subShapeModule.meshShapeType = cloudShape.meshShapeType;
                //        subShapeModule.mesh = cloudShape.mesh;
                //        subShapeModule.scale = cloudShape.scale;
                //
                //        // Set rotation to zero in order to sync with parent cloud mesh shape
                //        subEmitter.transform.localEulerAngles = Vector3.zero;
                //
                //        // Run cloud simulation
                //        subParticleSystem.Simulate(1);
                //        subParticleSystem.Play(true);
                //    }
                //}

                //particles = clouds.GetComponentsInChildren<Transform>(true);

                if (windMovement)
                {
                    if (cloud.GetComponent<DynamicSpawner>() == null)
                    {
                        DynamicSpawner dynamicSpawner = cloud.AddComponent<DynamicSpawner>();
                        dynamicSpawner.center = center;
                        dynamicSpawner.simulationAreaSize = new Vector2(areaSize, areaSize);
                    }
                    else
                    {
                        DynamicSpawner dynamicSpawner = cloud.GetComponent<DynamicSpawner>();
                        dynamicSpawner.center = center;
                        dynamicSpawner.simulationAreaSize = new Vector2(areaSize, areaSize);
                    }
                }

                ParticleSystemRenderer cloudRenderer = cloud.GetComponent<ParticleSystemRenderer>();

                if (castShadows)
                {
                    cloudRenderer.shadowCastingMode = ShadowCastingMode.On;
                    cloudRenderer.receiveShadows = false;
                }
                else
                    cloudRenderer.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;

                if (meshMode == 0)
                {
                    cloudRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                    cloudRenderer.mesh = null;
                }
                else if (meshMode == 1)
                {
                    cloudRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                    cloudRenderer.mesh = cloudMesh;
                }

                if (isFlatShading)
                    cloudRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
                else
                    cloudRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Object;

                // Run cloud simulation
                cloudSystem.Simulate(1);
                cloudSystem.Play(true);
            }

            UpdateCloudsAltitude();

            OriginShift OS = clouds.AddComponent<OriginShift>();
            OS.cloudsManager = this;
        }

        private void GetCloudsMaterial()
        {
            cloudPrefab.GetComponent<ParticleSystem>().GetComponent<Renderer>().sharedMaterial = cloudsMaterial;

            if (cloudsMaterial.HasProperty("_MainTex")) cloudTexture = cloudsMaterial.GetTexture("_MainTex");
            if (cloudsMaterial.HasProperty("_BumpMap")) cloudTextureNormal = cloudsMaterial.GetTexture("_BumpMap");
        }

        private void SetVisibility()
        {
            if (cloudsMaterial.HasProperty("_VisibilityDistance")) cloudsMaterial.SetFloat("_VisibilityDistance", visibilityDistance);
            if (cloudsMaterial.HasProperty("_VisibilityFalloff")) cloudsMaterial.SetFloat("_VisibilityFalloff", visibilityFalloff);
            if (cloudsMaterial.HasProperty("_VisibilityFalloffPower")) cloudsMaterial.SetFloat("_VisibilityFalloffPower", visibilityFalloffPower);
            if (cloudsMaterial.HasProperty("_Color")) cloudsMaterial.SetColor("_Color", new Color(cloudColor.r, cloudColor.g, cloudColor.b, cloudOpacity));
        }

        private void SetTextureQuality()
        {
            if (cloudTexture != null) cloudTexture.mipMapBias = textureMipMapBias;
            if (cloudTextureNormal != null) cloudTextureNormal.mipMapBias = textureMipMapBias;
        }

        //private void DeactivateDistantParticles()
        //{
        //    Vector3 playerPosition = player.transform.position;
        //
        //    foreach (Transform p in particles)
        //    {
        //        if (emitProbability > 0 && !p.Equals(clouds.transform) && p.name.EndsWith(subEmitterStr) && playerPosition.y < p.position.y)
        //        {
        //            Vector2 distanceXZ = new Vector2(Mathf.Abs(p.position.x - playerPosition.x), Mathf.Abs(p.position.z - playerPosition.z));
        //            float particleDistance = distanceXZ.x + distanceXZ.y;
        //
        //            ParticleSystem cloudSystem = p.GetComponent<ParticleSystem>();
        //            ParticleSystem.EmissionModule emissionModule = cloudSystem.emission;
        //
        //            if (simulateWind)
        //            {
        //                float height = playerPosition.y;
        //                float normalizedHeight = Mathf.InverseLerp(minWorldHeight, maxWorldHeight, height);
        //
        //                ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = cloudSystem.velocityOverLifetime;
        //                velocityOverLifetime.enabled = true;
        //                velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        //                velocityOverLifetime.speedModifierMultiplier = normalizedHeight * windMultiplier;
        //            }
        //
        //            if (particleDistance <= subEmitterCheckDistance)
        //                emissionModule.enabled = true;
        //            else
        //                emissionModule.enabled = false;
        //
        //            if (particleDistance <= checkDistance2X)
        //                p.gameObject.SetActive(true);
        //            else
        //                p.gameObject.SetActive(false);
        //        }
        //    }
        //}

        public void MoveCloudsWithWind()
        {
            if (!windMovement) return;
            if (clouds == null) return;
            clouds.transform.eulerAngles = new Vector3(0, windDirection, 0);
            clouds.transform.position += clouds.transform.forward * Time.deltaTime * windSpeed * windMultiplier;
        }

        private void UpdateCloudsAltitude()
        {
            if (clouds == null) return;

            clouds.transform.position = new Vector3
                (
                    clouds.transform.position.x,
                    altitude,
                    clouds.transform.position.z
                );
        }

        //private void SetCloudCullDistance()
        //{
        //    renderingDistance = checkDistance2X;
        //    Camera camera = Camera.main;
        //    float[] distances = new float[32];
        //    distances[renderingLayer] = renderingDistance;
        //    camera.layerCullDistances = distances;
        //    print("Set rendering distance for layer " + LayerMask.LayerToName(renderingLayer));
        //}

        void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += OnEditorUpdate;
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= OnEditorUpdate;
#endif
        }

#if UNITY_EDITOR
        protected virtual void OnEditorUpdate()
        {
            Update();
        }
#endif
    }
}

