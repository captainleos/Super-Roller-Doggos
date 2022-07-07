#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
    public static class THeightmapProcessors
    {
        public enum Neighbourhood { Moore = 0, VonNeumann = 1 }
        public enum ASPECT_TYPE { ASPECT, NORTHERNESS, EASTERNESS };
        public enum CURVATURE_TYPE { HORIZONTAL, VERTICAL, AVERAGE };
        public enum ResampleMode { UP, DOWN };

        public static float[,] NormalizeHeightmap(float[,] Src, float minElevation = -500, float maxElevation = 8848)
        {
            //minElevation *= TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration;
            //maxElevation *= TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration;

            float[,] heightmap = Src.Clone() as float[,];

            for (int i = 0; i < heightmap.GetLength(0); i++)
                for (int j = 0; j < heightmap.GetLength(1); j++)
                    heightmap[i, j] = (Src[i, j] - minElevation) / (maxElevation - minElevation);
                    //heightmap[i, j] = ((Src[i, j] - minElevation) / (maxElevation - minElevation)) - (Math.Abs(minElevation) / (Math.Abs(minElevation) + maxElevation));

            return heightmap;
        }

        public static float[,] DenormalizeHeightmap(float[,] Src, float minElevation = -500, float maxElevation = 8848)
        {
            //  minElevation *= TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration;
            //  maxElevation *= TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource().elevationExaggeration;

            float[,] heightmap = Src.Clone() as float[,];

            for (int i = 0; i < heightmap.GetLength(0); i++)
                for (int j = 0; j < heightmap.GetLength(1); j++)
                    heightmap[i, j] = heightmap[i, j] * (maxElevation - minElevation) + minElevation;

            return heightmap;
        }

        public static void HeightmapCenterCarve(ref float[,] heightmap, int fromCenterX, int fromCenterY, float carveValue = -1000f, int padding = 1)
        {
            for (int i = (heightmap.GetLength(0) / 2 - fromCenterX / 2) + padding; i < (heightmap.GetLength(0) / 2 + fromCenterX / 2) - padding; i++)
                for (int j = (heightmap.GetLength(1) / 2 - fromCenterY / 2) + padding; j < (heightmap.GetLength(1) / 2 + fromCenterY / 2) - padding; j++)
                    heightmap[i, j] = carveValue;
        }

        public static void GetMinMaxElevationFromHeights(float[,] heightmap, out float minElevation, out float maxElevation)
        {
            float? min = null;
            float? max = null;

            foreach (float v in heightmap)
            {
                min = (min.HasValue) ? Math.Min(min.Value, v) : v;
                max = (max.HasValue) ? Math.Max(max.Value, v) : v;
            }

            minElevation = (float)min;
            maxElevation = (float)max;
        }

        public static void GetMinMaxElevationFromHeights(float[] heightmap, out float? minElevation, out float? maxElevation)
        {
            float? min = null;
            float? max = null;

            foreach (float v in heightmap)
            {
                min = (min.HasValue) ? Math.Min(min.Value, v) : v;
                max = (max.HasValue) ? Math.Max(max.Value, v) : v;
            }

            minElevation = min;
            maxElevation = max;
        }

        public static float[,] CropHeightmap(TArea baseArea, TArea cropArea, float[,] Src)
        {
            int w = Src.GetLength(0);
            int h = Src.GetLength(1);
            int newH, newW, offsetX, offsetY;
            newH = newW = offsetX = offsetY = 0;
            TUtils.GeoToPixelCrop(baseArea, cropArea, w, h, out newW, out newH, out offsetX, out offsetY);
            float[,] Dest = new float[newW, newH];

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (x >= offsetX && x < offsetX + newW)
                        if (y >= (h - offsetY - newH) && y < (h - offsetY - newH) + newH)
                            Dest[x - offsetX, y - (h - offsetY - newH)] = Src[x, y];

            return Dest;
        }

        public static float[,] ResampleHeightmap(float[,] Src, ResampleMode resampleMode = ResampleMode.DOWN, int targetResolution = 0)
        {
            int w = Src.GetLength(0);
            int h = Src.GetLength(1);
            int resolution = 0;

            if (targetResolution == Src.GetLength(0))
                return Src.Clone() as float[,];

            if (targetResolution != 0)
                resolution = targetResolution;
            else if (resampleMode.Equals(ResampleMode.UP))
            {
                if (w >= h)
                    resolution = (int)Math.Pow(2, Math.Ceiling(Math.Log(w) / Math.Log(2))) + 1;
                else
                    resolution = (int)Math.Pow(2, Math.Ceiling(Math.Log(h) / Math.Log(2))) + 1;
            }
            else if (resampleMode.Equals(ResampleMode.DOWN))
            {
                if (w >= h)
                    resolution = (int)(Math.Pow(2, Math.Ceiling(Math.Log(w) / Math.Log(2))) / 2) + 1;
                else
                    resolution = (int)(Math.Pow(2, Math.Ceiling(Math.Log(h) / Math.Log(2))) / 2) + 1;
            }

            if (resolution < 33)
                resolution = 33;
            else if (resolution > 8193)
                resolution = 8193;

            double scaleFactorWidth = (float)resolution / w;
            double scaleFactorHeight = (float)resolution / h;
            float[,] Dest = new float[resolution, resolution];

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    double normalizedX = x / scaleFactorWidth;
                    double normalizedY = y / scaleFactorHeight;
                    int indexX = (int)(normalizedX + (float)w % w);
                    int indexY = (int)(normalizedY + (float)h % h);
                    Dest[x, y] = Src[indexX, indexY];
                }
            }

            return Dest;
        }

        private static float GetInterpolateHeight(float H1, float H2, float H3, float H4, double normalDisXH1, double normalDisYH1)
        {
            double d1 = Math.Sqrt(normalDisXH1 * normalDisXH1 + normalDisYH1 * normalDisYH1);
            double d2 = Math.Sqrt((1 - normalDisXH1) * (1 - normalDisXH1) + normalDisYH1 * normalDisYH1);
            double d3 = Math.Sqrt(normalDisXH1 * normalDisXH1 + (1 - normalDisYH1) * (1 - normalDisYH1));
            double d4 = Math.Sqrt((1 - normalDisXH1) * (1 - normalDisXH1) + (1 - normalDisYH1) * (1 - normalDisYH1));
            double total_d = d1 + d2 + d3 + d4;
            //double resulte = H1 * (1 - (d1 / total_d)) + H2 * (1 - (d2 / total_d)) + H3 * (1 - (d3 / total_d)) + H4 * (1 - (d4 / total_d));

            double resulte = H1 * (1 - normalDisXH1) + H1 * (1 - normalDisYH1) + H2 * (normalDisXH1) + H2 * (1 - normalDisYH1) + H3 * (1 - normalDisXH1) + H1 * (normalDisYH1) + H4 * (normalDisXH1) + H4 * (normalDisYH1);
            resulte = resulte / 4;
            return (float)resulte;
        }

        //public static float[,] ResampleHeightmap2(float[,] Src, ResampleMode resampleMode = ResampleMode.DOWN, int targetResolution = 0)
        //{
        //    int w = Src.GetLength(0);
        //    int h = Src.GetLength(1);
        //    int resolution = 0;
        //
        //    if (targetResolution == Src.GetLength(0))
        //        return Src.Clone() as float[,];
        //
        //    if (targetResolution != 0)
        //        resolution = targetResolution;
        //    else if (resampleMode.Equals(ResampleMode.UP))
        //    {
        //        if (w >= h)
        //            resolution = (int)Math.Pow(2, Math.Ceiling(Math.Log(w) / Math.Log(2))) + 1;
        //        else
        //            resolution = (int)Math.Pow(2, Math.Ceiling(Math.Log(h) / Math.Log(2))) + 1;
        //    }
        //    else if (resampleMode.Equals(ResampleMode.DOWN))
        //    {
        //        if (w >= h)
        //            resolution = (int)(Math.Pow(2, Math.Ceiling(Math.Log(w) / Math.Log(2))) / 2) + 1;
        //        else
        //            resolution = (int)(Math.Pow(2, Math.Ceiling(Math.Log(h) / Math.Log(2))) / 2) + 1;
        //    }
        //
        //    if (resolution < 33)
        //        resolution = 33;
        //    else if (resolution > 8193)
        //        resolution = 8193;
        //
        //    //double scaleFactorX = resolution * 1.0d / w;
        //    //double scaleFactorY = resolution * 1.0d / h;
        //    float[,] Dest = new float[resolution, resolution];
        //
        //    for (int x = 0; x < resolution; x++)
        //    {
        //        for (int y = 0; y < resolution; y++)
        //        {
        //            double normalizedX = x * 1.0d / resolution;
        //            double normalizedY = y * 1.0d / resolution;
        //            int indexX = (int)(normalizedX * w);
        //            int indexY = (int)(normalizedX * h);
        //            double normalDisXH1 = normalizedX * w - Math.Floor(normalizedX * w);
        //            double normalDisYH1 = normalizedY * h - Math.Floor(normalizedY * h);
        //            float interpolateHeight;
        //
        //            if ((indexX + 1) < w && (indexY + 1) < h)
        //                interpolateHeight = GetInterpolateHeight(Src[indexX, indexY], Src[indexX + 1, indexY], Src[indexX, indexY + 1], Src[indexX + 1, indexY + 1], normalDisXH1, normalDisYH1);
        //            else
        //                interpolateHeight = GetInterpolateHeight(Src[indexX, indexY], Src[indexX, indexY], Src[indexX, indexY], Src[indexX, indexY], normalDisXH1, normalDisYH1);
        //
        //            Dest[x, y] = interpolateHeight;
        //        }
        //    }
        //
        //    return Dest;
        //}

        public static float[,] RotateHeightmap(float[,] Src)
        {
            int width = Src.GetLength(0);
            int height = Src.GetLength(1);
            float[,] Dest = new float[height, width];
            int newColumn, newRow = 0;

            for (int oldColumn = height - 1; oldColumn >= 0; oldColumn--)
            {
                newColumn = 0;

                for (int oldRow = 0; oldRow < width; oldRow++)
                {
                    Dest[newRow, newColumn] = Src[oldRow, oldColumn];
                    newColumn++;
                }

                newRow++;
            }

            return Dest;
        }

        public static float[,] ExaggerateHeightmap(float[,] Src, float verticalFactor)
        {
            int w = Src.GetLength(0);
            int h = Src.GetLength(1);
            float[,] Dest = new float[w, h];

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    Dest[x, y] = Src[x, y] * verticalFactor;

            return Dest;
        }

        public static void DeformByMask(ref float[,] heightmap, TMask mask, float depth, bool applyMaskData, TNode parentNode, int EdgeSize)
        {
            if (mask == null) return;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);
            if (EdgeSize < 0 || EdgeSize > (width / 3)) throw new Exception("Edge Size Error!");

            //float MinValue = 0;
            //float MaxValue = 0;
            //float AvgValue = 0;
            double correctionFactor = (mask.Width - 1) * 1.0d / mask.Width;
            double correctionWidth = correctionFactor / width;
            double correctionLength = correctionFactor / length;
            //if (flat) GetminValue(ref heightmap, mask , out MinValue, out MaxValue, out AvgValue);
            double normalI = 0;
            double normalJ = 0;
            bool allInMask = false;

            for (int i = 1 + EdgeSize; i < width - EdgeSize; i++)
            {
                for (int j = 1 + EdgeSize; j < length - EdgeSize; j++)
                {
                    allInMask = AllInMask(i, j, out normalI, out normalJ, mask, correctionWidth, correctionLength, EdgeSize);

                    if (allInMask)
                    {
                        if (applyMaskData)
                            //heightmap[i, j] = MinValue - depth ;
                            heightmap[i, j] = mask.GetValue(normalJ, normalI) - depth;
                        else
                            heightmap[i, j] = heightmap[i, j] - depth;
                    }

                    if (parentNode != null)
                        parentNode._progress = (float)(i + width * j) / ((length + 1) * (j + 1));
                }
            }
        }

        public static bool AllInMask(int i, int j, out double normalI, out double normalJ, TMask mask, double correctionWidth, double correctionLength, int EdgeSize)
        {
            bool allInMask = false;
            normalI = (float)(i) * correctionWidth;
            normalJ = (float)(j) * correctionLength;
            allInMask = mask.CheckNormal(normalJ, normalI);

            if (allInMask && EdgeSize > 0)
            {
                normalI = (float)(i - EdgeSize) * correctionWidth;
                normalJ = (float)(j + EdgeSize) * correctionLength;
                allInMask = allInMask && mask.CheckNormal(normalJ, normalI);

                if (allInMask)
                {
                    normalI = (float)(i) * correctionWidth;
                    normalJ = (float)(j + EdgeSize) * correctionLength;
                    allInMask = allInMask && mask.CheckNormal(normalJ, normalI);

                    if (allInMask)
                    {
                        normalI = (float)(i + EdgeSize) * correctionWidth;
                        normalJ = (float)(j + EdgeSize) * correctionLength;
                        allInMask = allInMask && mask.CheckNormal(normalJ, normalI);

                        if (allInMask)
                        {
                            normalI = (float)(i - EdgeSize) * correctionWidth;
                            normalJ = (float)(j) * correctionLength;
                            allInMask = allInMask && mask.CheckNormal(normalJ, normalI);

                            if (allInMask)
                            {
                                normalI = (float)(i + EdgeSize) * correctionWidth;
                                normalJ = (float)(j) * correctionLength;
                                allInMask = allInMask && mask.CheckNormal(normalJ, normalI);

                                if (allInMask)
                                {
                                    normalI = (float)(i - EdgeSize) * correctionWidth;
                                    normalJ = (float)(j - EdgeSize) * correctionLength;
                                    allInMask = allInMask && mask.CheckNormal(normalJ, normalI);

                                    if (allInMask)
                                    {
                                        normalI = (float)(i) * correctionWidth;
                                        normalJ = (float)(j - EdgeSize) * correctionLength;
                                        allInMask = allInMask && mask.CheckNormal(normalJ, normalI);

                                        if (allInMask)
                                        {
                                            normalI = (float)(i + EdgeSize) * correctionWidth;
                                            normalJ = (float)(j - EdgeSize) * correctionLength;
                                            allInMask = allInMask && mask.CheckNormal(normalJ, normalI);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return allInMask;
        }

        public static void DeformWaterByMask(ref float[,] heightMap, TMask mask, bool applyMaskData, TNode parentNode, float EdgeSizeInMeter, float WorldSizeKMLat, float DeformAngle, out float MinHeight, out float MaxHeight, out float AvgHeight)
        {
            MinHeight = float.MaxValue;
            MaxHeight = float.MinValue;
            AvgHeight = 0;
            int nodeCount = 0;
            if (mask == null) return;
            float depth;
            int EdgeSize;
            int width = heightMap.GetLength(0);
            int length = heightMap.GetLength(1);

            //EdgeSize = (int)(EdgeSizeInMeter * (float)(width - 1) / (WorldSizeKMLat * 1000f));
            EdgeSize = (int)EdgeSizeInMeter;
            if (EdgeSize < 0 || EdgeSize > (width / 3)) throw new Exception("Edge Size Error!");

            double deformAngle;
            double meterPerPixel = WorldSizeKMLat * 1000f / (width - 1);
            double correctionFactor = (mask.Width - 1) * 1.0d / mask.Width;
            double correctionWidth = correctionFactor / width;
            double correctionLength = correctionFactor / length;
            double normalI = 0;
            double normalJ = 0;
            bool allInMask = false;
            float minDistance = 0f;

            for (int i = 1 + EdgeSize; i < width - EdgeSize; i++)
            {
                for (int j = 1 + EdgeSize; j < length - EdgeSize; j++)
                {
                    allInMask = AllInMask(i, j, out normalI, out normalJ, mask, correctionWidth, correctionLength, EdgeSize);

                    if (allInMask)
                    {
                        depth = 0;

                        if (mask.polygonsMask.Count > 0)
                        {
                            Vector3 point = new Vector3(j, 0, i);

                            if (FindNearestPolygonPoint(mask, point, ref minDistance))
                            {
                                if (minDistance * meterPerPixel < 20)
                                    deformAngle = DeformAngle * minDistance * meterPerPixel / 20.0;
                                else
                                    deformAngle = DeformAngle;

                                depth = minDistance * (float)meterPerPixel * (float)Math.Tan(deformAngle);
                            }
                        }

                        if (applyMaskData)
                            heightMap[i, j] = mask.GetValue(normalJ, normalI) - depth;
                        else
                            heightMap[i, j] = heightMap[i, j] - depth;

                        if (MinHeight > heightMap[i, j]) MinHeight = heightMap[i, j];
                        if (MaxHeight < heightMap[i, j]) MaxHeight = heightMap[i, j];
                        AvgHeight = (AvgHeight * nodeCount + heightMap[i, j]) / (nodeCount + 1);
                        nodeCount++;
                    }

                    if (parentNode != null)
                        parentNode._progress = (float)(i + width * j) / ((length + 1) * (j + 1));
                }
            }
        }

        public static void SmoothWaterByMask(ref float[,] heightMap, TMask mask, float WorldSizeKMLat, int EdgeSizeInMeter = 0, int iterations = 1)
        {
            if (mask == null) return;
            int iter_count, EdgeSize;
            int xNeighbours, yNeighbours;
            int xShift, yShift;
            int xIndex, yIndex;
            int width = heightMap.GetLength(0);
            int length = heightMap.GetLength(1);
            EdgeSize = (int)(EdgeSizeInMeter * (float)(width - 1) / (WorldSizeKMLat * 1000f));
            double correctionFactor = (mask.Width - 1) * 1.0d / mask.Width;
            double correctionWidth = correctionFactor / width;
            double correctionLength = correctionFactor / length;
            double normalI, normalJ;
            bool oneInMask;

            for (iter_count = 0; iter_count < iterations; iter_count++)
            {
                for (int i = 1 + EdgeSize; i < width - EdgeSize; i++)
                {
                    Neighbours_Shift_Index(i, width, out xNeighbours, out xShift, out xIndex);

                    for (int j = 1 + EdgeSize; j < length - EdgeSize; j++)
                    {
                        oneInMask = OneInMask(i, j, out normalI, out normalJ, mask, correctionWidth, correctionLength, EdgeSize);

                        if (oneInMask)
                        {
                            Neighbours_Shift_Index(j, length, out yNeighbours, out yShift, out yIndex);
                            SmoothPointHeight(ref heightMap, i, j, xNeighbours, yNeighbours, xShift, yShift, xIndex, yIndex);
                        }
                    }
                }
            }
        }

        public static bool OneInMask(int i, int j, out double normalI, out double normalJ, TMask mask, double correctionWidth, double correctionLength, int EdgeSize)
        {
            bool oneInMask;
            normalI = (float)(i) * correctionWidth;
            normalJ = (float)(j) * correctionLength;
            oneInMask = mask.CheckNormal(normalJ, normalI);

            if (!oneInMask && EdgeSize != 0)
            {
                normalI = (float)(i - EdgeSize) * correctionWidth;
                normalJ = (float)(j + EdgeSize) * correctionLength;
                oneInMask = mask.CheckNormal(normalJ, normalI);

                if (!oneInMask)
                {
                    normalI = (float)(i) * correctionWidth;
                    normalJ = (float)(j + EdgeSize) * correctionLength;
                    oneInMask = mask.CheckNormal(normalJ, normalI);

                    if (!oneInMask)
                    {
                        normalI = (float)(i + EdgeSize) * correctionWidth;
                        normalJ = (float)(j + EdgeSize) * correctionLength;
                        oneInMask = mask.CheckNormal(normalJ, normalI);

                        if (!oneInMask)
                        {
                            normalI = (float)(i - EdgeSize) * correctionWidth;
                            normalJ = (float)(j) * correctionLength;
                            oneInMask = mask.CheckNormal(normalJ, normalI);

                            if (!oneInMask)
                            {
                                normalI = (float)(i + EdgeSize) * correctionWidth;
                                normalJ = (float)(j) * correctionLength;
                                oneInMask = mask.CheckNormal(normalJ, normalI);

                                if (!oneInMask)
                                {
                                    normalI = (float)(i - EdgeSize) * correctionWidth;
                                    normalJ = (float)(j - EdgeSize) * correctionLength;
                                    oneInMask = mask.CheckNormal(normalJ, normalI);

                                    if (!oneInMask)
                                    {
                                        normalI = (float)(i) * correctionWidth;
                                        normalJ = (float)(j - EdgeSize) * correctionLength;
                                        oneInMask = mask.CheckNormal(normalJ, normalI);

                                        if (!oneInMask)
                                        {
                                            normalI = (float)(i + EdgeSize) * correctionWidth;
                                            normalJ = (float)(j - EdgeSize) * correctionLength;
                                            oneInMask = mask.CheckNormal(normalJ, normalI);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return oneInMask;
        }

        public static bool FindNearestPolygonPoint(TMask mask, Vector3 point, ref float minDistance)
        {
            int i, j;
            bool findMinDistancePointFlag = false;
            float distance;
            T2DPoint t2DPoint = new T2DPoint();
            T2DPoint pointIntersect = new T2DPoint();
            minDistance = float.MaxValue;

            for (i = 0; i < mask.polygonsMask.Count; i++)
            {
                for (j = 0; j < mask.polygonsMask[i].aroundPoints.Count - 1; j++)
                {
                    distance = (float)OSMParser.CalcDistanceTwoPoints(point.X, point.Z, mask.polygonsMask[i].aroundPoints[j].X,
                                                                                        mask.polygonsMask[i].aroundPoints[j].Z);
                    if (distance < minDistance)
                    {
                        findMinDistancePointFlag = true;
                        minDistance = distance;
                    }

                    /*t2DPoint.x = point.X;
                    t2DPoint.y = point.Z;
                    TLine line = new TLine(mask.polygonsMask[i].aroundPoints[j].X, mask.polygonsMask[i].aroundPoints[j].Z,
                                           mask.polygonsMask[i].aroundPoints[j + 1].X, mask.polygonsMask[i].aroundPoints[j + 1].Z);
                    distance = (float)line.CalcDistancePointFromLine(t2DPoint);
                    line.CalcDistancePointFromLine(t2DPoint);
                    if(line.CalcPerpendicularIntersection(t2DPoint, ref pointIntersect, distance))
                    {
                        if (distance < minDistance)
                        {
                            findMinDistancePointFlag = true;
                            minDistance = distance;
                        }
                    }*/
                }
            }

            return findMinDistancePointFlag;
        }

        public static void GetminValue(ref float[,] heightmap, TMask mask, out float MinHeight, out float MaxHeight, out float AvgHeight)
        {
            MinHeight = float.MaxValue;
            MaxHeight = float.MinValue;
            AvgHeight = 0;
            int nodeCount = 0;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);
            bool allInMask = false;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    allInMask = false;

                    if (mask != null)
                    {
                        double normalI = (float)(i) * 1.0d / width;
                        double normalJ = (float)(j) * 1.0d / length;
                        allInMask = mask.CheckNormal(normalJ, normalI);
                    }
                    else
                        allInMask = true;

                    if (allInMask)
                    {
                        if (MinHeight > heightmap[i, j]) MinHeight = heightmap[i, j];
                        if (MaxHeight < heightmap[i, j]) MaxHeight = heightmap[i, j];
                        AvgHeight = (AvgHeight * nodeCount + heightmap[i, j]) / (nodeCount + 1);
                        nodeCount++;
                    }
                }
            }
        }

        // Additional Processors
        //----------------------------------------------------------------------------------------------------------------------------------------------------
        public static float[,] SmoothHeightmap(float[,] SrcOriginal, int iterations = 1, float blending = 0.5f, Neighbourhood neighbourhood = Neighbourhood.Moore)
        {
            float[,] Src = SrcOriginal.Clone() as float[,];
            if (iterations == 0) return Src;
            int width = Src.GetLength(0);
            int height = Src.GetLength(1);

            if (blending > 0)
            {
                float[,] Dest = (float[,])Src.Clone();
                Dest = SmoothedHeights(Dest, width, height, iterations, neighbourhood);

                for (int Ty = 0; Ty < height; Ty++)
                {
                    for (int Tx = 0; Tx < width; Tx++)
                    {
                        float oldHeightAtPoint = Src[Tx, Ty];
                        float newHeightAtPoint = Dest[Tx, Ty];
                        float blendedHeightAtPoint = (newHeightAtPoint * blending) + (oldHeightAtPoint * (1.0f - blending));

                        Src[Tx, Ty] = blendedHeightAtPoint;
                    }
                }

                return Dest;
            }
            else
                return SmoothedHeights(Src, width, height, iterations, neighbourhood);
        }

        public static float[,] VoxelizeHeightmap(TNode parentNode, float[,] SrcOriginal, int voxelSize)
        {
            float[,] Src = SrcOriginal.Clone() as float[,];
            if (voxelSize == 0) return Src;
            int width = Src.GetLength(0);
            int height = Src.GetLength(1);
            float maxHeight = Src.Cast<float>().Max();
            float minHeight = Src.Cast<float>().Min();
            float[,] Dest = (float[,])Src.Clone();
            float normalizedSize = (float)voxelSize * (maxHeight - minHeight) / (float)width;
            int steps = (int)((maxHeight - minHeight) / normalizedSize);
            float[] blocks = new float[steps];

            for (int i = 0; i < steps; i++)
                blocks[i] = (float)TUtils.InverseLerp(0, steps, i);

            for (int x = 0; x < width; x += voxelSize)
            {
                for (int y = 0; y < height; y += voxelSize)
                {
                    if (x >= width || y >= height) continue;
                    float currentHeight = Dest[x, y];
                    int index = (int)(TUtils.InverseLerp(minHeight, maxHeight + 1, currentHeight) * (float)steps);
                    for (int i = 0; i < voxelSize; i++)
                    {
                        for (int j = 0; j < voxelSize; j++)
                        {
                            if (x + i >= width || y + j >= height) continue;
                            Dest[x + i, y + j] = blocks[index] * (maxHeight - minHeight) + minHeight;
                        }
                    }

                    if (parentNode != null)
                        parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                }
            }

            Array.Clear(blocks, 0, blocks.Length);
            blocks = null;

            return Dest;
        }

        private static float[,] SmoothedHeights(float[,] heightMap, int width, int height, int iterations, Neighbourhood neighbourhood)
        {
            int x, y, iter_count;
            int xNeighbours, yNeighbours;
            int xShift, yShift;
            int xIndex, yIndex;

            for (iter_count = 0; iter_count < iterations; iter_count++)
            {
                for (y = 0; y < height; y++)
                {
                    Neighbours_Shift_Index(y, height, out yNeighbours, out yShift, out yIndex);

                    for (x = 0; x < width; x++)
                    {
                        Neighbours_Shift_Index(x, width, out xNeighbours, out xShift, out xIndex);
                        SmoothPointHeight(ref heightMap, x, y, xNeighbours, yNeighbours, xShift, yShift, xIndex, yIndex, neighbourhood);
                    }
                }
            }

            return heightMap;
        }

        private static void Neighbours_Shift_Index(int l, int length, out int Neighbours, out int Shift, out int Index)
        {
            if (l == 0)
            {
                Neighbours = 2;
                Shift = 0;
                Index = 0;
            }
            else if (l == length - 1)
            {
                Neighbours = 2;
                Shift = -1;
                Index = 1;
            }
            else
            {
                Neighbours = 3;
                Shift = -1;
                Index = 1;
            }
        }

        private static void SmoothPointHeight(ref float[,] heightMap, int x, int y, int xNeighbours, int yNeighbours,
                                              int xShift, int yShift, int xIndex, int yIndex, Neighbourhood neighbourhood = Neighbourhood.Moore)
        {
            int Nx, Ny, nNeighbours;
            float hCumulative, hAverage;
            hCumulative = 0.0f;
            nNeighbours = 0;

            for (Ny = 0; Ny < yNeighbours; Ny++)
            {
                for (Nx = 0; Nx < xNeighbours; Nx++)
                {
                    if (neighbourhood == Neighbourhood.Moore || (neighbourhood == Neighbourhood.VonNeumann && (Nx == xIndex || Ny == yIndex)))
                    {
                        hCumulative += heightMap[x + Nx + xShift, y + Ny + yShift]; // Get height at point
                        nNeighbours++;
                    }
                }
            }

            hAverage = hCumulative / nNeighbours;
            heightMap[x + xIndex + xShift, y + yIndex + yShift] = hAverage;
        }

        public static float[,] HydraulicErosionComputeShader(TNode parentNode, float[,] SrcOriginal, UnityEngine.ComputeShader erosion, int numErosionIterations = 50000)
        {
            float resolution = SrcOriginal.GetLength(0);

            if (resolution < 256 || erosion == null)
            {
                UnityEngine.Debug.Log("Skipped Hydraulic Erosion process!");
                float[,] SrcClone = SrcOriginal.Clone() as float[,];
                return SrcClone;
            }

            float[,] Src = NormalizeHeightmap(SrcOriginal);
            int mapSize = Src.GetLength(0);
            float[] Src1D = Convert2DTo1DArray(Src, mapSize);

            int erosionBrushRadius = 3; //6

            int maxLifetime = 30; //90
            float sedimentCapacityFactor = 3; //3f
            float minSedimentCapacity = .01f; //0f
            float depositSpeed = 0.3f;
            float erodeSpeed = 0.3f;

            float evaporateSpeed = .01f; //0.2f
            float gravity = 4;
            float startSpeed = 1;
            float startWater = 1;
            float inertia = 0.3f; // 0.065f (Range 0, 1)

            int mapSizeWithBorder = mapSize + erosionBrushRadius * 2;

            int numThreads = numErosionIterations / 1024;

            // Create brush
            List<int> brushIndexOffsets = new List<int>();
            List<float> brushWeights = new List<float>();

            float weightSum = 0;

            for (int brushY = -erosionBrushRadius; brushY <= erosionBrushRadius; brushY++)
            {
                for (int brushX = -erosionBrushRadius; brushX <= erosionBrushRadius; brushX++)
                {
                    float sqrDst = brushX * brushX + brushY * brushY;

                    if (sqrDst < erosionBrushRadius * erosionBrushRadius)
                    {
                        brushIndexOffsets.Add(brushY * mapSize + brushX);
                        float brushWeight = 1 - UnityEngine.Mathf.Sqrt(sqrDst) / erosionBrushRadius;
                        weightSum += brushWeight;
                        brushWeights.Add(brushWeight);
                    }
                }
            }

            for (int i = 0; i < brushWeights.Count; i++)
                brushWeights[i] /= weightSum;

            // Send brush data to compute shader
            UnityEngine.ComputeBuffer brushIndexBuffer = new UnityEngine.ComputeBuffer(brushIndexOffsets.Count, sizeof(int));
            UnityEngine.ComputeBuffer brushWeightBuffer = new UnityEngine.ComputeBuffer(brushWeights.Count, sizeof(int));
            brushIndexBuffer.SetData(brushIndexOffsets);
            brushWeightBuffer.SetData(brushWeights);
            erosion.SetBuffer(0, "brushIndices", brushIndexBuffer);
            erosion.SetBuffer(0, "brushWeights", brushWeightBuffer);

            // Generate random indices for droplet placement
            int[] randomIndices = new int[numErosionIterations];

            for (int i = 0; i < numErosionIterations; i++)
            {
                int randomX = UnityEngine.Random.Range(erosionBrushRadius, mapSize + erosionBrushRadius);
                int randomY = UnityEngine.Random.Range(erosionBrushRadius, mapSize + erosionBrushRadius);
                randomIndices[i] = randomY * mapSize + randomX;
            }

            // Send random indices to compute shader
            UnityEngine.ComputeBuffer randomIndexBuffer = new UnityEngine.ComputeBuffer(randomIndices.Length, sizeof(int));
            randomIndexBuffer.SetData(randomIndices);
            erosion.SetBuffer(0, "randomIndices", randomIndexBuffer);

            // Heightmap buffer
            UnityEngine.ComputeBuffer mapBuffer = new UnityEngine.ComputeBuffer(Src1D.Length, sizeof(float));
            mapBuffer.SetData(Src1D);
            erosion.SetBuffer(0, "map", mapBuffer);

            // Settings
            erosion.SetInt("borderSize", erosionBrushRadius);
            erosion.SetInt("mapSize", mapSizeWithBorder);
            erosion.SetInt("brushLength", brushIndexOffsets.Count);
            erosion.SetInt("maxLifetime", maxLifetime);
            erosion.SetFloat("inertia", inertia);
            erosion.SetFloat("sedimentCapacityFactor", sedimentCapacityFactor);
            erosion.SetFloat("minSedimentCapacity", minSedimentCapacity);
            erosion.SetFloat("depositSpeed", depositSpeed);
            erosion.SetFloat("erodeSpeed", erodeSpeed);
            erosion.SetFloat("evaporateSpeed", evaporateSpeed);
            erosion.SetFloat("gravity", gravity);
            erosion.SetFloat("startSpeed", startSpeed);
            erosion.SetFloat("startWater", startWater);

            // Run compute shader
            erosion.Dispatch(0, numThreads, 1, 1);
            mapBuffer.GetData(Src1D);

            // Release buffers
            mapBuffer.Release();
            randomIndexBuffer.Release();
            brushIndexBuffer.Release();
            brushWeightBuffer.Release();

            Src = Convert1DTo2DArray(Src1D, mapSize, mapSize);
            Array.Clear(Src1D, 0, Src1D.Length);
            Src1D = null;

            return DenormalizeHeightmap(Src);
        }

        public static float[,] HydraulicErosionUltimate(TNode parentNode, float[,] SrcOriginal, int numIterations = 50000)
        {
            float resolution = SrcOriginal.GetLength(0);

            if (resolution < 256)
            {
                float[,] SrcClone = SrcOriginal.Clone() as float[,];
                return SrcClone;
            }

            //float minHeight = 0;
            // float[,] Src = NormalizeHeightmap(SrcOriginal, out minHeight, 8848);
            float[,] Src = NormalizeHeightmap(SrcOriginal);
            int mapSize = Src.GetLength(0);
            float[] Src1D = Convert2DTo1DArray(Src, mapSize);
            int seed = 0;
            Random prng = new Random(seed);
            int erosionRadius = 3;
            float inertia = .05f; // At zero, water will instantly change direction to flow downhill. At 1, water will never change direction. 
            float sedimentCapacityFactor = 4; // Multiplier for how much sediment a droplet can carry
            float minSedimentCapacity = .01f; // Used to prevent carry capacity getting too close to zero on flatter terrain
            float erodeSpeed = .3f;
            float depositSpeed = .3f;
            float evaporateSpeed = .01f;
            float gravity = 4;
            int maxDropletLifetime = 30;
            float initialWaterVolume = 1;
            float initialSpeed = 1;

            // Indices and weights of erosion brush precomputed for every node
            int[][] erosionBrushIndices = new int[mapSize * mapSize][];
            float[][] erosionBrushWeights = new float[mapSize * mapSize][];

            InitializeBrushIndices(mapSize, erosionRadius, out erosionBrushIndices, out erosionBrushWeights);

            for (int iteration = 0; iteration < numIterations; iteration++)
            {
                // Create water droplet at random point on map
                float posX = prng.Next(0, mapSize - 1);
                float posY = prng.Next(0, mapSize - 1);
                float dirX = 0;
                float dirY = 0;
                float speed = initialSpeed;
                float water = initialWaterVolume;
                float sediment = 0;

                for (int lifetime = 0; lifetime < maxDropletLifetime; lifetime++)
                {
                    int nodeX = (int)posX;
                    int nodeY = (int)posY;
                    int dropletIndex = nodeY * mapSize + nodeX;
                    // Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
                    float cellOffsetX = posX - nodeX;
                    float cellOffsetY = posY - nodeY;

                    // Calculate droplet's height and direction of flow with bilinear interpolation of surrounding heights
                    HeightAndGradient heightAndGradient = CalculateHeightAndGradient(Src1D, mapSize, posX, posY);

                    // Update the droplet's direction and position (move position 1 unit regardless of speed)
                    dirX = (dirX * inertia - heightAndGradient.gradientX * (1 - inertia));
                    dirY = (dirY * inertia - heightAndGradient.gradientY * (1 - inertia));
                    // Normalize direction
                    float len = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
                    if (len != 0)
                    {
                        dirX /= len;
                        dirY /= len;
                    }
                    posX += dirX;
                    posY += dirY;

                    // Stop simulating droplet if it's not moving or has flowed over edge of map
                    if ((dirX == 0 && dirY == 0) || posX < 0 || posX >= mapSize - 1 || posY < 0 || posY >= mapSize - 1)
                    {
                        break;
                    }

                    // Find the droplet's new height and calculate the deltaHeight
                    float newHeight = CalculateHeightAndGradient(Src1D, mapSize, posX, posY).height;
                    float deltaHeight = newHeight - heightAndGradient.height;

                    // Calculate the droplet's sediment capacity (higher when moving fast down a slope and contains lots of water)
                    float sedimentCapacity = Math.Max(-deltaHeight * speed * water * sedimentCapacityFactor, minSedimentCapacity);

                    // If carrying more sediment than capacity, or if flowing uphill:
                    if (sediment > sedimentCapacity || deltaHeight > 0)
                    {
                        // If moving uphill (deltaHeight > 0) try fill up to the current height, otherwise deposit a fraction of the excess sediment
                        float amountToDeposit = (deltaHeight > 0) ? Math.Min(deltaHeight, sediment) : (sediment - sedimentCapacity) * depositSpeed;
                        sediment -= amountToDeposit;

                        // Add the sediment to the four nodes of the current cell using bilinear interpolation
                        // Deposition is not distributed over a radius (like erosion) so that it can fill small pits
                        Src1D[dropletIndex] += amountToDeposit * (1 - cellOffsetX) * (1 - cellOffsetY);
                        Src1D[dropletIndex + 1] += amountToDeposit * cellOffsetX * (1 - cellOffsetY);
                        Src1D[dropletIndex + mapSize] += amountToDeposit * (1 - cellOffsetX) * cellOffsetY;
                        Src1D[dropletIndex + mapSize + 1] += amountToDeposit * cellOffsetX * cellOffsetY;
                    }
                    else
                    {
                        // Erode a fraction of the droplet's current carry capacity.
                        // Clamp the erosion to the change in height so that it doesn't dig a hole in the terrain behind the droplet
                        float amountToErode = Math.Min((sedimentCapacity - sediment) * erodeSpeed, -deltaHeight);

                        // Use erosion brush to erode from all nodes inside the droplet's erosion radius
                        for (int brushPointIndex = 0; brushPointIndex < erosionBrushIndices[dropletIndex].Length; brushPointIndex++)
                        {
                            int nodeIndex = erosionBrushIndices[dropletIndex][brushPointIndex];
                            float weighedErodeAmount = amountToErode * erosionBrushWeights[dropletIndex][brushPointIndex];
                            float deltaSediment = (Src1D[nodeIndex] < weighedErodeAmount) ? Src1D[nodeIndex] : weighedErodeAmount;
                            Src1D[nodeIndex] -= deltaSediment;
                            sediment += deltaSediment;
                        }
                    }

                    // Update droplet's speed and water content
                    speed = (float)Math.Sqrt(speed * speed + deltaHeight * gravity);
                    water *= (1 - evaporateSpeed);
                }

                if (parentNode != null)
                    parentNode._progress = (float)iteration / numIterations;
            }

            Src = Convert1DTo2DArray(Src1D, mapSize, mapSize);
            Array.Clear(Src1D, 0, Src1D.Length);
            Array.Clear(erosionBrushIndices, 0, erosionBrushIndices.Length);
            Array.Clear(erosionBrushWeights, 0, erosionBrushWeights.Length);
            Src1D = null;
            erosionBrushIndices = null;
            erosionBrushWeights = null;

            return DenormalizeHeightmap(Src);
        }

        public static float[,] HydraulicErosion(float[,] SrcOriginal, int iterations = 2, float rainAmount = 0.01f, float sediment = 0.05f, float evaporation = 1.65f)
        {
            float[,] Src = SrcOriginal.Clone() as float[,];
            int width = Src.GetLength(0);
            int height = Src.GetLength(1);
            int x, y, i, j, iter_count;
            int lowest_x = 0, lowest_y = 0;
            float[,] water_map = new float[width, height];
            float capacity = sediment;
            float water_lost, current_height, current_difference, max_dif; //temporary variables

            //for each iteration...
            for (iter_count = 0; iter_count < iterations; iter_count++)
            {
                //step 1: rain
                for (x = 0; x < width; ++x)
                    for (y = 0; y < height; ++y)
                        water_map[x, y] += rainAmount;

                //step 2: erosion
                for (x = 0; x < width; ++x)
                    for (y = 0; y < height; ++y)
                        Src[x, y] -= water_map[x, y] * sediment;

                //step 3: movement
                for (x = 1; x < (width - 1); ++x)
                {
                    for (y = 1; y < (height - 1); ++y)
                    {
                        //find the lowest neighbor
                        current_height = Src[x, y] + water_map[x, y];
                        max_dif = -float.MaxValue;

                        for (i = -1; i < 2; i += 1)
                        {
                            for (j = -1; j < 2; j += 1)
                            {
                                current_difference = current_height - Src[x + i, y + j] - water_map[x + i, y + i];

                                if (current_difference > max_dif)
                                {
                                    max_dif = current_difference;
                                    lowest_x = i;
                                    lowest_y = j;
                                }
                            }
                        }

                        //now either do nothing, level off, or move all the water
                        if (max_dif > 0.0f)
                        {
                            //move it all...
                            if (water_map[x, y] < max_dif)
                            {
                                water_map[x + lowest_x, y + lowest_y] += water_map[x, y];
                                water_map[x, y] = 0.0f;
                            }
                            //level off...
                            else
                            {
                                water_map[x + lowest_x, y + lowest_y] += max_dif / 2.0f;
                                water_map[x, y] -= max_dif / 2.0f;
                            }
                        }

                    }
                }

                //step 4: evaporation / deposition
                for (x = 0; x < width; x++)
                {
                    for (y = 0; y < height; y++)
                    {
                        water_lost = water_map[x, y] * evaporation;
                        water_map[x, y] -= water_lost;
                        Src[x, y] += (water_lost * capacity);
                    }
                }

                CornerCorrection2D(Src, width);
            }

            return Src;
        }

        public static float[,] WaterErosion(TNode parentNode, float[,] SrcOriginal, int iterations = 2, float shape = 0.0001f, float rivers = 0.001f, float vertical = 5, float seaBedCarve = 0.0002f)
        {
            float[,] Src = SrcOriginal.Clone() as float[,];
            int width = Src.GetLength(0);
            int height = Src.GetLength(1);
            float[] Src1D = Convert2DTo1DArray(Src, width);
            float[] afWaterMap, afSedimentMap, afAltMap, afNewWaterMap, afNewSedimentMap, afNewAltMap;
            float[] pTempMap;
            int i, j, x, y, u32Offset;

            afWaterMap = new float[width * height];
            afSedimentMap = new float[width * height];
            afAltMap = new float[width * height];
            afNewWaterMap = new float[width * height];
            afNewSedimentMap = new float[width * height];
            afNewAltMap = new float[width * height];

            float dSedCap;
            //double dMaxWater = Src1D->getMaxValue(true);
            //double dMinVal = Src1D->getMinValue(true);

            float dDeltaW;
            float[] adNeighborsWeights = new float[8];
            float dNeighborsWeightsSum;
            int[] aiOffsetArray = new int[8];

            aiOffsetArray[0] = -width - 1;
            aiOffsetArray[1] = -width;
            aiOffsetArray[2] = -width + 1;
            aiOffsetArray[3] = -1;
            aiOffsetArray[4] = 1;
            aiOffsetArray[5] = width - 1;
            aiOffsetArray[6] = width;
            aiOffsetArray[7] = width + 1;

            u32Offset = 0;

            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    afWaterMap[u32Offset] = 0.5f;
                    afSedimentMap[u32Offset] = 0.0f;
                    afAltMap[u32Offset] = Src1D[u32Offset];
                    u32Offset++;
                }
            }

            for (i = 0; i < iterations; i++)
            {
                u32Offset = 0;

                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        afNewSedimentMap[u32Offset] = 0.0f;
                        afNewWaterMap[u32Offset] = 0.0f;
                        afNewAltMap[u32Offset] = 0.0f;

                        u32Offset++;
                    }
                }

                u32Offset = width * (height - 1);

                for (x = 0; x < width; x++)
                {
                    afNewAltMap[x] = afAltMap[x];
                    afNewAltMap[u32Offset + x] = afAltMap[u32Offset + x];
                }

                u32Offset = 0;

                for (y = 0; y < height; y++)
                {
                    afNewAltMap[u32Offset] = afAltMap[u32Offset];
                    afNewAltMap[u32Offset + height - 1] = afAltMap[u32Offset + height - 1];
                    u32Offset += height;
                }

                for (y = 1; y < width - 1; y++)
                {
                    u32Offset = y * width + 1;

                    for (x = 1; x < width - 1; x++)
                    {
                        u32Offset = y * width + x;
                        GetNeighborsDelta(afAltMap, width, u32Offset, adNeighborsWeights);
                        dNeighborsWeightsSum = 0.0f;

                        for (j = 0; j < 8; j++)
                            if (adNeighborsWeights[j] < 0.0f)
                                dNeighborsWeightsSum -= adNeighborsWeights[j];

                        if (dNeighborsWeightsSum <= 0.0f)
                        {
                            afNewAltMap[u32Offset] = afAltMap[u32Offset] - seaBedCarve; // Water bed
                            afNewWaterMap[u32Offset] = afWaterMap[u32Offset]; // Shores
                            afNewSedimentMap[u32Offset] = afSedimentMap[u32Offset]; // Slopes
                        }
                        else
                        {
                            for (j = 0; j < 8; j++)
                            {
                                if (adNeighborsWeights[j] < 0.0f)
                                {
                                    adNeighborsWeights[j] /= dNeighborsWeightsSum;
                                    dDeltaW = afWaterMap[u32Offset] - afWaterMap[(int)u32Offset + aiOffsetArray[j]];
                                    dDeltaW += afAltMap[u32Offset] - afAltMap[(int)u32Offset + aiOffsetArray[j]];

                                    //dDeltaW *= -adNeighborsWeights[j];
                                    dDeltaW += -adNeighborsWeights[j];

                                    if (afWaterMap[u32Offset] < dDeltaW)
                                        dDeltaW = afWaterMap[u32Offset];

                                    if (dDeltaW <= 0.0f) // depose sediment
                                    {
                                        afNewAltMap[u32Offset] += -adNeighborsWeights[j] * (afAltMap[u32Offset] + shape * afSedimentMap[u32Offset]);
                                        afNewSedimentMap[u32Offset] += -adNeighborsWeights[j] * (afSedimentMap[u32Offset] * (1 - shape));
                                    }
                                    else  // flow water
                                    {
                                        afNewWaterMap[u32Offset] += -adNeighborsWeights[j] * (afWaterMap[u32Offset] - dDeltaW);
                                        afNewWaterMap[(int)u32Offset + aiOffsetArray[j]] += -adNeighborsWeights[j] * (afWaterMap[(int)u32Offset + aiOffsetArray[j]] + dDeltaW);
                                        dSedCap = rivers * dDeltaW;

                                        if (afSedimentMap[u32Offset] >= dSedCap)
                                        {
                                            afNewSedimentMap[(int)u32Offset + aiOffsetArray[j]] += -adNeighborsWeights[j] * (afSedimentMap[(int)u32Offset + aiOffsetArray[j]] + dSedCap);
                                            afNewAltMap[u32Offset] += -adNeighborsWeights[j] * (afAltMap[u32Offset] + shape * (afSedimentMap[u32Offset] - dSedCap));
                                            afNewSedimentMap[u32Offset] += -adNeighborsWeights[j] * (1.0f - shape) * (afSedimentMap[u32Offset] - dSedCap);
                                        }
                                        else
                                        {
                                            afNewSedimentMap[(int)u32Offset + aiOffsetArray[j]] += -adNeighborsWeights[j] * (afSedimentMap[(int)u32Offset + aiOffsetArray[j]] + afSedimentMap[u32Offset] + vertical * (dSedCap - afSedimentMap[u32Offset]));
                                            afNewAltMap[u32Offset] += -adNeighborsWeights[j] * (afAltMap[u32Offset] - vertical * (dSedCap - afSedimentMap[u32Offset]));

                                            //afNewSedimentMap[u32Offset] += 0.0f;
                                        }
                                    }
                                }
                            }
                        }

                        u32Offset++;

                        if (parentNode != null)
                            parentNode._progress = ((x + width * y) * ((float)i + 1)) / ((height + 1) * (y + 1) * ((float)iterations));
                    }
                }

                pTempMap = afWaterMap;
                afWaterMap = afNewWaterMap;
                afNewWaterMap = pTempMap;

                pTempMap = afSedimentMap;
                afSedimentMap = afNewSedimentMap;
                afNewSedimentMap = pTempMap;

                pTempMap = afAltMap;
                afAltMap = afNewAltMap;
                afNewAltMap = pTempMap;

                u32Offset = 0;

                //TODO
                for (y = 0; y < height; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        Src1D[u32Offset] = afAltMap[u32Offset];
                        u32Offset++;
                    }
                }
            }

            for (y = 1; y < height - 1; y++)
            {
                for (x = 1; x < width - 1; x++)
                {
                    Src1D[x + y * height] = afAltMap[x - 1 + (y - 1) * height]
                    + 2 * afAltMap[x + (y - 1) * height]
                    + afAltMap[x + 1 + (y - 1) * height]
                    + 2 * afAltMap[x - 1 + y * height]
                    + 4 * afAltMap[x + y * height]
                    + 2 * afAltMap[x + 1 + y * height]
                    + afAltMap[x - 1 + (y + 1) * height]
                    + 2 * afAltMap[x + (y + 1) * height]
                    + afAltMap[x + 1 + (y + 1) * height];

                    Src1D[x + y * height] /= 16f;
                }
            }

            CornerCorrection1D(Src1D, width);

            //TODO
            u32Offset = width * (height - 1);

            for (x = 0; x < width; x++)
            {
                Src1D[x] = afAltMap[x];
                Src1D[u32Offset + x] = afAltMap[u32Offset + x];
            }

            u32Offset = 0;

            for (y = 0; y < height; y++)
            {
                Src1D[u32Offset] = afAltMap[u32Offset];
                Src1D[u32Offset + height - 1] = afAltMap[u32Offset + height - 1];
                u32Offset += height;
            }

            Array.Clear(afWaterMap, 0, afWaterMap.Length);
            Array.Clear(afSedimentMap, 0, afSedimentMap.Length);
            Array.Clear(afAltMap, 0, afAltMap.Length);
            Array.Clear(afNewWaterMap, 0, afNewWaterMap.Length);
            Array.Clear(afNewSedimentMap, 0, afNewSedimentMap.Length);
            Array.Clear(afNewAltMap, 0, afNewAltMap.Length);
            afWaterMap = null;
            afSedimentMap = null;
            afAltMap = null;
            afNewWaterMap = null;
            afNewSedimentMap = null;
            afNewAltMap = null;
            pTempMap = null;

            return Convert1DTo2DArray(Src1D, width, height);
        }

        public static float[,] ThermalErosion(TNode parentNode, float[,] SrcOriginal, int iterations = 5)
        {
            float[,] Src = SrcOriginal.Clone() as float[,];
            int width = Src.GetLength(0);
            int height = Src.GetLength(1);
            int x, y, i, j, iter_count;
            int lowest_x = 0, lowest_y = 0;

            float talus = 0.1f / (float)width,
            current_difference, current_height,
            max_dif,
            new_height;

            //for each iteration...
            for (iter_count = 0; iter_count < iterations; iter_count++)
            {
                //for each pixel...
                for (x = 1; x < (width - 1); ++x)
                {
                    for (y = 1; y < (height - 1); ++y)
                    {
                        current_height = Src[x, y];
                        max_dif = -float.MaxValue;

                        for (i = -1; i < 2; i += 2)
                        {
                            for (j = -1; j < 2; j += 2)
                            {
                                current_difference = current_height - Src[x + i, y + j];

                                if (current_difference > max_dif)
                                {
                                    max_dif = current_difference;

                                    lowest_x = i;
                                    lowest_y = j;
                                }
                            }
                        }

                        if (max_dif > talus)
                        {
                            //new_height = current_height - max_dif / 2.0f;
                            new_height = current_height - max_dif / 4.0f;

                            Src[x, y] = new_height;
                            Src[x + lowest_x, y + lowest_y] = new_height;
                        }

                        if (parentNode != null)
                            parentNode._progress = ((x + width * y) * ((float)iter_count + 1)) / ((height + 1) * (y + 1) * ((float)iterations));
                    }
                }
            }

            return Src;
        }

        public static float[,] Terrace(TNode parentNode, float[,] SrcOriginal, ref float[] controlPoints, int terraceCount = 7, float strength = 0.5f, float terraceVariation = 0.75f)
        {
            //float minHeight1 = 0;
            float[,] Src = NormalizeHeightmap(SrcOriginal);
            ModuleBase moduleBase = new Heights();
            moduleBase.tData = Src;
            int width = Src.GetLength(0);
            int height = Src.GetLength(1);
            float maxHeight = Src.Cast<float>().Max();
            float minHeight = Src.Cast<float>().Min();
            moduleBase.GetValue(width, maxHeight, 0);   // x: Resolution,   y: Normalized Terrain Size Y,   z: Reference To The Active Terrain

            //UnityEngine.Debug.Log(maxHeight);
            //UnityEngine.Debug.Log(minHeight);

            // Any Operators Come Here
            Terrace terrace = new Terrace(moduleBase);
            controlPoints = AutoSetTerraces(terraceCount, terraceVariation, minHeight, 1);

            for (int i = 0; i < terraceCount; i++)
                terrace.Add(controlPoints[i]);

            moduleBase = terrace;
            Noise2D heightMap = new Noise2D(width, height, moduleBase);
            heightMap.GeneratePlanar(0.0, 1.0, 0.0, 1.0, true);

            float[,] finalHeightsNoise = heightMap.GetData(true, 0, 0);
            float[,] finalHeights = new float[width, height];

            if (strength == 1.0f)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        //float height = finalHeightsNoise[y, x] * (maxHeight - minHeight) + minHeight;
                        //finalHeights[x, y] = Math.Max(Math.Min(height, maxHeight), minHeight);
                        //finalHeights[x, y] = height;

                        finalHeights[x, y] = finalHeightsNoise[y, x] * maxHeight;

                        if (parentNode != null)
                            parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                    }
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        finalHeights[x, y] = ((1.0f - strength) * Src[x, y]) + (finalHeightsNoise[y, x] * maxHeight * strength);

                        if (parentNode != null)
                            parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                    }
                }
            }

            Array.Clear(finalHeightsNoise, 0, finalHeightsNoise.Length);
            finalHeightsNoise = null;

            return DenormalizeHeightmap(finalHeights);
        }

        public static Bitmap CreateAspectMap(TNode parentNode, float[,] Src, float widthMultiplier = 1, float heightMultiplier = 1, ASPECT_TYPE aspectType = ASPECT_TYPE.EASTERNESS)
        {
            int width = (int)(Src.GetLength(0) * widthMultiplier);
            int height = (int)(Src.GetLength(1) * widthMultiplier);
            float[] heights = Convert2DTo1DArrayCW90(Src, Src.GetLength(0));
            float ux = 1.0f / (width - 1.0f);
            float uy = 1.0f / (height - 1.0f);

            //Texture2D aspectMap = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            Bitmap aspectMap = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xp1 = (x == width - 1) ? x : x + 1;
                    int xn1 = (x == 0) ? x : x - 1;

                    int yp1 = (y == height - 1) ? y : y + 1;
                    int yn1 = (y == 0) ? y : y - 1;

                    float l = heights[xn1 + y * width];
                    float r = heights[xp1 + y * width];

                    float b = heights[x + yn1 * width];
                    float t = heights[x + yp1 * width];

                    float dx = (r - l) / (2.0f * ux);
                    float dy = (t - b) / (2.0f * uy);

                    float m = (float)Math.Sqrt(dx * dx + dy * dy);
                    //float a = Math.Acos(-dy / m) * Math.Rad2Deg;
                    float a = (float)TUtils.Rad2Deg(Math.Acos(-dy / m));

                    if (float.IsInfinity(a) || float.IsNaN(a))
                        a = 0.0f;

                    float aspect = 180.0f * (1.0f + Sign(dx)) - Sign(dx) * a;

                    switch (aspectType)
                    {
                        case ASPECT_TYPE.NORTHERNESS:
                            aspect = (float)Math.Cos(TUtils.Deg2Rad(aspect));
                            aspect = aspect * 0.5f + 0.5f;
                            break;

                        case ASPECT_TYPE.EASTERNESS:
                            aspect = (float)Math.Sin(TUtils.Deg2Rad(aspect));
                            aspect = aspect * 0.5f + 0.5f;
                            break;

                        default:
                            aspect /= 360.0f;
                            break;
                    }

                    //aspectMap.SetPixel(x, y, new Color(aspect, aspect, aspect, 1));
                    int rgb = (int)(aspect * 255);
                    Color pixel = Color.FromArgb(255, rgb, rgb, rgb);
                    aspectMap.SetPixel(x, y, pixel);

                    if (parentNode != null)
                        parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                }
            }

            Array.Clear(heights, 0, heights.Length);
            heights = null;

            //aspectMap.Apply();
            return aspectMap;
        }

        public static TMask CreateAspectMask(TNode parentNode, float[,] SrcOriginal, float widthMultiplier = 1, float heightMultiplier = 1, ASPECT_TYPE aspectType = ASPECT_TYPE.EASTERNESS)
        {
            //float minHeight1 = 0;
            float[,] Src = NormalizeHeightmap(SrcOriginal);
            int width = (int)(Src.GetLength(0) * widthMultiplier);
            int height = (int)(Src.GetLength(1) * widthMultiplier);
            float[] heights = Convert2DTo1DArrayCW90(Src, Src.GetLength(0));
            float ux = 1.0f / (width - 1.0f);
            float uy = 1.0f / (height - 1.0f);

            //Texture2D aspectMap = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            TMask aspectMap = new TMask(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xp1 = (x == width - 1) ? x : x + 1;
                    int xn1 = (x == 0) ? x : x - 1;

                    int yp1 = (y == height - 1) ? y : y + 1;
                    int yn1 = (y == 0) ? y : y - 1;

                    float l = heights[xn1 + y * width];
                    float r = heights[xp1 + y * width];

                    float b = heights[x + yn1 * width];
                    float t = heights[x + yp1 * width];

                    float dx = (r - l) / (2.0f * ux);
                    float dy = (t - b) / (2.0f * uy);

                    float m = (float)Math.Sqrt(dx * dx + dy * dy);
                    //float a = Math.Acos(-dy / m) * Math.Rad2Deg;
                    float a = (float)TUtils.Rad2Deg(Math.Acos(-dy / m));

                    if (float.IsInfinity(a) || float.IsNaN(a))
                        a = 0.0f;

                    float aspect = 180.0f * (1.0f + Sign(dx)) - Sign(dx) * a;

                    switch (aspectType)
                    {
                        case ASPECT_TYPE.NORTHERNESS:
                            aspect = (float)Math.Cos(TUtils.Deg2Rad(aspect));
                            aspect = aspect * 0.5f + 0.5f;
                            break;

                        case ASPECT_TYPE.EASTERNESS:
                            aspect = (float)Math.Sin(TUtils.Deg2Rad(aspect));
                            aspect = aspect * 0.5f + 0.5f;
                            break;

                        default:
                            aspect /= 360.0f;
                            break;
                    }

                    aspectMap.SetValue(x, height - 1 - y, aspect);

                    if (parentNode != null)
                        parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                }
            }

            aspectMap.SetValue(0, 0, 0);
            aspectMap.SetValue(0, 1, 1);
            Array.Clear(heights, 0, heights.Length);
            heights = null;

            return aspectMap;
        }

        public static Bitmap CreateCurvatureMap(TNode parentNode, float[,] Src, float widthMultiplier = 1, float heightMultiplier = 1, float limit = 10000, CURVATURE_TYPE curvatureType = CURVATURE_TYPE.AVERAGE)
        {
            int width = (int)(Src.GetLength(0) * widthMultiplier);
            int height = (int)(Src.GetLength(1) * widthMultiplier);
            float[] heights = Convert2DTo1DArrayCW90(Src, Src.GetLength(0));
            float ux = 1.0f / (width - 1.0f);
            float uy = 1.0f / (height - 1.0f);
            Bitmap curveMap = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xp1 = (x == width - 1) ? x : x + 1;
                    int xn1 = (x == 0) ? x : x - 1;

                    int yp1 = (y == height - 1) ? y : y + 1;
                    int yn1 = (y == 0) ? y : y - 1;

                    float v = heights[x + y * width];

                    float l = heights[xn1 + y * width];
                    float r = heights[xp1 + y * width];

                    float b = heights[x + yn1 * width];
                    float t = heights[x + yp1 * width];

                    float lb = heights[xn1 + yn1 * width];
                    float lt = heights[xn1 + yp1 * width];

                    float rb = heights[xp1 + yn1 * width];
                    float rt = heights[xp1 + yp1 * width];

                    float dx = (r - l) / (2.0f * ux);
                    float dy = (t - b) / (2.0f * uy);

                    float dxx = (r - 2.0f * v + l) / (ux * ux);
                    float dyy = (t - 2.0f * v + b) / (uy * uy);

                    float dxy = (rt - rb - lt + lb) / (4.0f * ux * uy);

                    float curve = 0.0f;

                    switch (curvatureType)
                    {
                        case CURVATURE_TYPE.HORIZONTAL:
                            curve = Horizontal(dx, dy, dxx, dyy, dxy, limit);
                            break;

                        case CURVATURE_TYPE.VERTICAL:
                            curve = Vertical(dx, dy, dxx, dyy, dxy, limit);
                            break;

                        case CURVATURE_TYPE.AVERAGE:
                            curve = Average(dx, dy, dxx, dyy, dxy, limit);
                            break;
                    }

                    int rgb = (int)(curve * 255);
                    Color pixel = Color.FromArgb(255, rgb, rgb, rgb);
                    curveMap.SetPixel(x, y, pixel);

                    parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                }
            }

            Array.Clear(heights, 0, heights.Length);
            heights = null;

            return curveMap;
        }

        public static TMask CreateCurvatureMask(TNode parentNode, float[,] SrcOriginal, float widthMultiplier = 1, float heightMultiplier = 1, float limit = 10000, CURVATURE_TYPE curvatureType = CURVATURE_TYPE.AVERAGE)
        {
            //float minHeight1 = 0;
            float[,] Src = NormalizeHeightmap(SrcOriginal);
            int width = (int)(Src.GetLength(0) * widthMultiplier);
            int height = (int)(Src.GetLength(1) * widthMultiplier);
            float[] heights = Convert2DTo1DArrayCW90(Src, Src.GetLength(0));
            float ux = 1.0f / (width - 1.0f);
            float uy = 1.0f / (height - 1.0f);
            TMask curveMap = new TMask(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xp1 = (x == width - 1) ? x : x + 1;
                    int xn1 = (x == 0) ? x : x - 1;

                    int yp1 = (y == height - 1) ? y : y + 1;
                    int yn1 = (y == 0) ? y : y - 1;

                    float v = heights[x + y * width];

                    float l = heights[xn1 + y * width];
                    float r = heights[xp1 + y * width];

                    float b = heights[x + yn1 * width];
                    float t = heights[x + yp1 * width];

                    float lb = heights[xn1 + yn1 * width];
                    float lt = heights[xn1 + yp1 * width];

                    float rb = heights[xp1 + yn1 * width];
                    float rt = heights[xp1 + yp1 * width];

                    float dx = (r - l) / (2.0f * ux);
                    float dy = (t - b) / (2.0f * uy);

                    float dxx = (r - 2.0f * v + l) / (ux * ux);
                    float dyy = (t - 2.0f * v + b) / (uy * uy);

                    float dxy = (rt - rb - lt + lb) / (4.0f * ux * uy);

                    float curve = 0.0f;

                    switch (curvatureType)
                    {
                        case CURVATURE_TYPE.HORIZONTAL:
                            curve = Horizontal(dx, dy, dxx, dyy, dxy, limit);
                            break;

                        case CURVATURE_TYPE.VERTICAL:
                            curve = Vertical(dx, dy, dxx, dyy, dxy, limit);
                            break;

                        case CURVATURE_TYPE.AVERAGE:
                            curve = Average(dx, dy, dxx, dyy, dxy, limit);
                            break;
                    }

                    curveMap.SetValue(x, height - 1 - y, curve);

                    if (parentNode != null)
                        parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                }
            }

            curveMap.SetValue(0, 0, 0);
            curveMap.SetValue(0, 1, 1);
            Array.Clear(heights, 0, heights.Length);
            heights = null;

            return curveMap;
        }

        public static Bitmap CreateFlowMap(TNode parentNode, float[,] SrcOriginal, float widthMultiplier = 1, float heightMultiplier = 1, int iteration = 5)
        {
            //float minHeight1 = 0;
            float[,] Src = NormalizeHeightmap(SrcOriginal);
            int width = (int)(Src.GetLength(0) * widthMultiplier);
            int height = (int)(Src.GetLength(1) * widthMultiplier);
            const int LEFT = 0;
            const int RIGHT = 1;
            const int BOTTOM = 2;
            const int TOP = 3;
            const float TIME = 0.2f;

            float[] heights = Convert2DTo1DArrayCW90(Src, Src.GetLength(0));
            float[,] waterMap = new float[width, height];
            float[,,] outFlow = new float[width, height, 4];

            FillWaterMap(0.0001f, waterMap, width, height);

            for (int i = 0; i < iteration; i++)
            {
                ComputeOutflow(waterMap, outFlow, heights, width, height, TIME);
                UpdateWaterMap(waterMap, outFlow, width, height, TOP, LEFT, BOTTOM, RIGHT, TIME);
            }

            float[,] velocityMap = new float[width, height];

            CalculateVelocityField(velocityMap, outFlow, width, height, TOP, LEFT, BOTTOM, RIGHT);
            NormalizeMap(velocityMap, width, height);

            Bitmap flowMap = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float v = velocityMap[x, y];
                    int rgb = (int)(v * 255);
                    Color pixel = Color.FromArgb(255, rgb, rgb, rgb);
                    flowMap.SetPixel(x, y, pixel);

                    if (parentNode != null)
                        parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                }
            }

            Array.Clear(heights, 0, heights.Length);
            Array.Clear(waterMap, 0, waterMap.Length);
            Array.Clear(outFlow, 0, outFlow.Length);
            Array.Clear(velocityMap, 0, velocityMap.Length);
            heights = null;
            waterMap = null;
            outFlow = null;
            velocityMap = null;

            return flowMap;
        }

        public static TMask CreateFlowMask(TNode parentNode, float[,] SrcOriginal, float widthMultiplier = 1, float heightMultiplier = 1, int iteration = 5)
        {
            //float minHeight1 = 0;
            float[,] Src = NormalizeHeightmap(SrcOriginal);
            int width = (int)(Src.GetLength(0) * widthMultiplier);
            int height = (int)(Src.GetLength(1) * widthMultiplier);
            const int LEFT = 0;
            const int RIGHT = 1;
            const int BOTTOM = 2;
            const int TOP = 3;
            const float TIME = 0.2f;

            float[] heights = Convert2DTo1DArrayCW90(Src, Src.GetLength(0));
            float[,] waterMap = new float[width, height];
            float[,,] outFlow = new float[width, height, 4];

            FillWaterMap(0.0001f, waterMap, width, height);

            for (int i = 0; i < iteration; i++)
            {
                ComputeOutflow(waterMap, outFlow, heights, width, height, TIME);
                UpdateWaterMap(waterMap, outFlow, width, height, TOP, LEFT, BOTTOM, RIGHT, TIME);
            }

            float[,] velocityMap = new float[width, height];

            CalculateVelocityField(velocityMap, outFlow, width, height, TOP, LEFT, BOTTOM, RIGHT);
            NormalizeMap(velocityMap, width, height);

            TMask flowMap = new TMask(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float v = velocityMap[x, y];

                    //int rgb = (int)(v * 255);
                    //Color pixel = Color.FromArgb(255, rgb, rgb, rgb);
                    //flowMap.SetPixel(x, y, pixel);

                    flowMap.SetValue(x, height - 1 - y, v);

                    if (parentNode != null)
                        parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                }
            }

            flowMap.SetValue(0, 0, 0);
            flowMap.SetValue(0, 1, 1);
            Array.Clear(heights, 0, heights.Length);
            Array.Clear(waterMap, 0, waterMap.Length);
            Array.Clear(outFlow, 0, outFlow.Length);
            Array.Clear(velocityMap, 0, velocityMap.Length);
            heights = null;
            waterMap = null;
            outFlow = null;
            velocityMap = null;

            return flowMap;
        }

        public static Bitmap CreateNormalMap(TNode parentNode, float[,] Src, float widthMultiplier = 1, float heightMultiplier = 1, float strength = 1)
        {
            int width = (int)(Src.GetLength(0) * widthMultiplier);
            int height = (int)(Src.GetLength(1) * heightMultiplier);
            float[] heights = Convert2DTo1DArrayCW90(Src, Src.GetLength(0));
            float ux = 1.0f / (width - 1.0f);
            float uy = 1.0f / (height - 1.0f);
            float terrainWidth = Src.GetLength(0);
            float terrainLength = Src.GetLength(1);
            float terrainHeight = Src.Cast<float>().Max() * strength;
            float scaleX = terrainHeight / terrainWidth;
            float scaleY = terrainHeight / terrainLength;
            Bitmap normalMap = new Bitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xp1 = (x == width - 1) ? x : x + 1;
                    int xn1 = (x == 0) ? x : x - 1;

                    int yp1 = (y == height - 1) ? y : y + 1;
                    int yn1 = (y == 0) ? y : y - 1;

                    float l = heights[xn1 + y * width] * scaleX;
                    float r = heights[xp1 + y * width] * scaleX;

                    float b = heights[x + yn1 * width] * scaleY;
                    float t = heights[x + yp1 * width] * scaleY;

                    float dx = (r - l) / (2.0f * ux);
                    float dy = (t - b) / (2.0f * uy);

                    Vector3 normal = new Vector3(-dx, dy, 1); //might need to flip y
                    float length = (float)Math.Sqrt((normal.X * normal.X) + (normal.Y * normal.Y) + (normal.Z * normal.Z));
                    normal.X /= length;
                    normal.Y /= length;
                    normal.Z /= length;

                    int R = (int)((normal.X * 0.5f + 0.5f) * 255);
                    int G = (int)((normal.Y * 0.5f + 0.5f) * 255);
                    int B = (int)normal.Z;
                    int A = 255;
                    Color pixel = Color.FromArgb(A, R, G, B);
                    normalMap.SetPixel(x, y, pixel);

                    if (parentNode != null)
                        parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                }
            }

            Array.Clear(heights, 0, heights.Length);
            heights = null;

            return normalMap;
        }

        //TODO: The output is not a grayscale image to be converted into a mask, it needs all RGBA channles to create a normalmap
        public static TMask CreateNormalMask(TNode parentNode, float[,] Src, float widthMultiplier = 1, float heightMultiplier = 1, float strength = 1)
        {
            int width = (int)(Src.GetLength(0) * widthMultiplier);
            int height = (int)(Src.GetLength(1) * heightMultiplier);
            float[] heights = Convert2DTo1DArrayCW90(Src, Src.GetLength(0));
            float ux = 1.0f / (width - 1.0f);
            float uy = 1.0f / (height - 1.0f);
            float terrainWidth = Src.GetLength(0);
            float terrainLength = Src.GetLength(1);
            float terrainHeight = Src.Cast<float>().Max() * strength;
            float scaleX = terrainHeight / terrainWidth;
            float scaleY = terrainHeight / terrainLength;
            TMask normalMap = new TMask(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xp1 = (x == width - 1) ? x : x + 1;
                    int xn1 = (x == 0) ? x : x - 1;

                    int yp1 = (y == height - 1) ? y : y + 1;
                    int yn1 = (y == 0) ? y : y - 1;

                    float l = heights[xn1 + y * width] * scaleX;
                    float r = heights[xp1 + y * width] * scaleX;

                    float b = heights[x + yn1 * width] * scaleY;
                    float t = heights[x + yp1 * width] * scaleY;

                    float dx = (r - l) / (2.0f * ux);
                    float dy = (t - b) / (2.0f * uy);

                    Vector3 normal = new Vector3(-dx, dy, 1); //might need to flip y
                    float length = (float)Math.Sqrt((normal.X * normal.X) + (normal.Y * normal.Y) + (normal.Z * normal.Z));
                    normalMap.SetValue(x, height - 1 - y, length);

                    if (parentNode != null)
                        parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
                }
            }

            normalMap.SetValue(0, 0, 0);
            normalMap.SetValue(0, 1, 1);
            Array.Clear(heights, 0, heights.Length);
            heights = null;

            return normalMap;
        }

        public static TMask GetSlopeMap(TMap map, THeightmap heightmap, float maxSlope, float minSlope)
        {
            return FilteredMask(heightmap, map._area._areaSizeLat, map._area._areaSizeLon, minSlope, maxSlope);
        }

        public static TMask FilteredMask(THeightmap heightmap, float areaLatSizeKm, float areaLonSizeKm, float minSlope, float maxSlope)
        {
            TMask result = new TMask(heightmap.heightsData.GetLength(0), heightmap.heightsData.GetLength(1), true);

            for (int i = 0; i < result.Width; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    float latNormalized = j * 1.0f / result.Width;
                    float lonNormalized = i * 1.0f / result.Height;
                    float steepness = heightmap.GetSteepness(lonNormalized, latNormalized, areaLatSizeKm * 1000, areaLonSizeKm * 1000);

                    if (steepness > maxSlope || steepness < minSlope)
                    {
                        result.Clear(i, j);
                        continue;
                    }
                }
            }

            return result;
        }

        //public static Bitmap CreateSlopeMap(TNode parentNode, float[,] SrcOriginal, float widthMeters, float lenghtMeters, float widthMultiplier = 1, float heightMultiplier = 1, float strength = 1)
        //{
        //    TMask slopeMask = CreateSlopeMask(parentNode, SrcOriginal, widthMeters, lenghtMeters, widthMultiplier, heightMultiplier, strength);
        //    Bitmap result = slopeMask.GetGrayImage();
        //    return result;
        //}
        //
        //public static TMask CreateSlopeMask(TNode parentNode, float[,] SrcOriginal, float widthMeters , float lenghtMeters, float widthMultiplier = 1, float heightMultiplier = 1, float strength = 1)
        //{
        //    //float minHeight1;
        //    float[,] Src = NormalizeHeightmap(SrcOriginal);
        //    int width = (int)(Src.GetLength(0) * widthMultiplier);
        //    int height = (int)(Src.GetLength(1) * heightMultiplier);
        //    float[] heights = Convert2DTo1DArrayCW90(Src, Src.GetLength(0));
        //    float ux = 1.0f / (width - 1.0f);
        //    float uy = 1.0f / (height - 1.0f);
        //    float scaleX = width / widthMeters;
        //    float scaleY = height / lenghtMeters;
        //    TMask slopeMap = new TMask(width, height);
        //
        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            int xp1 = (x == width - 1) ? x : x + 1;
        //            int xn1 = (x == 0) ? x : x - 1;
        //
        //            int yp1 = (y == height - 1) ? y : y + 1;
        //            int yn1 = (y == 0) ? y : y - 1;
        //
        //            float l = heights[xn1 + y * width] * scaleX;
        //            float r = heights[xp1 + y * width] * scaleX;
        //
        //            float b = heights[x + yn1 * width] * scaleY;
        //            float t = heights[x + yp1 * width] * scaleY;
        //
        //            float dx = (r - l) / (2.0f * ux);
        //            float dy = (t - b) / (2.0f * uy);
        //
        //            float g = (float)Math.Sqrt(dx * dx + dy * dy);
        //            float slope = g / (float)Math.Sqrt(1.0f + g * g);
        //
        //            slopeMap.SetValue(x, height - 1 - y, slope);
        //            parentNode._progress = (float)(x + width * y) / ((height + 1) * (y + 1));
        //        }
        //    }
        //
        //    slopeMap.SetValue(0, 0, 0);
        //    slopeMap.SetValue(0, 1, 1);
        //
        //    return slopeMap;
        //}

        //public static float GetInterpolatedHeight(ref THeightmap Src, float y, float x)
        //{
        //    int indexX = (int)Math.Floor(x * Src.heightsData.GetLength(0));
        //    int indexY = (int)Math.Floor(y * Src.heightsData.GetLength(1));
        //
        //    float indexXF = x * Src.heightsData.GetLength(0);
        //    float indexYF = y * Src.heightsData.GetLength(1);
        //
        //    if (indexX < 0) indexX = 0;
        //    if (indexXF < 0) indexXF = 0;
        //    if (indexY < 0) indexY = 0;
        //    if (indexYF < 0) indexYF = 0;
        //
        //    float[] neighbors = new float[4];
        //    neighbors[0] = Src.heightsData[indexX, indexY];
        //    neighbors[1] = Src.heightsData[indexX + 1, indexY];
        //    neighbors[2] = Src.heightsData[indexX, indexY + 1];
        //    neighbors[3] = Src.heightsData[indexX + 1, indexY + 1];
        //
        //    Vector3 p1 = new Vector3(indexX, indexY, neighbors[0]);
        //    Vector3 p2 = new Vector3(indexX + 1, indexY, neighbors[1]);
        //    Vector3 p3 = new Vector3(indexX, indexY + 1, neighbors[2]);
        //    Vector3 p4 = new Vector3(indexX + 1, indexY + 1, neighbors[3]);
        //
        //    float result = GetZOnPlane(indexXF, indexYF, p2, p4, p3);
        //    return result;
        //}

        //public static float GetZOnPlane(float x, float y, Vector3 p1, Vector3 p2, Vector3 p3)
        //{
        //    Vector3 abc;
        //    float d;
        //    Vector3 v1 = new Vector3(p1.X - p3.X, p1.Y - p3.Y, p1.Z - p3.Z);
        //    Vector3 v2 = new Vector3(p2.X - p3.X, p2.Y - p3.Y, p2.Z - p3.Z);
        //    abc = new Vector3(v1.Y * v2.Z - v1.Z * v2.Y, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);
        //    d = abc.X * p3.X + abc.Y * p3.Y + abc.Z * p3.Z;
        //    float result = (d - abc.X * x - abc.Y * y) / abc.Z;
        //    return result;
        //}

        //public static TMask FilteredMask(THeightmap heightmap, float areaLatSizeKm, float areaLonSizeKm, float minElevation, float maxElevation, float minSlope, float maxSlope, float topNormal, float bottomNormal, float leftNormal, float rightNormal)
        //{   
        //    TMask result = new TMask(heightmap.heightsData.GetLength(0), heightmap.heightsData.GetLength(1), true);
        //
        //    for (int i = 0; i < result.Width; i++)
        //    {
        //        for (int j = 0; j < result.Height; j++)
        //        {
        //            float latNormalized = j * 1.0f / result.Width;
        //            float lonNormalized = i * 1.0f / result.Height;
        //
        //            if (lonNormalized > rightNormal || lonNormalized < leftNormal || latNormalized > topNormal || latNormalized < bottomNormal)
        //            {
        //                result.Clear(i, j);
        //                continue;
        //            }
        //
        //            float steepness = heightmap.GetSteepness(lonNormalized, latNormalized, areaLatSizeKm * 1000, areaLonSizeKm * 1000);
        //
        //            if (steepness > maxSlope || steepness < minSlope)
        //            {
        //                result.Clear(i, j);
        //                continue;
        //            }
        //
        //            double height = heightmap.GetInterpolatedHeight(lonNormalized, latNormalized);
        //            
        //            if (height > (maxElevation + 50) || height < (minElevation - 50))
        //            {
        //                result.Clear(i, j);
        //                continue;
        //            }
        //        }
        //    }
        //
        //    return result;
        //}


        // Utils
        //----------------------------------------------------------------------------------------------------------------------------------------------------


        public static float[] AutoSetTerraces(int terraceCount, float terraceVariation, float terracePointMin, float terracePointMax)
        {
            float[] controlPoints = new float[terraceCount];

            for (int i = 0; i < terraceCount; i++)
            {
                if (i == 0)
                    controlPoints[i] = terracePointMin;
                else if (i == terraceCount - 1)
                    controlPoints[i] = terracePointMax;
                else
                {
                    double range = terracePointMax - terracePointMin;
                    Random rand = new Random();
                    double random = rand.NextDouble() * (2 * terraceVariation) - terraceVariation;
                    double currentPoint = terracePointMin + (range * (i + random) / terraceCount);
                    controlPoints[i] = (float)TUtils.Clamp(terracePointMin, terracePointMax, currentPoint);
                }
            }

            return controlPoints;
        }

        private static void GetNeighborsDelta(float[] afMap, int size, int u32Offset, float[] adNeighborsDelta)
        {
            adNeighborsDelta[0] = afMap[u32Offset - size - 1] - afMap[u32Offset];
            adNeighborsDelta[1] = afMap[u32Offset - size] - afMap[u32Offset];
            adNeighborsDelta[2] = afMap[u32Offset - size + 1] - afMap[u32Offset];
            adNeighborsDelta[3] = afMap[u32Offset - 1] - afMap[u32Offset];
            adNeighborsDelta[4] = afMap[u32Offset + 1] - afMap[u32Offset];
            adNeighborsDelta[5] = afMap[u32Offset + size - 1] - afMap[u32Offset];
            adNeighborsDelta[6] = afMap[u32Offset + size] - afMap[u32Offset];
            adNeighborsDelta[7] = afMap[u32Offset + size + 1] - afMap[u32Offset];
        }

        public static float[] Convert2DTo1DArray(float[,] heights2D, int resolution)
        {
            int arrayWidth = heights2D.GetLength(0);
            int arrayHeight = heights2D.GetLength(1);
            float[] array1D = new float[arrayWidth * arrayHeight];

            for (int i = 0; i < arrayHeight; i++)
                for (int j = 0; j < arrayWidth; j++)
                    array1D[i * resolution + j] = heights2D[j, i];

            return array1D;
        }

        public static float[] Convert2DTo1DArrayCW90(float[,] heights2D, int resolution)
        {
            int arrayWidth = heights2D.GetLength(0);
            int arrayHeight = heights2D.GetLength(1);
            float[] array1D = new float[arrayWidth * arrayHeight];

            for (int i = 0; i < arrayHeight; i++)
                for (int j = 0; j < arrayWidth; j++)
                    array1D[i * resolution + j] = heights2D[arrayHeight - 1 - i, j];

            return array1D;
        }

        public static float[,] Convert1DTo2DArray(float[] heights1D, int width, int height)
        {
            float[,] array2D = new float[width, height];
            int i = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    array2D[y, x] = heights1D[i];
                    i++;
                }
            }

            return array2D;
        }

        public static void CornerCorrection1D(float[] heights, int resolution)
        {
            int padding = 2;
            float currentHeight = 0f;
            float variationUp = -0.008f;
            float variationDown = -0.01f;
            Random r = new Random();

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    if (x < padding)
                    {
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x + padding) + y * resolution];
                        heights[x + y * resolution] = currentHeight + (currentHeight * rnd);
                    }

                    if (x > (resolution - 1) - padding)
                    {
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x - padding) + y * resolution];
                        heights[x + y * resolution] = currentHeight + (currentHeight * rnd);
                    }

                    if (y < padding)
                    {
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[x + (y + padding) * resolution];
                        heights[x + y * resolution] = currentHeight + (currentHeight * rnd);
                    }

                    if (y > (resolution - 1) - padding)
                    {
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[x + (y - padding) * resolution];
                        heights[x + y * resolution] = currentHeight + (currentHeight * rnd);
                    }

                    // 4 Corners
                    if (x < padding && y < padding)
                    {
                        // Bottom Left
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x + padding) + (y + padding) * resolution];
                        heights[x + y * resolution] = currentHeight + (currentHeight * rnd);
                    }

                    if (x < padding && y > (resolution - 1) - padding)
                    {
                        // Top Left
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x + padding) + (y - padding) * resolution];
                        heights[x + y * resolution] = currentHeight + (currentHeight * rnd);
                    }

                    if (x > (resolution - 1) - padding && y < padding)
                    {
                        // Bottom Right
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x - padding) + (y + padding) * resolution];
                        heights[x + y * resolution] = currentHeight + (currentHeight * rnd);
                    }

                    if (x > (resolution - 1) - padding && y > (resolution - 1) - padding)
                    {
                        // Top Right
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x - padding) + (y - padding) * resolution];
                        heights[x + y * resolution] = currentHeight + (currentHeight * rnd);
                    }
                }
            }
        }

        public static void CornerCorrection2D(float[,] heights, int resolution)
        {
            int padding = 1;
            float currentHeight = 0f;
            float variationUp = -0.0008f;
            float variationDown = -0.001f;
            Random r = new Random();

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    if (x < padding)
                    {
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x + padding), y];
                        heights[x, y] = currentHeight + (currentHeight * rnd);
                    }

                    if (x > (resolution - 1) - padding)
                    {
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x - padding), y];
                        heights[x, y] = currentHeight + (currentHeight * rnd);
                    }

                    if (y < padding)
                    {
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[x, (y + padding)];
                        heights[x, y] = currentHeight + (currentHeight * rnd);
                    }

                    if (y > (resolution - 1) - padding)
                    {
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[x, (y - padding)];
                        heights[x, y] = currentHeight + (currentHeight * rnd);
                    }

                    // 4 Corners
                    if (x < padding && y < padding)
                    {
                        // Bottom Left
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x + padding), (y + padding)];
                        heights[x, y] = currentHeight + (currentHeight * rnd);
                    }

                    if (x < padding && y > (resolution - 1) - padding)
                    {
                        // Top Left
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x + padding), (y - padding)];
                        heights[x, y] = currentHeight + (currentHeight * rnd);
                    }

                    if (x > (resolution - 1) - padding && y < padding)
                    {
                        // Bottom Right
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x - padding), (y + padding)];
                        heights[x, y] = currentHeight + (currentHeight * rnd);
                    }

                    if (x > (resolution - 1) - padding && y > (resolution - 1) - padding)
                    {
                        // Top Right
                        float rnd = (float)(r.NextDouble() * (variationDown - variationUp) + variationDown);
                        currentHeight = heights[(x - padding), (y - padding)];
                        heights[x, y] = currentHeight + (currentHeight * rnd);
                    }
                }
            }
        }

        private static float Sign(float v)
        {
            if (v > 0) return 1;
            if (v < 0) return -1;
            return 0;
        }

        private static float Horizontal(float dx, float dy, float dxx, float dyy, float dxy, float limit)
        {
            float kh = -2.0f * (dy * dy * dxx + dx * dx * dyy - dx * dy * dxy);
            kh /= dx * dx + dy * dy;

            if (float.IsInfinity(kh) || float.IsNaN(kh)) kh = 0.0f;

            if (kh < -limit) kh = -limit;
            if (kh > limit) kh = limit;

            kh /= limit;
            kh = kh * 0.5f + 0.5f;

            return kh;
        }

        private static float Vertical(float dx, float dy, float dxx, float dyy, float dxy, float limit)
        {
            float kv = -2.0f * (dx * dx * dxx + dy * dy * dyy + dx * dy * dxy);
            kv /= dx * dx + dy * dy;

            if (float.IsInfinity(kv) || float.IsNaN(kv)) kv = 0.0f;

            if (kv < -limit) kv = -limit;
            if (kv > limit) kv = limit;

            kv /= limit;
            kv = kv * 0.5f + 0.5f;

            return kv;
        }

        private static float Average(float dx, float dy, float dxx, float dyy, float dxy, float limit)
        {
            float kh = Horizontal(dx, dy, dxx, dyy, dxy, limit);
            float kv = Vertical(dx, dy, dxx, dyy, dxy, limit);

            return (kh + kv) * 0.5f;
        }

        private static void FillWaterMap(float amount, float[,] waterMap, int width, int height)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    waterMap[x, y] = amount;
        }

        private static void ComputeOutflow(float[,] waterMap, float[,,] outFlow, float[] heightMap, int width, int height, float TIME)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int xn1 = (x == 0) ? 0 : x - 1;
                    int xp1 = (x == width - 1) ? width - 1 : x + 1;
                    int yn1 = (y == 0) ? 0 : y - 1;
                    int yp1 = (y == height - 1) ? height - 1 : y + 1;

                    float waterHt = waterMap[x, y];
                    float waterHts0 = waterMap[xn1, y];
                    float waterHts1 = waterMap[xp1, y];
                    float waterHts2 = waterMap[x, yn1];
                    float waterHts3 = waterMap[x, yp1];

                    float landHt = heightMap[x + y * width];
                    float landHts0 = heightMap[xn1 + y * width];
                    float landHts1 = heightMap[xp1 + y * width];
                    float landHts2 = heightMap[x + yn1 * width];
                    float landHts3 = heightMap[x + yp1 * width];

                    float diff0 = (waterHt + landHt) - (waterHts0 + landHts0);
                    float diff1 = (waterHt + landHt) - (waterHts1 + landHts1);
                    float diff2 = (waterHt + landHt) - (waterHts2 + landHts2);
                    float diff3 = (waterHt + landHt) - (waterHts3 + landHts3);

                    //out flow is previous flow plus flow for this time step.
                    float flow0 = Math.Max(0, outFlow[x, y, 0] + diff0);
                    float flow1 = Math.Max(0, outFlow[x, y, 1] + diff1);
                    float flow2 = Math.Max(0, outFlow[x, y, 2] + diff2);
                    float flow3 = Math.Max(0, outFlow[x, y, 3] + diff3);

                    float sum = flow0 + flow1 + flow2 + flow3;

                    if (sum > 0.0f)
                    {
                        //If the sum of the outflow flux exceeds the amount in the cell
                        //flow value will be scaled down by a factor K to avoid negative update.
                        float K = waterHt / (sum * TIME);
                        if (K > 1.0f) K = 1.0f;
                        if (K < 0.0f) K = 0.0f;

                        outFlow[x, y, 0] = flow0 * K;
                        outFlow[x, y, 1] = flow1 * K;
                        outFlow[x, y, 2] = flow2 * K;
                        outFlow[x, y, 3] = flow3 * K;
                    }
                    else
                    {
                        outFlow[x, y, 0] = 0.0f;
                        outFlow[x, y, 1] = 0.0f;
                        outFlow[x, y, 2] = 0.0f;
                        outFlow[x, y, 3] = 0.0f;
                    }
                }
            }
        }

        private static void UpdateWaterMap(float[,] waterMap, float[,,] outFlow, int width, int height, int TOP, int LEFT, int BOTTOM, int RIGHT, float TIME)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float flowOUT = outFlow[x, y, 0] + outFlow[x, y, 1] + outFlow[x, y, 2] + outFlow[x, y, 3];
                    float flowIN = 0.0f;

                    //Flow in is inflow from neighour cells. Note for the cell on the left you need 
                    //thats cells flow to the right (ie it flows into this cell)
                    flowIN += (x == 0) ? 0.0f : outFlow[x - 1, y, RIGHT];
                    flowIN += (x == width - 1) ? 0.0f : outFlow[x + 1, y, LEFT];
                    flowIN += (y == 0) ? 0.0f : outFlow[x, y - 1, TOP];
                    flowIN += (y == height - 1) ? 0.0f : outFlow[x, y + 1, BOTTOM];

                    float ht = waterMap[x, y] + (flowIN - flowOUT) * TIME;
                    if (ht < 0.0f) ht = 0.0f;

                    //Result is net volume change over time
                    waterMap[x, y] = ht;
                }
            }
        }

        private static void CalculateVelocityField(float[,] velocityMap, float[,,] outFlow, int width, int height, int TOP, int LEFT, int BOTTOM, int RIGHT)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dl = (x == 0) ? 0.0f : outFlow[x - 1, y, RIGHT] - outFlow[x, y, LEFT];
                    float dr = (x == width - 1) ? 0.0f : outFlow[x, y, RIGHT] - outFlow[x + 1, y, LEFT];
                    float dt = (y == height - 1) ? 0.0f : outFlow[x, y + 1, BOTTOM] - outFlow[x, y, TOP];
                    float db = (y == 0) ? 0.0f : outFlow[x, y, BOTTOM] - outFlow[x, y - 1, TOP];
                    float vx = (dl + dr) * 0.5f;
                    float vy = (db + dt) * 0.5f;

                    velocityMap[x, y] = (float)Math.Sqrt(vx * vx + vy * vy);
                }
            }
        }

        public static void NormalizeMap(float[,] map, int width, int height)
        {
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float v = map[x, y];
                    if (v < min) min = v;
                    if (v > max) max = v;
                }
            }

            float size = max - min;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float v = map[x, y];

                    if (size < 1e-12f)
                        v = 0;
                    else
                        v = (v - min) / size;

                    map[x, y] = v;
                }
            }
        }

        private struct HeightAndGradient
        {
            public float height;
            public float gradientX;
            public float gradientY;
        }

        private static HeightAndGradient CalculateHeightAndGradient(float[] nodes, int mapSize, float posX, float posY)
        {
            int coordX = (int)posX;
            int coordY = (int)posY;

            // Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
            float x = posX - coordX;
            float y = posY - coordY;

            // Calculate heights of the four nodes of the droplet's cell
            int nodeIndexNW = coordY * mapSize + coordX;
            float heightNW = nodes[nodeIndexNW];
            float heightNE = nodes[nodeIndexNW + 1];
            float heightSW = nodes[nodeIndexNW + mapSize];
            float heightSE = nodes[nodeIndexNW + mapSize + 1];

            // Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
            float gradientX = (heightNE - heightNW) * (1 - y) + (heightSE - heightSW) * y;
            float gradientY = (heightSW - heightNW) * (1 - x) + (heightSE - heightNE) * x;

            // Calculate height with bilinear interpolation of the heights of the nodes of the cell
            float height = heightNW * (1 - x) * (1 - y) + heightNE * x * (1 - y) + heightSW * (1 - x) * y + heightSE * x * y;

            return new HeightAndGradient() { height = height, gradientX = gradientX, gradientY = gradientY };
        }

        private static void InitializeBrushIndices(int mapSize, int radius, out int[][] erosionBrushIndices, out float[][] erosionBrushWeights)
        {
            erosionBrushIndices = new int[mapSize * mapSize][];
            erosionBrushWeights = new float[mapSize * mapSize][];
            int[] xOffsets = new int[radius * radius * 4];
            int[] yOffsets = new int[radius * radius * 4];
            float[] weights = new float[radius * radius * 4];
            float weightSum = 0;
            int addIndex = 0;

            for (int i = 0; i < erosionBrushIndices.GetLength(0); i++)
            {
                int centreX = i % mapSize;
                int centreY = i / mapSize;

                if (centreY <= radius || centreY >= mapSize - radius || centreX <= radius + 1 || centreX >= mapSize - radius)
                {
                    weightSum = 0;
                    addIndex = 0;
                    for (int y = -radius; y <= radius; y++)
                    {
                        for (int x = -radius; x <= radius; x++)
                        {
                            float sqrDst = x * x + y * y;
                            if (sqrDst < radius * radius)
                            {
                                int coordX = centreX + x;
                                int coordY = centreY + y;

                                if (coordX >= 0 && coordX < mapSize && coordY >= 0 && coordY < mapSize)
                                {
                                    float weight = 1 - (float)Math.Sqrt(sqrDst) / radius;
                                    weightSum += weight;
                                    weights[addIndex] = weight;
                                    xOffsets[addIndex] = x;
                                    yOffsets[addIndex] = y;
                                    addIndex++;
                                }
                            }
                        }
                    }
                }

                int numEntries = addIndex;
                erosionBrushIndices[i] = new int[numEntries];
                erosionBrushWeights[i] = new float[numEntries];

                for (int j = 0; j < numEntries; j++)
                {
                    erosionBrushIndices[i][j] = (yOffsets[j] + centreY) * mapSize + xOffsets[j] + centreX;
                    erosionBrushWeights[i][j] = weights[j] / weightSum;
                }
            }
        }
    }
}
#endif
#endif

