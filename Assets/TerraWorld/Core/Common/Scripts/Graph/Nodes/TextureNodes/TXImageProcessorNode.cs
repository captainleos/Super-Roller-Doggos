#if TERRAWORLD_XPRO
using System;
using System.Drawing;
using System.Numerics;
using TerraUnity.Edittime;
using TerraUnity.Utils;
using UnityEngine;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Textures/Image Processor")]

    public class TXImageProcessorNode : TXImageModules
    {

        public enum TextureProcessAction
        {
            FillColor,
            Simplified
        }

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXMaskModules Mask;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXImageModules InputImage;

        public TextureProcessAction ProcessMethod = TextureProcessAction.FillColor;
        public UnityEngine.Color _selectedColor = new UnityEngine.Color();
        public int mostUsedColor = 16;



        protected override void Init()
        {
            base.Init();
            SetName("Image Processor");
        }

        protected override void ModuleAction(TMap CurrentMap)
        {
            switch (ProcessMethod)
            {
                case TextureProcessAction.FillColor:
                    {
                        if (Mask == null)
                        {
                            _outputImage = InputImage.GetImage(CurrentMap._refTerrain);
                        }
                        else
                        {
                            _outputImage = InputImage.GetImage(CurrentMap._refTerrain);
                            TMask mask = TMask.MergeMasks(Mask.GetMasks(CurrentMap._refTerrain));
                            _outputImage.FillImage(TUtils.CastToDrawingColor(_selectedColor), mask);
                        }
                    }
                    break;
                default:
                    break;
            }

        }

        public override void CheckEssentioalInputs()
        {
            InputImage = GetInputValue<TXImageModules>("InputImage");
            if (InputImage == null) throw new Exception("input" + " is missed for " + name + " Node.");
            Mask = GetInputValue<TXMaskModules>("Mask");
        }

    }
}
#endif