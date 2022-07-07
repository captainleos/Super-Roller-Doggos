#if TERRAWORLD_PRO
using UnityEngine;
using UnityEditor;
using System;
using System.Drawing;
using TerraUnity.Edittime;
using TerraUnity.Runtime;

namespace TerraUnity.UI
{
    [ExecuteInEditMode]
    public class TAreaPreview //: MonoBehaviour
    {
        public static Terrain terrain;
        private static TerrainData tData;
        private static GameObject terrainGO;
        private const float everestHeight = 8848f;
        private static Vector3 terrainSize = new Vector3(2000, everestHeight, 2000);
        private static int _activeZoomLevel;
        private static float baseMapDistance = 100000;
        private static string previewTerrainName = "TerraWorld Preview Terrain";
        public static float areaWidthMeters, areaLengthMeters;
        private static int heightmapMaximumLOD = 20;
        private static latlong_class coordsTopLft = new latlong_class(0, 0);
        private static latlong_class coordsBotRgt = new latlong_class(0, 0);
        private static latlong_class coordsTopLftPrevious = new latlong_class(90, -180);
        private static latlong_class coordsBotRgtPrevious = new latlong_class(-90, 180);
        private static TMap _previewMap = null;
        private static TArea previewArea;
        private static int scaleX, scaleZ, offsetX, offsetZ;
        private static float _minElevation = -450;
        private static float _maxElevation = 8848;
        private static Bitmap _coverImage = null;
        private static Texture2D _coverImageTexture = null;
        private static Texture2D _satImageTexture = null;
        private static bool isVisible = false;
        private static bool isForcedVisible = false;
        //private static bool showbounds = false;
        //private static bool mapReady = false;
        private static Action _lastActions = null;
        private static TerrainLayer[] TL = null;

        public static int Progress
        {
            get
            {
                return _previewMap.Progress;
            }
        }

        public static float MinElevation
        {
            get
            {
                return _minElevation;
            }
        }

        public static float MaxElevation
        {
            get
            {
                return _maxElevation;
            }
        }

        public static bool Visible
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = (value || isForcedVisible);

                //if (!value) showbounds = false;

                if (!isVisible)
                    UpdateVisibility();
                else
                    UpdatePreview3D();
            }
        }

        public static bool ForceVisible
        {
            get
            {
                return isForcedVisible;
            }
            set
            {
                isForcedVisible = value;
                if (THelpersUI.showPreviewMask || THelpersUI.showOutputMask) UpdateTerrainImagery();
                else Visible = value;
            }
        }

        //public static bool BoundsVisible { get => showbounds; }
        public static TMap PreviewMap { get => _previewMap; }

        public static void ShowImage(Bitmap bitmap)
        {
            if (terrain == null) return;
            if (_coverImage == bitmap) return;

            if (bitmap == null)
            {
                _coverImage = null;
                _coverImageTexture = null;
            }
            else
            {
                _coverImage = bitmap;
                _coverImageTexture = new Texture2D(_coverImage.Width, _coverImage.Height);
                _coverImageTexture.LoadImage(TImageProcessors.ImageToByte(_coverImage));
            }

            Visible = true;
        }

        public static void ShowImage(Texture2D bitmap)
        {
            if (terrain == null) return;
            if (_coverImageTexture == bitmap) return;

            if (bitmap == null)
            {
                _coverImage = null;
                _coverImageTexture = null;
            }
            else
            {
                _coverImage = null;
                _coverImageTexture = bitmap;
            }

            Visible = true;
        }

        public static void ShowMaskOnTerrain(TMask mask)
        {
            if (terrain == null) return;
            if (_satImageTexture == null) return;
           _coverImageTexture = new Texture2D( _satImageTexture.width, _satImageTexture.height);

            for (int i = 0; i < _satImageTexture.width; i ++)
            {
                for (int j = 0; j < _satImageTexture.height; j ++)
                {
                    float indexX = i * 1.0f / _satImageTexture.width;
                    float indexY = j * 1.0f / _satImageTexture.height;

                    if (mask.CheckNormal(indexX, indexY))
                        _coverImageTexture.SetPixel(i, j, UnityEngine.Color.red);
                    else
                        _coverImageTexture.SetPixel(i, j, _satImageTexture.GetPixel(i,j));
                }
            }

            _coverImageTexture.Apply();
            _coverImage = null;
            Visible = true;
        }

        public static void UpdatePreview3D (Action action = null)
        {
            //mapReady = false;
            _lastActions = action;
            int _zoomLevel = TMap.GetZoomLevel(TProjectSettings.PreviewResolution, TTerraWorld.Area);

            if
                (

                    terrain != null && _activeZoomLevel == _zoomLevel && _previewMap != null &&
                    previewArea._top == TTerraWorld.Area._top &&
                    previewArea._left == TTerraWorld.Area._left &&
                    previewArea._bottom == TTerraWorld.Area._bottom &&
                    previewArea._right == TTerraWorld.Area._right
                )
            {
                UpdateVisibility();
                //mapReady = true;
                if (_lastActions != null) _lastActions.Invoke();
            }
            else 
                RequestArea();
        }

        private static void UpdateVisibility()
        {
            if (terrain != null) terrain.drawHeightmap = isVisible;
            if (!isVisible ) _coverImageTexture = null;
            if (isVisible) UpdateTerrainImagery();

            //if (showbounds && isVisible)
            //    TBoundingBox.UpdateBoundingBox();
            //else if (TBoundingBox.boundingBox != null && !showbounds)
            //    DestroyImmediate(TBoundingBox.boundingBox);

            if(TTerraWorldManager.MainTerrainGO != null)
            {
                if (isVisible)
                    TTerraWorldManager.MainTerrainGO.SetActive(false);
                else
                    TTerraWorldManager.MainTerrainGO.SetActive(true);
            }

            SceneView.RepaintAll();
        }

        private static void RequestArea()
        {
            previewArea = new TArea(TTerraWorld.Area._top, TTerraWorld.Area._left, TTerraWorld.Area._bottom, TTerraWorld.Area._right);
            _activeZoomLevel = TMap.GetZoomLevel(TProjectSettings.PreviewResolution, TTerraWorld.Area);
            coordsTopLft = new latlong_class(previewArea._top, previewArea._left);
            coordsBotRgt = new latlong_class(previewArea._bottom, previewArea._right);
            _previewMap?.Dispose();
            _previewMap = new TMap(coordsTopLft.latitude, coordsTopLft.longitude, coordsBotRgt.latitude, coordsBotRgt.longitude, null, _activeZoomLevel, _activeZoomLevel);

            // Download elevation tiles (for scene view preview terrain)
            _previewMap.RequestElevationData = true;
            _previewMap.RequestImageData = true;
            _previewMap.RequestLandcoverData = false;

            // Cache previous coords here for entry checkout
            coordsTopLftPrevious = coordsTopLft;
            coordsBotRgtPrevious = coordsBotRgt;

            // Run corresponding action
            _previewMap.UpdateMap(GeneratePreviewWorld);
        }

        // Real-time scene preview of the world
        public static void GeneratePreviewWorld (TMap previewMap)
        {
            _previewMap = previewMap;
            areaWidthMeters = _previewMap._area._areaSizeLon * 1000;
            areaLengthMeters = _previewMap._area._areaSizeLat * 1000;
            InitializeTerrain();
            UpdateTerrainHeightmap(terrain, previewMap.Heightmap.heightsData, areaWidthMeters, areaLengthMeters);

            _satImageTexture = new Texture2D(previewMap.Image.Image.Width, previewMap.Image.Image.Height);
            _satImageTexture.LoadImage(TImageProcessors.ImageToByte(previewMap.Image.Image));

            TL = new TerrainLayer[1];
            TL[0] = new TerrainLayer();
            TL[0].tileSize = new Vector2(tData.size.x, tData.size.z);
            tData.terrainLayers = TL;

            float minElevation, maxElevation;
            THeightmapProcessors.GetMinMaxElevationFromHeights(previewMap.Heightmap.heightsData, out minElevation, out maxElevation);
            _minElevation = minElevation ;
            _maxElevation = maxElevation ;
            if (_lastActions != null) _lastActions.Invoke();

            UpdateVisibility();
            //mapReady = true;
        }

        public static void InitializeTerrain ()
        {
            if (GameObject.Find(previewTerrainName) != null)
            {
                terrain = GameObject.Find(previewTerrainName).GetComponent<Terrain>();
                tData = terrain.terrainData;
                terrain.heightmapMaximumLOD = heightmapMaximumLOD;
                return;
            }

            // Create TerrainData
            tData = new TerrainData();

            // Create Terrain Object
            terrainGO = Terrain.CreateTerrainGameObject(tData);
            terrainGO.name = previewTerrainName;
            terrainGO.hideFlags = HideFlags.HideAndDontSave;
            terrainGO.isStatic = false;

            // Reference to the Terrain system
            terrain = terrainGO.GetComponent<Terrain>();
            terrain.heightmapMaximumLOD = heightmapMaximumLOD;
            terrain.drawHeightmap = false;
        }

        public static void UpdateTerrainHeightmap(Terrain terrain, float[,] orginalHeightmap, float areaSizeX, float areaSizeZ)
        {
            float[,] heightmap = THeightmapProcessors.NormalizeHeightmap(orginalHeightmap);
            tData.heightmapResolution = heightmap.GetLength(0);
            tData.SetHeights(0, 0, heightmap);
            Vector3 terrainSize = new Vector3(areaSizeX, everestHeight + 500, areaSizeZ) * TTerraWorldGraph.scaleFactor;
            tData.size = terrainSize;
            terrain.transform.position = new Vector3(-(terrainSize.x / 2f), 0, -(terrainSize.z / 2f));
            terrain.basemapDistance = baseMapDistance;
            terrain.drawInstanced = false;
            terrain.GetComponent<TerrainCollider>().enabled = false;
            terrain.Flush();
            terrain.drawHeightmap = true;
        }

        public static void UpdateTerrainImagery()
        {
            if (_coverImageTexture == null || isForcedVisible )
                TL[0].diffuseTexture = _satImageTexture; 
            else
                TL[0].diffuseTexture = _coverImageTexture;
        }

        //public static void GetBBox ()
        //{
        //    showbounds = true;
        //    Visible = true;
        //}
    }
}
#endif

