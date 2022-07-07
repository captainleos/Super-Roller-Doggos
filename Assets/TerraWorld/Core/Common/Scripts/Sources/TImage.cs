#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace TerraUnity.Edittime
{
    public enum ImageFormats
    {
        PNG,
        JPG
    }

    public class TImage : TObjects
    {
        public ImageFormats _imageType = ImageFormats.JPG;
        public TextureImporterFormat imageFormat = TextureImporterFormat.Automatic;
        public bool isReadable = false;
        private Bitmap _cachedImage;

        public TImage() : base(AssetTypes.Image)
        {
        }

        public TImage(Bitmap Image) : base(AssetTypes.Image)
        {
            _cachedImage = Image;
        }

        public void FillImage(System.Drawing.Color color, TMask mask)
        {
            int Width = _cachedImage.Width;
            int Length = _cachedImage.Height;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    double normilizX = (i * 1.0 / Width);
                    double normilizY = (j * 1.0 / Length);

                    if (mask.CheckNormal(normilizX, normilizY))
                        _cachedImage.SetPixel(i, Length - j-1, color);
                }
            }
        }

        public override string GenerateAssetPath()
        {
            if (_assetType == AssetTypes.Image) return TTerraWorld.WorkDirectoryLocalPath + "Asset_" + ID + ".jpg";
            else return "";
        }

        public void Copy(Bitmap Image) 
        {
            _cachedImage = Image.Clone() as Bitmap;
        }

        public Bitmap Image
        {
            get { return _cachedImage; }
            set { _cachedImage = value; }
        }

        private Bitmap Texture2DToBitmap (Texture2D texture)
        {
            Bitmap result = new Bitmap(texture.width, texture.height);

            if (_imageType == ImageFormats.PNG)
            {
                byte[] bytes = texture.EncodeToPNG();

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return result = new Bitmap(ms);
                }
            }
            else if (_imageType == ImageFormats.JPG)
            {
                byte[] bytes = texture.EncodeToJPG();

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return result = new Bitmap(ms);
                }
            }

            return null;
        }

        private Texture2D BitmapToTexture2D (Bitmap bmp)
        {
            Texture2D result = new Texture2D(bmp.Width, bmp.Height);

            using (MemoryStream stream = new MemoryStream())
            {
                if(_imageType == ImageFormats.PNG)
                    bmp.Save(stream, ImageFormat.Png);
                else if (_imageType == ImageFormats.JPG)
                    bmp.Save(stream, ImageFormat.Jpeg);

                byte[] bytes = stream.ToArray();
                result.LoadImage(bytes);
            }

            return result;
        }

        public override void SaveObject(string path)
        {
            _ObjPath = path;
            //if (File.Exists(_ObjPath)) return;

            string extension = Path.GetExtension(path);

            if (extension == ".png")
                _imageType = ImageFormats.PNG;
            else if (extension == ".jpg")
                _imageType = ImageFormats.JPG;
            else
                throw new Exception("Unknown Format!");

            string fullPath = Path.GetFullPath(path);
            string projectPath = fullPath.Substring(fullPath.LastIndexOf("Assets"));

            if (_cachedImage != null)
            {
                Texture2D tempImage = BitmapToTexture2D(_cachedImage);

                if (extension == ".png")
                    File.WriteAllBytes(path, tempImage.EncodeToPNG());
                else if (extension == ".jpg")
                    File.WriteAllBytes(path, tempImage.EncodeToJPG());

                AssetDatabase.Refresh();
                TextureImporter imageImport = AssetImporter.GetAtPath(projectPath) as TextureImporter;

                TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings();
                platformSettings.format = imageFormat;
                imageImport.SetPlatformTextureSettings(platformSettings);

                // set texture importer parameters
                imageImport.mipmapEnabled = true;
                imageImport.wrapMode = TextureWrapMode.Clamp;

                imageImport.maxTextureSize = Mathf.ClosestPowerOfTwo(_cachedImage.Width);
                imageImport.isReadable = isReadable;

                if (imageFormat == TextureImporterFormat.Alpha8)
                {
                    imageImport.sRGBTexture = false;
                    imageImport.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                }

                AssetDatabase.ImportAsset(projectPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }
        }

        public Texture2D GetTexture(string assetname)
        {
            imageFormat = TextureImporterFormat.Alpha8;
            isReadable = true;
            SaveObject(TTerraWorld.WorkDirectoryLocalPath + assetname + ".jpg");
            return GetObject() as Texture2D;
        }

        public Texture2D GetTextureRGBA(string assetname)
        {
            imageFormat = TextureImporterFormat.Automatic;
            isReadable = true;
            SaveObject(TTerraWorld.WorkDirectoryLocalPath + assetname + ".jpg");
            return GetObject() as Texture2D;
        }
    }
}
#endif
#endif

