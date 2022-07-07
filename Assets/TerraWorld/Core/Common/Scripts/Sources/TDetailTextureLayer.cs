#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Drawing;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
    public enum TTextureTilingMode
    {
        Full
    }

    public enum DetailTextureMergingMode
    {
        FirstTakeOver,
        LastTakeOver
    }

    public class TDetailTextureCollection
    {
        public List<TDetailTexture> _textures;
        private TTerrain refTerrain;

        public TDetailTextureCollection(TTerrain terrain)
        {
            _textures = new List<TDetailTexture>();
            refTerrain = terrain;
        }

        public void Add(TDetailTexture detailTexture)
        {
            _textures.Add(detailTexture);
        }

        public float[,,] GetAplphaMaps(int resolution)
        {
            TDebug.TraceMessage("GetAplphaMaps");
            resolution = TUtils.nearestPowerOfTwo(resolution);
            resolution = (resolution < 32 ? 32 : resolution);
            resolution = (resolution > 4096 ? 4096 : resolution);
            float[,,] result = new float[resolution, resolution, _textures.Count];
            float[,] resultSum = new float[resolution, resolution];
            //float[,] sumOfOpacities = new float[resolution, resolution];
            float sumOfOpacities = 0;

            for (int i = 0; i < resolution; i++)
                for (int j = 0; j < resolution; j++)
                {
                    for (int k = 0; k < _textures.Count; k++)
                    {
                        TMask mask = _textures[k].Mask;
                        double normalX = (float)i / (resolution);
                        double normalY = (float)j / (resolution);

                        if (mask == null || mask.CheckNormal(normalX, normalY))
                        {
                            float opacity = _textures[k].Opacity;
                            sumOfOpacities = 0;

                            for (int u = 0; u < k; u++)
                            {
                                result[j, i, u] = (1 - opacity) * result[j, i, u];
                                resultSum[j, i] += result[j, i, u];
                                sumOfOpacities += result[j, i, u];
                            }

                            result[j, i, k] = (1 - sumOfOpacities);

                        }
                    }
                }

            float[,] temp = new float[resolution, resolution];

//            int iteration = refTerrain.TerraWorld.SplatmapSmoothness;
            int iteration = TTerraWorld.WorldGraph.renderingGraph.GetEntryNode().renderingParams.splatmapSmoothness;
            int heightmapResolution = refTerrain.TerraWorld._terrains[0].Heightmap.heightsData.GetLength(0) - 1;

            if (resolution > heightmapResolution && iteration == 0)
                iteration = 1;

            //   float[,] temp = THeightmapProcessors.SmoothHeightmap(null, resultSum, iteration, 0, THeightmapProcessors.Neighbourhood.Moore);
            //
            //
            //   for (int k = 0; k < _textures.Count; k++)
            //       for (int i = 0; i < resolution; i++)
            //           for (int j = 0; j < resolution; j++)
            //               if (resultSum[j, i] != 0 )
            //                   result[j, i, k] = (temp[i, j] / resultSum[j, i]) * result[j, i, k];

            for (int k = 0; k < _textures.Count; k++)
            {
                for (int i = 0; i < resolution; i++)
                    for (int j = 0; j < resolution; j++)
                        temp[i, j] = result[j, i, k];

                temp = THeightmapProcessors.SmoothHeightmap(temp, iteration, 0, THeightmapProcessors.Neighbourhood.Moore);

                for (int i = 0; i < resolution; i++)
                    for (int j = 0; j < resolution; j++)
                        result[j, i, k] = temp[i, j];
            }

            return result;
        }

        public static float[,,] GetFilledAplphaMap(int resolution, int ChanelNum)
        {
            TDebug.TraceMessage("Generating Background splatmap");
            resolution = TUtils.nearestPowerOfTwo(resolution);
            resolution = (resolution < 32 ? 32 : resolution);
            resolution = (resolution > 4096 ? 4096 : resolution);
            float[,,] result = new float[resolution, resolution, ChanelNum + 1];

            for (int i = 0; i < resolution; i++)
                for (int j = 0; j < resolution; j++)
                    result[j, i, ChanelNum] = 1;

            return result;
        }

        public static TDetailTexture Merge(List<TDetailTexture> DetailTextures, DetailTextureMergingMode mergeMode)
        {
            // Finding biggest resolution
            int maxMaskResolution = 0;
            int maxImageSize = 0;

            for (int i = 0; i < DetailTextures.Count; i++)
            {
                TMask mask = DetailTextures[i].Mask;
                if (maxMaskResolution < DetailTextures[i].Mask.Height) maxMaskResolution = DetailTextures[i].Mask.Height;
                if (maxImageSize < DetailTextures[i].DiffuseMap.Image.Size.Width) maxImageSize = DetailTextures[i].DiffuseMap.Image.Size.Width;
            }

            TMask ResultMask = new TMask(maxMaskResolution, maxMaskResolution);
            Bitmap resultDiffuseMap = new Bitmap(maxImageSize, maxImageSize);

            for (int i = 0; i < DetailTextures.Count; i++)
            {
                TMask mask = DetailTextures[i].Mask;
                Bitmap bitmap = DetailTextures[i].DiffuseMap.Image;

                for (int u = 0; u < bitmap.Width; u++)
                    for (int k = 0; k < bitmap.Height; k++)
                    {
                        double normalX = u * 1.0d / bitmap.Width;
                        double normalY = k * 1.0d / bitmap.Height;

                        if (mask.CheckNormal(normalX, normalY))
                        {
                            ResultMask.SetNormal(normalX, normalY);
                            System.Drawing.Color color = bitmap.GetPixel((int)(normalX * bitmap.Width), (int)(normalY * bitmap.Height));
                            resultDiffuseMap.SetPixel((int)(normalX * resultDiffuseMap.Width), (int)(normalY * resultDiffuseMap.Height), color);
                        }
                    }
            }

            return new TDetailTexture(ResultMask, new TImage(resultDiffuseMap), null, null);
        }

        public TDetailTexture GetColorMap()
        {
            TDebug.TraceMessage("GetColorMap");

            if (_textures.Count == 0) return null;
            if (_textures.Count == 1) return _textures[0];

            // Find Bigest Mask & Diffuse Image
            int maskSize = 0;
            int imageSize = 0;

            for (int i = 0; i < _textures.Count; i++)
            {
                TDetailTexture current = _textures[i];
                if (current.Mask.Height > maskSize) maskSize = current.Mask.Height;
                if (current.Mask.Width > maskSize) maskSize = current.Mask.Width;
                if (current.DiffuseMap.Image.Width > imageSize) imageSize = current.DiffuseMap.Image.Width;
                if (current.DiffuseMap.Image.Height > imageSize) imageSize = current.DiffuseMap.Image.Height;
            }

            Bitmap bitmap = new Bitmap(imageSize, imageSize);
            TMask mask = new TMask(imageSize, imageSize);

            for (int k = 0; k < _textures.Count; k++)
            {
                Bitmap orgBitmap = _textures[k].DiffuseMap.Image;
                TMask orgMask = _textures[k].Mask;

                for (int i = 0; i < bitmap.Width; i++)
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        double normalI = i * 1.0d / bitmap.Width;
                        double normalJ = j * 1.0d / bitmap.Height;
                        int imageI = (int)(normalI * orgBitmap.Width);
                        int imageJ = (int)(normalJ * orgBitmap.Height);

                        if (orgMask.CheckNormal(normalI, normalJ))
                        {
                            mask.SetNormal(normalI, normalJ);
                            System.Drawing.Color color = orgBitmap.GetPixel(imageI, orgBitmap.Height - imageJ - 1);
                            bitmap.SetPixel(i, bitmap.Height - j - 1, color);
                        }
                    }
            }

            TDetailTexture result = new TDetailTexture(bitmap, mask);
            return result;
        }
    }

    public enum TDetailTextureMode
    {
        None,
        TerrainLayer,
        Deffuse
    }

    public class TDetailTexture : TImageLayer
    {
        private TMask _mask = null;
        private TImage _diffusemap = null;
        private TImage _normalmap = null;
        private TImage _maskmap = null;
        private TDetailTextureMode mode = TDetailTextureMode.None;
        private float _opacity = 1;
        private TerrainLayer terrainLayer;
        private Texture2D terrainDiffuse;
        private Texture2D terrainNormalmap;
        private Texture2D terrainMaskmap;

        private string _terrainLayerPath;
        public string TerrainLayerPath { get => _terrainLayerPath; }

        public TImage DiffuseMap
        {
            get
            {
                return _diffusemap;
            }
            set
            {
                _diffusemap = value;
            }
        }

        public TImage NormalMap
        {
            get
            {
                return _normalmap;
            }
            set
            {
                _normalmap = value;
            }
        }

        public TImage MaskMap
        {
            get
            {
                return _maskmap;
            }
            set
            {
                _maskmap = value;
            }
        }

        public TMask Mask { get => _mask; set => _mask = value; }
        public TDetailTextureMode Mode { get => mode; }
        public float Opacity { get => _opacity; set { _opacity = (value < 0) ? 0 : (value > 1) ? 1 : value; ; } }

        public TerrainLayer TerrainLayer { get { if (terrainLayer == null) GenerateTerrainLayerFromDiffusemap(); return terrainLayer; } }

        public void SetFromImage(Bitmap Scr, TMask mask = null)
        {
            _diffusemap = new TImage();
            _diffusemap.Image = Scr;
            mode = TDetailTextureMode.Deffuse;

            if (mask == null)
            {
                Mask = new TMask(_diffusemap.Image.Width, _diffusemap.Image.Height);
                Mask.fulfill();
            }
            else
                Mask = mask;
        }

        public TDetailTexture(Bitmap Scr)
        {
            layerType = LayerType.DetailTexture;
            mode = TDetailTextureMode.Deffuse;
            SetFromImage(Scr);
        }

        public TDetailTexture(Bitmap Scr, TMask mask)
        {
            layerType = LayerType.DetailTexture;
            mode = TDetailTextureMode.Deffuse;
            SetFromImage(Scr, mask);
        }

        public TDetailTexture(TMask mask, System.Drawing.Color color)
        {
            layerType = LayerType.DetailTexture;
            mode = TDetailTextureMode.Deffuse;
            SetFromColor(mask, color);
        }

        public void SetFromColor(TMask mask, System.Drawing.Color color)
        {
            _mask = mask;
            _diffusemap = new TImage(mask.GetColoredImage(color));
        }

        public TDetailTexture(TMask mask, TImage _Diffusemap, TImage _Normalmap, TImage _Maskmap)
        {
            layerType = LayerType.DetailTexture;
            _diffusemap = _Diffusemap;
            mode = TDetailTextureMode.Deffuse;

            if (_Normalmap != null)
                _normalmap = _Normalmap;

            if (_Maskmap != null)
                _maskmap = _Maskmap;

            Mask = mask;
        }

        public TDetailTexture(TMask mask, TerrainLayer terrainLayer)
        {
            layerType = LayerType.DetailTexture;
            mode = TDetailTextureMode.TerrainLayer;
            Mask = mask;
            this.terrainLayer = terrainLayer;
        }

        public TDetailTexture(TMask mask, Texture2D diffusemap, Texture2D normalmap, Texture2D maskmap)
        {
            layerType = LayerType.DetailTexture;
            mode = TDetailTextureMode.Deffuse;
            Mask = mask;

            this.terrainDiffuse = diffusemap;
            this.terrainNormalmap = normalmap;
            this.terrainMaskmap = maskmap;
        }

        private void GenerateTerrainLayerFromDiffusemap()
        {
            if (DiffuseMap == null && terrainDiffuse == null) return;
            if (DiffuseMap != null && terrainDiffuse == null) terrainDiffuse = DiffuseMap.GetObject() as Texture2D;
            if (NormalMap != null && terrainNormalmap == null) terrainNormalmap = NormalMap.GetObject() as Texture2D;
            if (MaskMap != null && terrainMaskmap == null) terrainMaskmap = MaskMap.GetObject() as Texture2D;
            terrainLayer = new TerrainLayer();
            terrainLayer.diffuseTexture = terrainDiffuse;
            if (terrainNormalmap != null) terrainLayer.normalMapTexture = terrainNormalmap;
            if (terrainMaskmap != null) terrainLayer.maskMapTexture = terrainMaskmap;
            terrainLayer.tileSize = TUtils.CastToUnity(Tiling);
            terrainLayer.tileOffset = TUtils.CastToUnity(TilingOffset);
            terrainLayer.specular = TUtils.CastToUnityColor(Specular);
            terrainLayer.metallic = Metallic;
            terrainLayer.smoothness = Smoothness;
            terrainLayer.normalScale = NormalScale;
        }

        public TDetailTexture(string TerrainLayerPath, TMask mask)
        {
            layerType = LayerType.DetailTexture;
            _terrainLayerPath = TerrainLayerPath;
            this.Mask = mask;
            mode = TDetailTextureMode.TerrainLayer;
        }

        public TDetailTexture Clone()
        {
            return (TDetailTexture)MemberwiseClone();
        }
    }
}
#endif
#endif

