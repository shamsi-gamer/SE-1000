using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;


namespace IngameScript
{
    partial class Program
    {
        void New()
        {
            if (CurSrc > -1)
            {
                curSet = -1;
                g_settings.Clear();


                var inst = CurrentInstrument;

                if (inst.Sources.Count < maxDspSrc)
                { 
                    //for (int i = g_song.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    inst.Sources.Insert(CurSrc+1, new Source(inst));

                    CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.CurSrc);
                UpdateSrcOff();

                g_sampleValid = false;

                MarkLight(lblNew, false);
            }
            else if (SelChan > -1)
            {
                curSet = -1;
                g_settings.Clear();


                var inst = new Instrument();
                inst.Sources.Add(new Source(inst));

                inst.Name = GetNewName(inst.Name, str => g_inst.Exists(_s => _s.Name == str));

                g_inst.Insert(g_inst.IndexOf(CurrentInstrument) + 1, inst);
                SetCurInst(inst);

                UpdateOctaveLight();
                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_inst.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurChan);

                UpdateInstName();
                inputValid = false;

                g_sampleValid = false;

                MarkLight(lblNew);
            }
        }

        void Duplicate()
        {
            if (CurSrc > -1)
            {
                curSet = -1;
                g_settings.Clear();


                var inst = CurrentInstrument;

                if (inst.Sources.Count < 8)
                {
                    //for (int i = g_song.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    var src = new Source(inst.Sources[CurSrc], inst);
                    inst.Sources.Insert(CurSrc+1, src);

                    CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.CurSrc);
                UpdateSrcOff();

                g_sampleValid = false; 
                
                MarkLight(lblDuplicate, false);
            }
            else if (SelChan > -1)
            {
                curSet = -1;
                g_settings.Clear();


                var inst = new Instrument(CurrentInstrument);
                inst.Name = GetNewName(inst.Name, newName => g_inst.Exists(s => s.Name == newName));

                g_inst.Insert(g_inst.IndexOf(CurrentInstrument) + 1, inst);
                SetCurInst(inst);
                g_srcOff = 0;

                UpdateOctaveLight();
                UpdateInstName();

                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_inst.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurChan);

                g_sampleValid = false; 
                
                MarkLight(lblDuplicate);
            }
        }


        void Delete()
        {
            if (CurSrc > -1)
            {
                curSet = -1;
                g_settings.Clear();


                var inst = CurrentInstrument;

                inst.Sources.RemoveAt(CurSrc);

                //for (int i = g_song.CurSrc; i < inst.Sources.Count; i++)
                //    inst.Sources[i].Index--;

                if (inst.Sources.Count == 0)
                {
                    inst.Sources.Add(new Source(inst));
                    CurSrc = 0;
                }

                if (CurSrc >= inst.Sources.Count)
                    CurSrc = inst.Sources.Count - 1;

                //UpdateDspOffset(ref srcOff, g_song.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.CurSrc);
                UpdateSrcOff();

                g_sampleValid = false; 
                
                MarkLight(lblDelete, false);
            }
            else if (SelChan > -1)
            {
                curSet = -1;
                g_settings.Clear();


                var i = g_inst.IndexOf(CurrentInstrument);
                var inst = CurrentInstrument;

                g_inst.Remove(CurrentInstrument);

                if (g_inst.Count == 0)
                {
                    g_inst.Add(new Instrument());
                    g_inst[0].Sources.Add(new Source(g_inst[0]));
                }

                i = MinMax(0, i - 1, g_inst.Count - 1);

                foreach (var p in g_song.Patterns)
                    foreach (var c in p.Channels)
                        if (c.Instrument == inst) c.Instrument = g_inst[i];

                g_srcOff = 0;

                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_inst.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurChan);

                UpdateOctaveLight();
                UpdateInstName();

                g_sampleValid = false; 
                
                MarkLight(lblDelete);
            }
        }


        void Move(int move)
        {
            if (curSet > -1)
                return;


            //var bl =
            //       g_set >= Set.LfoAmplitude
            //    && g_set <= Set.LfoFixed;


            if (SelChan < 0)
            {
                SetChan(-move);
            }
            else if (CurSrc < 0) // inst
            {
                var i = g_inst.IndexOf(CurrentInstrument);
                var n = i + move;

                if (n >= g_inst.Count) n = 0;
                if (n < 0) n = g_inst.Count - 1;

                if (g_move)
                {
                    var inst = g_inst[i];

                    g_inst.RemoveAt(i);
                    g_inst.Insert(n, inst);
                }
                else
                {
                    int first, last;
                    GetPatterns(g_song, CurPat, out first, out last);

                    for (int p = first; p <= last; p++)
                        g_song.Patterns[p].Channels[CurChan].Instrument = g_inst[n];

                    UpdateOctaveLight();
                }


                UpdateInstName();
                inputValid = false;

                g_sampleValid = false;

                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_inst.Count, maxDspInst, 0, 1);
                UpdateInstOff(CurChan);
                
                g_srcOff = 0;

                //UpdateLight(lblOn,       false);
                //UpdateLight(lblLfoFixed, false, bl);
            }
            else // src
            {
                var inst = CurrentInstrument;
                var next = CurSrc + move;

                if (next >= inst.Sources.Count) next = 0;
                if (next < 0) next = inst.Sources.Count - 1;

                if (g_move)
                {
                    var src = inst.Sources[CurSrc];
                    inst.Sources.RemoveAt(CurSrc);
                    inst.Sources.Insert(next, src);
                }

                CurSrc = next;

                dspMain.Surface.WriteText("", false);

                //UpdateDspOffset(ref srcOff, g_song.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.CurSrc);
                UpdateSrcOff();

                g_sampleValid = false; 
                
                //UpdateLight(lblOn,       CurrentSource.On);
                //UpdateLight(lblLfoFixed, CurrentSource.Lfo.Fixed, bl);
            }


            MarkLight(
                move >= 0 ? lblNext : lblPrev, 
                !g_move && CurSrc < 0);
        }


        void BackOut()
        {
            g_move = false;

            g_paramKeys = false;
            g_paramAuto = false;

            if (curSet > -1)
            {
                bool ucl = false;

                if (IsCurParam())
                { 
                    g_settings[curSet]._IsCurrent = false;
                    if (IsCurParam("Tune")) ucl = true;
                }

                curSet = -1;
                g_settings.Clear();
                    
                if (ucl)
                { 
                    UpdateKeyLights();
                    UpdateChordLights();
                    UpdateShuffleLight();
                }

                UpdateEnterLight();
                MarkLight(lblBackOut, CurSrc < 0);
            }

            if (CurSrc > -1)
            {
                CurSrc = -1;
                g_srcOff =  0;

                g_shift = false;

                UpdateNewLights();
                UpdateAdjustLights(g_song);

                UpdateInstName(true);
                inputValid = false;

                MarkLight(lblBackOut, CurSrc < 0);
            }

            if (SelChan > -1)
            {
                SelChan = -1;

                g_shift = false;

                g_move = false;
                //UpdateLight(lblMove, g_move ^ (g_song.CurSrc > -1), g_song.SelChan > -1 && !g_move);

                UpdateNewLights();
                UpdateAdjustLights(g_song);

                UpdateInstName(false);
                MarkLight(lblBackOut, CurSrc < 0);
            }
        }


        void Back()
        {
            g_move = false;

            if (curSet > -1)
            {
                if (g_paramKeys)
                {
                    g_paramKeys    = false;
                    g_song.EditPos = float.NaN;
                    UpdateEditLight(lblEdit, false);
                }
                else if (g_paramAuto)
                {
                    g_paramAuto    = false;
                    g_song.EditPos = float.NaN;
                    UpdateEditLight(lblEdit, false);
                }
                else
                {
                    bool ucl = false;

                    if (IsCurSetting(typeof(Arpeggio)))
                    {
                        //Stop();

                        CurArpeggio.Song.EditPos = -Math.Abs(CurArpeggio.Song.EditPos); // turn off but keep value
                        UpdateEditLight(lblEdit, false);
                    }
                    else if (IsCurParam())
                    { 
                        g_settings[curSet]._IsCurrent = false;
                        if (IsCurParam("Tune")) ucl = true;
                    }

                    curSet--;
                    g_settings.RemoveAt(g_settings.Count-1);
                    
                    if (ucl)
                    { 
                        UpdateKeyLights();
                        UpdateChordLights();
                        UpdateShuffleLight();
                    }
                }

                MarkLight(lblBack, CurSrc < 0);
                UpdateEnterLight();
            }
            else if (CurSrc > -1)
            {
                CurSrc = -1;
                g_srcOff =  0;

                g_shift = false;

                UpdateNewLights();
                UpdateAdjustLights(g_song);

                UpdateInstName(true);
                inputValid = false;

                MarkLight(lblBack, CurSrc < 0);
                g_sampleValid = false;
            }
            else if (SelChan > -1)
            {
                SelChan = -1;

                g_shift = false;
                g_move  = false;
                //UpdateLight(lblMove, g_move ^ (g_song.CurSrc > -1), g_song.SelChan > -1 && !g_move);

                g_song.EditPos = float.NaN;
                UpdateEditLight(lblEdit, false);

                UpdateNewLights();
                UpdateAdjustLights(g_song);

                UpdateInstName(false);

                g_sampleValid = false;

                g_paramKeys = false;
                g_paramAuto = false;

                MarkLight(lblBack, CurSrc < 0);
                //foreach (var btn in funcButtons)
                //    ((IMyFunctionalBlock)btn).Enabled = false;
            }
        }


        void Enter()
        {
            if (curSet > -1)
                return;

            g_move = false;

            if (SelChan < 0)
            {
                g_song.EditPos = float.NaN;
                UpdateEditLight(lblEdit, false);

                SelChan = CurChan;

                UpdateNewLights();
                UpdateAdjustLights(g_song);

                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_inst.Count, maxDspInst, 0, 1);
                UpdateInstOff(SelChan);

                UpdateInstName(true);
                inputValid = false;
                
                g_sampleValid = false;
                MarkLight(lblEnter, CurSrc < 0);
            }
            else if (CurSrc < 0)
            {
                CurSrc = 0;

                g_shift = false;

                UpdateNewLights();
                UpdateAdjustLights(g_song);

                UpdateInstName(false);

                g_sampleValid = false;
                MarkLight(lblEnter, CurSrc < 0);
            }
        }


        void Command1()
        {
            if (   IsCurParam()
                && OK(g_song.EditPos))
            {
                     if (g_paramKeys) Interpolate(g_song);
                else if (g_paramAuto) EnableKeyMove(g_song);
            }
            else if (curSet > -1
                  && IsCurSetting(typeof(Arpeggio))
                  && OK(CurArpeggio.Song.EditPos))
            {
                     if (g_paramKeys) Interpolate  (CurArpeggio.Song);
                else if (g_paramAuto) EnableKeyMove(CurArpeggio.Song);
            }
            else
            {
                if (CurSrc > -1)
                {
                    var src = SelectedSource;
                    src.On = !src.On;
                    UpdateLight(lblCmd1, src.On);
                }
                else
                { 
                    var inst = CurrentInstrument;

                    var src =
                        CurSrc > -1
                        ? inst.Sources[CurSrc]
                        : null;

                    //if (   src != null
                    //    && g_set >= Set.Oscillator
                    //    && g_set <= Set.Offset)
                    //{
                    //    copyW   = src.Oscillator;
                    //    //copyV   = src.Volume;
                    //    //copyOff = src.Offset;

                    //    MarkLight(lblCopy);
                    //}
                    //else if (src != null
                    //      && g_set >= Set.LfoAmplitude
                    //      && g_set <= Set.LfoFixed)
                    //{
                    //    //copyLA  = src.LfoAmplitude;
                    //    //copyLF  = src.LfoFrequency;
                    //    //copyLO  = src.LfoOffset;
                    //    //copyLFx = src.LfoFixed;

                    //    MarkLight(lblCopy);
                    //}
                    //else if (src != null
                    //      && g_set >= Set.Attack
                    //      && g_set <= Set.Release)
                    //{
                    //    //copyA = src.Attack;
                    //    //copyD = src.Decay;
                    //    //copyS = src.Sustain;
                    //    //copyR = src.Release;

                    //    MarkLight(lblCopy);
                    //}
                    //else if (src == null
                    //      && g_song.SelChan > -1
                    //      && g_set >= Set.DelayCount
                    //      && g_set <= Set.DelayPower)
                    //{
                    //    //copyDC = inst.DelayCount;
                    //    //copyDT = inst.DelayTime;
                    //    //copyDL = inst.DelayLevel;
                    //    //copyDP = inst.DelayPower;

                    //    MarkLight(lblCopy);
                    //}
                    //else 
                    if (SelChan < 0)
                    { 
                        CopyChan(g_song, CurPat, CurChan);
                        MarkLight(lblCmd1);
                    }

                    UpdatePasteLight();
                }
            }
        }


        void Command2()
        {
            if (CurSrc > -1)
            {
                var src = SelectedSource;

                var newOsc = (int)src.Oscillator.Type + 1;
                //if (newOsc > (int)OscType.Sample) newOsc = 0;
                if (newOsc > (int)OscType.Crunch) newOsc = 0;
                src.Oscillator = OscillatorFromType((OscType)newOsc);
                MarkLight(lblCmd2);
            }
            else
            { 
                var inst = CurrentInstrument;

                var src =
                    CurSrc > -1
                    ? inst.Sources[CurSrc]
                    : null;

                //if (   src != null
                //    && g_set >= Set.Oscillator
                //    && g_set <= Set.Offset)
                //    //&& OK(copyV.Value))
                //{
                //    src.Oscillator = copyW;
                //    //src.Volume     = copyV;
                //    //src.Offset     = copyOff;

                //    MarkLight(lblPaste);
                //}
                //else if (src != null
                //      && g_set >= Set.LfoAmplitude
                //      && g_set <= Set.LfoFixed)
                //{
                //    //src.LfoAmplitude = copyLA;
                //    //src.LfoFrequency = copyLF;
                //    //src.LfoOffset    = copyLO;
                //    //src.LfoFixed     = copyLFx;

                //    MarkLight(lblPaste);
                //}
                //else if (src != null
                //      && g_set >= Set.Attack
                //      && g_set <= Set.Release)
                //{
                //    //src.Attack  = copyA;
                //    //src.Decay   = copyD;
                //    //src.Sustain = copyS;
                //    //src.Release = copyR;

                //    MarkLight(lblPaste);
                //}
                //else if (src == null
                //      && g_song.SelChan > -1
                //      && g_set >= Set.DelayCount
                //      && g_set <= Set.DelayPower)
                //{
                //    //inst.DelayCount = copyDC;
                //    //inst.DelayTime  = copyDT;
                //    //inst.DelayLevel = copyDL;
                //    //inst.DelayPower = copyDP;

                //    MarkLight(lblPaste);
                //}
                //else 
                if (SelChan < 0)
                {
                    int f, l;
                    GetPatterns(g_song, CurPat, out f, out l);

                    for (int p = f; p <= l; p++)
                        PasteChan(g_song, p, CurChan);

                    copyChan = null;

                    MarkLight(lblCmd2);
                }
            }
        }


        void Command3()
        {
            if (   IsCurParam()
                && OK(g_song.EditPos))
            {
                var param = CurParam;
                var path  = g_settings.Last().GetPath(CurSrc);

                
                if (g_paramKeys)
                { 
                    var notes = GetEditNotes(g_song);

                    foreach (var note in notes)
                    { 
                        var iKey = note.Keys.FindIndex(k => k.Path == path);
                        if (iKey > -1) note.Keys.RemoveAt(iKey);
                    }
                }
                else if (g_paramAuto)
                {
                    var chan = SelectedChannel;

                    var key = chan.AutoKeys.Find(k =>
                           k.Path == path
                        && k.StepTime >= (g_song.EditPos % nSteps)
                        && k.StepTime <  (g_song.EditPos % nSteps) + 1);

                    if (key == null) // create
                    {
                        var val = Parameter.GetAutoValue(g_song.EditPos, CurPat, SelChan, path);

                        var newKey = new Key(
                            CurSrc,
                            param,
                            OK(val) ? val : param.Value,
                            g_song.EditPos % nSteps,
                            SelectedChannel);

                        chan.AutoKeys.Add(newKey);
                        g_song.UpdateAutoKeys();
                    }
                    else // delete
                    {
                        chan.AutoKeys.Remove(key);
                        g_song.UpdateAutoKeys();
                    }
                }

                UpdateAdjustLights(g_song);
                MarkLight(lblCmd3);
            }
            else if (SelChan < 0)
            { 
                g_transpose = !g_transpose;
                UpdateAdjustLights(g_song);
            }
        }


        //void ToggleLfoFixed()
        //{
        //    if (g_song.CurSrc < 0) return;

        //    var src = CurrentInstrument.Sources[g_song.CurSrc];
        //    //src.LfoFixed = !src.LfoFixed;

        //    var bl =
        //           g_set >= Set.LfoAmplitude
        //        && g_set <= Set.LfoFixed;

        //    //UpdateLight(lblLfoFixed, g_song.CurSrc > -1 && CurrentSource.LfoFixed, bl);
        //}


        void Shift()
        {
            g_shift = !g_shift;
            UpdateAdjustLights(g_song);
        }


        void Adjust(Song song, float delta)
        {
            Adjust(song, CurSetting, delta);
        }


        void Adjust(Song song, Setting setting, float delta)
        {
            if (IsSettingType(setting, typeof(Harmonics)))
            {
                var hrm  = CurHarmonics;
                var tone = hrm.Tones[hrm.CurTone];

                tone.SetValue(tone.AdjustValue(tone.Value, delta, g_shift), null, -1);
            }
            else if (   IsParam(setting)
                     && (   g_paramKeys 
                         || g_paramAuto))
            {
                var chan = SelectedChannel;
                var path = g_settings.Last().GetPath(CurSrc);

                if (g_paramKeys)
                { 
                    var notes = GetEditNotes(song);

                    foreach (var note in notes)
                    { 
                        var iKey = note.Keys.FindIndex(k => k.Path == path);

                        if (iKey > -1)
                            AdjustKey(note.Keys[iKey], delta);
                        else
                        {
                            var param = (Parameter)GetSettingFromPath(chan.Instrument, path);
                            note.Keys.Add(new Key(CurSrc, param, param.Value, song.GetStep(note)));
                            AdjustKey(note.Keys.Last(), delta);
                        }
                    }
                }
                else if (g_paramAuto)
                {
                    if (OK(song.EditPos))
                    { 
                        var key = chan.AutoKeys.Find(
                               k => k.Path == path
                            && k.StepTime >= (song.EditPos % nSteps) 
                            && k.StepTime <  (song.EditPos % nSteps) + 1);

                        if (key != null)
                            AdjustKey(key, delta);
                    }
                    else
                    {
                        var keys = chan.AutoKeys.Where(k => k.Path == path).ToList();

                        foreach (var key in keys)
                            AdjustKey(key, delta);
                    }

                    song.UpdateAutoKeys();
                }

                UpdateAdjustLights(song);
                MarkLight(delta >= 0 ? lblUp : lblDown, !g_shift);
            }
            else if (SelChan > -1)
            {
                if (IsParam(setting))
                {
                    AdjustParam(g_song, (Parameter)setting, delta);
                    MarkLight(delta >= 0 ? lblUp : lblDown, !g_shift);
                    g_sampleValid = false;
                }
            }  
            else
            {
                if (g_transpose) 
                    Transpose(song, CurChan, delta);
            }
        }


        void AdjustFromController(Song song, Setting setting, float delta)
        {
            if (IsParam(setting))
            {
                AdjustParam(g_song, (Parameter)setting, delta);
                MarkLight(delta >= 0 ? lblUp : lblDown, !g_shift);
                g_sampleValid = false;
            }
        }


        void AdjustParam(Song song, Parameter param, float delta)
        {
            param.SetValue(
                MinMax(
                    param.Min, 
                    param.AdjustValue(param.Value, delta, g_shift),
                    param.Max),
                null,
                CurSrc);
        }


        void AdjustKey(Key key, float delta)
        {
            key.Value = MinMax(
                key.Parameter.Min,
                key.Parameter.AdjustValue(key.Value, delta, g_shift),
                key.Parameter.Max);
        }


        //float AdjustValue(string tag, float value, float d)
        //{
        //    float mod     = d * (g_shift ? 10 : 1);
        //    float modTune = d * (g_shift ? 24 : 1);

        //    var   sdVal  = dVal  * mod;
        //    var   sdTime = dTime * mod;

        //    if (   IsDigit(tag[0])
        //        || tag == "Tone")
        //    { 
        //        return value + sdVal * (float)Math.Pow(2, value); // not a real tag, used for Harmonics
        //    }
        //    else
        //    { 
        //        value 
        //        switch (tag)
        //        {
        //            case "Vol":  return value + sdVal; 
        //            case "Tune": return value + modTune/2; 

        //            case "Cut":  return value + sdVal;
        //            case "Res":  return value + sdVal;

        //            case "Att":  return value + sdVal; 
        //            case "Dec":  return value + sdVal; 
        //            case "Sus":  return value + sdVal; 
        //            case "Rel":  return value + sdVal; 

        //            case "Cnt":  return value + mod;   
        //            case "Time": return value + sdTime;
        //            case "Lvl":  return value + sdVal; 
        //            case "Pow":  return value + sdVal; 

        //            case "Amp":  return value + sdVal; 
        //            case "Freq": return value + sdVal; 
        //            case "Off":  return value + sdVal;

        //            case "Len":  return value + mod;
        //            case "Scl":  return value + sdVal;
        //        }
        //    }

        //    return float.NaN;
        //}


        void SetChan(int ch)
        {
            ch += CurChan;

            if (ch >= nChans) ch = 0;
            if (ch < 0) ch = nChans - 1;

            if (g_move)
            {
                CurrentPattern.Channels.RemoveAt(CurChan);
                CurrentPattern.Channels.Insert(ch, CurrentChannel);
            }

            CurChan = ch;

            if (CurSrc > -1)
                CurSrc = 0;

            UpdateOctaveLight();
            UpdateInstOff(CurChan);
        }


        void SetShuffle(int ch, int sh)
        {
            sh += CurrentPattern.Channels[ch].Shuffle;

            int first, last;
            GetPatterns(g_song, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
            {
                var pat = g_song.Patterns[p];
                var chan = pat.Channels[ch];

                chan.Shuffle = MinMax(0, sh, g_ticksPerStep - 1);
            }
        }
    }
}
