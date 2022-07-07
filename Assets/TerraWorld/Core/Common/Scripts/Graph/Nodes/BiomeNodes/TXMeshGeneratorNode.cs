#if TERRAWORLD_XPRO
using System;
using UnityEngine;
using TerraUnity;
using System.Collections.Generic;
using UnityEditor;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Generator/Mesh Generator")]
    public class TXMeshGeneratorNode : TXMeshModules
    {

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXMaskModules Input;




        public Material meshMaterial;
        public int densityResolutionPerKilometer = 30;
        public int vertexCount = 10000;
        public int density = 100;
        public float edgeCurve = -1;
        public int gridCount = 16;
        public float lodCulling = 20f;
        public System.Numerics.Vector3 scale = System.Numerics.Vector3.One;
        public string unityLayerName = "Default";
        public bool hasCollider = true;
        public bool hasPhysics = false;
        public bool seperatedObjects = false;
        public string layerName;
        public System.Numerics.Vector3 positionOffset = System.Numerics.Vector3.Zero;
        public int priority = 0;
        public List<TMask> _masks;
        public bool SeperatedObject = false;


        protected override void Init()
        {
            base.Init();
            SetName("Mesh Generator");
        }

        protected override void ModuleAction(TMap CurrentMap)
        {
            _gridLayer = new TGridLayer();
            _gridLayer.HasCollider = hasCollider;
            _gridLayer.HasPhysics = hasPhysics;
            _gridLayer.KM2Resulotion = densityResolutionPerKilometer;
            _gridLayer.LayerName = NodeName;
            _gridLayer.xMaterial = meshMaterial;
            _gridLayer.MinElevation = -100000;
            _gridLayer.MaxElevation = 100000;
            _gridLayer.MinSlope = 0;
            _gridLayer.MaxSlope = 90;
            _gridLayer.Priority = priority;
            _gridLayer.UnityLayerName = unityLayerName;
            _gridLayer.Offset = positionOffset;
            _gridLayer.Scale = scale;
            _gridLayer.layerName = layerName;
            _gridLayer.EdgeCurve = edgeCurve;
            _gridLayer.density = density;
            _gridLayer.LODCulling = lodCulling;

            _masks = Input.GetMasks(CurrentMap._refTerrain);
            //if (_masks == null)
            //    TLandcoverProccessor.GenerateGrid(currentMap._refTerrain, null, ref _gridLayer, areaBounds, gridCount, SeperatedObject);
            //else if (_masks.Count > 0)
            //    for (int i = 0; i < _masks.Count; i++)
            //        TLandcoverProccessor.GenerateGrid(currentMap._refTerrain, _masks[i], ref _gridLayer, areaBounds, gridCount, SeperatedObject);

            if (_masks == null)
                TLandcoverProccessor.GenerateGrid(CurrentMap._refTerrain, null, ref _gridLayer, gridCount, SeperatedObject);
            else if (_masks.Count > 0)
                for (int i = 0; i < _masks.Count; i++)
                    TLandcoverProccessor.GenerateGrid(CurrentMap._refTerrain, _masks[i], ref _gridLayer, gridCount, SeperatedObject);
        }


        public override void CheckEssentioalInputs()
        {
            Input = GetInputValue<TXMaskModules>("Input");
            if (Input == null) throw new Exception("Input" + " is missed for " + NodeName + " Node.");
        }

    }


}
#endif