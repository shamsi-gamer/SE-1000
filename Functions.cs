﻿using System;
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
                if (btn != null) funcButtons.Add(btn);
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
                case 2: ToggleNote(CurClip); break;
                case 3: CutNotes(CurClip);   break;
                }
            }

            g_mainPressed.Add(func);

            //UpdateEnterLabel();
            //UpdateAdjustLabels(CurClip);
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

            UpdateInstName(T);
            g_inputValid = F;


            var tags = path.Split('/');

            for (int i = 0; i < tags.Length; i++)
            { 
                var tag = tags[i];

                if (   i == 0
                    && IsDigit(tag[0]))
                { 
                    iSrc = int.Parse(tag);
                    continue;
                }

                AddNextSetting(tag, inst, iSrc);
            }
        }


        static void AddNextSetting(string tag, Instrument inst = null, int iSrc = -2)
        {
            if (inst == null) inst = SelInstrument;
            if (iSrc == -2)   iSrc = CurSrc;

            if (CurSet > -1)
                CurSetting._IsCurrent = F;

            Setting setting;

                 if (CurSet > -1) setting = CurSetting        .GetOrAddSettingFromTag(tag);
            else if (iSrc   > -1) setting = inst.Sources[iSrc].GetOrAddSettingFromTag(tag);
            else                  setting = inst              .GetOrAddSettingFromTag(tag);

            g_settings.Add(setting);

            CurSet++;

            if (IsCurParam())
                CurSetting._IsCurrent = T;
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

            Log($"CurSet = {CurSet}");
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
                    case strOff:  if (src != null) src.Offset    = null;                            break;
                    case strDel:  if (src != null) src.Delay     = null; else inst.Delay    = null; break;
                    case strTune: if (src != null) src.Tune      = null; else inst.Tune     = null; break;
                    case strHrm:  if (src != null) src.Harmonics = null;                            break;
                    case strFlt:  if (src != null) src.Filter    = null; else inst.Filter   = null; break;
                    case strArp:  if (src == null)                            inst.Arpeggio = null; break;
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
