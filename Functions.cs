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
                if (curSet > -1)
                {
                    var setting = g_settings[curSet];

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
                    else            SetSrcFunc(SelectedSource,      func);
                }

                g_sampleValid = false;
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


        void AddNextSetting(Setting setting)
        {
            if (curSet > -1)
                g_settings[curSet]._IsCurrent = false;

            g_settings.Add(setting);

            if (IsParam(setting))                  
                setting._IsCurrent = true;

            curSet++;
        }


        void RemoveSetting(Setting setting)
        {
            int set = curSet;

            if (   HasTag(setting, "Att")
                || HasTag(setting, "Dec")
                || HasTag(setting, "Sus")
                || HasTag(setting, "Rel"))
                set--;

            if (curSet > 0)
                g_settings[curSet-1].Remove(setting);
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

            curSet -= curSet - set + 1;
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

                if (param.Envelope == null)
                { 
                    param.Envelope = new Envelope();
                    param.Envelope.Parent = param;
                }

                AddNextSetting(param.Envelope);
                break;

            case 2:
                if (param.Lfo == null)
                { 
                    param.Lfo = new LFO();
                    param.Lfo.Parent = param;
                }

                AddNextSetting(param.Lfo);
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
                case 1: AddNextSetting(env.Attack);  break;
                case 2: AddNextSetting(env.Decay);   break;
                case 3: AddNextSetting(env.Sustain); break;
                case 4: AddNextSetting(env.Release); break;
                case 5: RemoveSetting(env);          break;
            }
        }


        void SetLfoFunc(LFO lfo, int func)
        {
            switch (func)
            {
                case 1: AddNextSetting(lfo.Amplitude); break;
                case 2: AddNextSetting(lfo.Frequency); break;
                case 3: AddNextSetting(lfo.Offset);    break;
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
                case 4: AddNextSetting(hrm.Tones[hrm.CurTone]); break;
                case 5: RemoveSetting(hrm); break;
            }
        }


        void SetFilterFunc(Filter flt, int func)
        {
            switch (func)
            {
                case 1: AddNextSetting(flt.Cutoff);    break;
                case 2: AddNextSetting(flt.Resonance); break;
                case 5: RemoveSetting(flt);            break;
            }
        }


        void SetDelayFunc(Delay del, int func)
        {
            switch (func)
            {
                case 1: AddNextSetting(del.Count); break;
                case 2: AddNextSetting(del.Time);  break;
                case 3: AddNextSetting(del.Level); break;
                case 4: AddNextSetting(del.Power); break;
                case 5: RemoveSetting(del);        break;
            }
        }


        void SetArpeggioFunc(Arpeggio arp, int func)
        {
            switch (func)
            { 
            case 1:
                arp.Song.EditPos = -1;
                UpdateEditLight(lblEdit, false);

                AddNextSetting(arp.Length);
                break;

            case 2:
                arp.Song.EditPos = -1;
                UpdateEditLight(lblEdit, false);

                AddNextSetting(arp.Scale);
                break;

            case 5: RemoveSetting(arp); break;
            }
        }


        void SetInstFunc(Instrument inst, int func)
        {
            switch (func)
            {
            case 1:
                AddNextSetting(inst.Volume); 
                break;

            case 2:
                if (inst.Tune == null)
                    inst.Tune = new Tune();

                AddNextSetting(inst.Tune);

                UpdateKeyLights();
                UpdateChordLights();
                UpdateShuffleLight();
                break;

            case 3:
                if (inst.Filter == null)
                    inst.Filter = new Filter();

                AddNextSetting(inst.Filter);
                break;

            case 4:
                if (inst.Delay == null)
                    inst.Delay = new Delay();

                AddNextSetting(inst.Delay);
                break;

            case 5:
                if (inst.Arpeggio == null)
                    inst.Arpeggio = new Arpeggio(inst);

                AddNextSetting(inst.Arpeggio);
                break;
            }
        }


        void SetSrcFunc(Source src, int func)
        {
            switch (func)
            {
            case 0:
                if (src.Offset == null)
                    src.Offset = new Parameter("Offset", "Off", -1, 1, 0, 0.3f, 0.01f, 0.1f, 0);

                AddNextSetting(src.Offset);
                break;

            case 1:
                AddNextSetting(src.Volume); 
                break;

            case 2:
                if (src.Tune == null)
                    src.Tune = new Tune();

                AddNextSetting(src.Tune);

                UpdateKeyLights();
                UpdateChordLights();
                UpdateShuffleLight();
                break;

            case 3:
                if (src.Harmonics == null)
                    src.Harmonics = new Harmonics();

                AddNextSetting(src.Harmonics);
                break;

            case 4:
                if (src.Filter == null)
                    src.Filter = new Filter();

                AddNextSetting(src.Filter);
                break;

            case 5:
                if (src.Delay == null)
                    src.Delay = new Delay();

                AddNextSetting(src.Delay);
                break;
            }
        }
    }
}
