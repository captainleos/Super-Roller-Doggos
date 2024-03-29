﻿#if TERRAWORLD_XPRO
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Generator/GPU Instance Scatter")]
    public class TXInstanceScatterNode : TXInstanceScatterModules
    {

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXMaskModules Input;

        public string prefabName;
        public UnityEngine.GameObject prefab = null;
        public int seedNo;
        public float averageDistance = 10f;
        public bool rotation90Degrees = false;
        public bool lockYRotation = false;
        public bool getSurfaceAngle = false;
        public float minRotationRange = 0f;
        public float maxRotationRange = 359f;
        public float positionVariation = 100f;
        public System.Numerics.Vector3 scaleMultiplier = System.Numerics.Vector3.One;
        public float minScale = 0.8f;
        public float maxScale = 1.5f;
        public string unityLayerName = "Default";
        public int maskLayer = ~0;
        public string layerName;
        public float minSlope = 0;
        public float maxSlope = 90;
        public System.Numerics.Vector3 positionOffset = System.Numerics.Vector3.Zero;
        public System.Numerics.Vector3 rotationOffset = System.Numerics.Vector3.Zero;
        public int priority = 0;
        public List<ObjectBounds> bounds;
        public float minRange = 0;
        public float maxRange = 1;
        public bool receiveShadows = true;
        public bool bypassLakes = true;
        public bool underLakes = false;
        public bool underLakesMask = false;
        public bool onLakes = false;
        public TShadowCastingMode shadowCastingMode = TShadowCastingMode.On;
        public int gridSizeValue = 100;
        private List<TMask> _masks = new List<TMask>();
        public bool isWorldOffset = true; //TODO: Ali should implement this
        public bool prefabHasCollider = false;


        protected override void Init()
        {
            base.Init();
            SetName("GPU Instance Scatter");
        }



        public override TInstanceScatterLayer GetInstancesLayer(TTerrain terrain)
        {

            if (!IsActive) return null;
            if (IsDone) return _instanceScatterLayer;


            return _instanceScatterLayer;
        }


        public override void CheckEssentioalInputs()
        {
            var iv = GetInputValue<TXNode>("Input");
            if (iv == null) throw new Exception("Input" + " is missed for " + NodeName + " Node.");
            if (prefab == null) throw new Exception("Prefab parameter" + " is missed for " + NodeName + " Node.");

        }

    }


}
#endif