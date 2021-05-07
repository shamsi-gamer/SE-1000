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
                if (btn != null) funcButtons.Add(btn);
            }
        }


        void SetFunc(int func)
        {
            if (CurClip.SelChan > -1)
            {
                if (CurClip.CurSet > -1)
                {
                    CurSetting.Func(func);
                }
                else 
                {
                    if (CurClip.CurSrc < 0) SetInstFunc(CurClip.SelectedInstrument, func);
                    else                   SetSrcFunc (CurClip.SelectedSource,     func);
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

            CurClip.CurSrc = iSrc;

            CurClip.CurChan =
            CurClip.SelChan = Array.FindIndex(
                CurClip.CurrentPattern.Channels, 
                ch => ch.Instrument == inst);

            UpdateInstOff(CurClip.SelChan);

            UpdateInstName(true);
            g_inputValid = false;


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
            if (inst == null) inst = CurClip.SelectedInstrument;
            if (iSrc == -2)   iSrc = CurClip.CurSrc;

            if (CurClip.CurSet > -1)
                CurSetting._IsCurrent = false;

            Setting setting;

                 if (CurClip.CurSet > -1) setting = CurSetting        .GetOrAddSettingFromTag(tag);
            else if (iSrc          > -1) setting = inst.Sources[iSrc].GetOrAddSettingFromTag(tag);
            else                         setting = inst.GetOrAddSettingFromTag(tag);

            g_settings.Add(setting);

            CurClip.CurSet++;

            if (IsCurParam())
                CurSetting._IsCurrent = true;
        }


        static void RemoveSetting(Setting setting)
        {
            int set = CurClip.CurSet;

            if (   HasTag(setting, strAtt)
                || HasTag(setting, strDec)
                || HasTag(setting, strSus)
                || HasTag(setting, strRel))
                set--;

            if (CurClip.CurSet > 0)
                g_settings[CurClip.CurSet -1].Remove(setting);
            else 
            {
                var inst = CurClip.SelectedInstrument;
                var src  = CurClip.SelectedSource;

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

            CurClip.CurSet -= CurClip.CurSet - set + 1;
        }


        void SetInstFunc(Instrument inst, int func)
        {
            switch (func)
            {
            case 1: AddNextSetting(strVol, inst, -1); break;
            case 2: 
                AddNextSetting(strTune, inst, -1);
                //UpdateKeyLabels();
                //UpdateChordLabels();
                //UpdateShuffleLabel();
                break;

            case 3: AddNextSetting(strFlt, inst, -1); break;
            case 4: AddNextSetting(strDel, inst, -1); break;
            case 5: AddNextSetting(strArp, inst, -1); break;
            }
        }


        void SetSrcFunc(Source src, int func)
        {
            switch (func)
            {
            case 0: AddNextSetting(strOff, src.Instrument, src.Index); break; 
            case 1: AddNextSetting(strVol, src.Instrument, src.Index); break;
            case 2: 
                AddNextSetting(strTune, src.Instrument, src.Index);
                //UpdateKeyLabels();
                //UpdateChordLabels();
                //UpdateShuffleLabel();
                break;

            case 3: AddNextSetting(strHrm, src.Instrument, src.Index);  break;
            case 4: AddNextSetting(strFlt, src.Instrument, src.Index); break;
            case 5: AddNextSetting(strDel, src.Instrument, src.Index); break;
            }
        }
    }
}
