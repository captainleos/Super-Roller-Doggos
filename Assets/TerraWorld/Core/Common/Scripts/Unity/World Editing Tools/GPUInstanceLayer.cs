using UnityEngine;
using System;

namespace TerraUnity.Runtime
{
    [Serializable]
    public class GPUInstanceLayer : WorldToolsParams
    {
        [SerializeField, HideInInspector] public TScatterParams parameters;

        public void OnEnable()
        {
            SetActiveState();
        }

        void OnDisable()
        {
            SetActiveState();
        }

        private void SetActiveState ()
        {
            if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
            if (parameters != null)
            {
                parameters.enabled = gameObject.activeSelf;
                parameters.gameObject.SetActive(gameObject.activeSelf);
            }
        }

        [SerializeField]
        public GameObject Prefab
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.prefab;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.prefab = value;
            }
        }

        [SerializeField]
        public float AverageDistance
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.averageDistance;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.averageDistance = value;
            }
        }

        //[SerializeField]
        //public TScatterLayer.MaskData[] maskData
        //{
        //    get
        //    {
        //        if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
        //        return parameters.maskData;
        //    }
        //    set
        //    {
        //        if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
        //        parameters.maskData = value;
        //    }
        //}

        //[SerializeField]
        //public Texture2D Filter
        //{
        //    get
        //    {
        //        if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
        //        return parameters.filter;
        //    }
        //    set
        //    {
        //        if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
        //        parameters.filter = value;
        //    }
        //}

        [SerializeField]
        public bool LODGroupNotDetected
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.LODGroupNotDetected;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.LODGroupNotDetected = value;
            }
        }

        [SerializeField]
        public float LODMultiplier
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.LODMultiplier;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.LODMultiplier = value;
            }
        }

        [SerializeField]
        public float maxDistance
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.maxDistance;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.maxDistance = value;
            }
        }

        [SerializeField]
        public bool receiveShadows
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.receiveShadows;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.receiveShadows = value;
            }
        }

        //[SerializeField]
        //public bool hasCollision
        //{
        //    get
        //    {
        //        if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
        //        return parameters.hasCollision;
        //    }
        //    set
        //    {
        //        if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
        //        parameters.hasCollision = value;
        //    }
        //}

        [SerializeField]
        public float frustumMultiplier
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.frustumMultiplier;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.frustumMultiplier = value;
            }
        }

        [SerializeField]
        public UnityEngine.Rendering.ShadowCastingMode shadowCastMode
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.shadowCastMode;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.shadowCastMode = value;
            }
        }

        [SerializeField]
        public bool bypassLake
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.bypassLake;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.bypassLake = value;
            }
        }

        [SerializeField]
        public bool underLake
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.underLake;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.underLake = value;
            }
        }

        [SerializeField]
        public bool onLake
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.onLake;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.onLake = value;
            }
        }

        [SerializeField]
        public int seedNo
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.seedNo;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.seedNo = value;
            }
        }

        [SerializeField]
        public int gridResolution
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.gridResolution;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.gridResolution = value;
            }
        }

        [SerializeField]
        public bool getSurfaceAngle
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.getSurfaceAngle;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.getSurfaceAngle = value;
            }
        }

        [SerializeField]
        public bool lock90DegreeRotation
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.lock90DegreeRotation;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.lock90DegreeRotation = value;
            }
        }

        [SerializeField]
        public bool lockYRotation
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.lockYRotation;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.lockYRotation = value;
            }
        }

        [SerializeField]
        public float minRotationRange
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.minRotationRange;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.minRotationRange = value;
            }
        }

        [SerializeField]
        public float maxRotationRange
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.maxRotationRange;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.maxRotationRange = value;
            }
        }

        [SerializeField]
        public float positionVariation
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.positionVariation;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.positionVariation = value;
            }
        }

        [SerializeField]
        public Vector3 scale
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.scale;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.scale = value;
            }
        }

        [SerializeField]
        public float minScale
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.minScale;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.minScale = value;
            }
        }

        [SerializeField]
        public float maxScale
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.maxScale;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.maxScale = value;
            }
        }

        [SerializeField]
        public float minAllowedAngle
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.minAllowedAngle;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.minAllowedAngle = value;
            }
        }

        [SerializeField]
        public float maxAllowedAngle
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.maxAllowedAngle;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.maxAllowedAngle = value;
            }
        }

        [SerializeField]
        public Vector3 positionOffset
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.positionOffset;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.positionOffset = value;
            }
        }

        [SerializeField]
        public Vector3 rotationOffset
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.rotationOffset;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.rotationOffset = value;
            }
        }

        [SerializeField]
        public string unityLayerName
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.unityLayerName;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.unityLayerName = value;
            }
        }

        [SerializeField]
        public int unityLayerMask
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.unityLayerMask;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.unityLayerMask = value;
            }
        }

        [SerializeField]
        public bool checkBoundingBox
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.checkBoundingBox;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.checkBoundingBox = value;
            }
        }

        [SerializeField]
        public bool occlusionCulling
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.occlusionCulling;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.occlusionCulling = value;
            }
        }

        [SerializeField]
        public float[] exclusionOpacities
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.exclusionOpacities;
            }
            set
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                parameters.exclusionOpacities = value;
            }
        }

        [SerializeField]
        public Terrain terrain
        {
            get
            {
                if (parameters == null) parameters = transform.GetChild(0).GetComponent<TScatterParams>();
                return parameters.terrain;
            }
        }
    }
}

