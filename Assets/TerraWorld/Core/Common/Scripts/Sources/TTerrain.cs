#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using TerraUnity.Graph;
using TerraUnity.Utils;
using TerraUnity.Runtime;

namespace TerraUnity.Edittime
{
    public class TTerrain 
    {
        private TArea _area;
        private TMap map;
        private TTerraWorld _terraWorldRef;
        private Action<TTerrain> _lastActions;
        public TDetailTextureCollection detailTextureCollection;
        public TDetailTextureCollection colorMapTextureCollection;
        private List<TOceanLayer> _oceanLayers = new List<TOceanLayer>();
        private List<TLakeLayer> _lakeLayers = new List<TLakeLayer>();
        private List<TRiverLayer> _riverLayers = new List<TRiverLayer>();
        private List<TGridLayer> _gridsLayers = new List<TGridLayer>();
        private List<TObjectScatterLayer> _objectScatterLayers = new List<TObjectScatterLayer>();
        private List<TInstanceScatterLayer> _instanceScatterLayers = new List<TInstanceScatterLayer>();
        private List<TGrassScatterLayer> _grassScatterLayers = new List<TGrassScatterLayer>();
        private THeightmap _heightmap;
        private string _ID;
        private float _progress;
        private Exception exception = null;
        public TMask overallWaterMask = null;
        private int maximumElevationResolution = 4096;
        public float[,] bgHeightmap = null;
        public Bitmap bgImage = null;
        public int PixelError = 5;

        public float Progress
        {
            get 
            {
                return (float)(_progress * 0.7 + 0.3 * Map.Progress);
            }
        }

        public TArea Area { get => _area; }
        public TMap Map { get => map; set => map = value; }
        public TTerraWorld TerraWorld { get => _terraWorldRef; }
        public Action<TTerrain> LastActions { get => _lastActions; set => _lastActions = value; }
        public List<TLakeLayer> LakeLayer { get => _lakeLayers; }
        public List<TOceanLayer> OceanLayer { get => _oceanLayers; }
        public List<TRiverLayer> RiverLayer { get => _riverLayers; }
        public THeightmap Heightmap { get => _heightmap; set => _heightmap = value; }
        public List<TGridLayer> GridsLayers { get => _gridsLayers; }
        public List<TObjectScatterLayer> ObjectScatterLayers { get => _objectScatterLayers; }
        public List<TInstanceScatterLayer> InstanceScatterLayers { get => _instanceScatterLayers; }
        public List<TGrassScatterLayer> GrassScatterLayers { get => _grassScatterLayers; }

        public static TMap currentMap;

        Bitmap aspectMap, curvatureMap, flowMap, normalMap, slopeMap;

        public TTerrain (double top, double left, double bottom, double right, TTerraWorld terraWorld)
        {
            _area = new TArea(top, left, bottom, right);
            _terraWorldRef = terraWorld;
            _ID = TTerraWorldGraph.GetNewID().ToString();
            detailTextureCollection = new TDetailTextureCollection(this);
            colorMapTextureCollection = new TDetailTextureCollection(this);
            _oceanLayers = new List<TOceanLayer>();
            _lakeLayers = new List<TLakeLayer>();
            _riverLayers = new List<TRiverLayer>();
            Heightmap = new THeightmap();
        }

        public void UpdateTerrain ()
        {
            TDebug.TraceMessage();
            TTerrainGenerator.SetStatusToOnProgress();
            bool requestElevationData = false;
            bool requestImageData = false;
            bool requestLandcoerData = false;
            SatelliteImage satelliteImage = TTerraWorld.WorldGraph.colormapGraph.SatelliteImage();

            if (satelliteImage != null)
            {
                TerraWorld.ImagerySource = satelliteImage._source;
                TerraWorld.ImageZoomLevel = TMap.GetZoomLevel(satelliteImage.resolution, _area);
                requestImageData = true;
            }

            HeightmapSource heightmapSource = TTerraWorld.WorldGraph.heightmapGraph.HeightmapSource();

            if (heightmapSource != null)
            {
                TerraWorld.ElevationSource = heightmapSource._source;
                TerraWorld.ElevationZoomLevel = heightmapSource.highestResolution ? TMap.GetZoomLevel(maximumElevationResolution, _area) : TMap.GetZoomLevel(heightmapSource._resolution, _area);
                requestElevationData = true;
            }

            if (TTerraWorld.WorldGraph.biomesGraph.AnyLandCoverDataNode())
                requestLandcoerData = true;

#if TERRAWORLD_XPRO
            TXRealWorldSourceNode XRWSourceNode = TTerraworldGenerator.XGraph.GetRealWorldSourceNode();

            if (XRWSourceNode != null)
            {
                TerraWorld.ImagerySource = XRWSourceNode.ImagerySource;
                TerraWorld.ImageZoomLevel = TMap.GetZoomLevel(XRWSourceNode.ImageryResolution, _area);
                requestImageData = true;
                TerraWorld.ElevationSource = XRWSourceNode.HeightmapSource;
                TerraWorld.ElevationZoomLevel = XRWSourceNode.highestResolution ? TMap.GetZoomLevel(maximumElevationResolution, _area) : TMap.GetZoomLevel(XRWSourceNode.HeightmapResolution, _area);
                requestElevationData = true;
                requestLandcoerData = true;
            }
#endif

            Map = new TMap(_area, this, TerraWorld.ImageZoomLevel, TerraWorld.ElevationZoomLevel);
            Map.SaveTilesImagery = TTerraWorld.CacheData;
            Map.SaveTilesElevation = TTerraWorld.CacheData;
            Map.SaveTilesLandcover = TTerraWorld.CacheData;
            Map._mapElevationSource = TerraWorld.ElevationSource;
            Map._mapImagerySource = TerraWorld.ImagerySource;
            Map._mapLandcoverSource = TerraWorld.LandcoverSource;
            Map.RequestElevationData = requestElevationData;
            Map.RequestImageData = requestImageData;
            Map.RequestLandcoverData = requestLandcoerData;
            Map.UpdateMap(TerrainAnalysis);
            currentMap = Map;
        }

        private void PerformGC ()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }

        private void TerrainAnalysis (TMap CurrentMap)
        {
            TDebug.TraceMessage();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RunAllModulesAsync(CurrentMap._refTerrain);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public void RunAllModulesASync(TTerrain terrain)
        {
            TDebug.TraceMessage();

            try
            {
                RunAllModules(terrain);
            }
            catch (Exception e)
            {
                exception = e;
            }
        }

        public void RunAllModules(TTerrain terrain)
        {
            TDebug.TraceMessage();
            List<TNode> lastNodes;
            if (terrain.map.Heightmap.heightsData == null) throw new Exception("Internal Error : 02");

            _progress = 5;
            overallWaterMask = new TMask(terrain.map.Heightmap.heightsData.GetLength(0), terrain.map.Heightmap.heightsData.GetLength(1));
            terrain.Heightmap.heightsData = terrain.map.Heightmap.heightsData;

            // Heightmap Interpreter
            //---------------------------------------------------------------------------------------------------------------------------------------------------

#if TERRAWORLD_XPRO
            TXTerrainNode XTerrainNode = TTerraworldGenerator.XGraph.GetTerrainNode();
            if (!TTerrainGenerator.WorldInProgress) return;
            if (XTerrainNode == null) throw new Exception("There is no terrain generator node in graph. Insert a \"Terrain Generator Node\" ");

            terrain.PixelError = XTerrainNode.PixelError;
            terrain.Heightmap.heightsData = XTerrainNode.GetProceededHeightMap(terrain);
            TDebug.TraceMessage("Heightmap Node : " + XTerrainNode.NodeName);

#else
            lastNodes = TTerraWorld.WorldGraph.heightmapGraph.RunGraph(terrain, ConnectionDataType.HeightmapMaster);
            if (!TTerrainGenerator.WorldInProgress) return;
            if (lastNodes.Count > 1) throw new Exception("More than one end node detected on heightmap graph!");
            if (lastNodes.Count < 1) throw new Exception("There is no end node for heightmap graph.");


            for (int i = 0; i < lastNodes.Count; i++)
            {
                terrain.PixelError = ((HeightmapMaster)lastNodes[0])._pixelError;
                THeightmapModules lastnode = (THeightmapModules)lastNodes[i];

                if (lastnode._heightmapData != null)
                {
                    terrain.Heightmap.heightsData = lastnode._heightmapData;
                }

                TDebug.TraceMessage("heightmapGraph result : " + lastnode.Data.name);
            }

#endif
            _progress = 20;
            PerformGC();

            // Lakes Interpreter
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO
            List<TXWaterGeneratorNode> waterGeneratorNodes = TTerraworldGenerator.XGraph.GetWaterGeneratorNodes();
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < waterGeneratorNodes.Count; i++)
            {
                TXWaterGeneratorNode lastnode = waterGeneratorNodes[i];
                TLakeLayer newlakesLayer = lastnode.GetLakes(terrain);
                TRiverLayer newriversLayer = lastnode.GetRivers(terrain);
                TOceanLayer newoceanLayer = lastnode.GetOceans(terrain);
                if (newlakesLayer != null) terrain.AddLakeLayer(newlakesLayer);
                if (newriversLayer != null) terrain.AddRiverLayer(newriversLayer);
                if (newoceanLayer != null) terrain.AddOceanLayer(newoceanLayer);
                TDebug.TraceMessage("Water Node : " + lastnode.NodeName);

            }

#else
            lastNodes = TTerraWorld.WorldGraph.biomesGraph.RunGraph(terrain, ConnectionDataType.Lakes);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < lastNodes.Count; i++)
            {
                TWaterModules lastnode = (TWaterModules)lastNodes[i];
                TLakeLayer newlakesLayer = lastnode._lakeLayer;
                TRiverLayer newriversLayer = lastnode._riverLayer;
                TOceanLayer newoceanLayer = lastnode._oceanLayer;
                if (newlakesLayer != null) terrain.AddLakeLayer(newlakesLayer);
                if (newriversLayer != null) terrain.AddRiverLayer(newriversLayer);
                if (newoceanLayer != null) terrain.AddOceanLayer(newoceanLayer);
                TDebug.TraceMessage("Water result : " + lastnode.Data.name);
            }
#endif
            _progress = 40;
            PerformGC();

            // ColorMap
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO

            if (!TTerrainGenerator.WorldInProgress) return;
            TImage XcolorMap = XTerrainNode.GetColorMap(terrain);
            if (XcolorMap != null )
            {
                TDetailTexture ColoMapDetailTexture = new TDetailTexture(XcolorMap.Image);
                ColoMapDetailTexture.Tiling = new Vector2(terrain.map._area._areaSizeLat * 1000, terrain.map._area._areaSizeLon * 1000);
                terrain.colorMapTextureCollection.Add(ColoMapDetailTexture);
            }

#else
            lastNodes = TTerraWorld.WorldGraph.colormapGraph.RunGraph(terrain, ConnectionDataType.ColormapMaster);
                if (!TTerrainGenerator.WorldInProgress) return;

                for (int i = 0; i < lastNodes.Count; i++)
                {
                    TImageModules lastnode = (TImageModules)lastNodes[i];
                    TDetailTexture colorMapDetailTexture = lastnode._detailTexture;

                    if (colorMapDetailTexture != null )
                    {
                        terrain.colorMapTextureCollection.Add(colorMapDetailTexture);
                        TDebug.TraceMessage("Graph result (Colormap): " + lastnode.Data.name);
                    }
                }

#endif
            _progress = 50;
            PerformGC();

            // TerrainLayers
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO

            List<TDetailTexture> terrainLayers = XTerrainNode.GetDetailedTextures(terrain);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < terrainLayers.Count; i++)
            {
                if (terrainLayers[i] != null)
                {
                    terrain.detailTextureCollection.Add(terrainLayers[i]);
                }
            }

#else
            lastNodes = TTerraWorld.WorldGraph.colormapGraph.RunGraph(terrain, ConnectionDataType.DetailTextureMaster);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < lastNodes.Count; i++)
            {
                TImageModules lastnode = (TImageModules)lastNodes[i];
                TDetailTexture detailTexture = lastnode._detailTexture;

                if (detailTexture != null)
                {
                    terrain.detailTextureCollection.Add(detailTexture);
                    TDebug.TraceMessage("Graph result (DetailTexture): " + lastnode.Data.name);
                }
            }

#endif
            _progress = 60;
            PerformGC();


            // Meshes
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO

            List<TXMeshGeneratorNode> meshGeneratorNodes = TTerraworldGenerator.XGraph.GetMeshGeneratorNodes();
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < meshGeneratorNodes.Count; i++)
            {
                TXMeshModules lastnode = meshGeneratorNodes[i];
                TGridLayer newGridLayer = lastnode.GetMeshLayer(terrain);
                if (newGridLayer != null) terrain.AddGridLayer(newGridLayer);
            }

#else
            lastNodes = TTerraWorld.WorldGraph.biomesGraph.RunGraph(terrain, ConnectionDataType.Mesh);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < lastNodes.Count; i++)
            {
                TGridModules lastnode = (TGridModules)lastNodes[i];
                TGridLayer newGridLayer = lastnode._gridLayer;
                if (newGridLayer != null) terrain.AddGridLayer(newGridLayer);
                TDebug.TraceMessage("Graph result : " + lastnode.Data.name);
            }
#endif
            _progress = 70;
            PerformGC();

            // OSM Landcover Interpreter
            //---------------------------------------------------------------------------------------------------------------------------------------------------
#if TERRAWORLD_XPRO


#else

            lastNodes = TTerraWorld.WorldGraph.biomesGraph.RunGraph(terrain, ConnectionDataType.ObjectScatter);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < lastNodes.Count; i++)
            {
                TScatteredObjectModules lastnode = (TScatteredObjectModules)lastNodes[i];
                TObjectScatterLayer newobjectsLayer = lastnode._objectScatterLayer;
                if (newobjectsLayer != null) terrain.AddObjectScatterLayer(newobjectsLayer);
                TDebug.TraceMessage("Graph result : " + lastnode.Data.name);
            }
            _progress = 80;
            PerformGC();

            lastNodes = TTerraWorld.WorldGraph.biomesGraph.RunGraph(terrain, ConnectionDataType.InstanceScatter);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < lastNodes.Count; i++)
            {
                TScatteredInstanceModules lastnode = (TScatteredInstanceModules)lastNodes[i];
                TInstanceScatterLayer newInstancesLayer = lastnode._instanceScatterLayer;
                if (newInstancesLayer != null)  terrain.AddInstanceScatterLayer(newInstancesLayer);
                TDebug.TraceMessage("Graph result : " + lastnode.Data.name);
            }
            _progress = 90;
            PerformGC();

            lastNodes = TTerraWorld.WorldGraph.biomesGraph.RunGraph(terrain, ConnectionDataType.GrassScatter);
            if (!TTerrainGenerator.WorldInProgress) return;

            for (int i = 0; i < lastNodes.Count; i++)
            {
                TScatteredGrassModules lastnode = (TScatteredGrassModules)lastNodes[i];
                TGrassScatterLayer newInstancesLayer = lastnode._grassScatterLayer;
                if (newInstancesLayer != null) terrain.AddGrassScatterLayer(newInstancesLayer);
                TDebug.TraceMessage("Graph result : " + lastnode.Data.name);
            }
#endif

        _progress = 100;
            PerformGC();
        }


        public async Task RunAllModulesAsync (TTerrain terrain)
        {
            TDebug.TraceMessage();
            exception = null;

            await Task.Run(() => RunAllModulesASync(terrain));


            if (exception == null)
            {
                if (TTerrainGenerator.WorldInProgress) 
                    WhenAllDone();
            }
            else
                TTerrainGenerator.RaiseException(exception);
        }


        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
                throw new ArgumentException("The type must be serializable.", "source");

            // Don't serialize a null object, simply return the default for that object
            if (System.Object.ReferenceEquals(source, null))
                return default(T);

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();

            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);

                return (T)formatter.Deserialize(stream);
            }
        }

        private bool IsAnyCollision (TPointObject pointObject)
        {
            for (int i = 0; i < _lakeLayers.Count; i++)
                for (int j = 0; j < _lakeLayers[i].LakesList.Count; j++)
                {
                    if (TUtils.PointInPolygon(_lakeLayers[i].LakesList[j].AroundPoints, pointObject.GeoPosition))
                        return true;
                }

            return false;
        }

        public void AddLakeLayer (TLakeLayer lakeLayer)
        {
            TMask mask = TMask.MergeMasks(lakeLayer.WaterMasks);
            overallWaterMask.OR(mask);
            _lakeLayers.Add(lakeLayer);
        }

        public void AddRiverLayer(TRiverLayer riverLayer)
        {
            TMask mask = TMask.MergeMasks(riverLayer.WaterMasks);
            overallWaterMask.OR(mask);
            _riverLayers.Add(riverLayer);
        }

        public void AddOceanLayer(TOceanLayer oceanLayer)
        {
            TMask mask = TMask.MergeMasks(oceanLayer.WaterMasks);
            overallWaterMask.OR(mask);
            _oceanLayers.Add(oceanLayer);
        }

        public void AddGridLayer(TGridLayer GridLayer)
        {
            _gridsLayers.Add(GridLayer);
        }

        public void AddObjectScatterLayer(TObjectScatterLayer ObjectScatterLayer)
        {
            //if (!ObjectScatterLayer.underLake)
            //{
            //    for (int i = 0; i < ObjectScatterLayer.points.Count; i++)
            //    {
            //        TPointObject Object = ObjectScatterLayer.points[i];
            //
            //        if (IsAnyCollision(Object))
            //        {
            //            ObjectScatterLayer.points.Remove(Object);
            //            i--;
            //        }
            //    }
            //}
        
            _objectScatterLayers.Add(ObjectScatterLayer);
        }

        public void AddInstanceScatterLayer(TInstanceScatterLayer instanceScatterLayer)
        {
            _instanceScatterLayers.Add(instanceScatterLayer);
        }

        public void AddGrassScatterLayer(TGrassScatterLayer grassScatterLayer)
        {
            _grassScatterLayers.Add(grassScatterLayer);
        }

        public Vector3 GetWorldPositionWithHeight(TGlobalPoint geoPoint)
        {
            Vector2 latlonDeltaNormalized = Map.GetLatLongNormalizedPositionN(geoPoint);
            Vector2 XZ = map.GetWorldPosition(geoPoint);
            float height = (float)_heightmap.GetInterpolatedHeight(latlonDeltaNormalized.Y, latlonDeltaNormalized.X);
            return new Vector3((float)XZ.X, height, (float)XZ.Y);
        }

        public Vector3 GetAngle(TGlobalPoint geoPoint)
        {
            Vector2 latlonDeltaNormalized = Map.GetLatLongNormalizedPositionN(geoPoint);
            return _heightmap.GetInterpolatedNormal(latlonDeltaNormalized.X, latlonDeltaNormalized.Y);
        }

        public Vector2 GetNormalPositionN(TGlobalPoint geoPoint)
        {
            return Map.GetLatLongNormalizedPositionN(geoPoint);
        }

        public float GetSteepness(TGlobalPoint geoPoint)
        {
            Vector2 latlonDeltaNormalized = Map.GetLatLongNormalizedPositionN(geoPoint);
            float result = _heightmap.GetSteepness(latlonDeltaNormalized.Y, latlonDeltaNormalized.X, map._area._areaSizeLat * 1000 , map._area._areaSizeLon * 1000);
            return result;
        }

        public void WhenAllDone()
        {
            TDebug.TraceMessage();
            TerraWorld.EachTerrainDone(this);
        }
    }
}
#endif
#endif

