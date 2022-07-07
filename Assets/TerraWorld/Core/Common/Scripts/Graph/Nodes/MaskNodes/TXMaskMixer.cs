#if TERRAWORLD_XPRO
using System;
using System.Collections.Generic;
using System.Numerics;
using TerraUnity.Edittime;

namespace TerraUnity.Graph
{
    [CreateNodeMenu("Masks/Mask Mixer")]
    public class TXMaskMixer : TXMaskModules
    {

        public enum BlendingMode
        {
            OR,
            AND,
            NOT,
            SUB,
            XOR,
            Exaggerate
        }
        public BlendingMode blendingMode = BlendingMode.OR;


        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXMaskModules Input1;

        [Input(ShowBackingValue.Never, ConnectionType.Override, TypeConstraint.Strict)]
        public TXMaskModules Input2;

        protected override void Init()
        {
            base.Init();
            SetName("Mask Mixer");
        }

        public override void CheckEssentioalInputs()
        {
            Input1 = GetInputValue<TXMaskModules>("Input1");
            if (Input1 == null) throw new Exception("Input1" + " is missed for " + name + " Node.");
            Input2 = GetInputValue<TXMaskModules>("Input2");
            if (Input2 == null) throw new Exception("Input2" + " is missed for " + name + " Node.");
        }

        protected override void ModuleAction(TMap CurrentMap)
        {
            _progress = 0;

            TMask resultMask = null;
            OutMasks = new List<TMask>();

            if (IsActive)
            {
                List<TMask> preNode1Masks = Input1.GetMasks(CurrentMap._refTerrain);
                List<TMask> preNode2Masks = Input2.GetMasks(CurrentMap._refTerrain);

                switch (blendingMode)
                {
                    case BlendingMode.OR:
                        {
                            List<TMask> inputMasks = new List<TMask>();
                            for (int i = 0; i < preNode1Masks.Count; i++)
                                inputMasks.Add(preNode1Masks[i]);
                            for (int i = 0; i < preNode2Masks.Count; i++)
                                inputMasks.Add(preNode2Masks[i]);
                            resultMask = TMask.MergeMasks(inputMasks);
                        }
                        break;
                    case BlendingMode.AND:
                        {
                            List<TMask> inputMasks = new List<TMask>();
                            for (int i = 0; i < preNode1Masks.Count; i++)
                                inputMasks.Add(preNode1Masks[i]);
                            for (int i = 0; i < preNode2Masks.Count; i++)
                                inputMasks.Add(preNode2Masks[i]);
                            resultMask = TMask.AND(inputMasks);
                        }
                        break;
                    case BlendingMode.NOT:
                        {
                            List<TMask> inputMasks = new List<TMask>();
                            for (int i = 0; i < preNode1Masks.Count; i++)
                                inputMasks.Add(preNode1Masks[i]);
                            //for (int i = 0; i < preNode2.OutMasks.Count; i++)
                            //    inputMasks.Add(preNode2.OutMasks[i]);
                            resultMask = TMask.Inverse(inputMasks);
                        }
                        break;
                    case BlendingMode.XOR:
                        {
                            List<TMask> inputMasks = new List<TMask>();
                            for (int i = 0; i < preNode1Masks.Count; i++)
                                inputMasks.Add(preNode1Masks[i]);
                            for (int i = 0; i < preNode2Masks.Count; i++)
                                inputMasks.Add(preNode2Masks[i]);
                            resultMask = TMask.XOR(inputMasks);
                        }
                        break;
                    case BlendingMode.SUB:
                        {
                            List<TMask> inputMasks = new List<TMask>();
                            inputMasks.Add(TMask.MergeMasks(preNode1Masks));
                            inputMasks.Add(TMask.MergeMasks(preNode2Masks));
                            resultMask = TMask.Subtract(inputMasks);
                        }
                        break;
                    case BlendingMode.Exaggerate:
                        {
                            resultMask = TMask.Exaggerate(preNode1Masks[0], 1); ;
                        }
                        break;
                    default:
                        break;
                }

                if (resultMask == null) resultMask = new TMask(8, 8);

                OutMasks.Add(resultMask);
                _progress = 1;
            }

        }

    }
}
#endif