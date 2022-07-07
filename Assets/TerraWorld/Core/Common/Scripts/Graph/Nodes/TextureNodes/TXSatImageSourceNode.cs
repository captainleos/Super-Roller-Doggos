#if TERRAWORLD_XPRO
using System;
using System.Drawing;
using System.Numerics;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
   
    public class TXSatImageSourceNode : TXImageModules
    {
        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXAreaModules Area;

        public TMapManager.mapImagerySourceEnum _source = TMapManager.mapImagerySourceEnum.ESRI;
        public int resolution = 1024;


        protected override void Init()
        {
            base.Init();
            SetName("Satellite Image Extractor");
        }

        protected override void ModuleAction(TMap CurrentMap)
        {
            Bitmap newImage = TImageProcessors.ResetResolution(CurrentMap.Image.Image, resolution);
            _outputImage = new TImage(newImage);
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
