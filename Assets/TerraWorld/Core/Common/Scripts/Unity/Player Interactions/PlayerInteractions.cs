using UnityEngine;
using System;
using System.Collections.Generic;

namespace TerraUnity.Runtime
{
    public class PlayerInteractions : MonoBehaviour
    {
        // Global Parameters
        public int maximumObjects = 64;
        public float deadZoneMeters = 1;
        public bool checkDistance3D = true;

        // Lists
        public List<GameObject> GPULayers = null;
        public TScatterParams[] GPULayersList = null;
        public bool[] statesGPU = null;
        public float[] GPULayersDistances = null;

        [HideInInspector] public GameObject[,] instanceObjects;
        [HideInInspector] public InstanceState[,] instanceStates;
        [HideInInspector] public List<Matrix4x4>[] incomingMatrices;
        private List<Matrix4x4> GPUReverts;
        private List<int> existingIndices;
        private List<Matrix4x4> newMatrices;
        private List<Type>[] originalTypes;
        private Rigidbody[,] rigidbodies;

        private GameObject CPUObjects;
        private Vector3 lastPosition;
        private Vector3 targetPos3D;
        private Vector2 targetPos2D;
        private bool isInitialized = false;
        
        private float defaultDistance = 150f;

        //[HideInInspector] public List<GameObject> CollisionLayers = null;
        //[HideInInspector] public List<GameObject> GameobjectLayers = null;
        //[HideInInspector] public List<GameObject> FXLayers = null;
        //[HideInInspector] public bool[] statesCollision = null;
        //[HideInInspector] public bool[] statesGameobject = null;
        //[HideInInspector] public bool[] statesFX = null;
        //[HideInInspector] private TScatterParams[] CollisionLayersList;
        //[HideInInspector] private GameObjectLayerManager[] GOLayersList;
        //[HideInInspector] private FXLayerManager[] FXLayersList;
        //private GameObject[] colliders;

#if UNITY_EDITOR
        public void ScanScatters(bool reset = false)
        {
            GPULayers = new List<GameObject>();
            //CollisionLayers = new List<GameObject>();
            //GameobjectLayers = new List<GameObject>();
            //FXLayers = new List<GameObject>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                {
                    if (go.activeSelf && go.GetComponent<TScatterParams>() != null)
                    {
                        //go.GetComponent<TScatterParams>().GetPrefabCollider();
                        //if (go.GetComponent<TScatterParams>().runtimeMode) 
                        GPULayers.Add(go);

                        //if (go.GetComponent<TScatterParams>().colliderDetected) CollisionLayers.Add(go);
                    }

                    //TODO: Implement Gameobject Spawner on parent object as TScatterParams
                    //if (go.GetComponent<GameObjectLayerManager>() != null && go.GetComponent<GameObjectLayerManager>().runtimeMode)
                    //GameobjectLayers.Add(go);

                    //TODO: Implement FX Spawner on parent object as TScatterParams when done implementing RuntimeFXNode
                    //if (go.GetComponent<FXLayerManager>() != null)
                    //FXLayers.Add(go);
                }
            }

            SetInitialStates(reset);
        }

        public void SetInitialStates(bool reset)
        {
            if (GPULayers != null && GPULayers.Count > 0)
            {
                if (reset || statesGPU == null || statesGPU.Length != GPULayers.Count)
                {
                    statesGPU = new bool[GPULayers.Count];
                    for (int i = 0; i < GPULayers.Count; i++) statesGPU[i] = true;
                }

                if (reset || GPULayersDistances == null || GPULayersDistances.Length != GPULayers.Count)
                {
                    GPULayersDistances = new float[GPULayers.Count];
                    for (int i = 0; i < GPULayers.Count; i++) GPULayersDistances[i] = defaultDistance;
                }

                if (reset || GPULayersList == null || GPULayersList.Length != GPULayers.Count)
                {
                    GPULayersList = new TScatterParams[GPULayers.Count];
                    for (int i = 0; i < GPULayers.Count; i++) GPULayersList[i] = GPULayers[i].GetComponent<TScatterParams>();
                }
            }

            //if (CollisionLayers != null && CollisionLayers.Count > 0)
            //{
            //    statesCollision = new bool[CollisionLayers.Count];
            //
            //    for (int i = 0; i < CollisionLayers.Count; i++)
            //        statesCollision[i] = true;
            //}
            //
            //if (GameobjectLayers != null && GameobjectLayers.Count > 0)
            //{
            //    statesGameobject = new bool[GameobjectLayers.Count];
            //
            //    for (int i = 0; i < GameobjectLayers.Count; i++)
            //        statesGameobject[i] = true;
            //}
            //
            //if (FXLayers != null && FXLayers.Count > 0)
            //{
            //    statesFX = new bool[FXLayers.Count];
            //
            //    for (int i = 0; i < FXLayers.Count; i++)
            //        statesFX[i] = true;
            //}
        }
#endif

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdatePlayerZone();
        }

        private void Initialize()
        {
            if (lastPosition == transform.position) lastPosition += Vector3.one;
            InitInstances();
        }

        private void InitInstances()
        {
            if (GPULayers == null || GPULayers.Count == 0) return;

            CPUObjects = new GameObject(gameObject.name + " Interactables");
            instanceObjects = new GameObject[GPULayersList.Length, maximumObjects];
            instanceStates = new InstanceState[GPULayersList.Length, maximumObjects];
            originalTypes = new List<Type>[GPULayersList.Length];
            rigidbodies = new Rigidbody[GPULayersList.Length, maximumObjects];

            for (int i = 0; i < GPULayersList.Length; i++)
            {
                if (statesGPU[i])
                {
                    for (int j = 0; j < maximumObjects; j++)
                    {
                        instanceObjects[i, j] = Instantiate(GPULayersList[i].Prefab);
                        instanceObjects[i, j].name = GPULayersList[i].Prefab.name + " " + (i + 1) + "_" + (j + 1);
                        instanceObjects[i, j].transform.parent = CPUObjects.transform;
                        instanceObjects[i, j].transform.position = Vector3.zero;
                        instanceObjects[i, j].transform.rotation = Quaternion.identity;
                        instanceObjects[i, j].layer = LayerMask.NameToLayer(GPULayersList[i].unityLayerName);

                        instanceStates[i, j] = instanceObjects[i, j].AddComponent<InstanceState>();
                        instanceStates[i, j].initialMatrix = Matrix4x4.identity;
                        instanceStates[i, j].PI = this;
                        instanceStates[i, j].instanceIndex = new Vector2Int(i, j);

                        instanceObjects[i, j].SetActive(false);

                        if (instanceObjects[i, j].GetComponent<Rigidbody>() != null)
                            rigidbodies[i, j] = instanceObjects[i, j].GetComponent<Rigidbody>();
                    }

                    Component[] originalComponents = instanceObjects[i, 0].GetComponentsInChildren(typeof(Component), true);
                    originalTypes[i] = new List<Type>();
                    for (int j = 0; j < originalComponents.Length; j++) originalTypes[i].Add(originalComponents[j].GetType());
                }
            }

            incomingMatrices = new List<Matrix4x4>[GPULayersList.Length];

            for (int i = 0; i < incomingMatrices.Length; i++)
                incomingMatrices[i] = new List<Matrix4x4>();

            GPUReverts = new List<Matrix4x4>();
            existingIndices = new List<int>();
            newMatrices = new List<Matrix4x4>();

            UpdatePlayerZone();
        }

        private void UpdatePlayerZone()
        {
            if (GPULayers == null || GPULayers.Count == 0) return;

            if (lastPosition != transform.position)
            {
                float lastPosDistance = (lastPosition - transform.position).sqrMagnitude;

                if (lastPosDistance > deadZoneMeters * deadZoneMeters)
                {
                    for (int i = 0; i < GPULayersList.Length; i++)
                    {
                        if (statesGPU[i])
                        {
                            if (GPULayersDistances[i] < GPULayersList[i].biggestFaceLength)
                                GPULayersDistances[i] = GPULayersList[i].biggestFaceLength;

                            int neighborPatchesCount = Mathf.Clamp(Mathf.CeilToInt(GPULayersDistances[i] / GPULayersList[i].patchScale) + 1, 1, (int)(GPULayersDistances[i] / GPULayersList[i].patchScale) + 1);

                            if (checkDistance3D)
                            {
                                targetPos3D = transform.position;
                                Matrix4x4[] matrices = GPULayersList[i].GetInstanceMatrices3D(targetPos3D, GPULayersDistances[i], neighborPatchesCount).ToArray();
                                if (matrices == null || matrices.Length == 0) continue;
                                GPURevert(matrices, i);
                            }
                            else
                            {
                                targetPos2D = new Vector2(transform.position.x, transform.position.z);
                                Matrix4x4[] matrices = GPULayersList[i].GetInstanceMatrices2D(targetPos2D, GPULayersDistances[i], neighborPatchesCount).ToArray();
                                if (matrices == null || matrices.Length == 0) continue;
                                GPURevert(matrices, i);
                            }
                        }
                    }

                    lastPosition = transform.position;
                }  
            }
        }

        private void GPURevert(Matrix4x4[] matrices, int i)
        {
            incomingMatrices[i].AddRange(matrices);

            if (incomingMatrices != null && incomingMatrices[i].Count > 0)
            {
                Matrix4x4[] incomingMatricesArray = incomingMatrices[i].ToArray();

                // Sort matrices from closest to furthest from target position (targetPos2D) if needed
                if (incomingMatricesArray.Length > maximumObjects)
                {
                    if (!isInitialized)
                        Debug.Log("Decrease \"Distance Radius\" on layer " + GPULayersList[i].gameObject.name + " for the best performance!");

                    if (checkDistance3D)
                        Array.Sort(incomingMatricesArray, SortMatricesFromTarget3D);
                    else
                        Array.Sort(incomingMatricesArray, SortMatricesFromTarget2D);
                }

                int maxAllowed = Mathf.Clamp(incomingMatricesArray.Length, 1, maximumObjects);
                int zoneCounter = 0;
                GPUReverts.Clear();
                existingIndices.Clear();
                newMatrices.Clear();

                for (int j = 0; j < incomingMatricesArray.Length; j++)
                {
                    Matrix4x4 matrix = incomingMatricesArray[j];
                    Matrix4x4 updatedMatrix = matrix;

                    for (int k = 0; k < maximumObjects; k++)
                    {
                        Matrix4x4 transformMatrix = instanceStates[i, k].initialMatrix;

                        if (matrix == transformMatrix)
                        {
                            updatedMatrix = instanceStates[i, k].GetCurrentMatrix();
                            continue;
                        }
                    }
                    
                    Vector2 instancePos2D = TMatrix.ExtractTranslationFromMatrix2D(ref updatedMatrix);
                    float distance = 0;

                    if (checkDistance3D)
                    {
                        Vector3 instancePos3D = TMatrix.ExtractTranslationFromMatrix(ref updatedMatrix);
                        distance = (instancePos3D - targetPos3D).sqrMagnitude;
                    }  
                    else
                        distance = (instancePos2D - targetPos2D).sqrMagnitude;

                    if (distance >= GPULayersDistances[i] * GPULayersDistances[i])
                    {
                        int index, row, col;

                        if (GPULayersList[i].GetPatchesRowCol(instancePos2D, out index, out row, out col))
                        {
                            GPULayersList[i]._patches[index].matrices.Add(updatedMatrix);
                            GPUReverts.Add(matrix);
                        }
                    }
                    else
                    {
                        if (zoneCounter < maxAllowed)
                        {
                            // Check if incoming matrix is already applied on any transforms
                            bool existing = false;

                            for (int k = 0; k < maximumObjects; k++)
                            {
                                if (matrix == instanceStates[i, k].initialMatrix)
                                {
                                    existingIndices.Add(k);
                                    existing = true;
                                    continue;
                                }
                            }

                            if (!existing)
                                newMatrices.Add(matrix);

                            zoneCounter++;
                        }
                        else
                        {
                            int index, row, col;

                            if (GPULayersList[i].GetPatchesRowCol(instancePos2D, out index, out row, out col))
                            {
                                GPULayersList[i]._patches[index].matrices.Add(updatedMatrix);
                                GPUReverts.Add(matrix);
                            }
                        }
                    }
                }

                int newCount = newMatrices.Count;
                int newCounter = 0;

                for (int j = 0; j < maximumObjects; j++)
                {
                    if (j < zoneCounter)
                    {
                        int index = j;

                        //int index = zoneCounter - 1 - j;
                        //int remained = zoneCounter - 1 - index;
                        //if (remained > newCount && instanceObjects[i, index].activeSelf) continue;

                        if (!existingIndices.Contains(index) && newCounter < newCount)
                        {
                            ResetInstance(i, index);

                            Matrix4x4 matrix = newMatrices[newCounter];

                            instanceObjects[i, index].SetActive(false);
                            instanceObjects[i, index].transform.position = TMatrix.ExtractTranslationFromMatrix(ref matrix);
                            instanceObjects[i, index].transform.rotation = TMatrix.ExtractRotationFromMatrix(ref matrix);
                            instanceObjects[i, index].transform.localScale = TMatrix.ExtractScaleFromMatrix(ref matrix);
                            instanceObjects[i, index].SetActive(true);
                            instanceStates[i, index].SP = GPULayersList[i];
                            instanceStates[i, index].initialMatrix = matrix;
                            instanceStates[i, index].instanceIndex = new Vector2Int(i, index);

                            newCounter++;
                        }
                    }
                    else
                    {
                        ResetInstance(i, j);

                        if (!existingIndices.Contains(j))
                        {
                            instanceObjects[i, j].SetActive(false);
                            instanceStates[i, j].initialMatrix = Matrix4x4.identity;
                        }
                    }
                }

                for (int j = 0; j < GPUReverts.Count; j++)
                    incomingMatrices[i].Remove(GPUReverts[j]);

                isInitialized = true;
            }
        }

        private void ResetInstance (int layerIndex, int instanceIndex)
        {
            RemoveRuntimeAdditions(layerIndex, instanceIndex);
            ResetRigidbody(layerIndex, instanceIndex);
        }

        // Any runtime actions on original instances through "RuntimeActions" script should be removed here
        // This funtion compares interactive object with the original prefab and remove added components if existing
        private void RemoveRuntimeAdditions (int layerIndex, int instanceIndex)
        {
            if (instanceObjects[layerIndex, instanceIndex].GetComponent<RuntimeActions>() == null) return;

            Component[] currentComponents = instanceObjects[layerIndex, instanceIndex].GetComponentsInChildren(typeof(Component), true);
            List<Type> currentTypes = new List<Type>();

            foreach (Component c in currentComponents)
                currentTypes.Add(c.GetType());

            if (originalTypes[layerIndex].Count != currentTypes.Count)
            {
                int removed = 0;

                for (int i = 0; i < currentTypes.Count - removed; i++)
                {
                    Type originalType = originalTypes[layerIndex][i];
                    Type currentType = currentTypes[i];

                    if (currentType != originalType)
                    {
                        Destroy(instanceObjects[layerIndex, instanceIndex].GetComponent(currentType));
                        currentTypes.Remove(currentType);
                        removed++;
                    }
                }
            }
        }

        private void ResetRigidbody(int layerIndex, int instanceIndex)
        {
            if (rigidbodies[layerIndex, instanceIndex] != null)
            {
                rigidbodies[layerIndex, instanceIndex].velocity = Vector3.zero;
                rigidbodies[layerIndex, instanceIndex].angularVelocity = Vector3.zero;
            }
        }

        private int SortMatricesFromTarget3D(Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            Vector3 a = TMatrix.ExtractTranslationFromMatrix(ref matrix1);
            float da = (a - targetPos3D).sqrMagnitude;

            Vector3 b = TMatrix.ExtractTranslationFromMatrix(ref matrix2);
            float db = (b - targetPos3D).sqrMagnitude;

            if (da < db) return -1;
            else if (db < da) return 1;
            return 0;
        }

        private int SortMatricesFromTarget2D(Matrix4x4 matrix1, Matrix4x4 matrix2)
        {
            Vector2 a = TMatrix.ExtractTranslationFromMatrix2D(ref matrix1);
            float da = (a - targetPos2D).sqrMagnitude;

            Vector2 b = TMatrix.ExtractTranslationFromMatrix2D(ref matrix2);
            float db = (b - targetPos2D).sqrMagnitude;

            if (da < db) return -1;
            else if (db < da) return 1;
            return 0;
        }
    }
}

