using System;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void New()
        {
            if (CurSrc > -1)
            {
                CurSet = -1;
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

                //g_sampleValid = F;

                MarkLight(lblNew, false);
            }
            else if (SelChan > -1)
            {
                CurSet = -1;
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

                //g_sampleValid = F;

                MarkLight(lblNew);
            }
        }


        void Duplicate()
        {
            if (CurSrc > -1)
            {
                CurSet = -1;
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

                //g_sampleValid = F; 
                
                MarkLight(lblDuplicate, false);
            }
            else if (SelChan > -1)
            {
                CurSet = -1;
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

                //g_sampleValid = F; 
                
                MarkLight(lblDuplicate);
            }
        }


        void Delete()
        {
            if (CurSrc > -1)
            {
                CurSet = -1;
                g_settings.Clear();


                var inst = CurrentInstrument;

                inst.Sources[CurSrc].Delete(g_song);
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

                //g_sampleValid = F; 
                
                MarkLight(lblDelete, false);
            }
            else if (SelChan > -1)
            {
                CurSet = -1;
                g_settings.Clear();


                var i = g_inst.IndexOf(CurrentInstrument);
                var inst = CurrentInstrument;

                CurrentInstrument.Delete(g_song);
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

                //g_sampleValid = F; 
                
                MarkLight(lblDelete);
            }
        }


        void Move(int move)
        {
            if (CurSet > -1)
                return;


            if (SelChan < 0)
            {
                SetChan(-move);
            }
            else if (CurSrc < 0) // instrument
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

                UpdateInstOff(CurChan);

                g_srcOff = 0;
            }
            else // source
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

                dspMain.Panel.WriteText("", false);

                UpdateSrcOff();

                UpdateLight(lblCmd1, "On", 10, 10);
                UpdateLight(lblCmd1, inst.Sources[CurSrc].On);
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

            if (CurSet > -1)
            {
                bool ucl = false;

                if (IsCurParam())
                { 
                    CurSetting._IsCurrent = false;
                    if (IsCurParam("Tune")) ucl = true;
                }

                CurSet = -1;
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


            SaveInstruments();
        }


        void Back()
        {
            g_move = false;

            if (CurSet > -1)
            {
                if (g_paramKeys)
                {
                    g_paramKeys    = false;
                    g_song.EditPos = fN;
                    UpdateEditLight(lblEdit, false);
                }
                else if (g_paramAuto)
                {
                    g_paramAuto    = false;
                    g_song.EditPos = fN;
                    UpdateEditLight(lblEdit, false);
                }
                else
                {
                    bool ucl = false;

                    if (IsCurSetting(typeof(Arpeggio)))
                    {
                        CurArpeggio.Song.EditPos = -Math.Abs(CurArpeggio.Song.EditPos); // turn off but keep value
                        UpdateEditLight(lblEdit, false);
                    }
                    else if (IsCurParam())
                    { 
                        CurSetting._IsCurrent = false;
                        if (IsCurParam("Tune")) ucl = true;
                    }

                    CurSet--;
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
                //g_sampleValid = F;
            }
            else if (SelChan > -1)
            {
                SelChan = -1;

                g_shift = false;
                g_move  = false;
                //UpdateLight(lblMove, g_move ^ (g_song.CurSrc > -1), g_song.SelChan > -1 && !g_move);

                g_song.EditPos = fN;
                UpdateEditLight(lblEdit, false);

                UpdateNewLights();
                UpdateAdjustLights(g_song);

                UpdateInstName(false);

                //g_sampleValid = F;

                g_paramKeys = false;
                g_paramAuto = false;

                MarkLight(lblBack, CurSrc < 0);
                //foreach (var btn in funcButtons)
                //    ((IMyFunctionalBlock)btn).Enabled = F;
            }


            SaveInstruments();
        }


        void Enter()
        {
            if (CurSet > -1)
                return;

            g_move = false;

            if (SelChan < 0)
            {
                g_song.EditPos = fN;
                UpdateEditLight(lblEdit, false);

                SelChan = CurChan;

                UpdateNewLights();
                UpdateAdjustLights(g_song);

                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_inst.Count, maxDspInst, 0, 1);
                UpdateInstOff(SelChan);

                UpdateInstName(true);
                inputValid = false;

                //g_sampleValid = F;
                MarkLight(lblEnter, CurSrc < 0);
                UpdateLight(lblCmd3, false);
            }
            else if (CurSrc < 0)
            {
                CurSrc = 0;

                g_shift = false;

                UpdateNewLights();
                UpdateAdjustLights(g_song);

                UpdateInstName(false);

                //g_sampleValid = F;
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
            else if (CurSet > -1
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
                if (newOsc > (int)OscType.Crunch) newOsc = 0;
                src.Oscillator = OscillatorFromType((OscType)newOsc);
                MarkLight(lblCmd2);
            }
            else
            { 
                var src =
                    CurSrc > -1
                    ? CurrentInstrument.Sources[CurSrc]
                    : null;

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
                        && k.StepTime >= (g_song.EditPos % g_nSteps)
                        && k.StepTime <  (g_song.EditPos % g_nSteps) + 1);

                    if (key == null) // create
                    {
                        var val = Parameter.GetAutoValue(g_song.EditPos, CurPat, SelChan, path);

                        var newKey = new Key(
                            CurSrc,
                            param,
                            OK(val) ? val : param.Value,
                            g_song.EditPos % g_nSteps,
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


        void Shift()
        {
            g_shift = !g_shift;
            UpdateAdjustLights(g_song);
        }


        void Adjust(Song song, Setting setting, float delta)
        {
            if (IsSettingType(setting, typeof(Harmonics)))
            {
                CurHarmonics.Adjust(delta);
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
                            note.Keys.Add(new Key(CurSrc, param, param.Value, note.SongStep));
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
                            && k.StepTime >= (song.EditPos % g_nSteps) 
                            && k.StepTime <  (song.EditPos % g_nSteps) + 1);

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
                    // = F;
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
                //g_sampleValid = F;
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


        void SetChan(int ch)
        {
            ch += CurChan;

            if (ch >= g_nChans) ch = 0;
            if (ch < 0) ch = g_nChans - 1;

            if (g_move)
            {
                var temp = CurrentPattern.Channels[CurChan];
                CurrentPattern.Channels[CurChan] = CurrentPattern.Channels[ch];
                CurrentPattern.Channels[ch]      = temp;

                CurrentPattern.Channels[CurChan].UpdateNotes();
                CurrentPattern.Channels[ch]     .UpdateNotes();
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
                var pat  = g_song.Patterns[p];
                var chan = pat.Channels[ch];

                chan.Shuffle = MinMax(0, sh, g_ticksPerStep - 1);
            }
        }
    }
}
