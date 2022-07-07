#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Numerics;
using TerraUnity.Utils;

namespace TerraUnity.Edittime
{
    public static class TLandcoverProccessor
    {
        public struct TriAngle
        {
            public TGlobalPoint P1;
            public TGlobalPoint P2;
            public TGlobalPoint P3;
        }

        public static List<T2DObject> areaBiomes;

        //public static void FilterLakesBordersByBoundArea(TAreaBounds boundingBox, TMap Map, ref TLakeLayer lakeLayer, float minSizeInM2)
        public static void FilterLakesBordersByBoundArea(TMap Map, ref TLakeLayer lakeLayer, float minSizeInM2)
        {
            THeightmap temp = new THeightmap(Map.Heightmap.heightsData);
            if (lakeLayer == null || lakeLayer.LakesList.Count == 0) return;

            for (int j = 0; j < lakeLayer.LakesList.Count; j++)
            {
                T2DObject Lake = lakeLayer.LakesList[j];
                bool LakeIsOK = true;

                //for (int i = 0; i < Lake.AroundPoints.Count; i++)
                //{
                //    TGlobalPoint node = Lake.AroundPoints[i];
                //    float NodeHeight = (float)Map.GetHeight(Lake.AroundPoints[i]);
                //
                //    if (node.latitude > boundingBox.top) LakeIsOK = false;
                //    if (node.latitude < boundingBox.bottom) LakeIsOK = false;
                //    if (node.longitude > boundingBox.right) LakeIsOK = false;
                //    if (node.longitude < boundingBox.left) LakeIsOK = false;
                //    if (NodeHeight < boundingBox.minElevation) LakeIsOK = false;
                //    if (NodeHeight > boundingBox.maxElevation) LakeIsOK = false;
                //}

                if (Lake.GetAreaInM2() < minSizeInM2) LakeIsOK = false;

                if (!LakeIsOK)
                    lakeLayer.LakesList.Remove(lakeLayer.LakesList[j--]);
            }
        }

        //public static void FilterOceansBordersByBoundArea(TAreaBounds boundingBox, TMap Map, ref TOceanLayer oceanLayer)
        //{
        //    int i, j, k;
        //    int boundLineIndex, enterBoundLineIndex, exitBoundLineIndex;
        //    int firstEnterBoundLineIndex, firstExitBoundLineIndex;
        //    int intersectionCount;
        //    bool prevPointInBoundFlag, pointInBoundFlag, OuterExistFlag;
        //    List<T2DPoint> boundIntersectionPoint = new List<T2DPoint>();
        //    List<TLine> lineBound = new List<TLine>();
        //
        //    if (oceanLayer != null && oceanLayer.Coastlines.Count > 0)
        //    {
        //        if ((boundingBox.left - Map._area._left) > 2e-5 && (Map._area._right - boundingBox.right) > 2e-5 &&
        //            (boundingBox.bottom - Map._area._bottom) > 2e-5 && (Map._area._top - boundingBox.top) > 2e-5)
        //        {
        //            lineBound.Clear();
        //            lineBound.Add(new TLine(boundingBox.right, boundingBox.top, boundingBox.right, boundingBox.bottom));
        //            lineBound.Add(new TLine(boundingBox.right, boundingBox.bottom, boundingBox.left, boundingBox.bottom));
        //            lineBound.Add(new TLine(boundingBox.left, boundingBox.bottom, boundingBox.left, boundingBox.top));
        //            lineBound.Add(new TLine(boundingBox.left, boundingBox.top, boundingBox.right, boundingBox.top));
        //            OuterExistFlag = false;
        //
        //            for (i = 0; i < oceanLayer.Coastlines.Count; i++)
        //            {
        //                T2DObject ocean = oceanLayer.Coastlines[i];
        //                prevPointInBoundFlag = false;
        //                firstEnterBoundLineIndex = -1;
        //                firstExitBoundLineIndex = -1;
        //                enterBoundLineIndex = -1;
        //                exitBoundLineIndex = -1;
        //                intersectionCount = 0;
        //                List<TGlobalPoint> AroundPoints = new List<TGlobalPoint>();
        //
        //                for (j = 0; j < ocean.AroundPoints.Count; j++)
        //                {
        //                    float NodeHeight = (float)Map.GetHeight(ocean.AroundPoints[j]);
        //                    pointInBoundFlag = PointInBoundArea(ocean.AroundPoints[j], boundingBox, NodeHeight);
        //
        //                    if (j == 0) prevPointInBoundFlag = pointInBoundFlag;
        //
        //                    if ((pointInBoundFlag == true && prevPointInBoundFlag == false) ||
        //                        (pointInBoundFlag == false && prevPointInBoundFlag == true))
        //                    {
        //                        TLine line = new TLine(ocean.AroundPoints[j - 1].longitude, ocean.AroundPoints[j - 1].latitude,
        //                                                ocean.AroundPoints[j].longitude, ocean.AroundPoints[j].latitude);
        //                        boundLineIndex = FindIntersectionLineWithAreaBound(line, lineBound, ref boundIntersectionPoint);
        //
        //                        if (boundLineIndex >= 0)
        //                        {
        //                            intersectionCount++;
        //
        //                            if (ocean.property == Property.Inner)
        //                            {
        //                                if (intersectionCount <= 2)
        //                                    AddEpsilonToBoundaryIntersectionPoint(boundIntersectionPoint, boundingBox, 1e-10);
        //                                else
        //                                    AddEpsilonToBoundaryIntersectionPoint(boundIntersectionPoint, boundingBox, 1e-9);
        //                            }
        //
        //                            if (prevPointInBoundFlag == false && pointInBoundFlag == true)
        //                            {
        //                                enterBoundLineIndex = boundLineIndex;
        //
        //                                if (firstEnterBoundLineIndex < 0)
        //                                    firstEnterBoundLineIndex = enterBoundLineIndex;
        //                                if (exitBoundLineIndex >= 0 && exitBoundLineIndex != enterBoundLineIndex)
        //                                    AddBoundCorner(exitBoundLineIndex, enterBoundLineIndex, lineBound, ref AroundPoints, ocean.property);
        //
        //                                for (k = 0; k < boundIntersectionPoint.Count; k++)
        //                                    AroundPoints.Add(new TGlobalPoint(boundIntersectionPoint[k].y, boundIntersectionPoint[k].x));
        //                            }
        //                            else if (prevPointInBoundFlag == true && pointInBoundFlag == false)
        //                            {
        //                                for (k = 0; k < boundIntersectionPoint.Count; k++)
        //                                    AroundPoints.Add(new TGlobalPoint(boundIntersectionPoint[k].y, boundIntersectionPoint[k].x));
        //
        //                                exitBoundLineIndex = boundLineIndex;
        //
        //                                if (firstExitBoundLineIndex < 0)
        //                                    firstExitBoundLineIndex = exitBoundLineIndex;
        //                                if (enterBoundLineIndex >= 0 && exitBoundLineIndex != enterBoundLineIndex && ocean.property == Property.Inner)
        //                                    AddBoundCorner(exitBoundLineIndex, enterBoundLineIndex, lineBound, ref AroundPoints, ocean.property);
        //                            }
        //                        }
        //
        //                        if (prevPointInBoundFlag == false && pointInBoundFlag == true)
        //                        {
        //                            AroundPoints.Add(ocean.AroundPoints[j]);
        //                            prevPointInBoundFlag = true;
        //                        }
        //                        else if (prevPointInBoundFlag == true && pointInBoundFlag == false)
        //                            prevPointInBoundFlag = false;
        //                    }
        //                    else if (pointInBoundFlag == true && prevPointInBoundFlag == true)
        //                        AroundPoints.Add(ocean.AroundPoints[j]);
        //                }
        //
        //                if (ocean.property == Property.Outer)
        //                {
        //                    if (firstEnterBoundLineIndex >= 0 && firstEnterBoundLineIndex != exitBoundLineIndex)
        //                    {
        //                        if (exitBoundLineIndex >= 0)
        //                        {
        //                            if (OuterExistFlag == false)
        //                            {
        //                                OuterExistFlag = true;
        //                                AddBoundCorner(exitBoundLineIndex, firstEnterBoundLineIndex, lineBound, ref AroundPoints, ocean.property);
        //                            }
        //                        }
        //                    }
        //                    else if (firstExitBoundLineIndex >= 0 && firstExitBoundLineIndex != exitBoundLineIndex)
        //                    {
        //                        if (enterBoundLineIndex >= 0)
        //                        {
        //                            if (OuterExistFlag == false)
        //                            {
        //                                OuterExistFlag = true;
        //                                AddBoundCorner(firstExitBoundLineIndex, exitBoundLineIndex, lineBound, ref AroundPoints, ocean.property);
        //                            }
        //                        }
        //                    }
        //
        //                    if (AroundPoints[0].latitude != ocean.AroundPoints[AroundPoints.Count - 1].latitude ||
        //                        AroundPoints[0].longitude != ocean.AroundPoints[AroundPoints.Count - 1].longitude)
        //                    {
        //                        AroundPoints.Add(new TGlobalPoint(AroundPoints[0].latitude, AroundPoints[0].longitude));
        //                    }
        //                }
        //
        //                if (AroundPoints.Count > 3)
        //                {
        //                    if (ocean.AroundPoints.Count != AroundPoints.Count)
        //                        ocean.AroundPoints = AroundPoints;
        //                }
        //                else
        //                    oceanLayer.Coastlines.RemoveAt(i--);
        //            }
        //
        //            OuterExistFlag = false;
        //
        //            for (i = 0; i < oceanLayer.Coastlines.Count; i++)
        //            {
        //                if (oceanLayer.Coastlines[i].property == Property.Outer)
        //                {
        //                    OuterExistFlag = true;
        //                    break;
        //                }
        //            }
        //
        //            if (OuterExistFlag == false)
        //            {
        //                List<TGlobalPoint> AroundPoints = new List<TGlobalPoint>();
        //                AroundPoints.Add(new TGlobalPoint(lineBound[0].p1.y, lineBound[0].p1.x));
        //                AddBoundCorner(0, 4, lineBound, ref AroundPoints, Property.Outer);
        //                T2DObject ocean = new T2DObject();
        //                ocean.property = Property.Outer;
        //                ocean.name = "Ocean";
        //                ocean.AroundPoints = AroundPoints;
        //                oceanLayer.Coastlines.Add(ocean);
        //            }
        //        }
        //    }
        //}

        public static void AddBoundCorner(int boundLineIndex1, int boundLineIndex2, List<TLine> lineBound, 
                                          ref List<TGlobalPoint> AroundPoints, Property property)
        {
            if (property == Property.Outer)
            {
                if (boundLineIndex1 < boundLineIndex2)
                {
                    for (int i = boundLineIndex1; i < boundLineIndex2; i++)
                        AroundPoints.Add(new TGlobalPoint(lineBound[i].p2.y, lineBound[i].p2.x));
                }
                else
                {
                    for (int i = boundLineIndex1; i < 4; i++)
                        AroundPoints.Add(new TGlobalPoint(lineBound[i].p2.y, lineBound[i].p2.x));

                    for (int i = 0; i < boundLineIndex2; i++)
                        AroundPoints.Add(new TGlobalPoint(lineBound[i].p2.y, lineBound[i].p2.x));
                }
            }
            else if (property == Property.Inner)
            {
                if (boundLineIndex1 > boundLineIndex2)
                {
                    for (int i = boundLineIndex1; i > boundLineIndex2; i--)
                        AroundPoints.Add(new TGlobalPoint(lineBound[i].p1.y, lineBound[i].p1.x));
                }
                else if (boundLineIndex1 < boundLineIndex2)
                {
                    for (int i = boundLineIndex1; i >= 0; i--)
                        AroundPoints.Add(new TGlobalPoint(lineBound[i].p1.y, lineBound[i].p1.x));

                    for (int i = 3; i > boundLineIndex2; i--)
                        AroundPoints.Add(new TGlobalPoint(lineBound[i].p1.y, lineBound[i].p1.x));
                }
            }
        }

        //public static bool PointInBoundArea(TGlobalPoint point, TAreaBounds boundingBox, float NodeHeight)
        //{
        //    bool pointInBoundFlag = false;
        //
        //    if (point.latitude >= boundingBox.bottom && point.latitude <= boundingBox.top &&
        //        point.longitude >= boundingBox.left && point.longitude <= boundingBox.right &&
        //        NodeHeight >= boundingBox.minElevation && NodeHeight <= boundingBox.maxElevation)
        //    {
        //        pointInBoundFlag = true;
        //    }
        //
        //    return pointInBoundFlag;
        //}

        private static int FindIntersectionLineWithAreaBound(TLine line, List<TLine> lineBound, ref List<T2DPoint> boundIntersectionPoint)
        {
            int boundLineIndex = -1;
            boundIntersectionPoint.Clear();

            for (int i = 0; i < 4; i++)
            {
                if (line.CalcIntersection(lineBound[i], ref boundIntersectionPoint))
                {
                    boundLineIndex = i;
                    break;
                }
            }

            return boundLineIndex;
        }

        //private static void AddEpsilonToBoundaryIntersectionPoint(List<T2DPoint> boundIntersectionPoint, TAreaBounds boundingBox, double epsilon)
        //{
        //    for (int k = 0; k < boundIntersectionPoint.Count; k++)
        //    {
        //        if ((float)boundIntersectionPoint[k].x == (float)boundingBox.right)        //  Right Bound
        //            boundIntersectionPoint[k].x -= epsilon;
        //        else if ((float)boundIntersectionPoint[k].x == (float)boundingBox.left)    //  left Bound
        //            boundIntersectionPoint[k].x += epsilon;
        //        else if ((float)boundIntersectionPoint[k].y == (float)boundingBox.bottom)  //  Bottom Bound
        //            boundIntersectionPoint[k].y += epsilon;
        //        else if ((float)boundIntersectionPoint[k].y == (float)boundingBox.top)     //  Top Bound
        //            boundIntersectionPoint[k].y -= epsilon;
        //    }
        //}

        //public static void FilterRiverBordersByBoundArea(TAreaBounds boundingBox, TMap Map, ref TLinearMeshLayer linearLayer)
        //{
        //    THeightmap temp = new THeightmap(Map.Heightmap.heightsData);
        //    if (linearLayer == null || linearLayer.Lines.Count == 0) return;
        //
        //    for (int j = 0; j < linearLayer.Lines.Count; j++)
        //    {
        //        TLinearObject river = linearLayer.Lines[j];
        //        bool riverIsOK = true;
        //
        //        for (int i = 0; i < river.Points.Count; i++)
        //        {
        //            TGlobalPoint node = river.Points[i];
        //            float NodeHeight = (float)Map.GetHeight(river.Points[i]);
        //        
        //            if (node.latitude > boundingBox.top) riverIsOK = false;
        //            if (node.latitude < boundingBox.bottom) riverIsOK = false;
        //            if (node.longitude > boundingBox.right) riverIsOK = false;
        //            if (node.longitude < boundingBox.left) riverIsOK = false;
        //            if (NodeHeight < boundingBox.minElevation) riverIsOK = false;
        //            if (NodeHeight > boundingBox.maxElevation) riverIsOK = false;
        //        }
        //
        //        if (!riverIsOK)
        //            linearLayer.Lines.Remove(linearLayer.Lines[j--]);
        //    }
        //}

        //public static void FilterBiomesBordersByBoundArea(TAreaBounds boundingBox, TMap Map, ref List<T2DObject> Areas , float minSizeInM2)
        public static void FilterBiomesBordersByBoundArea(TMap Map, ref List<T2DObject> Areas, float minSizeInM2)
        {
            THeightmap temp = new THeightmap(Map.Heightmap.heightsData);
            if (Areas.Count == 0) return;

            for (int j = 0; j < Areas.Count; j++)
            {
                T2DObject BiomeArea = Areas[j];
                bool IsOK = true;

                //for (int i = 0; i < BiomeArea.AroundPoints.Count; i++)
                //{
                //    TGlobalPoint node = BiomeArea.AroundPoints[i];
                //    float NodeHeight = (float)Map.GetHeight(BiomeArea.AroundPoints[i]);
                //
                //    if (node.latitude > boundingBox.top) IsOK = false;
                //    if (node.latitude < boundingBox.bottom) IsOK = false;
                //    if (node.longitude > boundingBox.right) IsOK = false;
                //    if (node.longitude < boundingBox.left) IsOK = false;
                //    if (NodeHeight < boundingBox.minElevation) IsOK = false;
                //    if (NodeHeight > boundingBox.maxElevation) IsOK = false;
                //}

                if (BiomeArea.GetAreaInM2() < minSizeInM2) IsOK = false;

                if (!IsOK)
                    Areas.Remove(Areas[j--]);
            }
        }

        //public static void GenerateGrid(TTerrain terrain, TMask mask, ref TGridLayer gridLayer, TAreaBounds bound, int GridGount, bool seperatedObject)
        public static void GenerateGrid(TTerrain terrain, TMask mask, ref TGridLayer gridLayer, int GridGount, bool seperatedObject)
        {
            //double activeAreaLeft = (bound.left > terrain.Map._area._left ? bound.left : terrain.Map._area._left);
            //double activeAreaRight = (bound.right < terrain.Map._area._right ? bound.right : terrain.Map._area._right);
            //double activeAreaTop = (bound.top < terrain.Map._area._top ? bound.top : terrain.Map._area._top);
            //double activeAreaBottom = (bound.bottom > terrain.Map._area._bottom ? bound.bottom : terrain.Map._area._bottom);
            double activeAreaLeft = terrain.Map._area._left;
            double activeAreaRight = terrain.Map._area._right;
            double activeAreaTop = terrain.Map._area._top;
            double activeAreaBottom = terrain.Map._area._bottom;

            TArea activearea = new TArea(activeAreaTop, activeAreaLeft, activeAreaBottom, activeAreaRight);
            float AreaWidthSize = activearea._areaSizeLon;
            float AreaLenghtSize = activearea._areaSizeLat;

            double HorizontalStep = (activeAreaRight - activeAreaLeft)  / (AreaLenghtSize * 1000 / gridLayer.KM2Resulotion);
            double verticalStep = (activeAreaTop - activeAreaBottom)  / (AreaWidthSize * 1000 / gridLayer.KM2Resulotion);

            double lat, lon, normalizedLat, normalizedLon;
            bool maskcheck1 = true;
            bool maskcheck2 = true;
            bool maskcheck3 = true;
            bool maskcheck4 = true;
            //int vertexCount = 0;
            int stepCountx = 0;
            int stepCounty = 0;
            T2DGrid _activeGrid = null;
            //vertexCount = 0;
            List<TriAngle> Triangles = new List<TriAngle>();
            
            //Extract Points
            for (double x = activeAreaLeft; x < activeAreaRight - HorizontalStep; x = x + (HorizontalStep))
            {
                stepCountx++;

                for (double y = activeAreaBottom; y < activeAreaTop - verticalStep; y = y + (verticalStep))
                {
                    stepCounty++;
                    if (!TTerrainGenerator.WorldInProgress) return;

                    //Triangle 1
                    maskcheck1 = false;
                    maskcheck2 = false;
                    maskcheck3 = false;
                    maskcheck4 = false;

                    //Point 1
                    lon = x;
                    lat = y;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck1 = mask.CheckNormal(normalizedLon, normalizedLat);

                    //Point 4
                    lon = x + HorizontalStep;
                    lat = y;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck4 = mask.CheckNormal(normalizedLon, normalizedLat);

                    if (!maskcheck1 && !maskcheck4) continue;

                    //Point 3
                    lon = x + HorizontalStep;
                    lat = y + verticalStep;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck3 = mask.CheckNormal(normalizedLon, normalizedLat);


                    //Point 2
                    lon = x;
                    lat = y + verticalStep;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck2 = mask.CheckNormal(normalizedLon, normalizedLat);

                    //Point 4
                    lon = x + HorizontalStep;
                    lat = y;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck4 = mask.CheckNormal(normalizedLon, normalizedLat);

                    // Select Grid
                    if (maskcheck1 && maskcheck2 && maskcheck3)
                    {
                        TriAngle triAngle = new TriAngle();
                        triAngle.P1 = new TGlobalPoint(y, x);
                        triAngle.P2 = new TGlobalPoint(y + verticalStep, x);
                        triAngle.P3 = new TGlobalPoint(y + verticalStep, x + HorizontalStep);
                        Triangles.Add(triAngle);
                    }

                    if (maskcheck1 && maskcheck3 && maskcheck4)
                    {
                        TriAngle triAngle = new TriAngle();
                        triAngle.P1 = new TGlobalPoint(y, x);
                        triAngle.P2 = new TGlobalPoint(y + verticalStep, x + HorizontalStep);
                        triAngle.P3 = new TGlobalPoint(y, x + HorizontalStep);
                        Triangles.Add(triAngle);
                    }

                    if (maskcheck1 && maskcheck2 && maskcheck4  && !maskcheck3)
                    {
                        TriAngle triAngle = new TriAngle();
                        triAngle.P1 = new TGlobalPoint(y, x);
                        triAngle.P2 = new TGlobalPoint(y + verticalStep, x);
                        triAngle.P3 = new TGlobalPoint(y, x + HorizontalStep);
                        Triangles.Add(triAngle);
                    }

                    if (!maskcheck1 && maskcheck2 && maskcheck3 && maskcheck4)
                    {
                        TriAngle triAngle = new TriAngle();
                        triAngle.P1 = new TGlobalPoint(y + verticalStep, x);
                        triAngle.P2 = new TGlobalPoint(y + verticalStep, x + HorizontalStep);
                        triAngle.P3 = new TGlobalPoint(y, x + HorizontalStep);
                        Triangles.Add(triAngle);
                    }
                }
            }

            double maxDistance = (HorizontalStep > verticalStep) ? HorizontalStep : verticalStep;
            maxDistance = maxDistance * 2;
            float bypassDensityValue = (gridLayer.density * 1.0f / 100);
            int insertedTriangle = 0;
            Random rnd = new Random(gridLayer.density);

            if (!seperatedObject)
            {
                int tilesCount = (int)Math.Pow(GridGount, 2);

            for (int i = 0; i < tilesCount; i++)
            {
                T2DGrid _grid = new T2DGrid();
                gridLayer.GridsList.Add(_grid);
            }

                for (int i = 0; i < Triangles.Count; i++)
                {
                  //  if ((insertedTriangle * 1.0f / (i + 1)) > (bypassDensityValue))
                  //  {
                  //      continue;
                  //  }
                    if ( rnd.NextDouble() > (bypassDensityValue))
                    {
                        continue;
                    }
                    double XIndex = ((Triangles[i].P1.longitude - activeAreaLeft) * 1.0d / (activeAreaRight - activeAreaLeft)) * GridGount;
                    double YIndex = ((Triangles[i].P1.latitude - activeAreaBottom) * 1.0d / (activeAreaTop - activeAreaBottom)) * GridGount;

                    int xIndex = Convert.ToInt32(Math.Floor(XIndex));
                    int yIndex = Convert.ToInt32(Math.Floor(YIndex));

                    if (xIndex < 0) xIndex = 0;
                    if (xIndex >= GridGount) xIndex = GridGount - 1;
                    if (yIndex < 0) yIndex = 0;
                    if (yIndex >= GridGount) yIndex = GridGount - 1;

                    int gridIndex = yIndex * (int)GridGount + xIndex;
                    _activeGrid = gridLayer.GridsList[gridIndex];
                    _activeGrid.AddTriangle(Triangles[i].P1, Triangles[i].P2, Triangles[i].P3);
                    insertedTriangle++;
                }
            }
            else
            {
                for (int i = 0; i < Triangles.Count; i++)
                {
                //    if ((insertedTriangle * 1.0f / (i + 1)) > bypassDensityValue)
                //    {
                //        continue;
                //    }
                    if (rnd.NextDouble() > (bypassDensityValue))
                    {
                        continue;
                    }
                    int gridIndex = gridLayer.GetGridofPoint(Triangles[i].P1);
                    if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(Triangles[i].P2);
                    if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(Triangles[i].P3);

                    if (gridIndex < 0)
                    {
                        T2DGrid _tempgrid = new T2DGrid();
                        gridLayer.GridsList.Add(_tempgrid);
                        _activeGrid = gridLayer.GridsList[gridLayer.GridsList.Count - 1];
                    }
                    else
                        _activeGrid = gridLayer.GridsList[gridIndex];

                    _activeGrid.AddTriangle(Triangles[i].P1, Triangles[i].P2, Triangles[i].P3);
                    insertedTriangle++;
                }
            }
            //    }

            for (int i = 0; i < gridLayer.GridsList.Count; i++)
            {
                T2DGrid _grid = gridLayer.GridsList[i];

                if (_grid.TrianglesList.Count == 0)
                {
                    gridLayer.GridsList.Remove(_grid);
                    i--;
                }
            }

/*
            if (!seperatedObject)
            {
                int tilesCount = (int)Math.Pow(GridGount, 2);

                for (int i = 0; i < tilesCount; i++)
                {
                    T2DGrid _grid = new T2DGrid();
                    gridLayer.GridsList.Add(_grid);
                }
                _activeGrid = gridLayer.GridsList[0];
            }

            for (double x = activeAreaLeft; x < activeAreaRight - HorizontalStep; x = x + (HorizontalStep))
            {
                stepCountx++;
            
                for (double y = activeAreaBottom; y < activeAreaTop - verticalStep; y = y + (verticalStep))
                {
                    stepCounty++;
                    if (!TTerrainGenerator.WorldInProgress) return;
                        
                    //Triangle 1
                    maskcheck1 = true;
                    maskcheck2 = true;
                    maskcheck3 = true;
                    maskcheck4 = true;
            
                    //Point 1
                    lon = x;
                    lat = y;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck1 = mask.CheckNormal(normalizedLon, normalizedLat);
                    if (!maskcheck1) continue;
                        
                    //Point 3
                    lon = x + HorizontalStep;
                    lat = y + verticalStep;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck3 = mask.CheckNormal(normalizedLon, normalizedLat);
            
                        
                    //Point 2
                    lon = x;
                    lat = y + verticalStep;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck2 = mask.CheckNormal(normalizedLon, normalizedLat);
                        
                    //Point 4
                    lon = x + HorizontalStep;
                    lat = y;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck4 = mask.CheckNormal(normalizedLon, normalizedLat);
            
                    // Select Grid
                    if (maskcheck1 && maskcheck2 && maskcheck3)
                    {
                        vertexCount++;
                        TGlobalPoint P1 = new TGlobalPoint(y, x);
                        TGlobalPoint P2 = new TGlobalPoint(y + verticalStep, x);
                        TGlobalPoint P3 = new TGlobalPoint(y + verticalStep, x + HorizontalStep);
            
                        if (seperatedObject)
                        {
                            int gridIndex = gridLayer.GetGridofPoint(P1);
                            if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(P2);
                            if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(P3);
            
                            if (gridIndex < 0)
                            {
                                T2DGrid _tempgrid = new T2DGrid();
                                gridLayer.GridsList.Add(_tempgrid);
                                _activeGrid = gridLayer.GridsList[gridLayer.GridsList.Count - 1];
                            }
                            else
                                _activeGrid = gridLayer.GridsList[gridIndex];
                        }
                        else
                        {
                            double XIndex = ((x - activeAreaLeft) / (activeAreaRight - HorizontalStep - activeAreaLeft)) * GridGount;
                            double YIndex = ((y - activeAreaBottom) / (activeAreaTop - verticalStep - activeAreaBottom)) * GridGount;
            
                            int xIndex = Convert.ToInt32(Math.Floor(XIndex));
                            int yIndex = Convert.ToInt32(Math.Floor(YIndex));
                                
                            //int gridIndex = yIndex * Convert.ToInt32(Math.Sqrt(GridGount)) + xIndex;
                            int gridIndex = yIndex * (int)GridGount + xIndex;
            
                            _activeGrid = gridLayer.GridsList[gridIndex];
                        }
            
                        _activeGrid.AddTriangle(P1, P2, P3);
                    }
            
                    if (maskcheck1 && maskcheck3 && maskcheck4)
                    {
                        vertexCount++;
                        TGlobalPoint P1 = new TGlobalPoint(y, x);
                        TGlobalPoint P3 = new TGlobalPoint(y + verticalStep, x + HorizontalStep);
                        TGlobalPoint P4 = new TGlobalPoint(y, x + HorizontalStep);
            
                        if (seperatedObject)
                        {
                            int gridIndex = gridLayer.GetGridofPoint(P1);
                            if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(P3);
                            if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(P4);
            
                            if (gridIndex < 0)
                            {
                                T2DGrid _tempgrid = new T2DGrid();
                                gridLayer.GridsList.Add(_tempgrid);
                                _activeGrid = gridLayer.GridsList[gridLayer.GridsList.Count - 1];
                            }
                            else
                                _activeGrid = gridLayer.GridsList[gridIndex];
                        }
                        else 
                        {
                            double XIndex = ((x - activeAreaLeft) / (activeAreaRight - HorizontalStep - activeAreaLeft)) * GridGount;
                            double YIndex = ((y - activeAreaBottom) / (activeAreaTop - verticalStep - activeAreaBottom)) * GridGount;
            
                            int xIndex = Convert.ToInt32(Math.Floor(XIndex));
                            int yIndex = Convert.ToInt32(Math.Floor(YIndex));
                                
                            //int gridIndex = yIndex * Convert.ToInt32(Math.Sqrt(GridGount)) + xIndex;
                            int gridIndex = yIndex * (int)GridGount + xIndex;
            
                            _activeGrid = gridLayer.GridsList[gridIndex];
                        }
                        _activeGrid.AddTriangle(P1, P3, P4);
                    }

                    if (maskcheck1 && maskcheck3 && maskcheck4)
                    {
                        vertexCount++;
                        TGlobalPoint P2 = new TGlobalPoint(y + verticalStep, x);
                        TGlobalPoint P3 = new TGlobalPoint(y + verticalStep, x + HorizontalStep);
                        TGlobalPoint P4 = new TGlobalPoint(y, x + HorizontalStep);

                        if (seperatedObject)
                        {
                            int gridIndex = gridLayer.GetGridofPoint(P2);
                            if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(P3);
                            if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(P4);

                            if (gridIndex < 0)
                            {
                                T2DGrid _tempgrid = new T2DGrid();
                                gridLayer.GridsList.Add(_tempgrid);
                                _activeGrid = gridLayer.GridsList[gridLayer.GridsList.Count - 1];
                            }
                            else
                                _activeGrid = gridLayer.GridsList[gridIndex];
                        }
                        else
                        {
                            double XIndex = ((x - activeAreaLeft) / (activeAreaRight - HorizontalStep - activeAreaLeft)) * GridGount;
                            double YIndex = ((y - activeAreaBottom) / (activeAreaTop - verticalStep - activeAreaBottom)) * GridGount;

                            int xIndex = Convert.ToInt32(Math.Floor(XIndex));
                            int yIndex = Convert.ToInt32(Math.Floor(YIndex));

                            //int gridIndex = yIndex * Convert.ToInt32(Math.Sqrt(GridGount)) + xIndex;
                            int gridIndex = yIndex * (int)GridGount + xIndex;

                            _activeGrid = gridLayer.GridsList[gridIndex];
                        }
                        _activeGrid.AddTriangle(P2, P3, P4);
                    }
                }
            }

            for (int i = 0; i < gridLayer.GridsList.Count; i++)
            {
                T2DGrid _grid = gridLayer.GridsList[i];
                if (_grid.TrianglesList.Count == 0)
                {
                    gridLayer.GridsList.Remove(_grid);
                    i--;
                }
            }

*/

            //--------------------------------------------------   Version 2 ------------------------------------------/
            /*
            for (int i = 0; i < tilesCount; i++)
            {
                T2DGrid _grid = new T2DGrid();
                gridLayer.GridsList.Add(_grid);
            }

            _activeGrid = gridLayer.GridsList[0];

            for (double x = activeAreaLeft; x < activeAreaRight - HorizontalStep; x = x + (HorizontalStep))
            {
                stepCountx++;

                for (double y = activeAreaBottom; y < activeAreaTop - verticalStep; y = y + (verticalStep))
                {
                    stepCounty++;
                    if (!TTerrainGenerator.WorldInProgress) return;

                    //Triangle 1
                    maskcheck1 = true;
                    maskcheck2 = true;
                    maskcheck3 = true;
                    maskcheck4 = true;

                    //Point 1
                    lon = x;
                    lat = y;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck1 = mask.CheckNormal(normalizedLon, normalizedLat);
                    if (!maskcheck1) continue;

                    //Point 3
                    lon = x + HorizontalStep;
                    lat = y + verticalStep;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck3 = mask.CheckNormal(normalizedLon, normalizedLat);


                    //Point 2
                    lon = x;
                    lat = y + verticalStep;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck2 = mask.CheckNormal(normalizedLon, normalizedLat);

                    //Point 4
                    lon = x + HorizontalStep;
                    lat = y;
                    normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
                    normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
                    if (mask != null) maskcheck4 = mask.CheckNormal(normalizedLon, normalizedLat);

                    // Select Grid
                    if (maskcheck1 && maskcheck2 && maskcheck3)
                    {
                        vertexCount++;
                        TGlobalPoint P1 = new TGlobalPoint(y, x);
                        TGlobalPoint P2 = new TGlobalPoint(y + verticalStep, x);
                        TGlobalPoint P3 = new TGlobalPoint(y + verticalStep, x + HorizontalStep);

                        if (true)
                        {
                            int gridIndex = gridLayer.GetGridofPoint(P1);
                            if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(P2);
                            if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(P3);

                            if (gridIndex < 0)
                            {
                                T2DGrid _tempgrid = new T2DGrid();
                                gridLayer.GridsList.Add(_tempgrid);
                                _activeGrid = gridLayer.GridsList[gridLayer.GridsList.Count - 1];
                            }
                            else
                                _activeGrid = gridLayer.GridsList[gridIndex];
                        }
                        else if (vertexCount > maxVertexCount)
                        {
                            gridLayer.GridsList.Add(_activeGrid);
                            maskcheck1 = true;
                            maskcheck2 = true;
                            maskcheck3 = true;
                            maskcheck4 = true;
                            _activeGrid = new T2DGrid();
                            vertexCount = 0;
                        }
                        else
                        {
                            //double XIndex = ((x - activeAreaLeft) / (activeAreaRight - HorizontalStep - activeAreaLeft)) * Math.Sqrt(GridGount);
                            //double YIndex = ((y - activeAreaBottom) / (activeAreaTop - verticalStep - activeAreaBottom)) * Math.Sqrt(GridGount);
                            double XIndex = ((x - activeAreaLeft) / (activeAreaRight - HorizontalStep - activeAreaLeft)) * GridGount;
                            double YIndex = ((y - activeAreaBottom) / (activeAreaTop - verticalStep - activeAreaBottom)) * GridGount;

                            int xIndex = Convert.ToInt32(Math.Floor(XIndex));
                            int yIndex = Convert.ToInt32(Math.Floor(YIndex));

                            //int gridIndex = yIndex * Convert.ToInt32(Math.Sqrt(GridGount)) + xIndex;
                            int gridIndex = yIndex * (int)GridGount + xIndex;

                            _activeGrid = gridLayer.GridsList[gridIndex];
                        }

                        _activeGrid.AddTriangle(P1, P2, P3);
                    }

                    if (maskcheck1 && maskcheck3 && maskcheck4)
                    {
                        vertexCount++;
                        TGlobalPoint P1 = new TGlobalPoint(y, x);
                        TGlobalPoint P3 = new TGlobalPoint(y + verticalStep, x + HorizontalStep);
                        TGlobalPoint P4 = new TGlobalPoint(y, x + HorizontalStep);

                        if (continoious)
                        {
                            int gridIndex = gridLayer.GetGridofPoint(P1);
                            if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(P3);
                            if (gridIndex < 0) gridIndex = gridLayer.GetGridofPoint(P4);

                            if (gridIndex < 0)
                            {
                                T2DGrid _tempgrid = new T2DGrid();
                                gridLayer.GridsList.Add(_tempgrid);
                                _activeGrid = gridLayer.GridsList[gridLayer.GridsList.Count - 1];
                            }
                            else
                                _activeGrid = gridLayer.GridsList[gridIndex];
                        }
                        else if (vertexCount > maxVertexCount)
                        {
                            gridLayer.GridsList.Add(_activeGrid);
                            maskcheck1 = true;
                            maskcheck2 = true;
                            maskcheck3 = true;
                            maskcheck4 = true;
                            _activeGrid = new T2DGrid();
                            vertexCount = 0;
                        }
                        else 
                        {
                            //double XIndex = ((x - activeAreaLeft) / (activeAreaRight - HorizontalStep - activeAreaLeft)) * Math.Sqrt(GridGount);
                            //double YIndex = ((y - activeAreaBottom) / (activeAreaTop - verticalStep - activeAreaBottom)) * Math.Sqrt(GridGount);
                            double XIndex = ((x - activeAreaLeft) / (activeAreaRight - HorizontalStep - activeAreaLeft)) * GridGount;
                            double YIndex = ((y - activeAreaBottom) / (activeAreaTop - verticalStep - activeAreaBottom)) * GridGount;

                            int xIndex = Convert.ToInt32(Math.Floor(XIndex));
                            int yIndex = Convert.ToInt32(Math.Floor(YIndex));

                            //int gridIndex = yIndex * Convert.ToInt32(Math.Sqrt(GridGount)) + xIndex;
                            int gridIndex = yIndex * (int)GridGount + xIndex;

                            _activeGrid = gridLayer.GridsList[gridIndex];
                        }

                        _activeGrid.AddTriangle(P1, P3, P4);
                    }
                }
            }
             * 
             */


        }

        //public static void ObjectScatter(TTerrain terrain, TMask mask, ref TObjectScatterLayer objectScatterLayer, TAreaBounds bound)
        //public static void ObjectScatter(TTerrain terrain, TMask mask, ref TObjectScatterLayer objectScatterLayer)
        //{
        //    Random rand = new Random(objectScatterLayer.SeedNo);
        //    float random = (float)rand.NextDouble();
        //
        //    //double activeAreaLeft = (bound.left > terrain.Map._area._left ? bound.left : terrain.Map._area._left);
        //    //double activeAreaRight = (bound.right < terrain.Map._area._right ? bound.right : terrain.Map._area._right);
        //    //double activeAreaTop = (bound.top < terrain.Map._area._top ? bound.top : terrain.Map._area._top);
        //    //double activeAreaBottom = (bound.bottom > terrain.Map._area._bottom ? bound.bottom : terrain.Map._area._bottom);
        //    double activeAreaLeft = terrain.Map._area._left;
        //    double activeAreaRight = terrain.Map._area._right;
        //    double activeAreaTop = terrain.Map._area._top;
        //    double activeAreaBottom = terrain.Map._area._bottom;
        //
        //    TArea activearea = new TArea(activeAreaTop, activeAreaLeft, activeAreaBottom, activeAreaRight);
        //    float AreaWidthSize = activearea._areaSizeLon;
        //    float AreaLenghtSize = activearea._areaSizeLat;
        //    double HorizontalStep = (activeAreaRight - activeAreaLeft) / (Math.Sqrt(objectScatterLayer.DensityResolutionPerKilometer) * AreaWidthSize);
        //    double verticalStep = (activeAreaTop - activeAreaBottom) / (Math.Sqrt(objectScatterLayer.DensityResolutionPerKilometer) * AreaLenghtSize);
        //    float NormalizedrandomPosition = objectScatterLayer.PositionVariation/100;
        //    if (NormalizedrandomPosition < 0) NormalizedrandomPosition = 0;
        //    if (NormalizedrandomPosition > 1) NormalizedrandomPosition = 1;
        //    double lat, lon, normalizedLat, normalizedLon;
        //    bool maskcheck = true;
        //
        //    for (double x = activeAreaLeft + (HorizontalStep); x < activeAreaRight- HorizontalStep; x = x + (HorizontalStep))
        //        for (double y = activeAreaBottom + (verticalStep); y < activeAreaTop - verticalStep; y = y + (verticalStep))
        //        {
        //            random = (float)rand.NextDouble();
        //            lon = x + random * NormalizedrandomPosition * HorizontalStep;
        //            random = (float)rand.NextDouble();
        //            lat = y + random * NormalizedrandomPosition * verticalStep;
        //            normalizedLat = (lat - activeAreaBottom) / (activeAreaTop - activeAreaBottom);
        //            normalizedLon = (lon - activeAreaLeft) / (activeAreaRight - activeAreaLeft);
        //
        //            if (mask != null)
        //                maskcheck = mask.CheckNormal(normalizedLon, normalizedLat);
        //
        //            if (maskcheck)
        //            {
        //                TPointObject _object = new TPointObject();
        //                _object.GeoPosition.longitude = lon;
        //                _object.GeoPosition.latitude = lat;
        //
        //                // --- Scaling
        //                float randomScale = (objectScatterLayer.MinScale + (float)rand.NextDouble() * (objectScatterLayer.MaxScale - objectScatterLayer.MinScale));
        //                float  ScaleX = objectScatterLayer.ScaleMultiplier.X * randomScale;
        //                float  ScaleY = objectScatterLayer.ScaleMultiplier.Y * randomScale;
        //                float  ScaleZ = objectScatterLayer.ScaleMultiplier.Z * randomScale;
        //                _object.scale = new Vector3 (ScaleX, ScaleY, ScaleZ);
        //
        //                // --- rotation
        //                float rotationX = 0;
        //                float rotationY = 0;
        //                float rotationZ = 0;
        //                
        //                if (!objectScatterLayer.rotation90Degrees)
        //                {
        //                    rotationX = objectScatterLayer.MinRotationRange + (float)rand.NextDouble() * (objectScatterLayer.MaxRotationRange - objectScatterLayer.MinRotationRange);
        //                    rotationY = objectScatterLayer.MinRotationRange + (float)rand.NextDouble() * (objectScatterLayer.MaxRotationRange - objectScatterLayer.MinRotationRange);
        //                    rotationZ = objectScatterLayer.MinRotationRange + (float)rand.NextDouble() * (objectScatterLayer.MaxRotationRange - objectScatterLayer.MinRotationRange);
        //                }
        //                else
        //                {
        //                    rotationX = (float)rand.Next(0, 4) * 90;
        //                    rotationY = (float)rand.Next(0, 4) * 90;
        //                    rotationZ = (float)rand.Next(0, 4) * 90;
        //                }
        //                
        //                if (objectScatterLayer.lockYRotation)
        //                {
        //                    rotationX = 0;
        //                    rotationZ = 0;
        //                }
        //                
        //                _object.rotation = new Vector3(rotationX + objectScatterLayer.RotationOffset.X, rotationY + objectScatterLayer.RotationOffset.Y, rotationZ + objectScatterLayer.RotationOffset.Z);
        //                objectScatterLayer.points.Add(_object);
        //            }
        //        }
        //}

        public static TMask GetShoreMask(TTerrain terrain, TLakeLayer LakeLayer, bool justBorders, int edge_size, float scaleFactor)
        {
            float[,] heightmap = terrain.Map.Heightmap.heightsData;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);
            TMask mask = new TMask(width, length);
            //bool negetiveAdge = (edge_size < 0 ? true : false);
            edge_size = Math.Abs(edge_size);

            foreach (T2DObject lake in LakeLayer.LakesList)
            {
                if (lake.AroundPoints.Count < 4) continue;
                List<TGlobalPoint> scaledLake = ScaleShape(lake.AroundPoints, scaleFactor);
                List<Vector2> polygonArray = new List<Vector2>();
                float minHeight = float.MaxValue;

                for (int i = 0; i < scaledLake.Count; i++)
                {
                    int pixelX;
                    int pixelY;
                    float pointHeight = (float)terrain.Map.GetHeight(scaledLake[i]); 

                    if (minHeight > pointHeight)
                        minHeight = pointHeight;

                    terrain.Map.GetNearestIndex(scaledLake[i], out pixelX, out pixelY);
                    polygonArray.Add(new Vector2(pixelX, pixelY));
                }

                if (polygonArray.Count < 4) continue;

                TArea lakeArea = lake.GetBounds();
                int minLatIndex, maxLatIndex, minLonIndex, maxLonIndex;
                terrain.Map.GetPixelBounds(lakeArea, out minLonIndex, out maxLonIndex, out minLatIndex, out maxLatIndex);
                bool TLState = false;
                bool TRState = false;
                bool BLState = false;
                bool BRState = false;
                Vector2 point;

                if (minLonIndex - edge_size < 0) minLonIndex = edge_size;
                if (maxLonIndex + edge_size > width) maxLonIndex = width - edge_size;
                if (minLatIndex - edge_size < 0) minLatIndex = edge_size;
                if (maxLatIndex + edge_size > length) maxLatIndex = length - edge_size;

                for (int i = minLonIndex - edge_size; i < maxLonIndex + edge_size; i++)
                {
                    for (int j = minLatIndex - edge_size; j < maxLatIndex + edge_size; j++)
                    {
                        point = new Vector2(i - edge_size, j + edge_size);
                        BRState = TUtils.PointInPolygon(polygonArray, point);

                        if (edge_size > 0)
                        {
                            point = new Vector2(i - edge_size, j - edge_size);
                            BLState = TUtils.PointInPolygon(polygonArray, point);
                            point = new Vector2(i + edge_size, j - edge_size);
                            TLState = TUtils.PointInPolygon(polygonArray, point);
                            point = new Vector2(i + edge_size, j + edge_size);
                            TRState = TUtils.PointInPolygon(polygonArray, point);
                        }
                        else
                        {
                            TRState = BRState;
                            TLState = BRState;
                            BLState = BRState;
                        }

                        bool allIn = TRState && TLState && BRState && BLState;
                        bool OneIn = TRState || TLState || BRState || BLState;

                        if (justBorders && OneIn && !allIn)
                            mask.SetValue(i, j, minHeight);

                        if (!justBorders && allIn)
                            mask.SetValue(i, j, minHeight);
                    }
                }
            }

            return mask;
        }

        public static List<TMask> GetShoreMasks(TTerrain terrain, TLakeLayer LakeLayer, bool justBorders, int edge_size, float scaleFactor)
        {
            List<TMask> result = new List<TMask>();
            float[,] heightmap = terrain.Map.Heightmap.heightsData;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);
            //bool negetiveAdge = (edge_size < 0 ? true : false);
            edge_size = Math.Abs(edge_size);
            int index = 0;

            foreach (T2DObject lake in LakeLayer.LakesList)
            {
                index++;
                if (lake.AroundPoints.Count < 4) continue;

                TMask mask = new TMask(width, length);
                List<TGlobalPoint> scaledLake = ScaleShape(lake.AroundPoints, scaleFactor);
                List<Vector2> polygonArray = new List<Vector2>();
                float minHeight = float.MaxValue;

                for (int i = 0; i < scaledLake.Count; i++)
                {
                    int pixelX;
                    int pixelY;
                    float pointHeight = (float)terrain.Map.GetHeight(scaledLake[i]);

                    if (minHeight > pointHeight)
                        minHeight = pointHeight;

                    terrain.Map.GetNearestIndex(scaledLake[i], out pixelX, out pixelY);
                    polygonArray.Add(new Vector2(pixelX, pixelY));
                }

                if (polygonArray.Count < 4) continue;

                TArea lakeArea = lake.GetBounds();
                int minLatIndex, maxLatIndex, minLonIndex, maxLonIndex;
                terrain.Map.GetPixelBounds(lakeArea, out minLonIndex, out maxLonIndex, out minLatIndex, out maxLatIndex);
                bool TLState = false;
                bool TRState = false;
                bool BLState = false;
                bool BRState = false;
                Vector2 point;

                if (minLonIndex - edge_size < 0) minLonIndex = edge_size;
                if (maxLonIndex + edge_size > width) maxLonIndex = width - edge_size;
                if (minLatIndex - edge_size < 0) minLatIndex = edge_size;
                if (maxLatIndex + edge_size > length) maxLatIndex = length - edge_size;

                for (int i = minLonIndex - edge_size; i < maxLonIndex + edge_size; i++)
                {
                    for (int j = minLatIndex - edge_size; j < maxLatIndex + edge_size; j++)
                    {
                        point = new Vector2(i - edge_size, j + edge_size);
                        BRState = TUtils.PointInPolygon(polygonArray, point);

                        if (edge_size > 0)
                        {
                            point = new Vector2(i - edge_size, j - edge_size);
                            BLState = TUtils.PointInPolygon(polygonArray, point);
                            point = new Vector2(i + edge_size, j - edge_size);
                            TLState = TUtils.PointInPolygon(polygonArray, point);
                            point = new Vector2(i + edge_size, j + edge_size);
                            TRState = TUtils.PointInPolygon(polygonArray, point);
                        }
                        else
                        {
                            TRState = BRState;
                            TLState = BRState;
                            BLState = BRState;
                        }

                        bool allIn = TRState && TLState && BRState && BLState;
                        bool OneIn = TRState || TLState || BRState || BLState;

                        if (justBorders && OneIn && !allIn)
                            mask.SetValue(i, j, minHeight);

                        if (!justBorders && allIn)
                            mask.SetValue(i, j, minHeight);
                    }
                }

                result.Add(mask);
            }

            return result;
        }

        //public static List<TMask> GetBiomesMasks(TAreaBounds boundingBox, TTerrain terrain, List<T2DObject> Areas, bool justBorders, int edge_sizeInMeter, float scaleFactor, bool MergeMasks)
        public static List<TMask> GetBiomesMasks(TTerrain terrain, List<T2DObject> Areas, bool justBorders, int edge_sizeInMeter, float scaleFactor, bool MergeMasks)
        {
            List<TMask> result = new List<TMask>();
            if (Areas.Count < 1) return result;
            TMask mask = null;
            float[,] heightmap = terrain.Heightmap.heightsData;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);
            int edgeSizeInPixel = (int)(edge_sizeInMeter * length * scaleFactor / (2.0 * terrain.Map._area._areaSizeLat * 1000));
            T2DPoint pixelBoundLBPoint, pixelBoundRTPoint;

            //if ((boundingBox.left - terrain.Map._area._left) > 2e-5 && (terrain.Map._area._right - boundingBox.right) > 2e-5 &&
            //     (boundingBox.bottom - terrain.Map._area._bottom) > 2e-5 && (terrain.Map._area._top - boundingBox.top) > 2e-5)
            //{
            //    TGlobalPoint boundLBPoint = new TGlobalPoint(boundingBox.bottom, boundingBox.left);
            //    TGlobalPoint boundRTPoint = new TGlobalPoint(boundingBox.top, boundingBox.right);
            //    System.Numerics.Vector2 BoundLBPointNormalized = terrain.Map.GetLatLongNormalizedPositionN(boundLBPoint);
            //    System.Numerics.Vector2 BoundRTPointNormalized = terrain.Map.GetLatLongNormalizedPositionN(boundRTPoint);
            //    pixelBoundLBPoint = new T2DPoint(BoundLBPointNormalized.Y * width, BoundLBPointNormalized.X * length);
            //    pixelBoundRTPoint = new T2DPoint(BoundRTPointNormalized.Y * width, BoundRTPointNormalized.X * length);
            //}
            //else
            //{
                pixelBoundLBPoint = new T2DPoint(0, 0);
                pixelBoundRTPoint = new T2DPoint(width, length);
            //}

            if (MergeMasks)
                mask = new TMask(width, length);

            if (Areas.Count > 0)
            {
                double minHeight;

                foreach (T2DObject BiomeArea in Areas)
                {
                    if (!MergeMasks) mask = new TMask(width, length);

                    if (BiomeArea.AroundPoints.Count >= 4)
                    {
                        List<TGlobalPoint> scaledPolygon = ScaleShape(BiomeArea.AroundPoints, scaleFactor);
                        TPolygonMask polygonMask = new TPolygonMask();
                        minHeight = float.MaxValue;

                        for (int i = 0; i < scaledPolygon.Count; i++)
                        {
                            Vector3 wordPosition = terrain.GetWorldPositionWithHeight(scaledPolygon[i]);

                            if (minHeight > wordPosition.Y)
                                minHeight = wordPosition.Y;

                            T2DPoint pointNormalized = terrain.Map.GetPointNormalized(scaledPolygon[i]);
                            Vector3 np = new Vector3((float)pointNormalized.x * (width - 1), wordPosition.Y,(float)pointNormalized.y * (length - 1));

                            if (i == 0 || i == scaledPolygon.Count - 1)
                                polygonMask.aroundPoints.Add(np);
                            else
                            {
                                float dist = Vector3.Distance(np, polygonMask.aroundPoints[polygonMask.aroundPoints.Count - 1]);
                                if (dist > 20) polygonMask.aroundPoints.Add(np);
                            }
                        }

                        if (polygonMask.aroundPoints[0].X != polygonMask.aroundPoints[polygonMask.aroundPoints.Count - 1].X ||
                            polygonMask.aroundPoints[0].Z != polygonMask.aroundPoints[polygonMask.aroundPoints.Count - 1].Z)
                        {
                            polygonMask.aroundPoints.Add(polygonMask.aroundPoints[0]);
                        }

                        polygonMask.minHeight = minHeight;
                        polygonMask.property = BiomeArea.property;
                        mask.polygonsMask.Add(polygonMask);
                    }

                    if (!MergeMasks)
                    {
                        PolygonMaskGenerate(mask, pixelBoundLBPoint, pixelBoundRTPoint, justBorders, edgeSizeInPixel);
                        result.Add(mask);
                    }
                }
            }

            if (MergeMasks)
            {
                PolygonMaskGenerate(mask, pixelBoundLBPoint, pixelBoundRTPoint, justBorders, edgeSizeInPixel);
                result.Add(mask);
            }

            return result;
        }


        public static List<TMask> GetBiomesMasksNew(TTerrain terrain, List<T2DObject> Areas, bool justBorders, int edge_sizeInMeter, float scaleFactor, bool MergeMasks)
        {
            List<TMask> result = new List<TMask>();
            if (Areas.Count < 1) return result;
            TMask mask = null;
            float[,] heightmap = terrain.Heightmap.heightsData;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);
            int edgeSizeInPixel = (int)(edge_sizeInMeter * length /* scaleFactor*/ / (2.0 * terrain.Map._area._areaSizeLat * 1000));
            //T2DPoint pixelBoundLBPoint, pixelBoundRTPoint;
            List<TGlobalPoint> scaledAroundPoints;
            List<List<TGlobalPoint>> scaledHolesAroundPoints = new List<List<TGlobalPoint>>(); ;

            if (MergeMasks)
                mask = new TMask(width, length);

            if (Areas.Count > 0)
            {
                double minHeight;
                foreach (T2DObject BiomeArea in Areas)
                {
                    if (!MergeMasks) mask = new TMask(width, length);

                    if (BiomeArea.AroundPoints.Count >= 4)
                    {
                        scaledAroundPoints = ScaleShape(BiomeArea.AroundPoints, scaleFactor);
                            
                        for (int i = 0; i < BiomeArea.Holes.Count; i++)
                        {
                            List<TGlobalPoint> scaledHole = ScaleShape(BiomeArea.Holes[i].AroundPoints, scaleFactor);
                        }

                        minHeight = float.MaxValue;

                        for (int i = 0; i < scaledAroundPoints.Count; i++)
                        {
                            Vector3 wordPosition = terrain.GetWorldPositionWithHeight(scaledAroundPoints[i]);

                            if (minHeight > wordPosition.Y)
                                minHeight = wordPosition.Y;

                            T2DPoint pointNormalized = terrain.Map.GetPointNormalized(scaledAroundPoints[i]);
                        }
                    }

                    if (!MergeMasks)
                        result.Add(mask);
                }
            }

            if (MergeMasks)
                result.Add(mask);

            return result;
        }

        public static void PolygonMaskGenerate(TMask mask, T2DPoint pixelBoundLBPoint, T2DPoint pixelBoundRTPoint, 
                                                                                        bool justBorders, int edgeSizeInPixel)
        {
            int x, y, pointY;
            float minHeight = float.MaxValue;

            for (int l = 0; l < mask.polygonsMask.Count; l++)
                if (minHeight > mask.polygonsMask[l].minHeight)
                    minHeight = (float)mask.polygonsMask[l].minHeight;

            if(minHeight < float.MaxValue)
                mask.minHeight = minHeight;

            for (x = (int)pixelBoundLBPoint.x; x < (int)pixelBoundRTPoint.x; x++)
            {
                TLine lineBrowser = new TLine(x, pixelBoundLBPoint.y, x, pixelBoundRTPoint.y - 1);
                for (int l = 0; l < mask.polygonsMask.Count; l++)
                {

                    List<int> listY = new List<int>();
                    List<T2DPoint> point = new List<T2DPoint>();
                    for (int j = 0; j < mask.polygonsMask[l].aroundPoints.Count - 1; j++)
                    {
                        TLine linePolygon = new TLine(mask.polygonsMask[l].aroundPoints[j].X, mask.polygonsMask[l].aroundPoints[j].Z,
                                                      mask.polygonsMask[l].aroundPoints[j + 1].X, mask.polygonsMask[l].aroundPoints[j + 1].Z);
                        point.Clear();
                        if (lineBrowser.CalcIntersection(linePolygon, ref point))
                        {
                            for (int k = 0; k < point.Count; k++)
                            {
                                pointY = (int)point[k].y;
                                if (listY.BinarySearch(pointY) < 0)
                                {
                                    listY.Add(pointY);
                                    listY.Sort();
                                }
                            }
                        }
                    }
                    if (listY.Count > 0)
                    {
                        List<T2DIndex> t2DIndex = new List<T2DIndex>();
                        Vector3 p = new Vector3();

                        if (listY.Count > 1)
                        {
                            for (int j = 0; j < listY.Count - 1; j++)
                            {
                                p.X = (float)x;
                                p.Z = (float)(listY[j] + listY[j + 1]) / 2f;
                                if (TUtils.PointInPolygon(mask.polygonsMask[l].aroundPoints, p))
                                {
                                    T2DIndex t2DIndexTemp = new T2DIndex();
                                    t2DIndexTemp.index1 = listY[j];
                                    t2DIndexTemp.index2 = listY[j + 1];
                                    t2DIndex.Add(t2DIndexTemp);
                                }
                            }
                        }
                        else
                        {
                            T2DIndex t2DIndexTemp = new T2DIndex();
                            t2DIndexTemp.index1 = listY[0];
                            t2DIndexTemp.index2 = listY[0];
                            t2DIndex.Add(t2DIndexTemp);
                        }

                        for (int j = 0; j < t2DIndex.Count; j++)
                        {
                            if (!justBorders)
                            {
                                for (y = t2DIndex[j].index1; y <= t2DIndex[j].index2; y++)
                                {
                                    if (mask.polygonsMask[l].property == Property.Inner)
                                        mask.Clear(x, y);
                                    else
                                    {
                                        if (edgeSizeInPixel == 0)
                                            mask.SetValue(x, y, (float)mask.polygonsMask[l].minHeight);
                                        else
                                        { 
                                            if (y == t2DIndex[j].index1 || y == t2DIndex[j].index2)
                                            {
                                                pointMaskSetValue(mask, x, y, edgeSizeInPixel, pixelBoundLBPoint, pixelBoundRTPoint,
                                                                                               (float)mask.polygonsMask[l].minHeight);
                                            }
                                            else
                                                mask.SetValue(x, y, (float)mask.polygonsMask[l].minHeight);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (edgeSizeInPixel == 0)
                                {
                                    mask.SetValue(x, t2DIndex[j].index1, (float)mask.polygonsMask[l].minHeight);
                                    if (t2DIndex[j].index2 != t2DIndex[j].index1)
                                        mask.SetValue(x, t2DIndex[j].index2, (float)mask.polygonsMask[l].minHeight);
                                }
                                else
                                {
                                    pointMaskSetValue(mask, x, t2DIndex[j].index1, edgeSizeInPixel, pixelBoundLBPoint, pixelBoundRTPoint,
                                                                                                      (float)mask.polygonsMask[l].minHeight);
                                    if (t2DIndex[j].index2 != t2DIndex[j].index1)
                                    {
                                        pointMaskSetValue(mask, x, t2DIndex[j].index2, edgeSizeInPixel, pixelBoundLBPoint, pixelBoundRTPoint,
                                                                                                      (float)mask.polygonsMask[l].minHeight);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void pointMaskSetValue(TMask mask, int xPoint, int yPoint, int edgeSizeInPixel,
                                                    T2DPoint pixelBoundLBPoint, T2DPoint pixelBoundRTPoint, float minHeight)
        {
            int i, j, k;
            int x, y;

            for (i = -edgeSizeInPixel; i <= edgeSizeInPixel; i++)
            {
                x = xPoint + i;

                if (x >= pixelBoundLBPoint.x && x < pixelBoundRTPoint.x)
                {
                    if( i <= 0 )
                        k = edgeSizeInPixel + i;
                    else
                        k = edgeSizeInPixel - i;

                    for (j = -k; j <= k; j++)
                    {
                        y = yPoint + j;

                        if (y >= pixelBoundLBPoint.y && y < pixelBoundRTPoint.y)
                            mask.SetValue(x, y, minHeight);
                    }
                }
            }
        }

        // Calculate the distance between
        // point pt and the segment p1 --> p2.
        // if pt wat out of line segment the closest point will return by closest
        private static double FindDistanceToSegment(Vector2 pt, Vector2 p1, Vector2 p2, out Vector2 closest)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;

            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Vector2(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new Vector2(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new Vector2(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static List<TMask> GetBiomesMasksLinearOld2(TTerrain terrain, TLinearMeshLayer BiomeLayer, bool justBorders, float edge_sizeInMeter, float scaleFactor, float widthSizeInMeter, bool MergeMasks)
        {
            List<TMask> result = new List<TMask>();
            if (BiomeLayer.Lines.Count < 1) return result;
            TMask mask = null;
            float[,] heightmap = terrain.Map.Heightmap.heightsData;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);
            //float widthSize = widthSizeInMeter * length * scaleFactor / (terrain.Map._area._areaSizeLat * 1000);
            //float edge_size = edge_sizeInMeter * length * scaleFactor / (terrain.Map._area._areaSizeLat * 1000);
            mask = new TMask(width, length);
            List<Vector2> linesWorldPos = new List<Vector2>();

            foreach (TLinearObject BiomeArea in BiomeLayer.Lines)
            {
                linesWorldPos.Clear();
                if (BiomeArea.Points.Count < 2) continue;
                if (!MergeMasks) mask = new TMask(width, length);

                //int top = 0;
                //int bottom = int.MaxValue;
                //int right = 0;
                //int left = int.MaxValue;
                int pixelX = 0;
                int pixelY = 0;
                //int pixelXold = 0;
                //int pixelYold = 0;

                for (int i = 0; i < BiomeArea.Points.Count; i++)
                {
                    terrain.Map.GetNearestIndex(BiomeArea.Points[i], out pixelX, out pixelY);
                    mask.SetValue(pixelX, pixelY, 1);
                }

                if (MergeMasks) result.Add(mask);
            }

            return result;
        }

        public static List<TMask> GetBiomesMasksLinearOld(TTerrain terrain, TLinearMeshLayer BiomeLayer, bool justBorders, float edge_sizeInMeter, float scaleFactor, float widthSizeInMeter, bool MergeMasks)
        {
            List<TMask> result = new List<TMask>();
            if (BiomeLayer.Lines.Count < 1) return result;
            TMask mask = null;
            float[,] heightmap = terrain.Map.Heightmap.heightsData;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);
            float widthSize = widthSizeInMeter * length * scaleFactor / (terrain.Map._area._areaSizeLat * 1000);
            float edge_size = edge_sizeInMeter * length * scaleFactor / (terrain.Map._area._areaSizeLat * 1000);
            if (MergeMasks) mask = new TMask(width, length);
            List<Vector2> linesWorldPos = new List<Vector2>();

            foreach (TLinearObject BiomeArea in BiomeLayer.Lines)
            {
                linesWorldPos.Clear();
                if (BiomeArea.Points.Count < 2) continue;
                if (!MergeMasks) mask = new TMask(width, length);

                int top = 0;
                int bottom = int.MaxValue;
                int right = 0;
                int left = int.MaxValue;
                int pixelX = 0;
                int pixelY = 0;
                int pixelXold = 0;
                int pixelYold = 0;

                for (int i = 0; i < BiomeArea.Points.Count; i++)
                {
                    terrain.Map.GetNearestIndex(BiomeArea.Points[i], out pixelX, out pixelY);

                    if (pixelX != pixelXold || pixelY != pixelYold)
                    {
                        linesWorldPos.Add(new Vector2(pixelX, pixelY));

                        if (top < pixelY) top = pixelY;
                        if (bottom > pixelY) bottom = pixelY;
                        if (right < pixelX) right = pixelX;
                        if (left > pixelX) left = pixelX;

                        pixelYold = pixelY;
                        pixelXold = pixelX;
                    }
                }

                top = (int)(top + widthSize + edge_size);
                bottom = (int)(bottom - widthSize - edge_size);
                right = (int)(right + widthSize + edge_size);
                left = (int)(left - widthSize - edge_size);

                if (top > length) top = length;
                if (bottom < 0) bottom = 0;
                if (right > width) right = width;
                if (left < 0) left = 0;

                for (int i = left; i < right; i++)
                    for (int j = bottom; j < top; j++)
                    {
                        for (int k = 0; k < linesWorldPos.Count - 1; k++)
                        {
                            Vector2 p1 = linesWorldPos[k];
                            Vector2 p2 = linesWorldPos[k + 1];
                            Vector2 pt = new Vector2(i, j);
                            Vector2 closest = p1;
                            double dist = FindDistanceToSegment(pt, p1, p2, out closest);

                            if (dist <= (widthSize + edge_size))
                                mask.SetValue(i, j, 1);

                            if (justBorders)
                            {
                                if (dist <= (widthSize))
                                {
                                    mask.Clear(i, j);
                                    break;
                                }
                            }
                        }
                    }

                if (!MergeMasks) result.Add(mask);
            }

            if (MergeMasks) result.Add(mask);
            return result;
        }


        public static List<TMask> GetBiomesMasksLinear(TTerrain terrain, List<TLinearObject> Lines, bool justBorders, float edge_sizeInMeter, float scaleFactor, float widthSizeInMeter, bool MergeMasks)
        {
            List<TMask> result = new List<TMask>();
            if (Lines.Count < 1) return result;
            TMask mask = null;
            float[,] heightmap = terrain.Map.Heightmap.heightsData;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);

            float minSize = (terrain.Map._area._areaSizeLat * 1000) * 2 / (width - 1);

            float widthSize = widthSizeInMeter * length * scaleFactor / (terrain.Map._area._areaSizeLat * 1000);
            float edge_size = edge_sizeInMeter * length * scaleFactor / (terrain.Map._area._areaSizeLat * 1000);
            if (MergeMasks) mask = new TMask(width, length);
            List<Vector2> linesWorldPos = new List<Vector2>();

            foreach (TLinearObject BiomeArea in Lines)
            {
                linesWorldPos.Clear();
                if (BiomeArea.Points.Count < 2) continue;
                if (!MergeMasks) mask = new TMask(width, length);

                int top = 0;
                int bottom = int.MaxValue;
                int right = 0;
                int left = int.MaxValue;

                double pixelXNormalIndex = 0;
                double pixelYNormalIndex = 0;

                float pixelXFloatIndex = 0;
                float pixelYFloatIndex = 0;

                //int pixelXold =0;
                //int pixelYold=0;

                for (int i = 0; i < BiomeArea.Points.Count; i++)
                {

                    terrain.Map.GetNormalizedIndex(BiomeArea.Points[i], out pixelXNormalIndex, out pixelYNormalIndex);
                    pixelXFloatIndex = (float)(width * pixelXNormalIndex);
                    pixelYFloatIndex = (float)(width * pixelYNormalIndex);

                    linesWorldPos.Add(new Vector2(pixelXFloatIndex, pixelYFloatIndex));

                    int pixelYtop = (int)Math.Floor(pixelYFloatIndex) + 1;
                    int pixelYbottom = (int)Math.Floor(pixelYFloatIndex);
                    int pixelYright = (int)Math.Floor(pixelXFloatIndex) + 1;
                    int pixelYleft = (int)Math.Floor(pixelXFloatIndex);

                    if (top < pixelYtop) top = pixelYtop;
                    if (bottom > pixelYbottom) bottom = pixelYbottom;
                    if (right < pixelYright) right = pixelYright;
                    if (left > pixelYleft) left = pixelYleft;
                }

                top = (int)(top + widthSize + edge_size);
                bottom = (int)(bottom - widthSize - edge_size);
                right = (int)(right + widthSize + edge_size);
                left = (int)(left - widthSize - edge_size);

                if (top > length) top = length;
                if (bottom < 0) bottom = 0;
                if (right > width) right = width;
                if (left < 0) left = 0;

                for (int i = left + 1; i < right; i++)
                    for (int j = bottom + 1; j < top; j++)
                    {
                        for (int k = 0; k < linesWorldPos.Count - 1; k++)
                        {
                            Vector2 p1 = linesWorldPos[k];
                            Vector2 p2 = linesWorldPos[k + 1];
                            Vector2 pt = new Vector2(i, j);
                            Vector2 closest = p1;
                            double dist = FindDistanceToSegment(pt, p1, p2, out closest);

                            if (dist <= (widthSize + edge_size))
                                mask.SetValue(i - 1, j - 1, 1);

                            if (justBorders)
                            {
                                if (dist <= (widthSize))
                                {
                                    mask.Clear(i - 1, j - 1);
                                    break;
                                }
                            }
                        }
                    }

                if (!MergeMasks) result.Add(mask);
            }

            if (MergeMasks) result.Add(mask);
            return result;
        }

        public static List<TMask> GetBiomesMasks2(TTerrain terrain, TLinearMeshLayer BiomeLayer, bool justBorders, float edge_sizeInMeter, float scaleFactor, float widthSizeInMeter, bool MergeMasks)
        {
            List<TMask> result = new List<TMask>();
            if (BiomeLayer.Lines.Count < 1) return result;
            TMask mask = null;
            float[,] heightmap = terrain.Map.Heightmap.heightsData;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);
            float widthSize = widthSizeInMeter * length * scaleFactor / (terrain.Map._area._areaSizeLat * 1000);
            float edge_size = edge_sizeInMeter * length * scaleFactor / (terrain.Map._area._areaSizeLat * 1000);
            if (MergeMasks) mask = new TMask(width, length);
            List<Vector2> linesWorldPos = new List<Vector2>();

            foreach (TLinearObject BiomeArea in BiomeLayer.Lines)
            {
                linesWorldPos.Clear();
                if (BiomeArea.Points.Count < 2) continue;
                if (!MergeMasks) mask = new TMask(width, length);

                for (int i = 0; i < BiomeArea.Points.Count; i++)
                {
                    int pixelX;
                    int pixelY;
                    terrain.Map.GetNearestIndex(BiomeArea.Points[i], out pixelX, out pixelY);
                    linesWorldPos.Add(new Vector2(pixelX, pixelY));
                }

                int aroundPoint = (int)(widthSize + edge_size) + 1;

                foreach(Vector2 _riverCenterPoint in linesWorldPos)
                {
                    int topL = (int)_riverCenterPoint.Y + aroundPoint;
                    int leftL = (int)_riverCenterPoint.X - aroundPoint;
                    int bottomL = (int)_riverCenterPoint.Y - aroundPoint;
                    int rightL = (int)_riverCenterPoint.X + aroundPoint;

                    if (topL > length) topL = length;
                    if (bottomL < 0) bottomL = 0;
                    if (rightL > width) rightL = width;
                    if (leftL < 0) leftL = 0;

                    for (int i = leftL; i < rightL; i++)
                        for (int j = bottomL; j < topL; j++)
                            mask.SetValue(i, j, 1);
                }

                if (!MergeMasks) result.Add(mask);
            }

            if (MergeMasks) result.Add(mask);
            return result;
        }


        public static void ResetLakesPointPositions(TTerrain terrain)
        {
            foreach (TLakeLayer LakeLayer in terrain.LakeLayer)
            {
                foreach (T2DObject lake in LakeLayer.LakesList)
                {
                    List<Vector2> pixelSpacepolygonArray = new List<Vector2>();
                    List<Vector2> newLakesPoints = new List<Vector2>();
                    List<Vector2> newLakesGeoPoints = new List<Vector2>();

                    for (int i = 0; i < lake.AroundPoints.Count; i++)
                    {
                        int pixelX;
                        int pixelY;
                        terrain.Map.GetNearestIndex(lake.AroundPoints[i], out pixelX, out pixelY);
                        bool ExistPoint = false;

                        for (int u = 0; u < pixelSpacepolygonArray.Count; u++)
                        {
                            if (pixelSpacepolygonArray[u].X == pixelX && pixelSpacepolygonArray[u].Y == pixelY)
                                ExistPoint = true;
                        }

                        if (!ExistPoint)
                            pixelSpacepolygonArray.Add(new Vector2(pixelX, pixelY));
                    }

                    List<Vector2> pixelSpaceMonopolypolygonArray = new List<Vector2>();

                    if (pixelSpacepolygonArray.Count > 4)
                    {
                        Vector2 oldPoint = pixelSpacepolygonArray[0];
                        pixelSpaceMonopolypolygonArray.Add(oldPoint);

                        for (int i = 1; i < pixelSpacepolygonArray.Count; i++)
                        {
                            if (pixelSpacepolygonArray[i].X != oldPoint.X && pixelSpacepolygonArray[i].Y != oldPoint.Y)
                            {
                                Vector2 midPoint = new Vector2(pixelSpacepolygonArray[i].X, oldPoint.Y);
                                pixelSpaceMonopolypolygonArray.Add(midPoint);
                            }

                            oldPoint = pixelSpacepolygonArray[i];
                            pixelSpaceMonopolypolygonArray.Add(oldPoint);
                        }

                        // Check for first point & Last Point
                        if (pixelSpaceMonopolypolygonArray[0].X != oldPoint.X && pixelSpaceMonopolypolygonArray[0].Y != oldPoint.Y)
                        {
                            Vector2 midPoint = new Vector2(pixelSpaceMonopolypolygonArray[0].X, oldPoint.Y);
                            pixelSpaceMonopolypolygonArray.Add(midPoint);
                        }
                    }

                    lake.AroundPoints.Clear();

                    if (pixelSpaceMonopolypolygonArray.Count > 4)
                    {
                        for (int i = 0; i < pixelSpaceMonopolypolygonArray.Count; i++)
                        {
                            TGlobalPoint point = terrain.Map.GetGeoPoint(pixelSpaceMonopolypolygonArray[i].X, pixelSpaceMonopolypolygonArray[i].Y);
                            lake.AroundPoints.Add(point);
                        }
                    }
                }
            }
        }

        public static void GetBounds(List<TGlobalPoint> Scr, out double top, out double left, out double bottom, out double right)
        {
            int count = Scr.Count;
            TGlobalPoint node;
            top = double.MinValue;
            left = double.MaxValue;
            bottom = double.MaxValue;
            right = double.MinValue;

            for (int i = 0; i < count; i++)
            {
                node.latitude = Scr[i].latitude;
                node.longitude = Scr[i].longitude;
                if (top < node.latitude) top = node.latitude;
                if (left > node.longitude) left = node.longitude;
                if (bottom > node.latitude) bottom = node.latitude;
                if (right < node.longitude) right = node.longitude;
            }
        }

        public static List<TGlobalPoint> ScaleShape(List<TGlobalPoint> Scr, float scalefactor)
        {
            if (scalefactor == 1) return Scr;

            double top, left, bottom, right;
            GetBounds(Scr, out top, out left, out bottom, out right);

            TGlobalPoint oldBoundCenter = new TGlobalPoint();
            oldBoundCenter.latitude = (top - bottom) / 2;
            oldBoundCenter.longitude = (right - left) / 2;

            TGlobalPoint newBoundCenter = new TGlobalPoint();
            newBoundCenter.latitude = oldBoundCenter.latitude * scalefactor;
            newBoundCenter.longitude = oldBoundCenter.longitude * scalefactor;

            TGlobalPoint diffCenter = new TGlobalPoint();
            diffCenter.latitude = newBoundCenter.latitude - oldBoundCenter.latitude;
            diffCenter.longitude = newBoundCenter.longitude - oldBoundCenter.longitude;

            List<TGlobalPoint> result = new List<TGlobalPoint>();
            for (int i = 0; i < Scr.Count; i++)
            {
                TGlobalPoint newCordinatedPoint = new TGlobalPoint();

                // recordinate points to local BBox area
                newCordinatedPoint.latitude = Scr[i].latitude - bottom;
                newCordinatedPoint.longitude = Scr[i].longitude - left;

                //applying scale factor
                newCordinatedPoint.latitude = newCordinatedPoint.latitude * scalefactor;
                newCordinatedPoint.longitude = newCordinatedPoint.longitude * scalefactor;

                //moving points to new position while center position is still fixed
                newCordinatedPoint.latitude = newCordinatedPoint.latitude - diffCenter.latitude;
                newCordinatedPoint.longitude = newCordinatedPoint.longitude - diffCenter.longitude;

                //recordinates points to old cordinate
                newCordinatedPoint.latitude = newCordinatedPoint.latitude + bottom;
                newCordinatedPoint.longitude = newCordinatedPoint.longitude + left;

                result.Add(newCordinatedPoint);
            }

            return result;
        }
    }
    /*
       public static List<TMask> GetBiomesMasks(TAreaBounds boundingBox, TTerrain terrain, TPolygonMeshLayer BiomeLayer, 
                                                         bool justBorders, int edge_sizeInMeter, float scaleFactor, bool MergeMasks)
        {
            List<TMask> result = new List<TMask>();
            if (BiomeLayer.MeshArea.Count < 1) return result;
            TMask mask = null;
            float[,] heightmap = terrain.Heightmap.heightsData;
            int width = heightmap.GetLength(0);
            int length = heightmap.GetLength(1);
            int edgeSizeInPixel = (int)(edge_sizeInMeter * length * scaleFactor / (2.0 * terrain.Map._area._areaSizeLat * 1000));
            T2DPoint pixelBoundLBPoint, pixelBoundRTPoint;

            if ( (boundingBox.left - terrain.Map._area._left) > 2e-5 && (terrain.Map._area._right - boundingBox.right) > 2e-5 && 
                 (boundingBox.bottom - terrain.Map._area._bottom) > 2e-5 && (terrain.Map._area._top - boundingBox.top) > 2e-5 )
            {
                TGlobalPoint boundLBPoint = new TGlobalPoint(boundingBox.bottom, boundingBox.left);
                TGlobalPoint boundRTPoint = new TGlobalPoint(boundingBox.top, boundingBox.right);
                System.Numerics.Vector2 BoundLBPointNormalized = terrain.Map.GetLatLongNormalizedPositionN(boundLBPoint);
                System.Numerics.Vector2 BoundRTPointNormalized = terrain.Map.GetLatLongNormalizedPositionN(boundRTPoint);
                pixelBoundLBPoint = new T2DPoint(BoundLBPointNormalized.Y * width, BoundLBPointNormalized.X * length);
                pixelBoundRTPoint = new T2DPoint(BoundRTPointNormalized.Y * width, BoundRTPointNormalized.X * length);
            }
            else
            {
                pixelBoundLBPoint = new T2DPoint(0, 0);
                pixelBoundRTPoint = new T2DPoint(width, length);
            }

            if (MergeMasks)
            {
                mask = new TMask(width, length);
            }

            if (BiomeLayer.MeshArea.Count > 0)
            {
                float minHeight;
                foreach (T2DObject BiomeArea in BiomeLayer.MeshArea)
                {
                    if (!MergeMasks)
                        mask = new TMask(width, length);

                    if (BiomeArea.AroundPoints.Count >= 4)
                    {
                        List<TGlobalPoint> scaledPolygon = ScaleShape(BiomeArea.AroundPoints, scaleFactor);

                        minHeight = float.MaxValue;
                        for (int i = 0; i < scaledPolygon.Count; i++)
                        {
                            Vector3 wordPosition = terrain.GetWorldPositionWithHeight(scaledPolygon[i]);
                            if (minHeight > wordPosition.Y)
                                minHeight = wordPosition.Y;

                            T2DPoint pointNormalized = terrain.Map.GetPointNormalized(scaledPolygon[i]);
                            mask.polygonMask.aroundPoints.Add(new Vector3((float)pointNormalized.x * (width - 1), wordPosition.Y, 
                                                                          (float)pointNormalized.y * (length - 1)));
                        }
                        if (mask.polygonMask.aroundPoints[0].X != mask.polygonMask.aroundPoints[mask.polygonMask.aroundPoints.Count - 1].X ||
                            mask.polygonMask.aroundPoints[0].Z != mask.polygonMask.aroundPoints[mask.polygonMask.aroundPoints.Count - 1].Z)
                        {
                            mask.polygonMask.aroundPoints.Add(mask.polygonMask.aroundPoints[0]);
                        }
                        mask.polygonMask.minHeight = minHeight;
                        mask.polygonMask.property = BiomeArea.property;
                    }
                    if (!MergeMasks)
                    {
                        PolygonMaskGenerate(mask, pixelBoundLBPoint, pixelBoundRTPoint, justBorders, edgeSizeInPixel);
                        result.Add(mask);
                    }
                }
            }

            if (MergeMasks)
            {
                PolygonMaskGenerate(mask, pixelBoundLBPoint, pixelBoundRTPoint, justBorders, edgeSizeInPixel);
                result.Add(mask);
            }

            return result;
        }

        public static void PolygonMaskGenerate(TMask mask, T2DPoint pixelBoundLBPoint, T2DPoint pixelBoundRTPoint, bool justBorders, int edgeSizeInPixel)
        {
            int x, y, pointY;

            if (mask.polygonMask.aroundPoints.Count > 0)
            {
                for (x = (int)pixelBoundLBPoint.x; x < (int)pixelBoundRTPoint.x; x++)
                {
                    TLine lineBrowser = new TLine(x, pixelBoundLBPoint.y, x, pixelBoundRTPoint.y - 1);
                    List<int> listY = new List<int>();
                    List<T2DPoint> point = new List<T2DPoint>();
                    for (int j = 0; j < mask.polygonMask.aroundPoints.Count - 1; j++)
                    {
                        TLine linePolygon = new TLine(mask.polygonMask.aroundPoints[j].X, mask.polygonMask.aroundPoints[j].Z,
                                                      mask.polygonMask.aroundPoints[j + 1].X, mask.polygonMask.aroundPoints[j + 1].Z);
                        point.Clear();
                        if (lineBrowser.CalcIntersection(linePolygon, ref point))
                        {
                            for (int k = 0; k < point.Count; k++)
                            {
                                pointY = (int)point[k].y;
                                if (listY.BinarySearch(pointY) < 0)
                                {
                                    listY.Add(pointY);
                                    listY.Sort();
                                }
                            }
                        }
                    }
                    if (listY.Count > 0)
                    {
                        List<T2DIndex> t2DIndex = new List<T2DIndex>();
                        Vector3 p = new Vector3();
                        if (listY.Count > 1)
                        {
                            for (int j = 0; j < listY.Count - 1; j++)
                            {
                                p.X = (float)x;
                                p.Z = (float)(listY[j] + listY[j + 1]) / 2f;
                                if (TUtils.PointInPolygon(mask.polygonMask.aroundPoints, p))
                                {
                                    T2DIndex t2DIndexTemp = new T2DIndex();
                                    t2DIndexTemp.index1 = listY[j];
                                    t2DIndexTemp.index2 = listY[j + 1];
                                    t2DIndex.Add(t2DIndexTemp);
                                }
                            }
                        }
                        else
                        {
                            T2DIndex t2DIndexTemp = new T2DIndex();
                            t2DIndexTemp.index1 = listY[0];
                            t2DIndexTemp.index2 = listY[0];
                            t2DIndex.Add(t2DIndexTemp);
                        }
                        for (int j = 0; j < t2DIndex.Count; j++)
                        {
                            if (!justBorders)
                            {
                                for (y = t2DIndex[j].index1; y <= t2DIndex[j].index2; y++)
                                {
                                    if (mask.polygonMask.property == Property.Inner)
                                        mask.Clear(x, y);
                                    else
                                        mask.SetValue(x, y, (float)mask.polygonMask.minHeight);
                                }
                            }
                            else
                            {
                                if (edgeSizeInPixel == 0)
                                {
                                    mask.SetValue(x, t2DIndex[j].index1, (float)mask.polygonMask.minHeight);
                                    if (t2DIndex[j].index2 != t2DIndex[j].index1)
                                        mask.SetValue(x, t2DIndex[j].index2, (float)mask.polygonMask.minHeight);
                                }
                                else
                                {
                                    pointMaskSetValue(mask, x, t2DIndex[j].index1, edgeSizeInPixel, pixelBoundLBPoint, pixelBoundRTPoint,
                                                                                                    (float)mask.polygonMask.minHeight);
                                    if (t2DIndex[j].index2 != t2DIndex[j].index1)
                                    {
                                        pointMaskSetValue(mask, x, t2DIndex[j].index2, edgeSizeInPixel, pixelBoundLBPoint, pixelBoundRTPoint,
                                                                                                    (float)mask.polygonMask.minHeight);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void pointMaskSetValue(TMask mask, int xPoint, int yPoint, int edgeSizeInPixel,
                                                    T2DPoint pixelBoundLBPoint, T2DPoint pixelBoundRTPoint, float minHeight)
        {
            int x, y;

            for (x = xPoint - edgeSizeInPixel; x <= xPoint + edgeSizeInPixel; x++)
            {
                if (x >= pixelBoundLBPoint.x && x < pixelBoundRTPoint.x)
                {
                    for (y = yPoint - edgeSizeInPixel; y <= yPoint + edgeSizeInPixel; y++)
                    {
                        if (y >= pixelBoundLBPoint.y && y < pixelBoundRTPoint.y)
                            mask.SetValue(x, y, minHeight);
                    }
                }
            }
        }
    }*/


}
#endif
#endif
