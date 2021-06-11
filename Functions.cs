using System.Collections.Generic;
using SpaceEngineers.Game.ModAPI.Ingame;


namespace IngameScript
{
    partial class Program
    {
         List<IMyButtonPanel> funcButtons = new List<IMyButtonPanel>();


        void InitFuncButtons()
        {
            for (int i = 0; i < 6; i++)
            {
                var btn = Get("Button F" + (i+1)) as IMyButtonPanel;
                if (OK(btn)) funcButtons.Add(btn);
            }
        }


        void SetFunc(int func)
        {
            if (OK(SelChan))
            {
                if (OK(CurSet))
                    EditedClip.CurSetting.Func(func);

                else
                {
                    if (!OK(CurSrc)) SetInstFunc(SelInstrument, func);
                    else             SetSrcFunc (SelSource,     func);
                }
            }

            g_lcdPressed.Add(lcdMain+func);
        }


        void SetInstFunc(Instrument inst, int func)
        {
            switch (func)
            {
            case 1: AddNextSetting(strVol,  inst, -1); break;
            case 2: AddNextSetting(strTune, inst, -1); break;
            case 3: AddNextSetting(strFlt,  inst, -1); break;
            case 4: AddNextSetting(strDel,  inst, -1); break;
            }
        }


        void SetSrcFunc(Source src, int func)
        {
            switch (func)
            {
            case 0: AddNextSetting(strOff,  src.Instrument, src.Index); break; 
            case 1: AddNextSetting(strVol,  src.Instrument, src.Index); break;
            case 2: AddNextSetting(strTune, src.Instrument, src.Index); break;
            case 3: AddNextSetting(strHrm,  src.Instrument, src.Index); break;
            case 4: AddNextSetting(strFlt,  src.Instrument, src.Index); break;
            case 5: AddNextSetting(strDel,  src.Instrument, src.Index); break;
            }
        }
    }
}
