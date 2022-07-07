#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;

namespace TerraUnity.Edittime
{
    public enum ColormapProcessors
    {
        Satellite_Image_Source,
        //ShadowRemover,
        Terrain_Global_Layer,
        Terrain_Detail_Layer,
    }

    public enum ColormapMasks
    {
        Color_Filter,
        Area_Mixer
    }

    // Entry
    //---------------------------------------------------------------------------------------------------------------------------------------------------

    public abstract class TImageModules : TNode
    {
        [XmlIgnore] public TDetailTexture _detailTexture;

        public TImageModules() : base()
        {
        }
    }

    // Master Node Colormap
    [XmlType("ColormapMaster")]
    public class ColormapMaster : TImageModules
    {
        public ColormapMaster() : base()
        {
         //   type = typeof(ColormapMaster).FullName;
            Data.name = "Global Colormap";
            isRemovable = false;
            isSource = false;
            Data.nodePosition = NodePosition._1;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() {};
            outputConnectionType = ConnectionDataType.ColormapMaster;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }
    }

    // Master Node Terrain Layer 1
    [XmlType("TerrainLayerMaster1")]
    public class TerrainLayerMaster1 : TImageModules
    {
        public TerrainLayerMaster1() : base()
        {
          //  type = typeof(TerrainLayerMaster1).FullName;
            Data.name = "Terrain Layer 1";
            isRemovable = false;
            isSource = false;
            Data.nodePosition = NodePosition._2;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.DetailTexture, true, "Terrain Layer 1") };
            outputConnectionType = ConnectionDataType.DetailTextureMaster;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }
    }

    // Master Node Terrain Layer 2
    [XmlType("TerrainLayerMaster2")]
    public class TerrainLayerMaster2 : TImageModules
    {
        public TerrainLayerMaster2() : base()
        {
           // type = typeof(TerrainLayerMaster2).FullName;
            Data.name = "Terrain Layer 2";
            isRemovable = false;
            isSource = false;
            Data.nodePosition = NodePosition._3;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.DetailTexture, true, "Terrain Layer 2") };
            outputConnectionType = ConnectionDataType.DetailTextureMaster;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }
    }

    // Master Node Terrain Layer 3
    [XmlType("TerrainLayerMaster3")]
    public class TerrainLayerMaster3 : TImageModules
    {
        public TerrainLayerMaster3() : base()
        {
          //  type = typeof(TerrainLayerMaster3).FullName;
            Data.name = "Terrain Layer 3";
            isRemovable = false;
            isSource = false;
            Data.nodePosition = NodePosition._4;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.DetailTexture, true, "Terrain Layer 3") };
            outputConnectionType = ConnectionDataType.DetailTextureMaster;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }
    }

    // Master Node Terrain Layer 4
    [XmlType("TerrainLayerMaster4")]
    public class TerrainLayerMaster4 : TImageModules
    {
        public TerrainLayerMaster4() : base()
        {
          //  type = typeof(TerrainLayerMaster4).FullName;
            Data.name = "Terrain Layer 4";
            isRemovable = false;
            isSource = false;
            Data.nodePosition = NodePosition._5;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.DetailTexture, true, "Terrain Layer 4") };
            outputConnectionType = ConnectionDataType.DetailTextureMaster;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }
    }

    // Raw satellite image from mapping servers
    [XmlType("SatelliteImage")]
    public class SatelliteImage : TImageModules
    {
        public static int lastNumber = 1;

        public TMapManager.mapImagerySourceEnum _source = TMapManager.mapImagerySourceEnum.ESRI;
        public int resolution = 1024;

        public SatelliteImage() : base()
        {
          //  type = typeof(SatelliteImage).FullName;
            Data.name = "Satellite Image Source " + lastNumber;
            isSource = true;
            lastNumber++;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.DetailTexture;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap CurrentMap)
        {
            if (isDone) return;
            _progress = 0;
            _detailTexture = null;

            if (isActive)
            {
                Bitmap newImage = TImageProcessors.ResetResolution(CurrentMap.Image.Image, resolution);
                _detailTexture = new TDetailTexture(newImage);
                _detailTexture.Tiling = new Vector2(CurrentMap._area._areaSizeLat * 1000 , CurrentMap._area._areaSizeLon * 1000);
                _progress = 1;
            }
            
            isDone = true;
        }
    }

    // Processors
    //---------------------------------------------------------------------------------------------------------------------------------------------------

    [XmlType("ShadowRemover")]
    public class ShadowRemover : TImageModules
    {
        public static int lastNumber = 1;

        public int _shadowColorR = 64;
        public int _shadowColorG = 64;
        public int _shadowColorB = 64;
        [XmlIgnore] private Color _shadowColor = new Color();
        public int _blockSize = 50;

        public ShadowRemover() : base()
        {
          //  type = typeof(ShadowRemover).FullName;
            Data.name = "Shadowless " + lastNumber;
            _shadowColor = Color.FromArgb(_shadowColorR, _shadowColorG, _shadowColorB);
            lastNumber++;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.DetailTexture,true, "Source Texture") };
            outputConnectionType =  ConnectionDataType.DetailTexture;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;
            TImageModules preNode = (TImageModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
            Bitmap _image = preNode._detailTexture.DiffuseMap.Image;
            _detailTexture = preNode._detailTexture.Clone();

            if (isActive)
            {
                _shadowColor = Color.FromArgb(_shadowColorR, _shadowColorG, _shadowColorB);
                _image = TImageProcessors.ShadowRemover(this, _image, _shadowColor, _blockSize);
                _detailTexture.DiffuseMap.Image = _image;
                _progress = 1;
            }

            isDone = true;
        }
    }

    [XmlType("Mask2DetailTexture")]
    public class Mask2DetailTexture : TImageModules
    {
        public static int lastNumber = 1;

        public string terrainLayerPath;
        public string diffusemapPath;
        public string normalmapPath;
        public string maskmapPath;
        public float minRange = 0;
        public float maxRange = 1;

        public Vector2 tiling = Vector2.One;
        public Vector2 tilingOffset = Vector2.Zero;
        public Vector4 specular = new Vector4(0, 0, 0, 1);
        public float metallic = 0f;
        public float smoothness = 0f;
        public float normalScale = 1;
        public bool isColorMap = false;
        public float opacity = 1;

        public int SelectionMethodIndex = 0;

        public Mask2DetailTexture() : base()
        {
           // type = typeof(Mask2DetailTexture).FullName;
            Data.name = "Terrain Detail Layer " + lastNumber;
            isSource = true;
            lastNumber++;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node") };
            outputConnectionType = ConnectionDataType.DetailTexture;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();

            if (SelectionMethodIndex == 0)
            {
                if (!string.IsNullOrEmpty(terrainLayerPath))
                {
                    if (!string.IsNullOrEmpty(terrainLayerPath) && File.Exists(Path.GetFullPath(terrainLayerPath)))
                        result.Add(terrainLayerPath);
                    else
                        result.Add(null);
                }
                else
                    result.Add(null);
            }
            else if (SelectionMethodIndex == 1)
            {
                if (!string.IsNullOrEmpty(diffusemapPath))
                {
                    if (!string.IsNullOrEmpty(diffusemapPath) && File.Exists(Path.GetFullPath(diffusemapPath)))
                        result.Add(diffusemapPath);
                    else
                        result.Add(null);
                }
                else
                    result.Add(null);

                if (!string.IsNullOrEmpty(normalmapPath) && File.Exists(Path.GetFullPath(normalmapPath)))
                    result.Add(normalmapPath);
                else
                    result.Add(null);

                if (!string.IsNullOrEmpty(maskmapPath) && File.Exists(Path.GetFullPath(maskmapPath)))
                    result.Add(maskmapPath);
                else
                    result.Add(null);
            }

            return result;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;
            _detailTexture = null; 

            if (isActive)
            {
                TNode preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
                if (preNode == null ) throw new Exception("No Inputs found for node :" + Data.name + "\n\n Please Check The Node.");
                TMask mask = (preNode!=null)? TMask.MergeMasks(preNode.OutMasks) : null ;
                TMask filteredMask = mask;
                TImage diffuse = new TImage();
                TImage normalmap = new TImage();
                TImage maskmap = new TImage();

                if (mask == null) mask = new TMask(32,32);

                switch (SelectionMethodIndex)
                {
                    case 0:
                    if (!string.IsNullOrEmpty(terrainLayerPath) && File.Exists(Path.GetFullPath(terrainLayerPath)))
                    {
                        filteredMask = mask?.FilteredMask(minRange, maxRange);
                        _detailTexture = new TDetailTexture(terrainLayerPath,filteredMask);
                        _detailTexture.Tiling = tiling;
                        _detailTexture.TilingOffset = tilingOffset;
                        _detailTexture.Specular = specular;
                        _detailTexture.Metallic = metallic;
                        _detailTexture.Smoothness = smoothness;
                        _detailTexture.NormalScale = normalScale;
                        //_detailTexture.Mode = TDetailTextureMode.TerrainLayer;
                        _detailTexture.Opacity = opacity;
                    }
                    else
                        throw new Exception("No Terrain Layers Selected For Node : " + Data.name + "\n\n Please Check The Node.");
                    break;

                    case 1:
                    {
                        if (!string.IsNullOrEmpty(diffusemapPath))
                        {
                            using (Image source = Bitmap.FromFile(diffusemapPath))
                            {
                                diffuse.Image = (Bitmap)source;
                                diffuse.ObjectPath = diffusemapPath;
                            }
                        }
                        else throw new Exception("No Diffusemap Selected For Node : " + Data.name + "\n\n Please Check The Node.");

                        if (!string.IsNullOrEmpty(normalmapPath))
                        {
                            using (Image source = Bitmap.FromFile(normalmapPath))
                            {
                                normalmap.Image = (Bitmap)source;
                                normalmap.ObjectPath = normalmapPath;
                            }
                        }
                        else
                            normalmap = null;

                        if (!string.IsNullOrEmpty(maskmapPath))
                        {
                            using (Image source = Bitmap.FromFile(maskmapPath))
                            {
                                maskmap.Image = (Bitmap)source;
                                maskmap.ObjectPath = maskmapPath;
                            }
                        }
                        else
                            maskmap = null;

                        filteredMask = mask?.FilteredMask(minRange, maxRange);
                        _detailTexture = new TDetailTexture(filteredMask, diffuse, normalmap, maskmap);
                        _detailTexture.Tiling = tiling;
                        _detailTexture.TilingOffset = tilingOffset;
                        _detailTexture.Specular = specular;
                        _detailTexture.Metallic = metallic;
                        _detailTexture.Smoothness = smoothness;
                        _detailTexture.NormalScale = normalScale;
                        //_detailTexture.Mode = TDetailTextureMode.Deffuse;
                        _detailTexture.Opacity = opacity;
                    }

                    break;
                }

                _progress = 1;
            }

            isDone = true;
        }
    }

    [XmlType("Mask2ColorMap")]
    public class Mask2ColorMap : TImageModules
    {
        public static int lastNumber = 1;
        public float minRange = 0;
        public float maxRange = 1;
        public bool isColorMap = true;
        //public float opacity = 1;
        public int _colorMapColorR = 64;
        public int _colorMapColorG = 64;
        public int _colorMapColorB = 64;
        [XmlIgnore] private Color _colorMapColor = new Color();
        public int mostUsedColor = 16;
        public int SelectionColormapMethodIndex = 1;

        public Mask2ColorMap() : base()
        {
          //  type = typeof(Mask2ColorMap).FullName;
            Data.name = "Terrain Global Layer" + lastNumber;
            _colorMapColor = Color.FromArgb(_colorMapColorR, _colorMapColorG, _colorMapColorB);
            isSource = true;
            lastNumber++;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, false, "Area Node"), new TConnection(null, this, ConnectionDataType.DetailTexture, false, "Terrain Layer Node") };
            outputConnectionType = ConnectionDataType.DetailTexture;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;

            TImageModules preNode2 = parentGraph.worldGraph.GetNodeByID(inputConnections[1].previousNodeID) as TImageModules;
            _detailTexture = preNode2?._detailTexture;

            if (isActive)
            {
                TNode preNode1 = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);

                //TODO : Rsolution should check
                TMask mask = (preNode1 != null) ? TMask.MergeMasks(preNode1.OutMasks) : new TMask(32, 32, true);
                TMask filteredMask = null;
                TImage diffuse = new TImage();
                TImage normalmap = new TImage();
                TImage maskmap = new TImage();

                switch (SelectionColormapMethodIndex)
                {
                    case 0:
                        {
                            if (preNode2 == null) throw new Exception("Input Error on node :" + Data.name + "\n\n Please Check The Node.");
                            if (preNode2._detailTexture == null) throw new Exception("Input Error on node :" + Data.name + "\n\n Please Check The Node.");
                            if (preNode2._detailTexture.DiffuseMap == null) throw new Exception("Use diffuse map instead of terrainlayer on node :" + Data.name + "\n\n Please Check The Node.");

                            TMask imageMask = (preNode2.OutMasks.Count > 0) ? TMask.MergeMasks(preNode2.OutMasks) : new TMask(32, 32, true);
                            filteredMask = mask.FilteredMask(minRange, maxRange);
                            filteredMask.AND(imageMask);
                            diffuse = preNode2._detailTexture.DiffuseMap;
                            normalmap = preNode2._detailTexture.NormalMap;
                            maskmap = preNode2._detailTexture.MaskMap;
                            _detailTexture = new TDetailTexture(filteredMask, diffuse, normalmap, maskmap);
                            _detailTexture.Tiling = new Vector2(currentMap._area._areaSizeLat * 1000 , currentMap._area._areaSizeLon * 1000);

                        }
                        break;
                    case 1:
                        {
                            if (preNode2 == null) throw new Exception("Input Error on node :" + Data.name + "\n\n Please Check The Node.");
                            if (preNode2._detailTexture == null) throw new Exception("Input Error on node :" + Data.name + "\n\n Please Check The Node.");
                            if (preNode2._detailTexture.DiffuseMap == null) throw new Exception("Use diffuse map instead of terrainlayer on node :" + Data.name + "\n\n Please Check The Node.");

                            TMask imageMask = (preNode2.OutMasks.Count > 0) ? TMask.MergeMasks(preNode2.OutMasks) : new TMask(32, 32, true);
                            filteredMask = mask.FilteredMask(minRange, maxRange);
                            filteredMask.AND(imageMask);
                            diffuse = preNode2._detailTexture.DiffuseMap;
                            diffuse.Image = TImageProcessors.QuantizeImage(diffuse.Image, mostUsedColor, filteredMask);
                            normalmap = preNode2._detailTexture.NormalMap;
                            maskmap = preNode2._detailTexture.MaskMap;
                            _detailTexture = new TDetailTexture(filteredMask, diffuse, normalmap, maskmap);
                            _detailTexture.Tiling = new Vector2(currentMap._area._areaSizeLat * 1000 , currentMap._area._areaSizeLon * 1000);

                        }
                        break;
                    case 2:
                        {
                            filteredMask = mask.FilteredMask(minRange, maxRange);
                            _colorMapColor = Color.FromArgb(_colorMapColorR, _colorMapColorG, _colorMapColorB);
                            diffuse.Image = filteredMask.GetColoredImage(_colorMapColor);
                            _detailTexture = new TDetailTexture(filteredMask, diffuse, normalmap, maskmap);
                            _detailTexture.Tiling = new Vector2(currentMap._area._areaSizeLat * 1000 , currentMap._area._areaSizeLon * 1000);
                        }
                        break;
                    default:
                        break;
                }

                _progress = 1;
            }

            isDone = true;
        }
    }

    [XmlType("Image2Mask")]
    public class Image2Mask : TImageModules, IMaskPreModules
    {
        public static int lastNumber = 1;

        public Vector3 scaleMultiplier = Vector3.One;
        public float MinSlope = 0;
        public float MaxSlope = 90;
        public int tolerance = 10;
        public int SelectionImage2MaskMethodIndex = 1;
        public string diffusemapPath;
        public int _selectedColorR = 64;
        public int _selectedColorG = 64;
        public int _selectedColorB = 64;
        [XmlIgnore] private Color _selectedColor = new Color();
        [XmlIgnore] public TMask _preMask = null;

        public Image2Mask() : base()
        {
          //  type = typeof(Image2Mask).FullName;
            Data.name = "Color Filter " + lastNumber;
            lastNumber++;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.DetailTexture, true, "Terrain Layer Node") };
            outputConnectionType = ConnectionDataType.Mask;
        }

        public override List<string> GetResourcePaths()
        {
            List<string> result = new List<string>();

            if (SelectionImage2MaskMethodIndex == 1)
            {
                if (!string.IsNullOrEmpty(diffusemapPath))
                {
                    if (!string.IsNullOrEmpty(diffusemapPath) && File.Exists(Path.GetFullPath(diffusemapPath)))
                        result.Add(diffusemapPath);
                    else
                        result.Add(null);
                }
                else
                    result.Add(null);
            }

            return result;
        }

        public override void ModuleAction (TMap currentMap)
        {
            if (isDone) return;
            _progress = 0;
            OutMasks.Clear();

            if (isActive)
            {
                switch (SelectionImage2MaskMethodIndex)
                {
                    case 0:
                        {
                            TImageModules preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID) as TImageModules;

                            if (preNode == null) throw new Exception("Input Error on node :" + Data.name + "\n\n Please Check The Node.");

                            TImage diffuse = preNode._detailTexture.DiffuseMap;
                            Bitmap bitmap = diffuse == null ? null : diffuse.Image;
                            TMask mask = (preNode != null) ? TMask.MergeMasks(preNode.OutMasks) : null;
                            _selectedColor = Color.FromArgb(_selectedColorR, _selectedColorG, _selectedColorB);
                            TMask result = TImageProcessors.GetMaskMap(currentMap, bitmap, _selectedColor, tolerance, mask);
                            OutMasks.Add(result);
                        }
                        break;
                    case 1:
                        {
                            Bitmap bitmap = null;

                            if (!string.IsNullOrEmpty(diffusemapPath))
                            {
                                using (Image source = Bitmap.FromFile(diffusemapPath))
                                {
                                    bitmap = (Bitmap)source;
                                }
                            }
                            else
                                throw new Exception("No Image selected on node : " + Data.name + "\n\n Please Check The Node.");

                            //TImageModules preNode = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID) as TImageModules;
                            //TMask mask = (preNode != null) ? TMask.MergeMasks(preNode.OutMasks) : null;
                            TMask mask = null;
                            _selectedColor = Color.FromArgb(_selectedColorR, _selectedColorG, _selectedColorB);
                            TMask result = TImageProcessors.GetMaskMap(currentMap, bitmap, _selectedColor, tolerance, mask);
                            OutMasks.Add(result);
                        }
                        break;
                    default:
                        break;
                }

                _progress = 1;
            }

            isDone = true;
        }

        public TMask GetPreMask(TMap currentMap)
        {
            throw new Exception("Preview not implemented for node : " + Data.name);
        }
    }


    // Operators
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    [XmlType("MaskBlendOperator")]
    public class MaskBlendOperator : TNode
    {
        public enum BlendingMode
        {
            OR,
            AND,
            NOT,
            SUB,
            XOR,
            Exaggerate
        }
        public BlendingMode blendingMode = BlendingMode.OR;
        public static int lastNumber = 1;

        public MaskBlendOperator() : base()
        {
          //  type = typeof(MaskBlendOperator).FullName;
            Data.name = "Area Mixer " + lastNumber;
            isSwitchable = false;
            lastNumber++;
        }

        public override void InitConnections()
        {
            inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Mask, true, "Area 1"), new TConnection(null, this, ConnectionDataType.Mask, true, "Area 2") };
            outputConnectionType = ConnectionDataType.Mask;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public override void ModuleAction(TMap CurrentMap)
        {
            if (isDone) return;
            _progress = 0;

            TMask resultMask = null;
            OutMasks.Clear();

            if (isActive)
            {
                TNode preNode1 = parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
                TNode preNode2 = parentGraph.worldGraph.GetNodeByID(inputConnections[1].previousNodeID);

                switch (blendingMode)
                {
                    case BlendingMode.OR:
                        {
                            List<TMask> inputMasks = new List<TMask>();
                            for (int i = 0; i < preNode1.OutMasks.Count; i++)
                                inputMasks.Add(preNode1.OutMasks[i]);
                            for (int i = 0; i < preNode2.OutMasks.Count; i++)
                                inputMasks.Add(preNode2.OutMasks[i]);
                            resultMask = TMask.MergeMasks(inputMasks);
                        }
                        break;
                    case BlendingMode.AND:
                        {
                            List<TMask> inputMasks = new List<TMask>();
                            for (int i = 0; i < preNode1.OutMasks.Count; i++)
                                inputMasks.Add(preNode1.OutMasks[i]);
                            for (int i = 0; i < preNode2.OutMasks.Count; i++)
                                inputMasks.Add(preNode2.OutMasks[i]);
                            resultMask = TMask.AND(inputMasks);
                        }
                        break;
                    case BlendingMode.NOT:
                        {
                            List<TMask> inputMasks = new List<TMask>();
                            for (int i = 0; i < preNode1.OutMasks.Count; i++)
                                inputMasks.Add(preNode1.OutMasks[i]);
                            //for (int i = 0; i < preNode2.OutMasks.Count; i++)
                            //    inputMasks.Add(preNode2.OutMasks[i]);
                            resultMask = TMask.Inverse(inputMasks);
                        }
                        break;
                    case BlendingMode.XOR:
                        {
                            List<TMask> inputMasks = new List<TMask>();
                            for (int i = 0; i < preNode1.OutMasks.Count; i++)
                                inputMasks.Add(preNode1.OutMasks[i]);
                            for (int i = 0; i < preNode2.OutMasks.Count; i++)
                                inputMasks.Add(preNode2.OutMasks[i]);
                            resultMask = TMask.XOR(inputMasks);
                        }
                        break;
                    case BlendingMode.SUB:
                        {
                            List<TMask> inputMasks = new List<TMask>();
                            inputMasks.Add(TMask.MergeMasks(preNode1.OutMasks));
                            inputMasks.Add(TMask.MergeMasks(preNode2.OutMasks));
                            resultMask = TMask.Subtract(inputMasks);
                        }
                        break;
                    case BlendingMode.Exaggerate:
                        {
                            resultMask = TMask.Exaggerate(preNode1.OutMasks[0],1); ;
                        }
                        break;
                    default:
                        break;
                }

                if (resultMask == null) resultMask = new TMask(8, 8);

                OutMasks.Add(resultMask);
                _progress = 1;
            }

            isDone = true;
        }
    }

    //[XmlType("ColormapFromSlope")]
    //public class ColormapFromSlope : TImageModules
    //{
    //    public static int lastNumber = 1;
    //
    //    public float _strength = 0.01f;
    //    public float _widthMultiplier = 1;
    //    public float _heightMultiplier = 1;
    //    public int _colorsCount = 16;
    //    public Color[] _colors;
    //    public float[] _slopes;
    //    public float _damping = 0.1f;
    //    public bool _useSatelliteImage = true;
    //    public int _tolerance = 10;
    //    public float _dampingTest = 0f;
    //
    //    public ColormapFromSlope() : base()
    //    {
    //        type = typeof(ColormapFromSlope).FullName;
    //        Data.name = "Slope Colors " + lastNumber;
    //        lastNumber++;
    //    }
    //
    //    public override void InitConnections()
    //    {
    //        inputConnections = new List<TConnection>() { new TConnection(null, this, ConnectionDataType.Heightmap,true, "Source Heightmap"), new TConnection(null, this, ConnectionDataType.DetailTexture,true, "Source Image") };
    //        outputConnectionType = ConnectionDataType.DetailTexture;
    //    }
    //
    //    public override List<string> GetResourcePaths()
    //    {
    //        return null;
    //    }
    //
    //    public override void ModuleAction(TMap currentMap)
    //    {
    //        if (isDone) return;
    //        _progress = 0;
    //
    //        if (isActive)
    //        {
    //            THeightmapModules preNodeHeightmap = (THeightmapModules)parentGraph.worldGraph.GetNodeByID(inputConnections[0].previousNodeID);
    //            TImageModules preNodeImage = (TImageModules)parentGraph.worldGraph.GetNodeByID(inputConnections[1].previousNodeID);
    //            Bitmap slope = THeightmapProcessors.CreateSlopeMap(this, preNodeHeightmap._heightmapData, currentMap._area._areaSizeLon * 1000, currentMap._area._areaSizeLat * 1000, _widthMultiplier, _heightMultiplier, _strength);
    //            _colors = new Color[_colorsCount];
    //            _slopes = new float[_colorsCount - 1];
    //
    //            for (int i = 0; i < _colorsCount; i++)
    //            {
    //                int grayscale = (int)((float)i / _colorsCount * 255);
    //                _colors[i] = Color.FromArgb(255, grayscale, grayscale, grayscale);
    //
    //                float steepness = 2;
    //
    //                if (i < _colorsCount - 1)
    //                {
    //                    float t = (float)i / _colorsCount;
    //                    float range = (float)(0.1 + 0.25 * t + 0.6 * t * t);
    //                    _slopes[i] = t / steepness;
    //                }
    //            }
    //
    //            Bitmap colormapSlope = TImageProcessors.CreateColormapSlope(slope, _colors, _slopes, _dampingTest);
    //
    //            if(_useSatelliteImage)
    //            {
    //                Color[] RGB = TImageProcessors.GetDominantColors(preNodeImage._detailTexture.DiffuseMap.Image, colormapSlope, _colors, _tolerance);
    //                Bitmap colormapSlopeSatellite = TImageProcessors.CreateColormapSlope(slope, RGB, _slopes, _damping);
    //
    //                Bitmap blendedColormap = TImageProcessors.BlendImages(preNodeImage._detailTexture.DiffuseMap.Image, colormapSlopeSatellite, TBlendingMode.Multiply);
    //
    //                _detailTexture = new TDetailTexture(blendedColormap);
    //            }
    //            else
    //            {
    //                Color[] RGB = TImageProcessors.GetDominantColors(preNodeImage._detailTexture.DiffuseMap.Image, slope, _colors, _tolerance);
    //
    //                Bitmap colormapSlopeSatellite = TImageProcessors.CreateColormapSlope(slope, RGB, _slopes, _damping);
    //                _detailTexture = new TDetailTexture(colormapSlopeSatellite);
    //            }
    //
    //            _progress = 1;
    //        }
    //
    //        isDone = true;
    //    }
    //
    //}





    // Graph
    //---------------------------------------------------------------------------------------------------------------------------------------------------


    public class TColormapGraph : TGraph
    {
        private static ColormapMaster colormapMaster;
        private static TerrainLayerMaster1 terrainLayerMaster1;
        private static TerrainLayerMaster2 terrainLayerMaster2;
        private static TerrainLayerMaster3 terrainLayerMaster3;
        private static TerrainLayerMaster4 terrainLayerMaster4;

        public TColormapGraph() : base(ConnectionDataType.DetailTexture, "COLORMAP Graph")
        {
        }

        public void InitGraph(TTerraWorldGraph terraWorldGraph)
        {

            worldGraph = terraWorldGraph;
            _title = "COLORMAP";

            if (nodes.Count > 4) return;

            colormapMaster = new ColormapMaster();
            colormapMaster.InitConnections();
            colormapMaster.Data.moduleType = ModuleType.Master;
            colormapMaster.parentGraph = this;
            nodes.Add(colormapMaster);
            
            terrainLayerMaster1 = new TerrainLayerMaster1();
            terrainLayerMaster1.InitConnections();
            terrainLayerMaster1.Data.moduleType = ModuleType.Master;
            terrainLayerMaster1.parentGraph = this;
            nodes.Add(terrainLayerMaster1);
            
            terrainLayerMaster2 = new TerrainLayerMaster2();
            terrainLayerMaster2.InitConnections();
            terrainLayerMaster2.Data.moduleType = ModuleType.Master;
            terrainLayerMaster2.parentGraph = this;
            nodes.Add(terrainLayerMaster2);
            
            terrainLayerMaster3 = new TerrainLayerMaster3();
            terrainLayerMaster3.InitConnections();
            terrainLayerMaster3.Data.moduleType = ModuleType.Master;
            terrainLayerMaster3.parentGraph = this;
            nodes.Add(terrainLayerMaster3);
            
            terrainLayerMaster4 = new TerrainLayerMaster4();
            terrainLayerMaster4.InitConnections();
            terrainLayerMaster4.Data.moduleType = ModuleType.Master;
            terrainLayerMaster4.parentGraph = this;
            nodes.Add(terrainLayerMaster4);
        }

        public void AddNode(ColormapProcessors colormapProcessors)
        {
            if (colormapProcessors.Equals(ColormapProcessors.Satellite_Image_Source))
            {
                SatelliteImage node = new SatelliteImage();
                node.Init(this);
                node.Data.moduleType = ModuleType.Processor;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
                TConnection connection = new TConnection(node, nodes[0], ConnectionDataType.ColormapMaster, false, node.Data.name);
                nodes[0].inputConnections.Add(connection);
            }

            else if (colormapProcessors.Equals(ColormapProcessors.Terrain_Detail_Layer))
            {
                Mask2DetailTexture node = new Mask2DetailTexture();
                node.Init(this);
                node.Data.moduleType = ModuleType.Processor;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (colormapProcessors.Equals(ColormapProcessors.Terrain_Global_Layer))
            {
                Mask2ColorMap node = new Mask2ColorMap();
                node.Init(this);
                node.Data.moduleType = ModuleType.Processor;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
                TConnection connection = new TConnection(node, nodes[0], ConnectionDataType.ColormapMaster, false, node.Data.name);
                nodes[0].inputConnections.Add(connection);
            }

            UpdateConnections();
        }

        public void AddNode(ColormapMasks colormapMasks)
        {
            if (colormapMasks.Equals(ColormapMasks.Area_Mixer))
            {
                MaskBlendOperator node = new MaskBlendOperator();
                node.Init(this);
                node.Data.moduleType = ModuleType.Operator;
                node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }
            else if (colormapMasks.Equals(ColormapMasks.Color_Filter))
            {
                Image2Mask node = new Image2Mask();
                node.Init(this);
                node.Data.moduleType = ModuleType.Processor;
                if (nodes.Count > 0) node.AddInputConnection(nodes[nodes.Count - 1], 0, -1);
                nodes.Add(node);
            }

            UpdateConnections();
        }

        public override List<TNode> GetLastNodes(ConnectionDataType connectionDataType)
        {
            List<TNode> result= new List<TNode>();

            if (connectionDataType == ConnectionDataType.ColormapMaster || connectionDataType == ConnectionDataType.DetailTexture)
            {
                TNode colormapmaster = nodes[0];
                for (int u = 0; u < colormapmaster.inputConnections.Count; u++)
                {
                    if (colormapmaster.inputConnections[u] != null && colormapmaster.inputConnections[u].previousNodeID != -1)
                    {
                        TNode colorMapNode = GetNodeByID(colormapmaster.inputConnections[u].previousNodeID);
                        result.Add(colorMapNode);
                    }
                }
            }

            if (connectionDataType == ConnectionDataType.DetailTextureMaster || connectionDataType == ConnectionDataType.DetailTexture)
            {
                TNode colormapmaster = nodes[0];

                for (int u = 0; u < nodes.Count; u++)
                {
                    if (nodes[u] != null && nodes[u].inputConnections.Count >0 && nodes[u].inputConnections[0].previousNodeID != -1 && nodes[u].outputConnectionType == ConnectionDataType.DetailTextureMaster)
                    {
                        TNode detatilTextureLastNode = GetNodeByID(nodes[u].inputConnections[0].previousNodeID);
                        result.Add(detatilTextureLastNode);
                    }
                }
            }

            if (connectionDataType == ConnectionDataType.DetailTexture)
            {
                List<TNode> Over4DetailTextures = base.GetLastNodes(connectionDataType);

                for (int u = 0; u < Over4DetailTextures.Count; u++)
                {
                  result.Add(Over4DetailTextures[u]);
                }
            }

            return result;
        }


        public SatelliteImage SatelliteImage()
        {
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].GetType() == typeof(SatelliteImage)) return (nodes[i] as SatelliteImage);

            return null;
        }


    }
}
#endif
#endif
