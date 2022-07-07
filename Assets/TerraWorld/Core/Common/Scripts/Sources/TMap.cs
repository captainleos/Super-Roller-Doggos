#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Drawing;
using System.Numerics;
using System.Xml;
using TerraUnity.Utils;
using System.Threading.Tasks;

namespace TerraUnity.Edittime
{
    public class TMap
    {
        public TArea _area, _mosaicAreaElevation, _mosaicAreaImagery;
        public TMap _ref;
        public TTerrain _refTerrain;
        private Action<TMap> _lastActions;
        private bool _requestImageData = true;
        private bool _requestElevationData = true;
        private bool _requestLandcoverData = true;
        public TMapManager.mapElevationSourceEnum _mapElevationSource = TMapManager.mapElevationSourceEnum.ESRI;
        public TMapManager.mapImagerySourceEnum _mapImagerySource = TMapManager.mapImagerySourceEnum.ESRI;
        public TMapManager.mapImagerySourceEnum _mapLandCoverImagerySource = TMapManager.mapImagerySourceEnum.OpenStreetMap;
        public TMapManager.mapTypeOSMEnum _OSMImagerySource = TMapManager.mapTypeOSMEnum.Standard;
        public TMapManager.mapLandcoverSourceEnum _mapLandcoverSource = TMapManager.mapLandcoverSourceEnum.OSM;

        public int _imageryZoomLevel;
        public int _elevationZoomLevel;
        private TCommunications Connection = null;

        public int rowLeftElevation, colTopElevation, rowRightElevation, colBottomElevation;
        public int rowLeftImagery, colTopImagery, rowRightImagery, colBottomImagery;
        public int rowLeftLandcover, colTopLandcover, rowRightLandcover, colBottomLandcover;

        private int _progress;
        private bool disposed = false;

        public int xTilesElevation, yTilesElevation;
        public int xTilesImagery, yTilesImagery;
        public int xTilesLandcover, yTilesLandcover;

        public THeightmap _heightmap ;
        private TImage _image, _Landcoverimage;
        private XmlDocument _landcoverXML;

        public int _lastError = 0;
        private bool saveTilesImagery = true;
        private bool saveTilesElevation = true;
        private bool saveTilesLandcover = true;
        public int _ID = 0;
        public bool _heightMapAvailableInZoomLevel = false;
        private static int Counter ;

        public TMap(TArea area, TTerrain refTerrain, int ImageryZoomLevel, int ElevationZoomLevel)
        {
            TDebug.TraceMessage();
            if (ElevationZoomLevel == 0) RequestElevationData = false;
            if (ImageryZoomLevel == 0) RequestImageData = false;
            _area = new TArea(area._top, area._left, area._bottom, area._right);
            Initialize(_area, refTerrain, ImageryZoomLevel , ElevationZoomLevel);
        }

        public TMap(Action<TMap> action , TArea area, int ImageryZoomLevel, int ElevationZoomLevel, bool getElevation = true, bool getImagery = true, bool getLandcover = false): this(area, null, ImageryZoomLevel,ElevationZoomLevel)
        {
            TDebug.TraceMessage();
            if (ElevationZoomLevel == 0) RequestElevationData = false;
            if (ImageryZoomLevel == 0) RequestImageData = false;
            RequestElevationData = getElevation;
            RequestImageData = getImagery;
            RequestLandcoverData = getLandcover;
            UpdateMap(action);
        }


        public TMap(double top, double left, double bottom, double right, TTerrain refTerrain , int ImageryZoomLevel, int ElevationZoomLevel)
        {
            TDebug.TraceMessage();
            if (ElevationZoomLevel == 0) RequestElevationData = false;
            if (ImageryZoomLevel == 0) RequestImageData = false;
            _area = new TArea(top, left, bottom, right);
            Initialize(_area, refTerrain, ImageryZoomLevel, ElevationZoomLevel);
        }

        public void SetArea(double top, double left, double bottom, double right)
        {
            if (
                    _area._top == top &&
                    _area._left == left &&
                    _area._bottom == bottom &&
                    _area._right == right
                )
                    {
                        return;
                    }

            _area.SetArea(top, left, bottom, right);
            GetBBoxMosaicElevation(_elevationZoomLevel);
            GetBBoxMosaicImagery(_imageryZoomLevel);
            RequestImageData = true;
            RequestElevationData = true;
            RequestLandcoverData = true;
        }

        public void Initialize(TArea area, TTerrain refTerrain, int ImageryzoomLevel, int ElevationZoomLevel)
        {
            TDebug.TraceMessage();
            _elevationZoomLevel = ElevationZoomLevel;
            _imageryZoomLevel = ImageryzoomLevel;
            GetBBoxMosaicElevation(_elevationZoomLevel);
            GetBBoxMosaicImagery(_imageryZoomLevel);
            Random r = new Random((int)DateTime.Now.Ticks);
            _ID = (r.Next() + Counter++);

            RequestImageData = true;
            RequestElevationData = true;
            RequestLandcoverData = true;
            _ref = this;
            _refTerrain = refTerrain;
            _heightmap = new THeightmap();
            _image = new TImage();
            _Landcoverimage = new TImage();
        }

        public void SetHeightMap(float[,] HeightMapsrc)
        {
            _heightmap = new THeightmap(HeightMapsrc);
        }

        public THeightmap Heightmap
        {
            get
            {
                if (_lastError == 0)
                    return _ref._heightmap;
                else
                    return null;
            }
            set
            {
                _ref._heightmap = value;
            }
        }

        public TImage Image
        {
            get
            {
                if (_lastError == 0)
                    return _ref._image;
                else
                    return null;
            }
            set
            {
                _ref._image = value;
            }
        }

        public TImage LandCoverImage
        {
            get
            {
                if (_lastError == 0)
                    return _ref._Landcoverimage;
                else
                    return null;
            }
            set
            {
                _ref._Landcoverimage = value;
            }
        }

        public XmlDocument LandcoverXML
        {
            get
            {
                if (_lastError == 0)
                    return _ref._landcoverXML;
                else
                    return null;
            }
            set
            {
                _ref._landcoverXML = value;
            }
        }

        /*
        public int ProgressElevation
        {
            get { return _progressElevation; }
            set
            {
                _progressElevation = value;
                UpdateProgress();
            }
        }

        public int ProgressImagery
        {
            get { return _progressImagery; }
            set
            {
                _progressImagery = value;
                UpdateProgress();
            }
        }

        public int ProgressLandcover
        {
            get { return _progressLandcover; }
            set
            {
                _progressLandcover = value;
                UpdateProgress();
            }
        }



        public void UpdateProgress()
        {
            int requestCount = 0;
    
            if (RequestImageData)
                requestCount++;
    
            if (RequestElevationData)
                requestCount++;
    
            if (RequestLandcoverData)
                requestCount++;
    
            if (requestCount == 0) _progress = 100;
    
            _progress = (_progressLandcover + _progressElevation + _progressImagery) / requestCount;
            _progress = (int) TUtils.Clamp(0, 100, _progress);
    
            if (_refTerrain != null ) _refTerrain.Progress = 0;
        }
        */

        public int Progress
        {
            get
            {
                UpdateProgress2();
                return _progress;
            }
        }

        public void UpdateProgress2()
        {
            if (Connection != null)
            {
                _progress = (int)(Connection.Progress * 100);
            }
            else
                _progress = 100;
            _progress = (int)TUtils.Clamp(0, 100, _progress);
        }

        public bool SaveTilesImagery { get => saveTilesImagery; set => saveTilesImagery = value; }
        public bool SaveTilesElevation { get => saveTilesElevation; set => saveTilesElevation = value; }
        public bool SaveTilesLandcover { get => saveTilesLandcover; set => saveTilesLandcover = value; }
        public bool RequestImageData { get => _requestImageData; set { _requestImageData = value;} }
        public bool RequestElevationData { get => _requestElevationData; set => _requestElevationData = value; }
        public bool RequestLandcoverData { get => _requestLandcoverData; set => _requestLandcoverData = value; }

        public void UpdateMap(Action<TMap> act = null)
        {
            TDebug.TraceMessage();

            if (act != null) _lastActions = act;

            if (Connection != null)
            {
                //Connection.CancelActiveConnection();
                Connection = null;
            }

            Connection = new TCommunications
            (
                AfterDataDownloaded,
                xTilesImagery, yTilesImagery, colTopImagery, rowLeftImagery, _imageryZoomLevel,
                xTilesElevation, yTilesElevation, colTopElevation, rowLeftElevation, _elevationZoomLevel,
                SaveTilesImagery, SaveTilesElevation, SaveTilesLandcover, _area._top, _area._left, _area._bottom, _area._right
            );

            if (RequestImageData)
                UpdateImageryData();

            if (RequestElevationData)
                UpdateElevationData();

            if (RequestLandcoverData)
            {
                UpdateLandCoverData();
                //UpdateLandcoverImageryData();
            }

            _lastError = 0;
        }

        public void UpdateElevationData()
        {
            TDebug.TraceMessage();

            if (Connection == null)
                throw new Exception("No Connection!");

            Connection.UpdateElevationData(_mapElevationSource, TAddresses.cachePathElevation);
            _lastError = 0;
        }

        public void UpdateImageryData()
        {
            TDebug.TraceMessage();

            if (Connection == null)
                throw new Exception("No Connection!");

            Connection.UpdateImageryData(TAddresses.cachePathImagery,_mapImagerySource);
            _lastError = 0;
        }

        public void UpdateLandcoverImageryData()
        {
            TDebug.TraceMessage();

            if (Connection == null)
                throw new Exception("No Connection!");

            Connection.UpdateLandCoverImageryData(TAddresses.cachePathLandcover, _mapLandCoverImagerySource);
            _lastError = 0;
        }

        public void UpdateLandCoverData(Action<TMap> act = null)
        {
            TDebug.TraceMessage();

            if (Connection == null)
                throw new Exception("No Connection!");

            Connection.UpdateLandcoverData(TAddresses.cachePathLandcover,_mapLandcoverSource, _area);
            _lastError = 0;
        }

        public void Dispose()
        {
            disposed = true;
            Connection?.CancelActiveConnection();
            Connection = null;
        }

        public void AfterDataDownloaded(bool _heightMapAvailableInZoomLevel, float[,] heightsData, Bitmap SatelliteImage, Bitmap LandCoverImage, XmlDocument landcoverData
            , int majorVersion, int minorVersion, int CounterNum, string Message, string webpage, Exception exception)
        {
            TDebug.TraceMessage();

            if (exception != null )
            {
                exception.Data.Add("TW", "Please Check The Internet Connection.");
                TTerrainGenerator.RaiseException(exception);
                return;
            }

            if (majorVersion > TVersionController.MajorVersion)
                TTerraWorld.NewVersionWebAddress = webpage;

            else if ((minorVersion > TVersionController.MinorVersion)  && (majorVersion == TVersionController.MajorVersion))
                TTerraWorld.NewVersionWebAddress = webpage;
            else
                TTerraWorld.NewVersionWebAddress = "";

            if (CounterNum > TProjectSettings.LastTeamMessageNum)
            {
                TProjectSettings.LastTeamMessageNum = CounterNum;
                TDebug.DisplayMessage(Message, "Terra World");
            }

            if (disposed) return;

            if (RequestImageData)
            {
                this.Image.Copy(SatelliteImage);
                AfterUpdateCompletedImagery();
            }

            if (RequestLandcoverData)
            {
                this.LandcoverXML = landcoverData.Clone() as XmlDocument;
                //this.LandCoverImage.Copy(LandCoverImage);
                AfterUpdateCompletedLandcover();
                //AfterUpdateCompletedLandCoverImagery();
            }
            this._heightMapAvailableInZoomLevel = _heightMapAvailableInZoomLevel;
            if (_heightMapAvailableInZoomLevel)
            {
                this.SetHeightMap(heightsData);
            }
            AnalyzeElevationData(this);
        }

        private void AnalyzeElevationData(TMap CurrentMap)
        {
            TDebug.TraceMessage();

            if (CurrentMap.RequestElevationData && !CurrentMap._heightMapAvailableInZoomLevel)
            {
                TDebug.LogWarningToUnityUI("No Elevation Data On Level : " + CurrentMap._elevationZoomLevel + ", Trying next zoom level ... ");
                TMap tempMap = new TMap (CurrentMap._area._top, CurrentMap._area._left, CurrentMap._area._bottom, CurrentMap._area._right, _refTerrain, CurrentMap._imageryZoomLevel, CurrentMap._elevationZoomLevel - 1);
                tempMap._ref = CurrentMap._ref;
                tempMap.SaveTilesElevation = SaveTilesElevation;
                tempMap._mapElevationSource = _mapElevationSource;
                tempMap.RequestElevationData = true;
                tempMap.RequestImageData = false;
                tempMap.RequestLandcoverData = false;
                tempMap._image = _image;
                tempMap.UpdateMap(AnalyzeElevationData);
            }
            else
            {
                if (RequestElevationData)
                    AfterUpdateCompletedElevation();
                WhenAllDone();
            }
        }

        private void AnalyzeImageryData(TMap CurrentMap)
        {
            TDebug.TraceMessage();

            if (CurrentMap.RequestImageData && !CurrentMap._heightMapAvailableInZoomLevel)
            {
                TDebug.LogWarningToUnityUI("No Imagery Data On Level : " + CurrentMap._imageryZoomLevel + ", Trying next zoom level ... ");
                TMap tempMap = new TMap(CurrentMap._area._top, CurrentMap._area._left, CurrentMap._area._bottom, CurrentMap._area._right, _refTerrain, CurrentMap._imageryZoomLevel - 1, CurrentMap._elevationZoomLevel);
                tempMap._ref = CurrentMap._ref;
                tempMap.SaveTilesImagery = SaveTilesImagery;
                tempMap._mapImagerySource = _mapImagerySource;
                tempMap.RequestElevationData = false;
                tempMap.RequestImageData = true;
                tempMap.RequestLandcoverData = false;
                tempMap._heightmap = _heightmap;
                tempMap.UpdateMap(AnalyzeImageryData);
            }
            else
            {
                if (RequestImageData)
                    AfterUpdateCompletedImagery();
                WhenAllDone();
            }
        }

        public void AfterUpdateCompletedElevation()
        {
            TDebug.TraceMessage();

            if (_heightmap == null || _heightmap.heightsData == null) throw new Exception("Downloading Elevation data error....!");

            _heightmap.heightsData = THeightmapProcessors.CropHeightmap(_mosaicAreaElevation, _area, _heightmap.heightsData);
            _heightmap.heightsData = THeightmapProcessors.ResampleHeightmap(_heightmap.heightsData, THeightmapProcessors.ResampleMode.DOWN);
            _heightmap.heightsData = THeightmapProcessors.RotateHeightmap(_heightmap.heightsData);

            if (_ref._elevationZoomLevel != this._elevationZoomLevel)
                _ref._heightmap.heightsData = _heightmap.heightsData.Clone() as float[,];
        }

        public void AfterUpdateCompletedImagery()
        {
            TDebug.TraceMessage();

            if (_image == null || _image.Image == null) throw new Exception("Downloading Imagery data error....!");
            _image.Image = TImageProcessors.CropImage(_mosaicAreaImagery, _area, _image.Image);
        }

        public void AfterUpdateCompletedLandCoverImagery()
        {
            TDebug.TraceMessage();

            if (_Landcoverimage == null || _Landcoverimage.Image == null) throw new Exception("Downloading Landcover data error....!");
            _Landcoverimage.Image = TImageProcessors.CropImage(_mosaicAreaImagery, _area, _Landcoverimage.Image);
        }

        public void AfterUpdateCompletedLandcover()
        {
            TDebug.TraceMessage();

            if (LandcoverXML == null )
                throw new Exception("Downloading Landcover data error....!");
        }

        public void WhenAllDone()
        {
            TDebug.TraceMessage();

            if (_ref._lastActions != null && !disposed )
                _ref._lastActions.Invoke(this);
        }

        public void GetBBoxMosaicElevation(int zoomLevel)
        {
            // Set Row/Col for TopLeft
            int[] rowColTL = TConvertors.WorldToTilePos(_area._left, _area._top, zoomLevel);
            rowLeftElevation = rowColTL[0];
            colTopElevation = rowColTL[1];
            double[] tileLatLonTL = TConvertors.TileToWorldPos(rowLeftElevation, colTopElevation, zoomLevel);
            double left = tileLatLonTL[0];
            double top = tileLatLonTL[1];

            // Set Row/Col for BottomRight
            int[] rowColBR = TConvertors.WorldToTilePos(_area._right, _area._bottom, zoomLevel);
            rowRightElevation = rowColBR[0];
            colBottomElevation = rowColBR[1];
            double[] tileLatLonBR = TConvertors.TileToWorldPos(rowRightElevation + 1, colBottomElevation + 1, zoomLevel);
            double right = tileLatLonBR[0];
            double bottom = tileLatLonBR[1];

            if (_mosaicAreaElevation == null)
                _mosaicAreaElevation = new TArea(top, left, bottom, right);
            else
                _mosaicAreaElevation.SetArea(top, left, bottom, right);

            xTilesElevation = Math.Abs(rowRightElevation - rowLeftElevation) + 1;
            yTilesElevation = Math.Abs(colTopElevation - colBottomElevation) + 1;
        }

        public void GetBBoxMosaicImagery(int zoomLevel)
        {
            // Set Row/Col for TopLeft
            int[] rowColTL = TConvertors.WorldToTilePos(_area._left, _area._top, zoomLevel);
            rowLeftImagery = rowColTL[0];
            colTopImagery = rowColTL[1];
            double[] tileLatLonTL = TConvertors.TileToWorldPos(rowLeftImagery, colTopImagery, zoomLevel);
            double left = tileLatLonTL[0];
            double top = tileLatLonTL[1];

            // Set Row/Col for BottomRight
            int[] rowColBR = TConvertors.WorldToTilePos(_area._right, _area._bottom, zoomLevel);
            rowRightImagery = rowColBR[0];
            colBottomImagery = rowColBR[1];
            double[] tileLatLonBR = TConvertors.TileToWorldPos(rowRightImagery + 1, colBottomImagery + 1, zoomLevel);
            double right = tileLatLonBR[0];
            double bottom = tileLatLonBR[1];

            if (_mosaicAreaImagery == null)
                _mosaicAreaImagery = new TArea(top, left, bottom, right);
            else
                _mosaicAreaImagery.SetArea(top, left, bottom, right);

            xTilesImagery = Math.Abs(rowRightImagery - rowLeftImagery) + 1;
            yTilesImagery = Math.Abs(colTopImagery - colBottomImagery) + 1;
        }

        public static int GetZoomLevel(int resolution , TArea area)
        {
            double temp = Math.Log(156543.03 * Math.Cos(area._center.latitude * Math.PI / 180) * resolution / (area._areaSizeLat * 1000));
//            double temp = Math.Log(156543.03 * Math.Cos(area._center.latitude * Math.PI / 180) * resolution / (area._areaSizeLat * 1000));
//            double mpp = (area._areaSizeLat * 1000) / resolution;

//            double zoomlevel2 = Math.Log(156543.03 *  mpp * Math.Cos(area._center.latitude * Math.PI / 180))/Math.Log(2) ; 

            double zoomlevel =temp / Math.Log(2);

            return (int)Math.Ceiling(zoomlevel);
        }
 
        public double GetHeight(TGlobalPoint node)
        {
            double normalizedX;
            double normalizedZ;
       
            GetNormalizedIndex(node, out normalizedX, out normalizedZ);
       
            if (normalizedX < 0 || normalizedX > 1) return 0;
            if (normalizedZ < 0 || normalizedZ > 1) return 0;
       
            double result = _heightmap.GetInterpolatedHeight(normalizedX, normalizedZ);
            return result;
        }

        public TGlobalPoint GetNearestPoint (TGlobalPoint point)
        {
            int pixelX, pixelY;
            pixelX = pixelY = 0;
            GetNearestIndex(point, out pixelX, out pixelY);
            double normalizedPixelX = (double)pixelX / Heightmap.heightsData.GetLength(0);
            double normalizedPixelY = (double)pixelY / Heightmap.heightsData.GetLength(1);
            return GetGeoPoint(normalizedPixelX, normalizedPixelY);
        }

        public TGlobalPoint GetGeoPoint(double IndexX, double IndexY)
        {
            TGlobalPoint result = new TGlobalPoint();
            double normalizedPixelX = (double)IndexX / Heightmap.heightsData.GetLength(0);
            double normalizedPixelY = (double)IndexY / Heightmap.heightsData.GetLength(1);
            result.longitude = (normalizedPixelX * (_area._right - _area._left)) + _area._left;
            result.latitude = (normalizedPixelY * (_area._top - _area._bottom)) + _area._bottom;
            return result;
        }

        public void GetNearestIndex (TGlobalPoint point , out int IndexX, out int IndexY)
        {
            Vector2 latlonDeltaNormalized = GetLatLongNormalizedPositionN(point);
            double normalizedX = latlonDeltaNormalized.Y;
            double normalizedZ = latlonDeltaNormalized.X;
            //GetNormalizedIndex(point, out normalizedX, out normalizedZ);

            IndexX = (int)(normalizedX * (Heightmap.heightsData.GetLength(0)-1));
            double tempNormalX1 = (IndexX * 1.0f/ (Heightmap.heightsData.GetLength(0) - 1));
            double tempNormalX2 = ((IndexX + 1) * 1.0f / (Heightmap.heightsData.GetLength(0) - 1));
            if ((normalizedX - tempNormalX1) > (tempNormalX2 - normalizedX))
                IndexX = IndexX + 1;

            IndexY = (int)(normalizedZ * (Heightmap.heightsData.GetLength(1) - 1));
            double tempNormalY1 = (IndexY * 1.0f/ (Heightmap.heightsData.GetLength(1) - 1));
            double tempNormalY2 = ((IndexY + 1) * 1.0f / (Heightmap.heightsData.GetLength(1) - 1));

            if ((normalizedZ - tempNormalY1) > (tempNormalY2 - normalizedZ))
               IndexY = IndexY + 1;
        }


        public T2DPoint GetPointNormalized(TGlobalPoint point)
        {
            Vector2 latlonDeltaNormalized = GetLatLongNormalizedPositionN(point);
            T2DPoint pointNormalized = new T2DPoint(latlonDeltaNormalized.Y, latlonDeltaNormalized.X);

            return pointNormalized;
        }

        public void GetNormalizedIndex(TGlobalPoint point, out double IndexX, out double IndexY)
        {
            IndexX = TUtils.InverseLerp(_area._left, _area._right, point.longitude);
            IndexY = TUtils.InverseLerp(_area._bottom, _area._top, point.latitude);
        }

        public void GetPixelBounds (TArea Src, out int minLonIndex, out int maxLonIndex, out int minLatIndex, out int maxLatIndex )
        {
            TGlobalPoint topLeft = new TGlobalPoint();
            topLeft.latitude = Src._top;
            topLeft.longitude = Src._left;
            TGlobalPoint bottomRight = new TGlobalPoint();
            bottomRight.latitude = Src._bottom;
            bottomRight.longitude = Src._right;
            GetNearestIndex(topLeft, out minLonIndex, out maxLatIndex);
            GetNearestIndex(bottomRight, out maxLonIndex, out minLatIndex);
        }

        public Vector2 GetWorldPosition(TGlobalPoint geoPoint)
        {
            double worldSizeX = _area._areaSizeLon * 1000;
            double worldSizeY = _area._areaSizeLat * 1000;

            Vector2 latlonDeltaNormalized = GetLatLongNormalizedPositionN( geoPoint);

            Vector2 worldPositionXZ = AreaBounds.GetWorldPositionFromTile(latlonDeltaNormalized.X, latlonDeltaNormalized.Y, worldSizeY, worldSizeX);
            //Vector3d worldPositionTemp = new Vector3d(worldPositionXZ.x + worldSizeY / 2, 0, worldPositionXZ.y - worldSizeX / 2);
            Vector3d worldPositionTemp = new Vector3d(worldPositionXZ.X , 0, worldPositionXZ.Y );

            return new Vector2((float)worldPositionTemp.x, (float)worldPositionTemp.z);
        }

        public Vector2 GetLatLongNormalizedPositionN(TGlobalPoint geoPoint)
        {
            double yMaxTop = AreaBounds.LatitudeToMercator(_area._top);
            double xMinLeft = AreaBounds.LongitudeToMercator(_area._left);
            double yMinBottom = AreaBounds.LatitudeToMercator(_area._bottom);
            double xMaxRight = AreaBounds.LongitudeToMercator(_area._right);
            double latSize = Math.Abs(yMaxTop - yMinBottom);
            double lonSize = Math.Abs(xMinLeft - xMaxRight);
            double LAT = AreaBounds.LatitudeToMercator(geoPoint.latitude);
            double LON = AreaBounds.LongitudeToMercator(geoPoint.longitude);

            double[] latlonDeltaNormalized = AreaBounds.GetNormalizedDeltaN(LAT, LON, yMaxTop, xMinLeft, latSize, lonSize);

            return new Vector2((float)latlonDeltaNormalized[0], (float)latlonDeltaNormalized[1]);
        }
    }
}
#endif
#endif

