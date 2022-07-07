using UnityEngine;
using System;
using System.Collections.Generic;

namespace TerraUnity.Runtime
{
    [Serializable]
    public class TScatterLayer : TLayerScript
    {
        [Serializable]
        public struct Patch
        {
            public Vector3 position;
            public float scale;
            public List<Matrix4x4> matrices;
        }

        [Serializable]
        public struct MaskData
        {
            [SerializeField] public float[] row;
        }
        [SerializeField] public MaskData[] maskData;


        public GameObject prefab;
        [HideInInspector, Range(1f, 100f)] public float density = 100f;
        [HideInInspector] public Vector3 scale;
        [HideInInspector, Range(0.1f, 20f)] public float minScale;
        [HideInInspector, Range(0.1f, 20f)] public float maxScale;
        [HideInInspector, Range(0f, 100f)] public float positionVariation;
        [HideInInspector] public float averageDistance;
        [HideInInspector] public bool lock90DegreeRotation;
        [HideInInspector] public bool lockYRotation;
        [HideInInspector] public bool getSurfaceAngle;
        [HideInInspector] public int seedNo;
        [HideInInspector] public int priority;
        [HideInInspector] public Vector3 positionOffset;
        [HideInInspector] public Vector3 rotationOffset;
        [HideInInspector, Range(0f, 359f)] public float minRotationRange;
        [HideInInspector, Range(0f, 359f)] public float maxRotationRange;
        [HideInInspector, Range(0f, 90f)] public float minAllowedAngle;
        [HideInInspector, Range(0f, 90f)] public float maxAllowedAngle;
        [HideInInspector, Range(-100000f, 100000f)] public float minAllowedHeight;
        [HideInInspector, Range(-100000f, 100000f)] public float maxAllowedHeight;
        [HideInInspector] public LayerMask layerMask;
        [HideInInspector] public bool bypassLake;
        [HideInInspector] public bool underLake;
        [HideInInspector] public bool underLakeMask;
        [HideInInspector] public bool onLake;

        //public class LayerAttribute : PropertyAttribute { }
        //[Layer]
        [HideInInspector] public int unityLayerMask;
        
        [HideInInspector] public bool isInitialized = false;
        [HideInInspector] public bool occlusionCulling = true;
        [HideInInspector] public bool checkBoundingBox;
        [HideInInspector] public float biggestFaceLength = float.MinValue;
        [HideInInspector, Range(0f, 1f)] public float[] exclusionOpacities;
        [HideInInspector] public LayerType layerType;
        [HideInInspector] public List<string> prefabNames;
        [HideInInspector] public int undoMode = 0;

        public Texture2D filter;

        protected void ConvertMaskFromTexture2D ()
        {
            if (maskData == null && filter != null)
            {
                int maskResolution = filter.width;
                maskData = new MaskData[maskResolution];

                for (int i = 0; i < maskResolution; i++)
                {
                    maskData[i].row = new float[maskResolution];

                    for (int j = 0; j < maskResolution; j++)
                        maskData[i].row[j] = filter.GetPixel(i, j).a;
                }
            }
        }

        protected bool CheckMask()
        {
            try
            {
                if (maskData == null)
                    throw new Exception("Mask is missing for " + gameObject.name + " layer! Aborting placement.");
                
                return true;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.Log(e);
#endif
                return false;
            }
        }
    }
}

