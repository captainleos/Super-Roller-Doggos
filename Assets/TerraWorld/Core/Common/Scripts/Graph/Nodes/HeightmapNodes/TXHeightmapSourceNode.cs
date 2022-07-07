#if TERRAWORLD_XPRO
using System;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    //[CreateNodeMenu("Sources/Elevation Data Extractor")]
    //[NodeWidth(300)]

    public class TXHeightmapSourceNode : TXHeightmapModules
    {
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXAreaModules Area;

        public TMapManager.mapElevationSourceEnum source = TMapManager.mapElevationSourceEnum.ESRI;
        private int _resolution = 1024;
        public int Resolution { get => _resolution; set => _resolution = value; }

        public bool highestResolution = true;

        public float elevationExaggeration = 1;

        protected override void Init()
        {
            base.Init();
            SetName("Elevation Data Extractor");
        }

        protected override void ModuleAction(TMap currentMap)
        {
            _progress = 0;

            if (!IsActive)
                _heightmapData = new float[_resolution, _resolution];
            else
            {
                _heightmapData = currentMap.Heightmap.heightsData.Clone() as float[,];

                if (elevationExaggeration != 1)
                    _heightmapData = THeightmapProcessors.ExaggerateHeightmap(_heightmapData, elevationExaggeration);

                if (!highestResolution)
                {
                    _heightmapData = THeightmapProcessors.ResampleHeightmap(_heightmapData, THeightmapProcessors.ResampleMode.DOWN, _resolution + 1);

                    int smoothIterration = (int)((_resolution + 1) * 1.0f / _heightmapData.GetLength(0));
                    _heightmapData = THeightmapProcessors.SmoothHeightmap(_heightmapData, smoothIterration);
                }

                _progress = 1;
            }

        }

        public override void CheckEssentioalInputs()
        {
        }

        // public float[,] GetFinalHeightmap()
        // {
        //     var iv = GetInputValue<float[,]>("HeightmapRawData");
        //     return null;
        // }

        //  public override object GetValue(XNode.NodePort port)
        //
        //  {
        //      if (port.fieldName == "Heightmap")
        //      {
        //          _heightmapSource.ModuleAction()
        //      }
        //      else return null;
        //  }

    }
}
#endif
