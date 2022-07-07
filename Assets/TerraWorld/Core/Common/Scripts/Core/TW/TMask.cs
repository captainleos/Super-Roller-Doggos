#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TerraUnity.Edittime
{
    public class TMask
    {
        public float[,] _maskData;
        private float _maskNullValue = -1 * float.Epsilon;
        private Bitmap _bitmapMask = null;
        //private int _tolerance;

        public float[,] MaskData { get => _maskData; set => _maskData = value; }
        public int Height { get => _maskData.GetLength(1); }
        public int Width { get => _maskData.GetLength(0); }

        public List<TPolygonMask> polygonsMask;
        public float minHeight;

        public TMask(int width, int height, bool FullFill = false)
        {
            _maskData = new float[width, height];
            FullClear();
            if (FullFill)
                fulfill();

            this.polygonsMask = new List<TPolygonMask>();
            this.minHeight = 0f;
        }

        public TMask Clone()
        {
            TMask result = new TMask(1, 1);
            result.MaskData = _maskData.Clone() as float[,];
            return result;
        }

        public Bitmap GetGrayImage()
        {
            float min, max;
            min = max = 0;
            THeightmapProcessors.GetMinMaxElevationFromHeights(_maskData, out min, out max);
            if (min == max && max == _maskNullValue) return new Bitmap(16, 16); ;
            if (min == max ) min = _maskNullValue;
            _bitmapMask = new Bitmap(_maskData.GetLength(0), _maskData.GetLength(1));

            for (int i = 0; i < _maskData.GetLength(0); i++)
            {
                for (int j = 0; j < _maskData.GetLength(1); j++)
                {
                    int pixelValue = (int)(((_maskData[i, (_maskData.GetLength(1) - 1) - j] - min) / (max - min)) * 255);
                    System.Drawing.Color grayscale = new System.Drawing.Color();
                    grayscale = System.Drawing.Color.FromArgb(255, pixelValue, pixelValue, pixelValue);
                    _bitmapMask.SetPixel(i, j, grayscale);
                }
            }

            return _bitmapMask;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                    if (_maskData[i, j] != _maskNullValue) return false;

            return true;
        }

        public Bitmap GetColoredImage(System.Drawing.Color color)
        {
            Bitmap Dest = new Bitmap(Width, Height);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Check(i, j))
                        Dest.SetPixel(i, Height - j - 1, color);
                    else
                        Dest.SetPixel(i, Height - j - 1, System.Drawing.Color.Black);
                }
            }

            return Dest;
        }

        public Bitmap GetBWImage ()
        {
            return GetColoredImage(System.Drawing.Color.White);
        }

        public float GetValue(int x, int y)
        {
            if (_maskData[x, y] == _maskNullValue) return 0; else return _maskData[x, y];
        }

        public float GetValue(double normilizedX, double normalizedY)
        {
            if (normilizedX >= 1 || normilizedX < 0 || normalizedY >= 1 || normalizedY < 0) throw new Exception("Range Error!");
            int x = (int)Math.Floor(normilizedX * (_maskData.GetLength(0)));
            int y = (int)Math.Floor(normalizedY * (_maskData.GetLength(1)));

            return GetValue(x, y);
        }

        public bool CheckNormal(double normilizedX, double normalizedY)
        {
            if (normilizedX >= 1 || normilizedX < 0 || normalizedY >= 1 || normalizedY < 0)
                throw new Exception("Range Error!");

            int x = (int)Math.Floor(normilizedX * (_maskData.GetLength(0)));
            int y = (int)Math.Floor(normalizedY * (_maskData.GetLength(1)));

            return Check(x, y);
        }

        public bool Check(int x, int y)
        {
            if (x >= Width || y >= Height)
                throw new Exception("Range Error!");

            if (_maskData[x, y] == _maskNullValue)
                return false;
            else
                return true;
        }

        public void SetValue (int x, int y, float vaule)
        {
            if (x >= Width || y >= Height)
                throw new Exception("Range Error!");
            else if (x < 0 || y < 0) throw new Exception("Range Error!");
            else
            {
                _maskData[x, y] = vaule;
                if (vaule == _maskNullValue) _maskData[x, y] = 0;
            }
        }

        public void SetNormal(double normilizedX, double normalizedY , float vaule = 1)
        {
            if (normilizedX >= 1 || normilizedX < 0 || normalizedY >= 1 || normalizedY < 0) throw new Exception("Range Error!");
            int x = (int)Math.Floor(normilizedX * (_maskData.GetLength(0)));
            int y = (int)Math.Floor(normalizedY * (_maskData.GetLength(1)));

            SetValue(x, y, vaule);
        }

        public void Clear(int x, int y)
        {
            _maskData[x, y] = _maskNullValue;
        }

        public void fulfill()
        {
            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                    SetValue(i, j,1);
        }

        public void FullClear()
        {
            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                    Clear(i, j);
        }

        public void ResizeMaskData(int width, int length)
        {
            int w = _maskData.GetLength(0);
            int h = _maskData.GetLength(1);
            if (width == w && length == h) return;
            double scaleFactorWidth = (float)width / w;
            double scaleFactorHeight = (float)length / h;
            float[,] Dest = new float[width, length];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    double normalizedX = x / scaleFactorWidth;
                    double normalizedY = y / scaleFactorHeight;
                    int indexX = (int)(normalizedX + (float)w % w);
                    int indexY = (int)(normalizedY + (float)h % h);
                    Dest[x, y] = _maskData[indexX, indexY];
                }
            }

            _maskData = Dest;
        }

        public TMask FilteredMask(float minRange, float maxRange)
        {
            if (minRange == 0 && maxRange == 1) return this;
            TMask result = new TMask(Width, Height);
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                {
                    if (minValue > _maskData[i, j])
                        minValue = _maskData[i, j];

                    if (maxValue < _maskData[i, j])
                        maxValue = _maskData[i, j];
                }

            float range = maxValue - minValue;
            float floor = minValue + (minRange * range);
            float ceil = minValue + (maxRange * range);

            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                {
                    if (_maskData[i, j] >= floor && _maskData[i, j] <= ceil)
                        result.MaskData[i, j] = _maskData[i, j];
                }

            return result;
        }

        public void AND(TMask otherMask, int count = 0, int k = 0, TNode parentNode = null)
        {
            if (otherMask == null) return;
            double normalI;
            double normalJ;

            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                {
                    normalI = (i * 1.0d / _maskData.GetLength(0));
                    normalJ = (j * 1.0d / _maskData.GetLength(1));

                    if (!otherMask.CheckNormal(normalI, normalJ))
                        _maskData[i, j] = _maskNullValue;

                    if (parentNode != null)
                        parentNode._progress = ((i + _maskData.GetLength(0) * j) * ((float)k + 1)) / ((_maskData.GetLength(1) + 1) * (j + 1) * ((float)count));
                }
        }

        public void OR(TMask otherMask)
        {
            if (otherMask == null) return;
            double normalI;
            double normalJ;

            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                {
                    normalI = (i * 1.0d / _maskData.GetLength(0));
                    normalJ = (j * 1.0d / _maskData.GetLength(1));

                    if (otherMask.CheckNormal(normalI, normalJ))
                        SetValue(i, j, otherMask.GetValue(normalI, normalJ));
                    
                }
        }

        public void Subtract(TMask otherMask)
        {
            if (otherMask == null) return;
            double normalI;
            double normalJ;

            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                {
                    normalI = (i * 1.0d / _maskData.GetLength(0));
                    normalJ = (j * 1.0d / _maskData.GetLength(1));

                    if (otherMask.CheckNormal(normalI, normalJ))
                        _maskData[i, j] = _maskNullValue;
                }
        }

        public void XOR(TMask otherMask)
        {
            if (otherMask == null) return;
            double normalI;
            double normalJ;

            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                {
                    normalI = (i * 1.0d / _maskData.GetLength(0));
                    normalJ = (j * 1.0d / _maskData.GetLength(1));

                    if (otherMask.CheckNormal(normalI, normalJ) && Check(i, j))
                        Clear(i, j);
                    else if (!otherMask.CheckNormal(normalI, normalJ) && !Check(i, j))
                        Clear(i, j);
                    else if (!Check(i, j))
                        SetValue (i, j, 1);
                }
        }

        public void Inverse()
        {
            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                {
                    if (Check(i, j))
                        Clear(i, j);
                    else
                        SetValue(i, j, 1);
                }
        }


        public static TMask MergeMasksOld(List<TMask> Masks, TNode parentNode = null)
        {
            int biggestWidth = 0;
            int biggestLength = 0;
            if (Masks == null || Masks.Count == 0) return null;

            if (Masks.Count == 1)
            {
                if (Masks[0] == null)
                    return new TMask(32, 32);
                else
                    return Masks[0].Clone();
            }

            for (int i = 0; i < Masks.Count; i++)
            {
                if (biggestWidth < Masks[i].Width) biggestWidth = Masks[i].Width;
                if (biggestLength < Masks[i].Height) biggestLength = Masks[i].Height;
            }

            TMask result = new TMask(biggestWidth, biggestLength);

            //   for (int i = 0; i < biggestWidth; i++)
            //       for (int j = 0; j < biggestLength; j++)
            //       {
            //           float NormalI = i * 1.0f / biggestWidth;
            //           float NormalJ = j * 1.0f / biggestLength;
            //
            //               TMask mask = Masks[0];
            //               if (mask.CheckNormal(NormalI, NormalJ))
            //               {
            //                   result.SetValue(i, j, mask.GetValue(NormalI, NormalJ));
            //               }
            //           
            //       }
            //
            //  for (int i = 0; i < biggestWidth; i++)
            //      for (int j = 0; j < biggestLength; j++)
            //      {
            //          TMask mask = Masks[0];
            //          if (mask.MaskData[i,j] != result.MaskData[i, j] )
            //          {
            //              float old = mask.MaskData[i, j];
            //              float old2 = result.MaskData[i, j];
            //              float old3 = old - old2;
            //              throw new Exception("Error");
            //          }
            //
            //      }

            for (int i = 0; i < biggestWidth; i++)
                for (int j = 0; j < biggestLength; j++)
                {
                    double NormalI = i * 1.0d / biggestWidth;
                    double NormalJ = j * 1.0d / biggestLength;

                    for (int k = 0; k < Masks.Count; k++)
                    {
                        TMask mask = Masks[k];

                        if (mask.CheckNormal(NormalI, NormalJ))
                            result.SetValue(i, j, mask.GetValue(NormalI, NormalJ));

                        if (parentNode != null)
                            parentNode._progress = ((i + biggestLength * j) * ((float)k + 1)) / ((biggestWidth + 1) * (j + 1) * ((float)Masks.Count));
                    }
                }

            return result;
        }

        public static TMask MergeMasks(List<TMask> Masks, TNode parentNode = null)
        {
            if (Masks == null || Masks.Count == 0) return null;
            int biggestWidth = 0;
            int biggestLength = 0;

            if (Masks.Count == 1)
            {
                if (Masks[0] == null)
                    return new TMask (32, 32);
                else
                    return Masks[0].Clone();
            }

            for (int i = 0; i < Masks.Count; i++)
            {
                if (biggestWidth < Masks[i].Width) biggestWidth = Masks[i].Width;
                if (biggestLength < Masks[i].Height) biggestLength = Masks[i].Height;
            }

            TMask result = new TMask(biggestWidth, biggestLength);

            for (int k = 0; k < Masks.Count; k++)
            {
                TMask mask = Masks[k];
                result.OR(mask);

                if (parentNode != null)
                    parentNode._progress = k/ Masks.Count;
            }

            return result;
        }

        public static TMask AND(List<TMask> Masks, TNode parentNode = null)
        {
            if (Masks.Count == 0) return null;
            int biggestWidth = 0;
            int biggestLength = 0;

            for (int i = 0; i < Masks.Count; i++)
            {
                if (biggestWidth < Masks[i].Width) biggestWidth = Masks[i].Width;
                if (biggestLength < Masks[i].Height) biggestLength = Masks[i].Height;
            }

            TMask result = new TMask(biggestWidth, biggestLength,true);

            for (int k = 0; k < Masks.Count; k++)
            {
                TMask mask = Masks[k];
                result.AND(mask, Masks.Count, k, parentNode);
            }

            return result;
        }

        public static TMask Inverse(List<TMask> Masks)
        {
            if (Masks.Count == 0) return null;
            int biggestWidth = 0;
            int biggestLength = 0;

            for (int i = 0; i < Masks.Count; i++)
            {
                if (biggestWidth < Masks[i].Width) biggestWidth = Masks[i].Width;
                if (biggestLength < Masks[i].Height) biggestLength = Masks[i].Height;
            }

            TMask result = new TMask(biggestWidth, biggestLength);

            for (int k = 0; k < Masks.Count; k++)
            {
                TMask mask = Masks[k];
                result.OR(mask);
            }

            result.Inverse();
            return result;
        }

        public static TMask Exaggerate(TMask mask , int edge)
        {
            if (mask == null) return null;

            TMask result = new TMask(mask.Width, mask.Height);

            for (int x = 0; x < mask.Width; x++)
                for (int y = 0; y < mask.Height ; y++)
                    for (int i = -edge; i < edge + 1; i++)
                        for (int j = -edge; j < edge + 1 ; j++)
                        {
                            if ((x + i) < 0 || (y + j) < 0  ) continue;
                            if ((x + i) >= mask.Width || (y + j) >= mask.Height) continue;
                            if (mask.Check(x+i, y+j))
                                result.SetValue(x, y, 1);
                        }

            return result;
        }

        public static TMask XOR(List<TMask> Masks)
        {
            if (Masks.Count == 0) return null;
            int biggestWidth = 0;
            int biggestLength = 0;

            for (int i = 0; i < Masks.Count; i++)
            {
                if (biggestWidth < Masks[i].Width) biggestWidth = Masks[i].Width;
                if (biggestLength < Masks[i].Height) biggestLength = Masks[i].Height;
            }

            TMask result = new TMask(biggestWidth, biggestLength);

            for (int k = 0; k < Masks.Count; k++)
            {
                TMask mask = Masks[k];
                result.XOR(mask);
            }

            return result;
        }

        public static TMask Subtract(List<TMask> Masks)
        {
            if (Masks.Count < 1) return null;
            if (Masks.Count < 2) return Masks[0].Clone();
            TMask result = Masks[0].Clone();

            for (int k = 1; k < Masks.Count; k++)
            {
                TMask mask = Masks[k];
                result.Subtract(mask);
            }

            return result;
        }

        public void FilteredMask(float topNormal, float bottomNormal, float leftNormal, float rightNormal)
        {
            for (int i = 0; i < _maskData.GetLength(0); i++)
                for (int j = 0; j < _maskData.GetLength(1); j++)
                {
                    double YNormalized = j * 1.0d / _maskData.GetLength(1);
                    double XNormalized = i * 1.0d / _maskData.GetLength(0);

                    if (XNormalized > rightNormal || XNormalized < leftNormal || YNormalized > topNormal || YNormalized < bottomNormal)
                        _maskData[i, j] = _maskNullValue;
                }
        }

        public void ReplaceValue(float oldValue, float newValue, int widthBound = -1, int heightBound = -1)
        {
            if (widthBound == -1) widthBound = Width;
            if (heightBound == -1) heightBound = Height;

            for (int j = 0; j < heightBound; j++)
                for (int i = 0; i < widthBound; i++)
                    if (_maskData[i, j] == oldValue)
                        _maskData[i, j] = newValue;
        }

        public TMask GetGroupedMaskByContiguous(out int groupCount)
        {
            groupCount = 1;
            int groupIndex = -1;
            TMask result = new TMask(Width, Height);

            for (int j = 0; j < Height; j++)
            {
                groupIndex = -1;
                for (int i = 0; i < Width; i++)
                {
                    groupIndex = -1;
                    if (_maskData[i, j] == _maskNullValue) 
                        groupIndex = 0;
                    else
                    {
                        if (i > 0 && j > 0)
                        {
                            int LIndex = (int)result.GetValue(i - 1, j);
                            int BIndex = (int)result.GetValue(i, j - 1);
                            //int BLIndex = (int)result.GetValue(i - 1, j-1);

                            if ((LIndex != BIndex) && (LIndex != 0) && (BIndex != 0)) result.ReplaceValue(LIndex, BIndex,i,j);

                            if (result.GetValue(i - 1, j) != 0) groupIndex = (int)result.GetValue(i - 1, j);
                            else if (result.GetValue(i, j - 1) != 0) groupIndex = (int)result.GetValue(i, j - 1);
                            else if (result.GetValue(i - 1, j - 1) != 0) groupIndex = (int)result.GetValue(i - 1, j - 1);
                        }

                        if (i == 0 && j > 0)
                            if (result.GetValue(i, j - 1) != 0) groupIndex = (int)result.GetValue(i, j - 1);

                        if (i > 0 && j == 0)
                            if (result.GetValue(i - 1, j) != 0) groupIndex = (int)result.GetValue(i - 1, j);

                        if (i == 0 && j == 0)
                        {
                            if (_maskData[i, j] == _maskNullValue)
                                groupIndex = 0;
                            else
                                groupIndex = 1;
                        }
                    }

                    if (groupIndex == -1)
                    {
                        groupCount++;
                        groupIndex = groupCount;
                    }

                    result.SetValue(i, j, groupIndex);
                }
            }

            return result;
        }

        public Texture2D GetTexture(string fileName)
        {
            Bitmap filterMask = GetBWImage();
            TImage filterImage = new TImage(filterMask);
            return filterImage.GetTexture(fileName);
        }

        public Texture2D GetTextureRGBA(string fileName)
        {
            Bitmap filterMask = GetBWImage();
            TImage filterImage = new TImage(filterMask);
            return filterImage.GetTextureRGBA(fileName);
        }
    }
}
#endif
#endif

