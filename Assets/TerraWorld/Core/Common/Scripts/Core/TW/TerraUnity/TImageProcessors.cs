#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using AForge.Imaging.Filters;
using Sc.Util.Rendering;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
    public enum TBlendingMode
    {
        Multiply,
        Additive
    }

    public static class TImageProcessors
    {

        // Base Processors
        //----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Bitmap CropImage(TArea baseArea, TArea cropArea, Bitmap Src)
        {
            int w = Src.Width;
            int h = Src.Height;
            int newH, newW, offsetX, offsetY;
            newH = newW = offsetX = offsetY = 0;
            TUtils.GeoToPixelCrop(baseArea, cropArea, w, h, out newW, out newH, out offsetX, out offsetY);

            if (newW <= 0 || newH <= 0) return Src;

            Rectangle cropRect = new Rectangle(offsetX, h - newH - offsetY, newW, newH);
            Bitmap Dest = new Bitmap(newW, newH);
            Dest.SetResolution(96, 96);

            using (Graphics g = Graphics.FromImage(Dest))
            {
                g.DrawImage(Src, new Rectangle(0, 0, Dest.Width, Dest.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            return Dest;
        }

        public static void CombineImages(string path, string format, int xTiles, int yTiles, int cellResolution)
        {
            int width = xTiles * cellResolution;
            int length = yTiles * cellResolution;
            Bitmap image = new Bitmap(width, length);

            string[] names = Directory.GetFiles(@path, "*" + format);
            names = TUtils.LogicalComparer(names);
            int index = 0;

            for (int y = (yTiles - 1); y >= 0; y--)
            {
                for (int x = 0; x < xTiles; x++)
                {
                    string name = names[index];
                    byte[] bytes = File.ReadAllBytes(name);
                    Bitmap cellImage = new Bitmap(cellResolution, cellResolution);
                    MemoryStream ms = new MemoryStream(bytes);
                    cellImage = new Bitmap(ms);
                    Rectangle regionDst = new Rectangle(x * cellResolution, y * cellResolution, cellResolution, cellResolution);
                    Rectangle regionSrc = new Rectangle(0, 0, cellResolution, cellResolution);
                    Graphics graphics = Graphics.FromImage(image);
                    graphics.DrawImage(cellImage, regionDst, regionSrc, GraphicsUnit.Pixel);

                    index++;
                }
            }

        }

        public static byte[] ImageToByte(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }


        public static Bitmap BlendImages(Bitmap Src1, Bitmap Src2, TBlendingMode blendingMode = TBlendingMode.Multiply)
        {
            int width1 = Src1.Width;
            int height1 = Src1.Height;
            int width2 = Src2.Width;
            int height2 = Src2.Height;

            if (width1 != width2 || height1 != height2)
                Src2 = ResizeBitmap(Src2, width1, height1);

            Bitmap dst = new Bitmap(width1, height1);

            switch (blendingMode)
            {
                case TBlendingMode.Multiply:

                    for (int i = 0; i < width1; i++)
                    {
                        for (int j = 0; j < height1; j++)
                        {
                            Color pixel1 = Src1.GetPixel(i, j);
                            Color pixel2 = Src2.GetPixel(i, j);
                            int R = (pixel1.R + pixel2.R) / 2;
                            int G = (pixel1.G + pixel2.G) / 2;
                            int B = (pixel1.B + pixel2.B) / 2;
                            int A = (pixel1.A + pixel2.A) / 2;


                            Color pixel = Color.FromArgb(A, R, G, B);
                            dst.SetPixel(i, j, pixel);
                        }
                    }

                    break;
            }

            return dst;

        }

        public static Bitmap CreateColormap(Bitmap Src1, Bitmap Src2)
        {
            int width = Src1.Width;
            int height = Src1.Height;
            Bitmap dst = new Bitmap(width, height);
            int black1 = 255;
            int white1 = 0;
            int black2 = 255;
            int white2 = 0;

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    Color pixel1 = Src1.GetPixel(i, j);
                    Color pixel2 = Src2.GetPixel(i, j);

                    if (pixel1.R > white1)
                        white1 = pixel1.R;

                    if (pixel1.R < black1)
                        black1 = pixel1.R;

                    if (pixel2.R > white2)
                        white2 = pixel2.R;

                    if (pixel2.R < black2)
                        black2 = pixel2.R;
                }

            float normalize1 = white1 - black1;
            float normalize2 = white2 - black2;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel1 = Src1.GetPixel(i, j);
                    Color pixel2 = Src2.GetPixel(i, j);
                    Color snowColor = Color.White;
                    Color soilColor = Color.Khaki;

                    double R1 = snowColor.R + ((normalize1 - pixel1.R) / normalize1) * (soilColor.R - snowColor.R);
                    double G1 = snowColor.G + ((normalize1 - pixel1.G) / normalize1) * (soilColor.G - snowColor.G);
                    double B1 = snowColor.B + ((normalize1 - pixel1.B) / normalize1) * (soilColor.B - snowColor.B);
                    double A1 = 255;

                    double R2 = snowColor.R + ((normalize2 - pixel2.R) / normalize2) * (soilColor.R - snowColor.R);
                    double G2 = snowColor.G + ((normalize2 - pixel2.G) / normalize2) * (soilColor.G - snowColor.G);
                    double B2 = snowColor.B + ((normalize2 - pixel2.B) / normalize2) * (soilColor.B - snowColor.B);
                    double A2 = 255;

                    int R = (int)((R1 + R2) / 2);
                    int G = (int)((G1 + G2) / 2);
                    int B = (int)((B1 + B2) / 2);
                    int A = (int)((A1 + A2) / 2);

                    Color col = Color.FromArgb(255, (int)R, (int)G, (int)B);
                    dst.SetPixel(i, j, col);
                }
            }

            return dst;
        }

        public static bool IsColorMatch(Color Color1, Color Color2, float Tolerance)
        {

            if ((Math.Abs(Color1.R - Color2.R) <= Tolerance) && (Math.Abs(Color1.G - Color2.G) <= Tolerance) && (Math.Abs(Color1.B - Color2.B) <= Tolerance))
                return true;

            return false;
        }

        public static bool IsColorMatchByHue(Color Color1, Color Color2, float Tolerance)
        {
          if (Color2.IsEmpty) return false;

            float hue1 = Color1.GetHue();
            float sat1 = Color1.GetSaturation();
            float lgt1 = Color1.GetBrightness();


            float hue2 = Color2.GetHue();
            float sat2 = Color2.GetSaturation();
            float lgt2 = Color2.GetBrightness();

            if (lgt1 < 0.2 && lgt2 < 0.2) return true;
            if (lgt1 > 0.8 && lgt2 > 0.8) return true;

            if (sat1 < 0.25 && sat2< 0.25) return true;

            if (Math.Abs(Color1.GetHue() - Color2.GetHue()) <= Tolerance)
              return true;
          else
              return false;
        }

        public static float ToleranceByeHue(Color Color1, Color Color2)
        {
            return (Math.Abs(Color1.GetHue() - Color2.GetHue()));
        }


        public static Color ChangeHue(Color HueSource , Color Other)
        {
            double[] src = SimpleColorTransforms.RgBtoHsl(HueSource);
            double[] oth = SimpleColorTransforms.RgBtoHsl(Other);
            src[0] = oth[0];
            return SimpleColorTransforms.HsLtoRgb(src[0], src[1], src[2]);

        }

        public static float ToleranceByeColor(Color Color1, Color Color2)
        {
           int DefR = (Color1.R - Color2.R);
           int DefG = (Color1.G - Color2.G);
           int DefB = (Color1.B - Color2.B);
          
           return (float)Math.Sqrt(DefR* DefR+ DefG * DefG+ DefB * DefB );

        }

        public static Color[] GetDominantColors(Bitmap Src, Bitmap mask, Color[] color, int Tolerance)
        {
            int width = Src.Width;
            int height = Src.Height;
            int width2 = mask.Width;
            int height2 = mask.Height;

            if (Tolerance <= 0)
                Tolerance = 0;
            else if (Tolerance >= 250)
                Tolerance = 250;

            if (width != width2 || height != height2)
                mask = ResizeBitmap(mask, width, height);

            int colorsCount = color.Length;
            Dictionary<Color, int>[] colors = new Dictionary<Color, int>[colorsCount];

            for (int x = 0; x < colorsCount; x++)
                colors[x] = new Dictionary<Color, int>();

            int[] maxIndex = new int[colorsCount];
            Color[] result = new Color[colorsCount];

            double[] SumR = new double[colorsCount];
            double[] SumG = new double[colorsCount];
            double[] SumB = new double[colorsCount];
            double[] Index = new double[colorsCount];
            double[] SumIndex = new double[colorsCount];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel1 = Src.GetPixel(i, j);
                    Color pixel2 = mask.GetPixel(i, j);

                    for (int x = 0; x < colorsCount; x++)
                    {
                        if (IsColorMatch(pixel2, color[x], Tolerance))
                        {
                            if (colors[x].ContainsKey(pixel1))
                            {
                                int value = -1;

                                if (colors[x].TryGetValue(pixel1, out value))
                                    colors[x][pixel1] = value + 1;

                                SumR[x] += pixel1.R;
                                SumG[x] += pixel1.G;
                                SumB[x] += pixel1.B;
                                Index[x] += 1;

                                if (maxIndex[x] < value)
                                {
                                    maxIndex[x] = value;
                                    result[x] = pixel1;
                                }
                            }
                            else
                                colors[x].Add(pixel1, 1);
                        }
                    }
                }
            }


            for (int x = 0; x < colorsCount; x++)
            {
                SumR[x] /= (Index[x]+1); 
                SumG[x] /= (Index[x]+1); 
                SumB[x] /= (Index[x]+1); 
                Color col = Color.FromArgb(255, (int)SumR[x], (int)SumG[x], (int)SumB[x]);
                result[x] = col;
            }

            mask.Dispose();
            return result;
        }

        public static Color[] GetDominantColors(Bitmap Src, int colorsCount , int Tolerance = 10, TMask mask = null)
        {
            int width = Src.Width;
            int height = Src.Height;
            Dictionary<Color, int> colors = new Dictionary<Color, int>();
            Color[] result = new Color[colorsCount];

            for (int i = 0; i < width; i++)
            {
                for (int j = 1; j < height; j++)
                {
                    double normalI = i * 1.0d / width;
                    double normalJ = j * 1.0d / height;
                    if (mask != null && !mask.CheckNormal(normalI, 1 - normalJ)) continue;

                    Color pixel1 = Src.GetPixel(i, j);

                    if (colors.ContainsKey(pixel1))
                    {
                        int value = -1;

                        if (colors.TryGetValue(pixel1, out value))
                            colors[pixel1] = value + 1;
                    }
                    else
                        colors.Add(pixel1, 1);
                }
            }

            int OutIndex = 0;

          

            // Order by values.
            // ... Use LINQ to specify sorting by value.
           var items = from pair in colors
                       orderby pair.Value descending
                       select pair;
        
           float indexpassed = 0;
           foreach (KeyValuePair<Color, int> pair in items)
           {
               Color maxColor = pair.Key;
               indexpassed = indexpassed + pair.Value;
               int percent = (int)(indexpassed * 100.0f / (width * height));
            //   bool isNewColor = true;
               for (int y = 0; y < colorsCount; y++)
                   if (IsColorMatch(maxColor, result[y], Tolerance))
                   {
                   //    isNewColor = false;
                       break;
                   }
       

               {
                   result[OutIndex++] = maxColor;
        
                   if (OutIndex == colorsCount)
                       return result;
               }
        
                 if (percent > 99)
                     return result;
        
           }
          return result;
        }



        public static Bitmap FlattenImage (Bitmap Src, Color[] baseColors)
        {
            int width = Src.Width;
            int height = Src.Height;
            int colorsCount = baseColors.Length;
            float tempTolerance = 0;
            Bitmap result = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel1 = Src.GetPixel(i, j);
                    tempTolerance = 1024;
                    int selectedColor = 0;
                    for (int u = 0; u < colorsCount; u++)
                    {
                        float ut = ToleranceByeHue(pixel1, baseColors[u]);
                        if (tempTolerance > ut )
                        {
                            tempTolerance = ut;
                            selectedColor = u;
                        }
                    }
                    result.SetPixel(i, j, baseColors[selectedColor]);
                }
            }

            return result;
        }


        public static Bitmap QuantizeImage(Bitmap Src, int colorCount, TMask mask)
        {
            
            int telorance = (int)(250 * 1.0f / (colorCount-1));
             Color[] baseColors = GetDominantColors(Src, colorCount, telorance, mask);
            Bitmap fi = FlattenImage(Src, baseColors);
            return fi;

        }

        public static Color Classify(Color c)
        {
            float hue = c.GetHue();
            float sat = c.GetSaturation();
            float lgt = c.GetBrightness();

            if (lgt < 0.2) return Color.FromArgb(0,0,0);
            if (lgt > 0.8) return Color.FromArgb(255, 255, 255);

            if (sat < 0.25) return Color.FromArgb(175, 175, 175);

            if (hue < 30) return Color.FromArgb(255, 0, 0);
            if (hue < 90) return Color.FromArgb(255, 255, 0);
            if (hue < 150) return Color.FromArgb(0, 255, 0);
            if (hue < 210) return Color.FromArgb(0, 255, 255);
            if (hue < 270) return Color.FromArgb(0, 0, 255);
            if (hue < 330) return Color.FromArgb(255, 0, 255);
            return Color.FromArgb(255, 0, 0);
        }

        public static Bitmap CreateColormapSlope (Bitmap Src, Color[] colors, float[] Slopes, float damping = 0.025f)
        {
            int width = Src.Width;
            int height = Src.Height;
            Bitmap dst = new Bitmap(width, height);
            int black = 255;
            int white = 0;

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    Color pixel = Src.GetPixel(i, j);

                    if (pixel.R > white)
                        white = pixel.R;

                    if (pixel.R < black)
                        black = pixel.R;
                }

            float normalize = white - black;
            Color col1 = colors[0];
            Color col2 = colors[1];
            Color col3 = colors[2];
            Color col4 = colors[3];
            int slope1 = (int)(Slopes[0] * 255);
            int slope2 = (int)(Slopes[1] * 255);
            int slope3 = (int)(Slopes[2] * 255);

            if (damping <= 0)
                damping = 0.004f;
            else if (damping >= 1)
                damping = 1;

            damping *= 255;
            Vector3 layerCol1 = Vector3.Zero;
            Vector3 layerCol2 = Vector3.Zero;
            Vector3 layerCol3 = Vector3.Zero;
            Vector3 layerCol4 = Vector3.Zero;
            Vector3 averageCol1 = Vector3.Zero;
            Vector3 averageCol2 = Vector3.Zero;
            Vector3 averageCol3 = Vector3.Zero;
            Vector3 averageCol4 = Vector3.Zero;
            Vector3 finalColor = Vector3.Zero;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel = Src.GetPixel(i, j);
                    float falloff = pixel.R;

                    if (falloff >= 0 && falloff <= slope1)
                        finalColor = new Vector3(col1.R, col1.G, col1.B);
                    else if (falloff > slope1 && falloff <= slope2)
                        finalColor = new Vector3(col2.R, col2.G, col2.B);
                    else if (falloff > slope2 && falloff <= slope3)
                        finalColor = new Vector3(col3.R, col3.G, col3.B);
                    else if (falloff > slope3)
                        finalColor = new Vector3(col4.R, col4.G, col4.B);

                    if (falloff >= (slope1 - damping) && falloff <= (slope1 + damping))
                    {
                        float t = (falloff - slope1 + damping) / (2 * damping);
                        finalColor.X = Lerp(col1.R, col2.R, t);
                        finalColor.Y = Lerp(col1.G, col2.G, t);
                        finalColor.Z = Lerp(col1.B, col2.B, t);
                    }

                    if (falloff >= (slope2 - damping) && falloff <= (slope2 + damping))
                    {
                        float t = (falloff - slope2 + damping) / (2 * damping);
                        finalColor.X = Lerp(col2.R, col3.R, t);
                        finalColor.Y = Lerp(col2.G, col3.G, t);
                        finalColor.Z = Lerp(col2.B, col3.B, t);
                    }

                    if (falloff >= (slope3 - damping) && falloff <= (slope3 + damping))
                    {
                        float t = (falloff - slope3 + damping) / (2 * damping);
                        finalColor.X = Lerp(col3.R, col4.R, t);
                        finalColor.Y = Lerp(col3.G, col4.G, t);
                        finalColor.Z = Lerp(col3.B, col4.B, t);
                    }

                    Vector3 color = finalColor;
                    Color col = Color.FromArgb(255, ClampChannel((int)color.X), ClampChannel((int)color.Y), ClampChannel((int)color.Z));
                    dst.SetPixel(i, j, col);
                }
            }

            return dst;
        }

        public static Bitmap CreateColormapSlopeFlow (Bitmap Src1, Bitmap Src2, Color color, float clamp = 0.25f)
        {
            int width = Src1.Width;
            int height = Src1.Height;
            int width2 = Src2.Width;
            int height2 = Src2.Height;

            if (width != width2 || height != height2)
                Src2 = ResizeBitmap(Src2, width, height);

            Bitmap dst = new Bitmap(width, height);
            int black = 255;
            int white = 0;

            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                {
                    Color pixel = Src2.GetPixel(i, j);

                    if (pixel.R > white)
                        white = pixel.R;

                    if (pixel.R < black)
                        black = pixel.R;
                }

            float normalize = white - black;
            float R, G, B, A;
            R = G = B = A = 0;
            clamp *= 255;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixelSrc = Src1.GetPixel(i, j);
                    Color t = Src2.GetPixel(i, j);

                    if(t.R > clamp)
                    {
                        R = Lerp(pixelSrc.R, color.R, t.R / normalize);
                        G = Lerp(pixelSrc.G, color.G, t.G / normalize);
                        B = Lerp(pixelSrc.B, color.B, t.B / normalize);
                        A = 255;
                    }
                    else
                    {
                        R = pixelSrc.R;
                        G = pixelSrc.G;
                        B = pixelSrc.B;
                        A = 255;
                    }

                    Color col = Color.FromArgb((int)A, (int)R, (int)G, (int)B);
                    dst.SetPixel(i, j, col);
                }
            }

            return dst;
        }

        public static Bitmap ShadowRemover (TNode parentNode, Bitmap Src, Color shadowColor, int blockSize)
        {
            Rectangle rect = new Rectangle(0, 0, Src.Width, Src.Height);
            Bitmap outputImage = (Bitmap)Src.Clone(rect, Src.PixelFormat);

            int R = (int)TUtils.Clamp(0, 255, shadowColor.R);
            int G = (int)TUtils.Clamp(0, 255, shadowColor.G);
            int B = (int)TUtils.Clamp(0, 255, shadowColor.B);

            Color shColor = Color.FromArgb(R, G, B);
            float shadowBrightness = shColor.GetBrightness(); //From 0 (black) to 1 (white)

            int imageWidth = Src.Width;
            int imageHeight = Src.Height;

            int healingSizeVariated = (int)((float)blockSize * 1.5f);

            float progressShadow = 0;

            Color pixelColor = new Color();
            float pixelColorBrightness = 0;

            for (int y = blockSize; y < imageHeight - blockSize; y++)
            {
                for (int x = blockSize; x < imageWidth - blockSize; x++)
                {
                    Color color = Src.GetPixel(x, y);
                    float currentPixelbrightness = color.GetBrightness();

                    try
                    {
                        if (currentPixelbrightness < shadowBrightness)
                        {
                            // LEFT ==> RIGHT
                            pixelColor = Src.GetPixel(x + blockSize, y);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);

                            // RIGHT ==> LEFT
                            pixelColor = Src.GetPixel(x - blockSize, y);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);

                            // TOP ==> BOTTOM
                            pixelColor = Src.GetPixel(x, y + blockSize);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);

                            // BOTTOM ==> TOP
                            pixelColor = Src.GetPixel(x, y - blockSize);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);
                        }
                        else
                        {
                            if (currentPixelbrightness < shadowBrightness * 1.25f)
                            {
                                pixelColor = Src.GetPixel(x - blockSize, y);
                                pixelColorBrightness = pixelColor.GetBrightness();

                                if (pixelColorBrightness > shadowBrightness)
                                    outputImage.SetPixel(x, y, pixelColor);
                            }
                            else
                            {
                                outputImage.SetPixel(x, y, color);
                            }
                        }
                    }
                    catch { }

                    progressShadow = (float)TUtils.InverseLerp(0f, (float)(imageHeight * 3), (float)y);

                }
            }

            for (int y = healingSizeVariated; y < imageHeight - healingSizeVariated; y++)
            {
                for (int x = healingSizeVariated; x < imageWidth- healingSizeVariated; x++)
                {
                    Color color = outputImage.GetPixel(x, y);
                    float currentPixelbrightness = color.GetBrightness();

                    try
                    {
                        if (currentPixelbrightness < shadowBrightness)
                        {
                            // TOP-LEFT ==> BOTTOM-RIGHT
                            pixelColor = outputImage.GetPixel(x + healingSizeVariated, y + healingSizeVariated);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);

                            // TOP-RIGHT ==> BOTTOM-LEFT
                            pixelColor = outputImage.GetPixel(x - healingSizeVariated, y + healingSizeVariated);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);

                            // BOTTOM-LEFT ==> TOP-RIGHT
                            pixelColor = outputImage.GetPixel(x + healingSizeVariated, y - healingSizeVariated);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);

                            // BOTTOM-RIGHT ==> TOP-LEFT
                            pixelColor = outputImage.GetPixel(x - healingSizeVariated, y - healingSizeVariated);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);
                        }
                        else
                        {
                            if (currentPixelbrightness < shadowBrightness * 1.25f)
                            {
                                pixelColor = Src.GetPixel(x - healingSizeVariated, y - healingSizeVariated);
                                pixelColorBrightness = pixelColor.GetBrightness();

                                if (pixelColorBrightness > shadowBrightness)
                                    outputImage.SetPixel(x, y, pixelColor);
                            }
                            else
                            {
                                outputImage.SetPixel(x, y, color);
                            }
                        }
                    }
                    catch { }

                    progressShadow = (float)TUtils.InverseLerp(0f, (float)(imageHeight * 3), (float)(y + imageHeight));
                }
            }

            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    Color color = outputImage.GetPixel(x, y);
                    float currentPixelbrightness = color.GetBrightness();

                    if (currentPixelbrightness < shadowBrightness)
                    {
                        // TOP-LEFT CORNER
                        if (x <= healingSizeVariated && y <= healingSizeVariated)
                        {
                            // LEFT ==> RIGHT
                            pixelColor = outputImage.GetPixel(x + healingSizeVariated, y);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);
                            else
                            {
                                // TOP-LEFT ==> BOTTOM-RIGHT
                                pixelColor = outputImage.GetPixel(x + healingSizeVariated, y + healingSizeVariated);
                                pixelColorBrightness = pixelColor.GetBrightness();

                                if (pixelColorBrightness > shadowBrightness)
                                    outputImage.SetPixel(x, y, pixelColor);
                                else
                                {
                                    // TOP ==> BOTTOM
                                    pixelColor = outputImage.GetPixel(x, y + healingSizeVariated);
                                    pixelColorBrightness = pixelColor.GetBrightness();

                                    if (pixelColorBrightness > shadowBrightness)
                                        outputImage.SetPixel(x, y, pixelColor);
                                }
                            }
                        }

                        // TOP-RIGHT CORNER
                        else if (x >= imageWidth - healingSizeVariated && y <= healingSizeVariated)
                        {
                            // RIGHT ==> LEFT
                            pixelColor = outputImage.GetPixel(x - healingSizeVariated, y);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);
                            {
                                // TOP-RIGHT ==> BOTTOM-LEFT
                                pixelColor = outputImage.GetPixel(x - healingSizeVariated, y + healingSizeVariated);
                                pixelColorBrightness = pixelColor.GetBrightness();

                                if (pixelColorBrightness > shadowBrightness)
                                    outputImage.SetPixel(x, y, pixelColor);
                                else
                                {
                                    // TOP ==> BOTTOM
                                    pixelColor = outputImage.GetPixel(x, y + healingSizeVariated);
                                    pixelColorBrightness = pixelColor.GetBrightness();

                                    if (pixelColorBrightness > shadowBrightness)
                                        outputImage.SetPixel(x, y, pixelColor);
                                }
                            }
                        }

                        // BOTTOM-LEFT CORNER
                        else if (x <= healingSizeVariated && y >= imageHeight - healingSizeVariated)
                        {
                            // LEFT ==> RIGHT
                            pixelColor = outputImage.GetPixel(x + healingSizeVariated, y);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);
                            else
                            {
                                // BOTTOM-LEFT ==> TOP-RIGHT
                                pixelColor = outputImage.GetPixel(x + healingSizeVariated, y - healingSizeVariated);
                                pixelColorBrightness = pixelColor.GetBrightness();

                                if (pixelColorBrightness > shadowBrightness)
                                    outputImage.SetPixel(x, y, pixelColor);
                                else
                                {
                                    // BOTTOM ==> TOP
                                    pixelColor = outputImage.GetPixel(x, y - healingSizeVariated);
                                    pixelColorBrightness = pixelColor.GetBrightness();

                                    if (pixelColorBrightness > shadowBrightness)
                                        outputImage.SetPixel(x, y, pixelColor);
                                }
                            }
                        }

                        // BOTTOM-RIGHT CORNER
                        else if (x >= imageWidth - healingSizeVariated && y >= imageHeight - healingSizeVariated)
                        {
                            // RIGHT ==> LEFT
                            pixelColor = outputImage.GetPixel(x - healingSizeVariated, y);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);
                            {
                                // BOTTOM-RIGHT ==> TOP-LEFT
                                pixelColor = outputImage.GetPixel(x - healingSizeVariated, y - healingSizeVariated);
                                pixelColorBrightness = pixelColor.GetBrightness();

                                if (pixelColorBrightness > shadowBrightness)
                                    outputImage.SetPixel(x, y, pixelColor);
                                else
                                {
                                    // BOTTOM ==> TOP
                                    pixelColor = outputImage.GetPixel(x, y - healingSizeVariated);
                                    pixelColorBrightness = pixelColor.GetBrightness();

                                    if (pixelColorBrightness > shadowBrightness)
                                        outputImage.SetPixel(x, y, pixelColor);
                                }
                            }
                        }
                        else
                        {
                            // LEFT COLUMN
                            if (x <= healingSizeVariated)
                            {
                                // LEFT ==> RIGHT
                                pixelColor = outputImage.GetPixel(x + healingSizeVariated, y);
                                pixelColorBrightness = pixelColor.GetBrightness();

                                if (pixelColorBrightness > shadowBrightness)
                                    outputImage.SetPixel(x, y, pixelColor);
                                else
                                {
                                    // TOP-LEFT ==> BOTTOM-RIGHT
                                    pixelColor = outputImage.GetPixel(x + healingSizeVariated, y + healingSizeVariated);
                                    pixelColorBrightness = pixelColor.GetBrightness();

                                    if (pixelColorBrightness > shadowBrightness)
                                        outputImage.SetPixel(x, y, pixelColor);
                                    else
                                    {
                                        // BOTTOM-LEFT ==> TOP-RIGHT
                                        pixelColor = outputImage.GetPixel(x + healingSizeVariated, y - healingSizeVariated);
                                        pixelColorBrightness = pixelColor.GetBrightness();

                                        if (pixelColorBrightness > shadowBrightness)
                                            outputImage.SetPixel(x, y, pixelColor);
                                    }
                                }
                            }

                            // RIGHT COLUMN
                            if (x >= imageWidth - healingSizeVariated)
                            {
                                // RIGHT ==> LEFT
                                pixelColor = outputImage.GetPixel(x - healingSizeVariated, y);
                                pixelColorBrightness = pixelColor.GetBrightness();

                                if (pixelColorBrightness > shadowBrightness)
                                    outputImage.SetPixel(x, y, pixelColor);
                                {
                                    // TOP-RIGHT ==> BOTTOM-LEFT
                                    pixelColor = outputImage.GetPixel(x - healingSizeVariated, y + healingSizeVariated);
                                    pixelColorBrightness = pixelColor.GetBrightness();

                                    if (pixelColorBrightness > shadowBrightness)
                                        outputImage.SetPixel(x, y, pixelColor);
                                    else
                                    {
                                        // BOTTOM-RIGHT ==> TOP-LEFT
                                        pixelColor = outputImage.GetPixel(x - healingSizeVariated, y - healingSizeVariated);
                                        pixelColorBrightness = pixelColor.GetBrightness();

                                        if (pixelColorBrightness > shadowBrightness)
                                            outputImage.SetPixel(x, y, pixelColor);
                                    }
                                }
                            }

                            // TOP ROW
                            if (y <= healingSizeVariated)
                            {
                                // TOP ==> BOTTOM
                                pixelColor = outputImage.GetPixel(x, y + healingSizeVariated);
                                pixelColorBrightness = pixelColor.GetBrightness();

                                if (pixelColorBrightness > shadowBrightness)
                                    outputImage.SetPixel(x, y, pixelColor);
                                else
                                {
                                    // TOP-LEFT ==> BOTTOM-RIGHT
                                    pixelColor = outputImage.GetPixel(x + healingSizeVariated, y + healingSizeVariated);
                                    pixelColorBrightness = pixelColor.GetBrightness();

                                    if (pixelColorBrightness > shadowBrightness)
                                        outputImage.SetPixel(x, y, pixelColor);
                                    else
                                    {
                                        // TOP-RIGHT ==> BOTTOM-LEFT
                                        pixelColor = outputImage.GetPixel(x - healingSizeVariated, y + healingSizeVariated);
                                        pixelColorBrightness = pixelColor.GetBrightness();

                                        if (pixelColorBrightness > shadowBrightness)
                                            outputImage.SetPixel(x, y, pixelColor);
                                    }
                                }
                            }

                            // BOTTOM ROW
                            if (y >= imageHeight - healingSizeVariated)
                            {
                                // BOTTOM ==> TOP
                                pixelColor = outputImage.GetPixel(x, y - healingSizeVariated);
                                pixelColorBrightness = pixelColor.GetBrightness();

                                if (pixelColorBrightness > shadowBrightness)
                                    outputImage.SetPixel(x, y, pixelColor);
                                else
                                {
                                    // BOTTOM-LEFT ==> TOP-RIGHT
                                    pixelColor = outputImage.GetPixel(x + healingSizeVariated, y - healingSizeVariated);
                                    pixelColorBrightness = pixelColor.GetBrightness();

                                    if (pixelColorBrightness > shadowBrightness)
                                        outputImage.SetPixel(x, y, pixelColor);
                                    else
                                    {
                                        // BOTTOM-RIGHT ==> TOP-LEFT
                                        pixelColor = outputImage.GetPixel(x - healingSizeVariated, y - healingSizeVariated);
                                        pixelColorBrightness = pixelColor.GetBrightness();

                                        if (pixelColorBrightness > shadowBrightness)
                                            outputImage.SetPixel(x, y, pixelColor);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (currentPixelbrightness < shadowBrightness * 1.25f)
                        {
                            pixelColor = Src.GetPixel(x - healingSizeVariated, y);
                            pixelColorBrightness = pixelColor.GetBrightness();

                            if (pixelColorBrightness > shadowBrightness)
                                outputImage.SetPixel(x, y, pixelColor);
                        }
                        else
                        {
                            outputImage.SetPixel(x, y, color);
                            //lastBrightPixel = color;
                        }
                    }

                    progressShadow = (float)TUtils.InverseLerp(0f, (float)(imageHeight * 3), (float)(y + (imageHeight * 2)));
                  //  progressShadowStage = 3;
                }
            }

            return outputImage;
        }

        public static TMask GetMaskMap(TMap map, Bitmap bitmap, Color color, int tolerance, TMask orgMask = null)
        {
            if (bitmap == null) return new TMask(32, 32);
            Vector2 resulotion = new Vector2(bitmap.Width, bitmap.Height);
            TMask result = new TMask((int)resulotion.X, (int)resulotion.Y, true);

            if (orgMask != null)
                result.AND(orgMask);

            result = FilteredMask(result, bitmap, color, tolerance);
            return result;
        }

        public static TMask FilteredMask(TMask srcMask, Bitmap bitmap, Color selectedColor , int tolerance)
        {
            TMask result = srcMask.Clone();

            for (int i = 0; i < result.Width; i++)
                for (int j = 0; j < result.Height; j++)
                    if (result.Check(i, j))
                    {
                        double latNormalized = i * 1.0d / result.Width;
                        double lonNormalized = j * 1.0d / result.Height;
                        int pixelIndexI = (int)(latNormalized * bitmap.Width);
                        int pixelIndexJ = (int)((lonNormalized) * bitmap.Height);

                        Color color = bitmap.GetPixel(pixelIndexI, bitmap.Height-pixelIndexJ-1);

                        if (!IsColorMatch(selectedColor, color, tolerance))
                        {
                            result.Clear(i, j);
                            continue;
                        }
                    }

            return result;
        }


        // Utils
        //----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Bitmap ResizeBitmap (Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(sourceBMP, 0, 0, width, height);
            }

            return result;
        }


        public static Color AverageColor (Color color1, Color color2, bool useAlpha = false)
        {
            int R = (color1.R + color2.R) / 2;
            int G = (color1.G + color2.G) / 2;
            int B = (color1.B + color2.B) / 2;
            int A = 255;

            if (useAlpha)
                A = (color1.A + color2.A) / 2;

            return Color.FromArgb(A, R, G, B);
        }

        public static float Lerp (float src, float dst, float t)
        {
            return src + t * (dst - src);
        }

        public static int ClampChannel (int value)
        {
            return (int)Math.Max(Math.Min(value, 255), 0);
        }

        public static Bitmap ResetResolution(Bitmap scr, int resolution)
        {
            if (scr == null) return null;
            // create filter
            ResizeNearestNeighbor filter = new ResizeNearestNeighbor(resolution, resolution);
            // apply the filter
            Bitmap newImage = filter.Apply(scr);
            return newImage;
        }
    }
}

#endif
#endif