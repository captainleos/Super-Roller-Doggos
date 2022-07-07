#if TERRAWORLD_PRO
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TerraUnity.Edittime
{
    public enum AreaProcessors
    {
        BaseArea
    }

    public class TAreaModules : TNode
    {
        public TAreaModules() : base()
        {
            isSource = true;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

    }

    [XmlType("WorldArea")]
    public class WorldArea : TAreaModules
    {
     //   public event TTerraWorldGraph.AreaChangedHandler AreaChanged;

        private int _zoomLevel = 13;
        private string _latitude = "36.05695";
        private string _longitude = "-112.136772";

        public string _address = "Grand Canyon";
        [XmlIgnore] public List<double[]> _coords;
        [XmlIgnore] public List<string> _locations;

        private float _worldSizeKMLat = 4f;
        private float _worldSizeKMLon = 4f;
        private string _top;
        private string _left;
        private string _bottom;
        private string _right;
        private TArea _area;
        public TArea Area { get { return _area; }  }

        public string latitude { get => _latitude; set { if (_latitude == value) return; _latitude = value; GetTerrainBounds(); } }
        public string longitude { get => _longitude; set { if (_longitude == value) return; _longitude = value; GetTerrainBounds(); } }
        public int zoomLevel { get => _zoomLevel; set => _zoomLevel = value; }
        public string Top { get => _top; set { if (_top == value) return; _top = value; GetLatLong(); } }
        public string Left { get => _left; set { if (_left == value) return; _left = value; GetLatLong(); } }
        public string Bottom { get => _bottom; set { if (_bottom == value) return; _bottom = value; GetLatLong(); } }
        public string Right { get => _right; set { if (_right == value) return; _right = value; GetLatLong(); } }
        public float WorldSizeKMLat { get => _worldSizeKMLat; set { if (_worldSizeKMLat == value) return; _worldSizeKMLat = value; GetTerrainBounds(); } }
        public float WorldSizeKMLon { get => _worldSizeKMLon; set { if (_worldSizeKMLon == value) return; _worldSizeKMLon = value; GetTerrainBounds(); } }

        public WorldArea () : base()
        {

         //   type = typeof(WorldArea).FullName;
            Data.name = "Area Source";
            Data.moduleType = ModuleType.Processor;
            isRemovable = false;
            isRunnable = false;
        }

        public override void InitConnections ()
        {
            inputConnections = new List<TConnection>();
            outputConnectionType = ConnectionDataType.Area;
        }

        public override List<string> GetResourcePaths()
        {
            return null;
        }

        public void GetTerrainBounds ()
        {
            if (double.Parse(latitude) > 90) latitude = "90";
            if (double.Parse(latitude) < -90) latitude = "-90";
            if (double.Parse(longitude) > 180) latitude = "180";
            if (double.Parse(longitude) < -180) latitude = "-180";

            // Center Lat Lon
            double lat = double.Parse(latitude);
            double lon = double.Parse(longitude);

            // Earth's radius, sphere
            double R = 6378137;

            // Offsets in meters
            double dn = (WorldSizeKMLat / 2f) * 1000.0;
            double de = (WorldSizeKMLon / 2f) * 1000.0;

            // Coordinate offsets in radians
            double dLat = dn / R;
            double dLon = de / (R * Math.Cos(Math.PI * lat / 180));

            _top = (lat + dLat * 180 / Math.PI).ToString(); // Top
            _left = (lon - dLon * 180 / Math.PI).ToString(); // Left
            _bottom = (lat - dLat * 180 / Math.PI).ToString(); // Bottom
            _right = (lon + dLon * 180 / Math.PI).ToString(); // Right

            GetLatLong();
        }

        public void GetLatLong ()
        {
            if (string.IsNullOrEmpty(_top)) return;
            if (string.IsNullOrEmpty(_left)) return;
            if (string.IsNullOrEmpty(_bottom)) return;
            if (string.IsNullOrEmpty(_right)) return;

            _area = new TArea(double.Parse(_top), double.Parse(_left), double.Parse(_bottom), double.Parse(_right));
            _latitude = _area._center.latitude.ToString();
            _longitude = _area._center.longitude.ToString();
            _worldSizeKMLat = _area._areaSizeLat;
            _worldSizeKMLon = _area._areaSizeLon;

            //if (parentGraph != null)
            //{
            //    // if (AreaChanged == null) AreaChanged += parentGraph.worldGraph.HandleAreaWorldChange;
            //    // AreaChanged();
            //    parentGraph.worldGraph?.HandleAreaWorldChange();
            //}
        }
    }

    public class TAreaGraph : TGraph
    {
        // UI Settings
        public static int areaSelectionMode = 0;
        public static bool squareArea = true;
        private static WorldArea WorldAreaNode;

        public TAreaGraph() : base(ConnectionDataType.Area, "Area Graph")
        {
        }

        // -----------------------------------------------------------------------------------------------------------------------------------


        public void InitGraph ( TTerraWorldGraph terraWorldGraph )
        {

            worldGraph = terraWorldGraph;

            _title = "AREA";

            if (nodes.Count > 0) return;

            WorldAreaNode = new WorldArea();
            WorldAreaNode.Init(this);
            WorldAreaNode.GetTerrainBounds();

            nodes.Add(WorldAreaNode);
        }

        public WorldArea WorldArea
        {
            get
            {
                return nodes[0] as WorldArea;
            }
        }
    }
}
#endif
#endif

