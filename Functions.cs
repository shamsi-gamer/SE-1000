using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;


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
                    var setting = g_settings[CurSet];

                         if (setting.GetType() == typeof(Envelope )) SetEnvelopeFunc ((Envelope) setting, func);
                    else if (setting.GetType() == typeof(LFO      )) SetLfoFunc      ((LFO)      setting, func);
                    else if (setting.GetType() == typeof(Harmonics)) SetHarmonicsFunc((Harmonics)setting, func);
                    else if (setting.GetType() == typeof(Filter   )) SetFilterFunc   ((Filter)   setting, func);
                    else if (setting.GetType() == typeof(Arpeggio )) SetArpeggioFunc ((Arpeggio) setting, func);
                    else if (setting.GetType() == typeof(Delay    )) SetDelayFunc    ((Delay)    setting, func);
                    else if (IsParam(setting))                       SetParamFunc    ((Parameter)setting, func);
                }
                else 
                {
                    if (CurSrc < 0) SetInstFunc(SelectedInstrument, func);
                    else            SetSrcFunc (SelectedSource,     func);
                }

                //g_sampleValid = F;
            }
            else
            {
                switch (func)
                {
                case 2:
                    ToggleNote(g_song);
                    break;

                case 3:
                    CutNotes(g_song);
                    break;
                }
            }


            mainPressed.Add(func);

            UpdateEnterLight();
            UpdateAdjustLights(g_song);
        }


        void SwitchToSetting(string path)
        {
            g_settings.Clear();
            CurSet = -1;

            var tags = path.Split('/');

            foreach (var tag in tags)
                AddNextSetting(tag);

            CurSet = tags.Length-1;
        }


        void AddNextSetting(string tag)
        {
            if (CurSet > -1)
                g_settings[CurSet]._IsCurrent = false;

            var setting = NewSettingFromTag(tag, CurSet > -1 ? g_settings[CurSet] : null);

            g_settings.Add(setting);

            CurSet++;// = g_settings.Count-1;
            Log("g_settings[CurSet] = " + g_settings[CurSet]);
            if (IsParam(g_settings[CurSet]))
                g_settings[CurSet]._IsCurrent = true;
        }


        void RemoveSetting(Setting setting)
        {
            int set = CurSet;

            if (   HasTag(setting, "Att")
                || HasTag(setting, "Dec")
                || HasTag(setting, "Sus")
                || HasTag(setting, "Rel"))
                set--;

            if (CurSet > 0)
                g_settings[CurSet-1].Remove(setting);
            else 
            {
                var inst = SelectedInstrument;
                var src  = CurSrc > -1 ? inst.Sources[CurSrc] : null;

                switch (setting.Tag)
                {
                    case "Off":  if (src != null) src.Offset    = null;                            break;
                    case "Del":  if (src != null) src.Delay     = null; else inst.Delay    = null; break;
                    case "Tune": if (src != null) src.Tune      = null; else inst.Tune     = null; break;
                    case "Hrm":  if (src != null) src.Harmonics = null;                            break;
                    case "Flt":  if (src != null) src.Filter    = null; else inst.Filter   = null; break;
                    case "Arp":  if (src == null)                            inst.Arpeggio = null; break;
                }
            }

            g_settings.RemoveAt(set);

            CurSet -= CurSet - set + 1;
        }


        void SetParamFunc(Parameter param, int func)
        {
            switch (func)
            {
            case 1:
                if (   SettingOrAnyParentHasTag(param, "Att")
                    || SettingOrAnyParentHasTag(param, "Dec")
                    || SettingOrAnyParentHasTag(param, "Sus")
                    || SettingOrAnyParentHasTag(param, "Rel"))
                    break;

                AddNextSetting("Env");
                break;

            case 2:
                AddNextSetting("LFO");
                break;

            case 3:
                g_paramKeys = true;
                UpdateChordLights();
                break;

            case 4:
                g_paramAuto = true;
                UpdateChordLights();
                break;

            case 5: 
                if (   param.Tag == "Att"
                    || param.Tag == "Dec"
                    || param.Tag == "Sus"
                    || param.Tag == "Rel"

                    || param.Tag == "Amp"
                    || param.Tag == "Freq"
                    ||    param.Parent != null
                       && param.Tag == "Off"
                       
                    || param.Tag.Substring(0, 3) == "Hrm"

                    || param.Tag == "Cut"
                    || param.Tag == "Res"

                    || param.Tag == "Len"
                    || param.Tag == "Scl")
                    break;

                RemoveSetting(param); 
                break;
            }
        }


        void SetEnvelopeFunc(Envelope env, int func)
        {
            switch (func)
            {
                case 1: AddNextSetting("Att"); break;
                case 2: AddNextSetting("Dec"); break;
                case 3: AddNextSetting("Sus"); break;
                case 4: AddNextSetting("Rel"); break;
                case 5: RemoveSetting(env);    break;
            }
        }


        void SetLfoFunc(LFO lfo, int func)
        {
            switch (func)
            {
                case 1: AddNextSetting("Amp");  break;
                case 2: AddNextSetting("Freq"); break;
                case 3: AddNextSetting("Off");  break;
                case 4:
                {
                    var newOsc = (int)lfo.Type + 1;
                    if (newOsc > (int)LFO.LfoType.Noise) newOsc = 0;
                    lfo.Type = (LFO.LfoType)newOsc;
                    mainPressed.Add(func);
                    break;
                }
                case 5: RemoveSetting(lfo); break;
            }
        }


        void SetHarmonicsFunc(Harmonics hrm, int func)
        {
            switch (func)
            {
                case 1:
                {
                    hrm.Smooth();
                    break;
                }
                case 2:
                { 
                    var cp = (int)hrm.CurPreset;

                    cp++;

                    if (cp > (int)Harmonics.Preset.Random24)
                        cp = (int)Harmonics.Preset.Sine;

                    hrm.CurPreset = (Harmonics.Preset)cp;

                    break;
                }
                case 3:
                {
                    hrm.SetPreset(hrm.CurPreset);
                    break;
                }
                case 4: AddNextSetting(S(hrm.CurTone)); break;
                case 5: RemoveSetting(hrm); break;
            }
        }


        void SetFilterFunc(Filter flt, int func)
        {
            switch (func)
            {
                case 1: AddNextSetting("Cut"); break;
                case 2: AddNextSetting("Res"); break;
                case 5: RemoveSetting(flt);    break;
            }
        }


        void SetDelayFunc(Delay del, int func)
        {
            switch (func)
            {
                case 1: AddNextSetting("Cnt");  break;
                case 2: AddNextSetting("Time"); break;
                case 3: AddNextSetting("Lvl");  break;
                case 4: AddNextSetting("Pow");  break;
                case 5: RemoveSetting(del);     break;
            }
        }


        void SetArpeggioFunc(Arpeggio arp, int func)
        {
            switch (func)
            { 
            case 1:
                arp.Song.EditPos = -1;
                UpdateEditLight(lblEdit, false);

                AddNextSetting("Len");
                break;

            case 2:
                arp.Song.EditPos = -1;
                UpdateEditLight(lblEdit, false);

                AddNextSetting("Scl");
                break;

            case 5: RemoveSetting(arp); break;
            }
        }


        void SetInstFunc(Instrument inst, int func)
        {
            switch (func)
            {
            case 1: AddNextSetting("Vol"); break;
            case 2: 
                AddNextSetting("Tune");
                UpdateKeyLights();
                UpdateChordLights();
                UpdateShuffleLight();
                break;

            case 3: AddNextSetting("Flt"); break;
            case 4: AddNextSetting("Del"); break;
            case 5: AddNextSetting("Arp"); break;
            }
        }


        void SetSrcFunc(Source src, int func)
        {
            switch (func)
            {
            case 0: AddNextSetting("Off"); break; 
            case 1: AddNextSetting("Vol"); break;
            case 2: 
                AddNextSetting("Tune");
                UpdateKeyLights();
                UpdateChordLights();
                UpdateShuffleLight();
                break;

            case 3: AddNextSetting("Hrm"); break;
            case 4: AddNextSetting("Flt"); break;
            case 5: AddNextSetting("Del"); break;
            }
        }
    }
}
