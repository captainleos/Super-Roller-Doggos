using System;
using UnityEngine;
using TerraUnity.Runtime;

namespace Mewlist.MassiveGrass
{
    public class LayerAttribute : PropertyAttribute { }

    public enum BuilderType
    {
        Quad = 0,
        FromMesh = 5,
    }

    public enum NormalType
    {
        KeepMesh = 0,
        Up = 1,
        Shading = 2,
    }

    public enum VertexDataType
    {
        VertexColorR,
        VertexColorG,
        VertexColorB,
        VertexColorA,
        UV1Z,
        UV1W,
    }

    public enum OutDataType
    {
        Density,
        Range,
        Random,
        VertexPosX,
        VertexPosY,
        VertexPosZ,
        PivotX,
        PivotY,
        PivotZ,
        CustomData1,
        CustomData2,
    }

    [Serializable]
    public struct CustomVertexData
    {
        public OutDataType OutDataType;
    }

    [CreateAssetMenu(fileName = "MassiveGrass", menuName = "MassiveGrass", order = 1)]
    public class MassiveGrassProfile : ScriptableObject
    {
        //[SerializeField]        public TerrainLayer[] TerrainLayers = new TerrainLayer[] { };
        [SerializeField] public Vector2 Scale;
        [SerializeField, Range(50f, 2000f)] public float Radius = 1000f;
        [SerializeField, Range(5f, 200f)] public float GridSize = 50f;
        [SerializeField, Range(0f, 1f)] public float Slant;
        [SerializeField, Range(-50f, 50f)] public float GroundOffset = 0f;
        [SerializeField, Range(10f, 5000f)] public int AmountPerBlock = 10000;
        [SerializeField] public Material Material;
        [SerializeField, Layer] public int Layer = 0;
        [SerializeField, Range(0.001f, 0.999f)] public float AlphaMapThreshold = 0.5f;
        [SerializeField, Range(0.0f, 1.0f)] public float DensityFactor = 0.5f;
        [SerializeField] public bool CastShadows = false;
        [SerializeField] public BuilderType BuilderType;
        [SerializeField] public Mesh Mesh;
        [SerializeField] public NormalType NormalType = NormalType.Up;
        [SerializeField] public CustomVertexData UV1Z;
        [SerializeField] public CustomVertexData UV1W;
        [SerializeField] public CustomVertexData VertexColorR;
        [SerializeField] public CustomVertexData VertexColorG;
        [SerializeField] public CustomVertexData VertexColorB;
        [SerializeField] public CustomVertexData VertexColorA;
        [SerializeField] public int Seed;
        [SerializeField] public Vector2 HeightRange;

        // TW Tweaks
        //[SerializeField] public Terrain terrain;

        // Ver. 2.13.2 and before
        [SerializeField] public Texture2D mask;

        // Ver. 2.2 and after
        [SerializeField, HideInInspector] public TScatterLayer.MaskData[] maskData;

        [SerializeField] public bool layerBasedPlacement = false;
        [SerializeField] public bool bypassWater = true;
        [SerializeField] public bool underWater = false;
        [SerializeField] public bool onWater = false;
        [SerializeField] public int unityLayerMask;
        [SerializeField] public float[] exclusionOpacities;
        [SerializeField] public int undoMode = 0;
        // TW Tweaks

        public IMeshBuilder CreateBuilder()
        {
            // TW Tweaks
            if (maskData == null && mask != null)
            {
                int maskResolution = mask.width;
                maskData = new TScatterLayer.MaskData[maskResolution];

                for (int i = 0; i < maskResolution; i++)
                {
                    maskData[i].row = new float[maskResolution];

                    for (int j = 0; j < maskResolution; j++)
                        maskData[i].row[j] = mask.GetPixel(i, j).a;
                }
            }

            if (maskData == null)
                throw new Exception("Mask is missing for " + this.name + " profile! Aborting placement.");

            //if (maskData == null)
            //{
            //    int maskResolution = 2048;
            //    maskData = new TScatterLayer.MaskData[maskResolution];
            //
            //    for (int i = 0; i < maskResolution; i++)
            //    {
            //        maskData[i].row = new float[maskResolution];
            //
            //        for (int j = 0; j < maskResolution; j++)
            //            maskData[i].row[j] = -1 * float.Epsilon;
            //    }
            //}

            Terrain terrain = null;
            TTerraWorldTerrainManager[] TTM = FindObjectsOfType<TTerraWorldTerrainManager>();
            if (TTM != null && TTM.Length > 0 && TTM[0] != null) terrain = TTM[0].gameObject.GetComponent<Terrain>();

            if (terrain != null)
            {
                int terrainLayersCount = terrain.terrainData.terrainLayers.Length;
                if (exclusionOpacities == null || exclusionOpacities.Length == 0 || exclusionOpacities.Length != terrainLayersCount)
                {
                    exclusionOpacities = new float[terrainLayersCount];
                    for (int i = 0; i < exclusionOpacities.Length; i++) exclusionOpacities[i] = 1f;
                }
            }
            // TW Tweaks

            switch (BuilderType)
            {
                case BuilderType.Quad:
                    return new QuadBuilder();
                case BuilderType.FromMesh:
                    return new FromMeshBuilder();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float GetCustomVertexData(VertexDataType vertexDataType, VertAttribute attr)
        {
            switch (vertexDataType)
            {
                case VertexDataType.VertexColorR:
                    return GetData(VertexColorR.OutDataType, attr);
                case VertexDataType.VertexColorG:
                    return GetData(VertexColorG.OutDataType, attr);
                case VertexDataType.VertexColorB:
                    return GetData(VertexColorB.OutDataType, attr);
                case VertexDataType.VertexColorA:
                    return GetData(VertexColorA.OutDataType, attr);
                case VertexDataType.UV1Z:
                    return GetData(UV1Z.OutDataType, attr);
                case VertexDataType.UV1W:
                    return GetData(UV1W.OutDataType, attr);
                default:
                    throw new ArgumentOutOfRangeException(nameof(vertexDataType), vertexDataType, null);
            }
        }

        private float GetData(OutDataType outDataType, VertAttribute attr)
        {
            switch (outDataType)
            {
                case OutDataType.Density:
                    return attr.Density;
                case OutDataType.Range:
                    return Radius;
                case OutDataType.Random:
                    return attr.Rand;
                case OutDataType.VertexPosX:
                    return attr.VertPos.x;
                case OutDataType.VertexPosY:
                    return attr.VertPos.y;
                case OutDataType.VertexPosZ:
                    return attr.VertPos.z;
                case OutDataType.PivotX:
                    return attr.Pivot.x;
                case OutDataType.PivotY:
                    return attr.Pivot.y;
                case OutDataType.PivotZ:
                    return attr.Pivot.z;
                case OutDataType.CustomData1:
                case OutDataType.CustomData2:
                default:
                    throw new ArgumentOutOfRangeException(nameof(outDataType), outDataType, null);
            }
        }

        public struct VertAttribute
        {
            public readonly float Density;
            public readonly float Rand;
            public readonly Vector3 Pivot;
            public readonly Vector3 VertPos;

            public VertAttribute(
                float density,
                float rand,
                Vector3 pivot,
                Vector3 vertPos)
            {
                Density = density;
                Rand = rand;
                Pivot = pivot;
                VertPos = vertPos;
            }
        }
    }
}

