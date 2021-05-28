using System;
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
            if (SelChan > -1)
            {
                if (CurSet > -1)
                {
                    CurSetting.Func(func);
                }
                else 
                {
                    if (CurSrc < 0) SetInstFunc(SelInstrument, func);
                    else                   SetSrcFunc (SelSource,     func);
                }
            }
            else
            {
                switch (func)
                {
                case 2: ToggleNote(EditedClip); break;
                case 3: CutNotes(EditedClip);   break;
                }
            }

            g_lcdPressed.Add(lcdMain+func);
        }


        void SwitchToSetting(Instrument inst, int iSrc, Setting set)
        {
            SwitchToSetting(inst, iSrc, set.GetPath(iSrc));
        }


        void SwitchToSetting(Instrument inst, int iSrc, string path)
        {
            BackOut();

            CurSrc = iSrc;

            CurChan =
            SelChan = Array.FindIndex(
                CurPattern.Channels, 
                ch => ch.Instrument == inst);

            UpdateInstOff(SelChan);

            UpdateInstName(True);
            g_inputValid = False;


            var tags = path.Split('/');

            for (int i = 0; i < tags.Length; i++)
            { 
                var tag = tags[i];

                if (   i == 0
                    && IsDigit(tag[0]))
                { 
                    iSrc = int_Parse(tag);
                    continue;
                }

                AddNextSetting(tag, inst, iSrc);
            }
        }


        static void AddNextSetting(string tag, Instrument inst = Instrument_null, int iSrc = -2)
        {
            if (!OK(inst))   inst = SelInstrument;
            if (iSrc == -2) iSrc = CurSrc;

            if (CurSet > -1)
                CurSetting._IsCurrent = False;

            Setting setting;

                 if (CurSet > -1) setting = CurSetting        .GetOrAddSettingFromTag(tag);
            else if (iSrc   > -1) setting = inst.Sources[iSrc].GetOrAddSettingFromTag(tag);
            else                  setting = inst              .GetOrAddSettingFromTag(tag);

            g_settings.Add(setting);

            CurSet++;

            if (IsCurParam())
                CurSetting._IsCurrent = True;
        }


        static void DeleteCurSetting()
        {
            var set     = CurSet;
            var setting = CurSetting;

            if (   HasTag(setting, strAtt)
                || HasTag(setting, strDec)
                || HasTag(setting, strSus)
                || HasTag(setting, strRel))
                set--;

            if (CurSet > 0)
            { 
                g_settings[CurSet-1].DeleteSetting(setting);
            }
            else 
            {
                var inst = SelInstrument;
                var src  = SelSource;

                switch (setting.Tag)
                {
                    case strOff:  if ( OK(src)) src.Offset    = Parameter_null;                                 break;
                    case strDel:  if ( OK(src)) src.Delay     =     Delay_null; else inst.Delay  =  Delay_null; break;
                    case strTune: if ( OK(src)) src.Tune      =      Tune_null; else inst.Tune   =   Tune_null; break;
                    case strHrm:  if ( OK(src)) src.Harmonics = Harmonics_null;                                 break;
                    case strFlt:  if ( OK(src)) src.Filter    =    Filter_null; else inst.Filter = Filter_null; break;
                    case strArp:  if (!OK(src)) inst.Arpeggio =  Arpeggio_null;                                 break;
                }
            }

            g_settings.RemoveAt(set);

            CurSet -= CurSet - set + 1;
        }


        void SetInstFunc(Instrument inst, int func)
        {
            switch (func)
            {
            case 1: AddNextSetting(strVol,  inst, -1); break;
            case 2: AddNextSetting(strTune, inst, -1); break;
            case 3: AddNextSetting(strFlt,  inst, -1); break;
            case 4: AddNextSetting(strDel,  inst, -1); break;
            case 5: AddNextSetting(strArp,  inst, -1); break;
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
