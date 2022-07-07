#if TERRAWORLD_XPRO
using System;
using System.Drawing;
using System.Numerics;
using TerraUnity.Edittime;
using TerraUnity.Utils;
using UnityEngine;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Terrain Layers/Image->TerrainLayer")]

    public class TXTerrainLayerNode : TXTerrainLayerModules
    {

        public enum TextureFrom
        {
            TerrainLayer, Texture2D,Image
        }

        public enum TilingMode
        {
            OverAll,Tiled
        }
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXImageModules Image;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXMaskModules Mask;

        public TextureFrom textureFrom = TextureFrom.TerrainLayer;
        public TilingMode tilingMode = TilingMode.OverAll;

        public TerrainLayer terrainLayer;

        public Texture2D terrainDiffuse;
        public Texture2D terrainNormalmap;
        public Texture2D terrainMaskmap;


        public UnityEngine.Vector2 tiling = UnityEngine.Vector2.one;
        public UnityEngine.Vector2 tilingOffset = UnityEngine.Vector2.zero;
        public UnityEngine.Vector4 specular = new UnityEngine.Vector4(0, 0, 0, 1);
        public float metallic = 0f;
        public float smoothness = 0f;
        public float normalScale = 1;
        public float opacity = 1;


        protected override void Init()
        {
            base.Init();
            SetName("Image->TerrainLayer");
        }

        protected override void ModuleAction(TMap CurrentMap)
        {
            switch (textureFrom)
            {
                case TextureFrom.TerrainLayer:
                    {
                        TMask filteredMask = null;
                        if (Mask == null)
                        {
                            filteredMask = new TMask(32, 32, true);
                        }
                        else
                            filteredMask = TMask.MergeMasks(Mask.GetMasks(CurrentMap._refTerrain));

                        _outputDetailTexture = new TDetailTexture(filteredMask, terrainLayer);
                        _outputDetailTexture.Tiling = TUtils.CastToNumerics(tiling);
                        _outputDetailTexture.TilingOffset = TUtils.CastToNumerics(tilingOffset);
                        _outputDetailTexture.Specular = TUtils.CastToNumerics(specular);
                    }
                    break;
                case TextureFrom.Texture2D:
                    {
                        TMask filteredMask = null;
                        if (Mask == null)
                        {
                            filteredMask = new TMask(32, 32, true);
                        }
                        else
                            filteredMask = TMask.MergeMasks(Mask.GetMasks(CurrentMap._refTerrain));

                        _outputDetailTexture = new TDetailTexture(filteredMask, terrainDiffuse, terrainNormalmap, terrainMaskmap);
                        _outputDetailTexture.Tiling = TUtils.CastToNumerics(tiling);
                        _outputDetailTexture.TilingOffset = TUtils.CastToNumerics(tilingOffset);
                        _outputDetailTexture.Specular = TUtils.CastToNumerics(specular);
                    }
                    break;
                case TextureFrom.Image:
                    {
                        TImage preNodeImage = null;
                         preNodeImage = Image.GetImage(CurrentMap._refTerrain);

                        TMask filteredMask = null;
                        if (Mask == null)
                        {
                            filteredMask = new TMask(32, 32, true);
                        }
                        else
                            filteredMask = TMask.MergeMasks(Mask.GetMasks(CurrentMap._refTerrain));

                        _outputDetailTexture = new TDetailTexture(filteredMask, preNodeImage, null, null);

                        switch (tilingMode)
                        {
                            case TilingMode.OverAll:
                                _outputDetailTexture.Tiling = new System.Numerics.Vector2(CurrentMap._refTerrain.Area._areaSizeLat * 1000, CurrentMap._refTerrain.Area._areaSizeLon * 1000);
                                break;
                            case TilingMode.Tiled:
                                _outputDetailTexture.Tiling = TUtils.CastToNumerics(tiling);
                                break;
                            default:
                                break;
                        }

                        _outputDetailTexture.TilingOffset = TUtils.CastToNumerics(tilingOffset);
                        _outputDetailTexture.Specular = TUtils.CastToNumerics(specular);
                    }
                    break;
                default:
                    break;
            }

            _outputDetailTexture.Metallic = metallic;
            _outputDetailTexture.Smoothness = smoothness;
            _outputDetailTexture.NormalScale = normalScale;
            _outputDetailTexture.Opacity = opacity;

        }

        public override void CheckEssentioalInputs()
        {
            switch (textureFrom)
            {
                case TextureFrom.TerrainLayer:
                    if (terrainLayer == null) throw new Exception("Terrain layer" + " is missed for " + name + " Node.");
                    break;
                case TextureFrom.Texture2D:
                    if (terrainDiffuse == null) throw new Exception("Diffuse " + " is missed for " + name + " Node.");
                    break;
                case TextureFrom.Image:
                         Image = GetInputValue<TXImageModules>("Image");
                        if (Image == null) throw new Exception("Image input" + " is missed for " + name + " Node.");
                    break;
                default:
                    break;
            }

            Mask = GetInputValue<TXMaskModules>("Mask");

        }


    }
}
#endif