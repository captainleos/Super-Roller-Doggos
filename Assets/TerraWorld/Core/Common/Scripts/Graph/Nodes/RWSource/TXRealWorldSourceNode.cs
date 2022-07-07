#if TERRAWORLD_XPRO
using System;
using System.Drawing;
using TerraUnity.Edittime;
using XNode;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Sources/Real World Data")]
    [NodeWidth(300)]

    public class TXRealWorldSourceNode : TXNode
    {
        [Output] public TXHeightmapModules Heightmap;
        [Output] public TXImageModules Image;
        [Output] public TXLandcoverModules Landcover_Data;

        public TMapManager.mapElevationSourceEnum HeightmapSource = TMapManager.mapElevationSourceEnum.ESRI;
        private int _HeightmapResolution = 1024;
        public int HeightmapResolution { get => _HeightmapResolution; set => _HeightmapResolution = value; }

        public bool highestResolution = true;

        public float elevationExaggeration = 1;

        public TMapManager.mapImagerySourceEnum ImagerySource = TMapManager.mapImagerySourceEnum.ESRI;
        private int _imageryResolution = 1024;
        public int ImageryResolution { get => _imageryResolution; set => _imageryResolution = value; }

        private TXHeightmapSourceNode hmsn;
        private TXSatImageSourceNode sisn;
        private TXLandcoverSourceNode lcsn;


        protected override void Init()
        {
            base.Init();
            SetName("Real World Data");
        }

       public override void CheckEssentioalInputs()
       {
       }

        public override object GetValue(NodePort port)
        {

            if (port.fieldName == "Heightmap")
            {
                if (hmsn == null) 
                    hmsn = new TXHeightmapSourceNode();
                hmsn.source = HeightmapSource;
                hmsn.Resolution = _HeightmapResolution;
                hmsn.highestResolution = highestResolution;
                hmsn.elevationExaggeration = elevationExaggeration;
                hmsn.IsDone = false;
                return hmsn;
            }

            if (port.fieldName == "Image")
            {
                if (sisn == null) sisn = new TXSatImageSourceNode();
                sisn._source = ImagerySource;
                sisn.resolution = ImageryResolution;
                sisn.IsDone = false;
                return sisn;
            }

            if (port.fieldName == "Landcover_Data")
            {
                if (lcsn == null) lcsn = new TXLandcoverSourceNode();
                lcsn.IsDone = false;
                return lcsn;
            }
            return null;
        }

    }
}
#endif