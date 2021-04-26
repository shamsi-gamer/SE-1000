using System;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void New()
        {
            if (g_clip.CurSrc > -1)
            {
                g_clip.CurSet = -1;
                g_settings.Clear();


                var inst = g_clip.CurrentInstrument;

                if (inst.Sources.Count < maxDspSrc)
                { 
                    //for (int i = g_song.g_clip.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    inst.Sources.Insert(g_clip.CurSrc+1, new Source(inst));

                    g_clip.CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.g_clip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.g_clip.CurSrc);
                UpdateSrcOff();


                MarkLight(lblNew, false);
            }
            else if (g_clip.SelChan > -1)
            {
                g_clip.CurSet = -1;
                g_settings.Clear();


                var inst = new Instrument();
                inst.Sources.Add(new Source(inst));

                inst.Name = GetNewName(inst.Name, str => g_inst.Exists(_s => _s.Name == str));

                g_inst.Insert(g_inst.IndexOf(g_clip.CurrentInstrument) + 1, inst);
                SetCurInst(inst);

                UpdateOctaveLight();
                //UpdateDspOffset(ref instOff, g_song.g_clip.CurSrc, g_inst.Count, maxDspSrc, 0, 1);
                UpdateInstOff(g_clip.CurChan);

                UpdateInstName();
                g_inputValid = false;

                MarkLight(lblNew);
            }
        }


        void Duplicate()
        {
            if (g_clip.CurSrc > -1)
            {
                g_clip.CurSet = -1;
                g_settings.Clear();


                var inst = g_clip.CurrentInstrument;

                if (inst.Sources.Count < 8)
                {
                    //for (int i = g_song.g_clip.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    var src = new Source(inst.Sources[g_clip.CurSrc], inst);
                    inst.Sources.Insert(g_clip.CurSrc+1, src);

                    g_clip.CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.g_clip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.g_clip.CurSrc);
                UpdateSrcOff();

                //g_sampleValid = F; 
                
                MarkLight(lblDuplicate, false);
            }
            else if (g_clip.SelChan > -1)
            {
                g_clip.CurSet = -1;
                g_settings.Clear();


                var inst = new Instrument(g_clip.CurrentInstrument);
                inst.Name = GetNewName(inst.Name, newName => g_inst.Exists(s => s.Name == newName));

                g_inst.Insert(g_inst.IndexOf(g_clip.CurrentInstrument) + 1, inst);
                SetCurInst(inst);
                g_clip.SrcOff = 0;

                UpdateOctaveLight();
                UpdateInstName();

                //UpdateDspOffset(ref instOff, g_song.g_clip.CurSrc, g_inst.Count, maxDspSrc, 0, 1);
                UpdateInstOff(g_clip.CurChan);

                //g_sampleValid = F; 
                
                MarkLight(lblDuplicate);
            }
        }


        void Delete()
        {
            if (g_clip.CurSrc > -1)
            {
                g_clip.CurSet = -1;
                g_settings.Clear();


                var inst = g_clip.CurrentInstrument;

                inst.Sources[g_clip.CurSrc].Delete(g_clip);
                inst.Sources.RemoveAt(g_clip.CurSrc);

                //for (int i = g_song.g_clip.CurSrc; i < inst.Sources.Count; i++)
                //    inst.Sources[i].Index--;

                if (inst.Sources.Count == 0)
                {
                    inst.Sources.Add(new Source(inst));
                    g_clip.CurSrc = 0;
                }

                if (g_clip.CurSrc >= inst.Sources.Count)
                    g_clip.CurSrc = inst.Sources.Count - 1;

                //UpdateDspOffset(ref srcOff, g_song.g_clip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.g_clip.CurSrc);
                UpdateSrcOff();

                //g_sampleValid = F; 
                
                MarkLight(lblDelete, false);
            }
            else if (g_clip.SelChan > -1)
            {
                g_clip.CurSet = -1;
                g_settings.Clear();


                var i = g_inst.IndexOf(g_clip.CurrentInstrument);
                var inst = g_clip.CurrentInstrument;

                g_clip.CurrentInstrument.Delete(g_clip);
                g_inst.Remove(g_clip.CurrentInstrument);

                if (g_inst.Count == 0)
                {
                    g_inst.Add(new Instrument());
                    g_inst[0].Sources.Add(new Source(g_inst[0]));
                }

                i = MinMax(0, i - 1, g_inst.Count - 1);

                foreach (var p in g_clip.Patterns)
                    foreach (var c in p.Channels)
                        if (c.Instrument == inst) c.Instrument = g_inst[i];

                g_clip.SrcOff = 0;

                //UpdateDspOffset(ref instOff, g_song.g_clip.CurSrc, g_inst.Count, maxDspSrc, 0, 1);
                UpdateInstOff(g_clip.CurChan);

                UpdateOctaveLight();
                UpdateInstName();

                //g_sampleValid = F; 
                
                MarkLight(lblDelete);
            }
        }


        void Move(int move)
        {
            if (g_clip.CurSet > -1)
                return;


            if (g_clip.SelChan < 0)
            {
                SetChan(-move);
            }
            else if (g_clip.CurSrc < 0) // instrument
            {
                var i = g_inst.IndexOf(g_clip.CurrentInstrument);
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
                    g_clip.GetCurPatterns(out first, out last);

                    for (int p = first; p <= last; p++)
                        g_clip.Patterns[p].Channels[g_clip.CurChan].Instrument = g_inst[n];

                    UpdateOctaveLight();
                }


                UpdateInstName();
                g_inputValid = false;

                UpdateInstOff(g_clip.CurChan);

                g_clip.SrcOff = 0;
            }
            else // source
            {
                var inst = g_clip.CurrentInstrument;
                var next = g_clip.CurSrc + move;

                if (next >= inst.Sources.Count) next = 0;
                if (next < 0) next = inst.Sources.Count - 1;

                if (g_move)
                {
                    var src = inst.Sources[g_clip.CurSrc];
                    inst.Sources.RemoveAt(g_clip.CurSrc);
                    inst.Sources.Insert(next, src);
                }

                g_clip.CurSrc = next;

                dspMain.Panel.WriteText("", false);

                UpdateSrcOff();

                UpdateLight(lblCmd1, "On", 10, 10);
                UpdateLight(lblCmd1, inst.Sources[g_clip.CurSrc].On);
            }


            MarkLight(
                move >= 0 ? lblNext : lblPrev, 
                !g_move && g_clip.CurSrc < 0);
        }


        void BackOut()
        {
            g_move = false;

            g_clip.ParamKeys = false;
            g_clip.ParamAuto = false;

            var _curSet = false;


            if (g_clip.CurSet > -1)
            {
                bool ucl = false;

                if (IsCurParam())
                { 
                    CurSetting._IsCurrent = false;
                    if (IsCurParam(strTune)) ucl = true;
                }

                g_clip.CurSet = -1;
                g_settings.Clear();
                    
                if (ucl)
                { 
                    UpdateKeyLights();
                    UpdateChordLights();
                    UpdateShuffleLight();
                }

                _curSet = true;
            }


            if (     g_clip.CurSrc > -1
                && !_curSet)
            {
                g_clip.CurSrc = -1;
                g_clip.SrcOff =  0;

                g_clip.Shift  = false;

                UpdateInstName(true);
                g_inputValid = false;
            }
            

            if (     g_clip.SelChan > -1
                && !_curSet)
            {
                g_clip.SelChan = -1;

                g_clip.Shift = false;
                g_move  = false;

                UpdateInstName(false);
            }

            UpdateAdjustLights(g_clip);
            UpdateNewLights();

            MarkLight(lblBackOut, g_clip.CurSrc < 0);

            SaveInstruments();
        }


        void Back()
        {
            g_move = false;

            if (g_clip.CurSet > -1)
            {
                if (g_clip.ParamKeys)
                {
                    g_clip.ParamKeys = false;
                    g_clip.EditPos   = fN;
                    UpdateEditLight(lblEdit, false);
                }
                else if (g_clip.ParamAuto)
                {
                    g_clip.ParamAuto = false;
                    g_clip.EditPos   = fN;
                    UpdateEditLight(lblEdit, false);
                }
                else
                {
                    bool ucl = false;

                    if (IsCurSetting(typeof(Arpeggio)))
                    {
                        CurArpeggio.Clip.EditPos = -Math.Abs(CurArpeggio.Clip.EditPos); // turn off but keep value
                        UpdateEditLight(lblEdit, false);
                    }
                    else if (IsCurParam())
                    { 
                        CurSetting._IsCurrent = false;
                        if (IsCurParam(strTune)) ucl = true;
                    }

                    g_clip.CurSet--;
                    g_settings.RemoveAt(g_settings.Count-1);
                    
                    if (ucl)
                    { 
                        UpdateKeyLights();
                        UpdateChordLights();
                        UpdateShuffleLight();
                    }
                }
            }
            else if (g_clip.CurSrc > -1)
            {
                g_clip.CurSrc = -1;
                g_clip.SrcOff =  0;

                g_clip.Shift = false;

                UpdateInstName(true);
                g_inputValid = false;
            }
            else if (g_clip.SelChan > -1)
            {
                g_clip.SelChan = -1;

                g_clip.Shift = false;
                g_move  = false;

                g_clip.EditPos = fN;
                UpdateEditLight(lblEdit, false);

                UpdateInstName(false);

                g_clip.ParamKeys = false;
                g_clip.ParamAuto = false;
            }

            MarkLight(lblBack, g_clip.CurSrc < 0);

            UpdateNewLights();
            UpdateAdjustLights(g_clip);

            SaveInstruments();
        }


        void Enter()
        {
            if (g_clip.CurSet > -1)
                return;

            g_move = false;

            if (g_clip.SelChan < 0)
            {
                g_clip.EditPos = fN;
                UpdateEditLight(lblEdit, false);

                g_clip.SelChan = g_clip.CurChan;

                UpdateNewLights();
                UpdateAdjustLights(g_clip);

                UpdateInstOff(g_clip.SelChan);

                UpdateInstName(true);
                g_inputValid = false;

                MarkLight(lblEnter, g_clip.CurSrc < 0);
                UpdateLight(lblCmd3, false);
            }
            else if (g_clip.CurSrc < 0)
            {
                g_clip.CurSrc = 0;

                g_clip.Shift = false;

                UpdateNewLights();
                UpdateAdjustLights(g_clip);

                UpdateInstName(false);

                MarkLight(lblEnter, g_clip.CurSrc < 0);
            }
        }


        void Command1()
        {
            if (ModDestConnecting != null)
            {
                if (ModDestConnecting == CurSetting)
                    ResetModConnecting();

                else
                { 
                    ModDestConnecting.SrcSettings   .Add(g_clip.CurSet > -1 ? CurSetting : null);
                    ModDestConnecting.SrcSources    .Add(g_clip.SelectedSource);
                    ModDestConnecting.SrcInstruments.Add(g_clip.SelectedInstrument);

                    SwitchToSetting(
                        ModDestChannel.Instrument,
                        ModDestSrcIndex, 
                        ModDestConnecting);

                    ResetModConnecting();
                }
            }
            else if (IsCurParam())
            {
                if (OK(g_clip.EditPos))
                {
                         if (g_clip.ParamKeys) Interpolate(g_clip);
                    else if (g_clip.ParamAuto) EnableKeyMove(g_clip);
                }
            }
            else if (IsCurSetting(typeof(Modulate)))
            {
                if (ModDestConnecting == null)
                {
                    ModDestConnecting = CurModulate;
                    ModCurChan        = g_clip.CurChan;
                    ModSelChan        = g_clip.SelChan;
                    ModDestSrcIndex   = g_clip.CurSrc;
                    ModDestChannel    = g_clip.SelectedChannel;

                    UpdateAdjustLights(g_clip);
                }
            }
            else if (IsCurSetting(typeof(Arpeggio))
                  && OK(CurArpeggio.Clip.EditPos))
            {
                     if (g_clip.ParamKeys) Interpolate  (CurArpeggio.Clip);
                else if (g_clip.ParamAuto) EnableKeyMove(CurArpeggio.Clip);
            }
            else
            {
                if (g_clip.CurSrc > -1)
                {
                    var src = g_clip.SelectedSource;
                    src.On = !src.On;
                    UpdateLight(lblCmd1, src.On);
                }
                else
                { 
                    var inst = g_clip.CurrentInstrument;

                    var src =
                        g_clip.CurSrc > -1
                        ? inst.Sources[g_clip.CurSrc]
                        : null;

                    if (g_clip.SelChan < 0)
                    { 
                        CopyChan(g_clip, g_clip.CurPat, g_clip.CurChan);
                        MarkLight(lblCmd1);
                    }

                    UpdatePasteLight();
                }
            }
        }


        void Command2()
        {
            if (g_clip.CurSrc > -1)
            {
                var src = g_clip.SelectedSource;

                var newOsc = (int)src.Oscillator.Type + 1;
                if (newOsc > (int)OscType.Crunch) newOsc = 0;
                src.Oscillator = OscillatorFromType((OscType)newOsc);
                MarkLight(lblCmd2);
            }
            else
            { 
                var src =
                    g_clip.CurSrc > -1
                    ? g_clip.CurrentInstrument.Sources[g_clip.CurSrc]
                    : null;

                if (g_clip.SelChan < 0)
                {
                    int f, l;
                    g_clip.GetCurPatterns(out f, out l);

                    for (int p = f; p <= l; p++)
                        PasteChan(g_clip, p, g_clip.CurChan);

                    copyChan = null;

                    MarkLight(lblCmd2);
                }
            }
        }


        void Command3()
        {
            if (IsCurParam())
            {
                var param = CurParam;
                var path  = g_settings.Last().GetPath(g_clip.CurSrc);

                
                if (   g_clip.ParamKeys
                    && OK(g_clip.EditPos))
                { 
                    var notes = GetEditNotes(g_clip);

                    foreach (var note in notes)
                    { 
                        var iKey = note.Keys.FindIndex(k => k.Path == path);
                        if (iKey > -1) note.Keys.RemoveAt(iKey);
                    }
                }
                else if (g_clip.ParamAuto
                      && OK(g_clip.EditPos))
                {
                    var chan = g_clip.SelectedChannel;

                    var key = chan.AutoKeys.Find(k =>
                           k.Path == path
                        && k.StepTime >= (g_clip.EditPos % g_nSteps)
                        && k.StepTime <  (g_clip.EditPos % g_nSteps) + 1);

                    if (key == null) // create
                    {
                        var val = Parameter.GetAutoValue(g_clip.EditPos, g_clip.CurPat, g_clip.SelChan, path);

                        var newKey = new Key(
                            g_clip.CurSrc,
                            param,
                            OK(val) ? val : param.Value,
                            g_clip.EditPos % g_nSteps,
                            g_clip.SelectedChannel);

                        chan.AutoKeys.Add(newKey);
                        g_clip.UpdateAutoKeys();
                    }
                    else // delete
                    {
                        chan.AutoKeys.Remove(key);
                        g_clip.UpdateAutoKeys();
                    }
                }

                UpdateAdjustLights(g_clip);
                MarkLight(lblCmd3);
            }
            else if (g_clip.CurSet > -1)
            {
                if (CurSetting.CanDelete())
                    RemoveSetting(CurSetting);

                UpdateAdjustLights(g_clip);
                MarkLight(lblCmd3);
            }
            else if (g_clip.SelChan < 0)
            { 
                g_clip.Transpose = !g_clip.Transpose;
                UpdateAdjustLights(g_clip);
            }
        }


        void Shift()
        {
            g_clip.Shift = !g_clip.Shift;
            UpdateAdjustLights(g_clip);
        }


        void Adjust(Clip clip, Setting setting, float delta)
        {
            if (IsSettingType(setting, typeof(Harmonics)))
            {
                CurHarmonics.Adjust(delta);
            }
            else if (   IsParam(setting)
                     && (   clip.ParamKeys 
                         || clip.ParamAuto))
            {
                var chan = g_clip.SelectedChannel;
                var path = g_settings.Last().GetPath(clip.CurSrc);

                if (clip.ParamKeys)
                { 
                    var notes = GetEditNotes(clip);

                    foreach (var note in notes)
                    { 
                        var iKey = note.Keys.FindIndex(k => k.Path == path);

                        if (iKey > -1)
                            AdjustKey(note.Keys[iKey], delta);
                        else
                        {
                            var param = (Parameter)GetSettingFromPath(chan.Instrument, path);
                            note.Keys.Add(new Key(g_clip.CurSrc, param, param.Value, note.SongStep));
                            AdjustKey(note.Keys.Last(), delta);
                        }
                    }
                }
                else if (g_clip.ParamAuto)
                {
                    if (OK(clip.EditPos))
                    { 
                        var key = chan.AutoKeys.Find(
                               k => k.Path == path
                            && k.StepTime >= (clip.EditPos % g_nSteps) 
                            && k.StepTime <  (clip.EditPos % g_nSteps) + 1);

                        if (key != null)
                            AdjustKey(key, delta);
                    }
                    else
                    {
                        var keys = chan.AutoKeys.Where(k => k.Path == path).ToList();

                        foreach (var key in keys)
                            AdjustKey(key, delta);
                    }

                    clip.UpdateAutoKeys();
                }

                UpdateAdjustLights(clip);
                MarkLight(delta >= 0 ? lblUp : lblDown, !clip.Shift);
            }
            else if (clip.SelChan > -1)
            {
                if (IsParam(setting))
                {
                    AdjustParam(clip, (Parameter)setting, delta);
                    MarkLight(delta >= 0 ? lblUp : lblDown, !clip.Shift);
                }
            }  
            else
            {
                if (clip.Transpose) 
                    Transpose(clip, clip.CurChan, delta);
            }
        }


        void AdjustFromController(Clip clip, Setting setting, float delta)
        {
            if (IsParam(setting))
            {
                AdjustParam(g_clip, (Parameter)setting, delta);
                MarkLight(delta >= 0 ? lblUp : lblDown, !g_clip.Shift);
                //g_sampleValid = F;
            }
        }


        void AdjustParam(Clip clip, Parameter param, float delta)
        {
            param.SetValue(
                MinMax(
                    param.Min, 
                    param.AdjustValue(param.Value, delta, g_clip.Shift),
                    param.Max),
                null,
                g_clip.CurSrc);
        }


        void AdjustKey(Key key, float delta)
        {
            key.Value = MinMax(
                key.Parameter.Min,
                key.Parameter.AdjustValue(key.Value, delta, g_clip.Shift),
                key.Parameter.Max);
        }


        void SetChan(int ch)
        {
            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            ch += g_clip.CurChan;

            for (int p = first; p <= last; p++)
            { 
                var pat = g_clip.Patterns[p];

                if (ch >= g_nChans) ch = 0;
                if (ch < 0) ch = g_nChans - 1;

                if (g_move)
                {
                    var temp = pat.Channels[g_clip.CurChan];
                    pat.Channels[g_clip.CurChan] = pat.Channels[ch];
                    pat.Channels[ch]      = temp;

                    pat.Channels[g_clip.CurChan].UpdateNotes();
                    pat.Channels[ch]     .UpdateNotes();
                }
            }

            g_clip.CurChan = ch;

            if (g_clip.CurSrc > -1)
                g_clip.CurSrc = 0;

            UpdateOctaveLight();
            UpdateInstOff(g_clip.CurChan);
        }


        void SetShuffle(int ch, int sh)
        {
            sh += g_clip.CurrentPattern.Channels[ch].Shuffle;

            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            {
                var pat  = g_clip.Patterns[p];
                var chan = pat.Channels[ch];

                chan.Shuffle = MinMax(0, sh, g_ticksPerStep - 1);
            }
        }
    }
}
