using System;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void New()
        {
            if (g_session.CurClip.CurSrc > -1)
            {
                g_session.CurClip.CurSet = -1;
                g_settings.Clear();


                var inst = g_session.CurClip.CurrentInstrument;

                if (inst.Sources.Count < maxDspSrc)
                { 
                    //for (int i = g_song.g_session.CurClip.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    inst.Sources.Insert(g_session.CurClip.CurSrc+1, new Source(inst));

                    g_session.CurClip.CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.g_session.CurClip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.g_session.CurClip.CurSrc);
                UpdateSrcOff();
            }
            else if (g_session.CurClip.SelChan > -1)
            {
                g_session.CurClip.CurSet = -1;
                g_settings.Clear();


                var inst = new Instrument();
                inst.Sources.Add(new Source(inst));

                inst.Name = GetNewName(inst.Name, str => g_session.Instruments.Exists(_s => _s.Name == str));

                g_session.Instruments.Insert(g_session.Instruments.IndexOf(g_session.CurClip.CurrentInstrument) + 1, inst);
                SetCurInst(inst);

                //UpdateOctaveLabel();
                //UpdateDspOffset(ref instOff, g_song.g_session.CurClip.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(g_session.CurClip.CurChan);

                UpdateInstName();
                g_inputValid = false;
            }


            lblNew.Mark();
        }


        void Duplicate()
        {
            if (g_session.CurClip.CurSrc > -1)
            {
                g_session.CurClip.CurSet = -1;
                g_settings.Clear();


                var inst = g_session.CurClip.CurrentInstrument;

                if (inst.Sources.Count < 8)
                {
                    //for (int i = g_song.g_session.CurClip.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    var src = new Source(inst.Sources[g_session.CurClip.CurSrc], inst);
                    inst.Sources.Insert(g_session.CurClip.CurSrc+1, src);

                    g_session.CurClip.CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.g_session.CurClip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.g_session.CurClip.CurSrc);
                UpdateSrcOff();
            }
            else if (g_session.CurClip.SelChan > -1)
            {
                g_session.CurClip.CurSet = -1;
                g_settings.Clear();


                var inst = new Instrument(g_session.CurClip.CurrentInstrument);
                inst.Name = GetNewName(inst.Name, newName => g_session.Instruments.Exists(s => s.Name == newName));

                g_session.Instruments.Insert(g_session.Instruments.IndexOf(g_session.CurClip.CurrentInstrument) + 1, inst);
                SetCurInst(inst);
                g_session.CurClip.SrcOff = 0;

                //UpdateOctaveLabel();
                UpdateInstName();

                //UpdateDspOffset(ref instOff, g_song.g_session.CurClip.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(g_session.CurClip.CurChan);
            }


            lblDuplicate.Mark();
        }


        void Delete()
        {
            if (g_session.CurClip.CurSrc > -1)
            {
                g_session.CurClip.CurSet = -1;
                g_settings.Clear();


                var inst = g_session.CurClip.CurrentInstrument;

                inst.Sources[g_session.CurClip.CurSrc].Delete(g_session.CurClip);
                inst.Sources.RemoveAt(g_session.CurClip.CurSrc);

                //for (int i = g_song.g_session.CurClip.CurSrc; i < inst.Sources.Count; i++)
                //    inst.Sources[i].Index--;

                if (inst.Sources.Count == 0)
                {
                    inst.Sources.Add(new Source(inst));
                    g_session.CurClip.CurSrc = 0;
                }

                if (g_session.CurClip.CurSrc >= inst.Sources.Count)
                    g_session.CurClip.CurSrc = inst.Sources.Count - 1;

                //UpdateDspOffset(ref srcOff, g_song.g_session.CurClip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.g_session.CurClip.CurSrc);
                UpdateSrcOff();

                //g_sampleValid = F; 
            }
            else if (g_session.CurClip.SelChan > -1)
            {
                g_session.CurClip.CurSet = -1;
                g_settings.Clear();


                var i = g_session.Instruments.IndexOf(g_session.CurClip.CurrentInstrument);
                var inst = g_session.CurClip.CurrentInstrument;

                g_session.CurClip.CurrentInstrument.Delete(g_session.CurClip);
                g_session.Instruments.Remove(g_session.CurClip.CurrentInstrument);

                if (g_session.Instruments.Count == 0)
                {
                    g_session.Instruments.Add(new Instrument());
                    g_session.Instruments[0].Sources.Add(new Source(g_session.Instruments[0]));
                }

                i = MinMax(0, i - 1, g_session.Instruments.Count - 1);

                foreach (var p in g_session.CurClip.Patterns)
                    foreach (var c in p.Channels)
                        if (c.Instrument == inst) c.Instrument = g_session.Instruments[i];

                g_session.CurClip.SrcOff = 0;

                //UpdateDspOffset(ref instOff, g_song.g_session.CurClip.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(g_session.CurClip.CurChan);

                //UpdateOctaveLabel();
                UpdateInstName();

                //g_sampleValid = F; 
            }

            lblDelete.Mark();
        }


        public void ToggleMove()
        {
            if (g_session.CurClip.CurSet > -1) return;

            g_move = !g_move;
        }


        void Move(int move)
        {
            if (g_session.CurClip.CurSet > -1)
                return;


            if (g_session.CurClip.SelChan < 0)
            {
                SetChan(-move);
            }
            else if (g_session.CurClip.CurSrc < 0) // instrument
            {
                var i = g_session.Instruments.IndexOf(g_session.CurClip.CurrentInstrument);
                var n = i + move;

                if (n >= g_session.Instruments.Count) n = 0;
                if (n < 0) n = g_session.Instruments.Count - 1;

                if (g_move)
                {
                    var inst = g_session.Instruments[i];

                    g_session.Instruments.RemoveAt(i);
                    g_session.Instruments.Insert(n, inst);
                }
                else
                {
                    int first, last;
                    g_session.CurClip.GetCurPatterns(out first, out last);

                    for (int p = first; p <= last; p++)
                        g_session.CurClip.Patterns[p].Channels[g_session.CurClip.CurChan].Instrument = g_session.Instruments[n];

                    //UpdateOctaveLabel();
                }


                UpdateInstName();
                g_inputValid = false;

                UpdateInstOff(g_session.CurClip.CurChan);

                g_session.CurClip.SrcOff = 0;
            }
            else // source
            {
                var inst = g_session.CurClip.CurrentInstrument;
                var next = g_session.CurClip.CurSrc + move;

                if (next >= inst.Sources.Count) next = 0;
                if (next < 0) next = inst.Sources.Count - 1;

                if (g_move)
                {
                    var src = inst.Sources[g_session.CurClip.CurSrc];
                    inst.Sources.RemoveAt(g_session.CurClip.CurSrc);
                    inst.Sources.Insert(next, src);
                }

                g_session.CurClip.CurSrc = next;

                dspMain.Panel.WriteText("", false);

                UpdateSrcOff();

                //UpdateLabel(lblCmd1, "On", 10, 10);
                //UpdateLabel(lblCmd1, inst.Sources[g_session.CurClip.CurSrc].On);
            }


            if (move >= 0) lblNext.Mark();
            else           lblPrev.Mark();
        }


        void BackOut()
        {
            g_move = false;

            g_session.CurClip.ParamKeys = false;
            g_session.CurClip.ParamAuto = false;

            var _curSet = false;


            if (g_session.CurClip.CurSet > -1)
            {
                bool ucl = false;

                if (IsCurParam())
                { 
                    CurSetting._IsCurrent = false;
                    if (IsCurParam(strTune)) ucl = true;
                }

                g_session.CurClip.CurSet = -1;
                g_settings.Clear();
                    
                if (ucl)
                { 
                    //UpdateKeyLabels();
                    //UpdateChordLabels();
                    //UpdateShuffleLabel();
                }

                _curSet = true;
            }


            if (     g_session.CurClip.CurSrc > -1
                && !_curSet)
            {
                g_session.CurClip.CurSrc = -1;
                g_session.CurClip.SrcOff =  0;

                g_session.CurClip.Shift  = false;

                UpdateInstName(true);
                g_inputValid = false;
            }
            

            if (     g_session.CurClip.SelChan > -1
                && !_curSet)
            {
                g_session.CurClip.SelChan = -1;

                g_session.CurClip.Shift = false;
                g_move  = false;

                UpdateInstName(false);
            }

            //UpdateAdjustLabels(g_session.CurClip);
            //UpdateNewLabels();
            
            lblBackOut.Mark();

            g_session.SaveInstruments();
        }


        void Back()
        {
            g_move = false;

            if (g_session.CurClip.CurSet > -1)
            {
                if (g_session.CurClip.ParamKeys)
                {
                    g_session.CurClip.ParamKeys = false;
                    g_session.CurClip.EditPos   = fN;
                    //UpdateEditLabel(lblEdit, false);
                }
                else if (g_session.CurClip.ParamAuto)
                {
                    g_session.CurClip.ParamAuto = false;
                    g_session.CurClip.EditPos   = fN;
                    //UpdateEditLabel(lblEdit, false);
                }
                else
                {
                    bool ucl = false;

                    if (IsCurSetting(typeof(Arpeggio)))
                    {
                        CurArpeggio.Clip.EditPos = -Math.Abs(CurArpeggio.Clip.EditPos); // turn off but keep value
                        //UpdateEditLabel(lblEdit, false);
                    }
                    else if (IsCurParam())
                    { 
                        CurSetting._IsCurrent = false;
                        if (IsCurParam(strTune)) ucl = true;
                    }

                    g_session.CurClip.CurSet--;
                    g_settings.RemoveAt(g_settings.Count-1);
                    
                    if (ucl)
                    { 
                        //UpdateKeyLabels();
                        //UpdateChordLabels();
                        //UpdateShuffleLabel();
                    }
                }
            }
            else if (g_session.CurClip.CurSrc > -1)
            {
                g_session.CurClip.CurSrc = -1;
                g_session.CurClip.SrcOff =  0;

                g_session.CurClip.Shift = false;

                UpdateInstName(true);
                g_inputValid = false;
            }
            else if (g_session.CurClip.SelChan > -1)
            {
                g_session.CurClip.SelChan = -1;

                g_session.CurClip.Shift = false;
                g_move  = false;

                g_session.CurClip.EditPos = fN;
                //UpdateEditLabel(lblEdit, false);

                UpdateInstName(false);

                g_session.CurClip.ParamKeys = false;
                g_session.CurClip.ParamAuto = false;
            }

            lblBack.Mark();

            //UpdateNewLabels();
            //UpdateAdjustLabels(g_session.CurClip);

            g_session.SaveInstruments();
        }


        void Enter()
        {
            if (g_session.CurClip.CurSet > -1)
                return;

            g_move = false;

            if (g_session.CurClip.SelChan < 0)
            {
                g_session.CurClip.EditPos = fN;
                //UpdateEditLabel(lblEdit, false);

                g_session.CurClip.SelChan = g_session.CurClip.CurChan;

                //UpdateNewLabels();
                //UpdateAdjustLabels(g_session.CurClip);

                UpdateInstOff(g_session.CurClip.SelChan);

                UpdateInstName(true);
                g_inputValid = false;

                //MarkLabel(lblEnter, g_session.CurClip.CurSrc < 0);
                //UpdateLabel(lblCmd3, false);
            }
            else if (g_session.CurClip.CurSrc < 0)
            {
                g_session.CurClip.CurSrc = 0;

                g_session.CurClip.Shift = false;

                //UpdateNewLabels();
                //UpdateAdjustLabels(g_session.CurClip);

                UpdateInstName(false);

                lblEnter.Mark();
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
                    ModDestConnecting.SrcSettings   .Add(g_session.CurClip.CurSet > -1 ? CurSetting : null);
                    ModDestConnecting.SrcSources    .Add(g_session.CurClip.SelectedSource);
                    ModDestConnecting.SrcInstruments.Add(g_session.CurClip.SelectedInstrument);

                    //g_session.SetClip(ModDestClip);

                    SwitchToSetting(
                        ModDestChannel.Instrument,
                        ModDestSrcIndex, 
                        ModDestConnecting);

                    ResetModConnecting();
                }
            }
            else if (IsCurParam())
            {
                if (OK(g_session.CurClip.EditPos))
                {
                         if (g_session.CurClip.ParamKeys) Interpolate(g_session.CurClip);
                    else if (g_session.CurClip.ParamAuto) EnableKeyMove(g_session.CurClip);
                }
            }
            else if (IsCurSetting(typeof(Modulate)))
            {
                if (ModDestConnecting == null)
                {
                    ModDestConnecting = CurModulate;
                    ModCurChan        = g_session.CurClip.CurChan;
                    ModSelChan        = g_session.CurClip.SelChan;
                    ModCurPat         = g_session.CurClip.CurPat;
                    ModDestSrcIndex   = g_session.CurClip.CurSrc;
                    ModDestChannel    = g_session.CurClip.SelectedChannel;
                    ModDestClip       = g_session.CurClip;

                    //UpdateAdjustLabels(g_session.CurClip);
                }
            }
            else if (IsCurSetting(typeof(Arpeggio))
                  && OK(CurArpeggio.Clip.EditPos))
            {
                     if (g_session.CurClip.ParamKeys) Interpolate  (CurArpeggio.Clip);
                else if (g_session.CurClip.ParamAuto) EnableKeyMove(CurArpeggio.Clip);
            }
            else
            {
                if (g_session.CurClip.CurSrc > -1)
                {
                    var src = g_session.CurClip.SelectedSource;
                    src.On = !src.On;
                    //UpdateLabel(lblCmd1, src.On);
                }
                else
                { 
                    var inst = g_session.CurClip.CurrentInstrument;

                    var src =
                        g_session.CurClip.CurSrc > -1
                        ? inst.Sources[g_session.CurClip.CurSrc]
                        : null;

                    if (g_session.CurClip.SelChan < 0)
                    { 
                        CopyChan(g_session.CurClip, g_session.CurClip.CurPat, g_session.CurClip.CurChan);
                        //MarkLabel(lblCmd1);
                    }

                    //UpdatePasteLabel();
                }
            }
        }


        void Command2()
        {
            if (g_session.CurClip.CurSrc > -1)
            {
                var src = g_session.CurClip.SelectedSource;

                var newOsc = (int)src.Oscillator.Type + 1;
                if (newOsc > (int)OscType.Crunch) newOsc = 0;
                src.Oscillator = OscillatorFromType((OscType)newOsc);
                //MarkLabel(lblCmd2);
            }
            else
            { 
                var src =
                    g_session.CurClip.CurSrc > -1
                    ? g_session.CurClip.CurrentInstrument.Sources[g_session.CurClip.CurSrc]
                    : null;

                if (g_session.CurClip.SelChan < 0)
                {
                    int f, l;
                    g_session.CurClip.GetCurPatterns(out f, out l);

                    for (int p = f; p <= l; p++)
                        PasteChan(g_session.CurClip, p, g_session.CurClip.CurChan);

                    copyChan = null;

                    //MarkLabel(lblCmd2);
                }
            }
        }


        void Command3()
        {
            if (IsCurParam())
            {
                var param = CurParam;
                var path  = g_settings.Last().GetPath(g_session.CurClip.CurSrc);

                
                if (   g_session.CurClip.ParamKeys
                    && OK(g_session.CurClip.EditPos))
                { 
                    var notes = GetEditNotes(g_session.CurClip);

                    foreach (var note in notes)
                    { 
                        var iKey = note.Keys.FindIndex(k => k.Path == path);
                        if (iKey > -1) note.Keys.RemoveAt(iKey);
                    }
                }
                else if (g_session.CurClip.ParamAuto
                      && OK(g_session.CurClip.EditPos))
                {
                    var chan = g_session.CurClip.SelectedChannel;

                    var key = chan.AutoKeys.Find(k =>
                           k.Path == path
                        && k.StepTime >= (g_session.CurClip.EditPos % g_nSteps)
                        && k.StepTime <  (g_session.CurClip.EditPos % g_nSteps) + 1);

                    if (key == null) // create
                    {
                        var val = Parameter.GetAutoValue(g_session.CurClip.EditPos, g_session.CurClip.CurPat, g_session.CurClip.SelChan, path);

                        var newKey = new Key(
                            g_session.CurClip.CurSrc,
                            param,
                            OK(val) ? val : param.Value,
                            g_session.CurClip.EditPos % g_nSteps,
                            g_session.CurClip.SelectedChannel);

                        chan.AutoKeys.Add(newKey);
                        g_session.CurClip.UpdateAutoKeys();
                    }
                    else // delete
                    {
                        chan.AutoKeys.Remove(key);
                        g_session.CurClip.UpdateAutoKeys();
                    }
                }

                //UpdateAdjustLabels(g_session.CurClip);
                //MarkLabel(lblCmd3);
            }
            else if (g_session.CurClip.CurSet > -1)
            {
                if (CurSetting.CanDelete())
                    RemoveSetting(CurSetting);

                //UpdateAdjustLabels(g_session.CurClip);
                //MarkLabel(lblCmd3);
            }
            else if (g_session.CurClip.SelChan < 0)
            { 
                g_session.CurClip.Transpose = !g_session.CurClip.Transpose;
                //UpdateAdjustLabels(g_session.CurClip);
            }
        }


        void Shift()
        {
            g_session.CurClip.Shift = !g_session.CurClip.Shift;
            //UpdateAdjustLabels(g_session.CurClip);
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
                var chan = g_session.CurClip.SelectedChannel;
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
                            note.Keys.Add(new Key(g_session.CurClip.CurSrc, param, param.Value, note.SongStep));
                            AdjustKey(note.Keys.Last(), delta);
                        }
                    }
                }
                else if (g_session.CurClip.ParamAuto)
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

                //UpdateAdjustLabels(clip);
                //MarkLabel(delta >= 0 ? lblUp : lblDown, !clip.Shift);
            }
            else if (clip.SelChan > -1)
            {
                if (IsParam(setting))
                {
                    AdjustParam(clip, (Parameter)setting, delta);
                    //MarkLabel(delta >= 0 ? lblUp : lblDown, !clip.Shift);
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
                AdjustParam(g_session.CurClip, (Parameter)setting, delta);
                //MarkLabel(delta >= 0 ? lblUp : lblDown, !g_session.CurClip.Shift);
                //g_sampleValid = F;
            }
        }


        void AdjustParam(Clip clip, Parameter param, float delta)
        {
            param.SetValue(
                MinMax(
                    param.Min, 
                    param.AdjustValue(param.Value, delta, g_session.CurClip.Shift),
                    param.Max),
                null,
                g_session.CurClip.CurSrc);
        }


        void AdjustKey(Key key, float delta)
        {
            key.Value = MinMax(
                key.Parameter.Min,
                key.Parameter.AdjustValue(key.Value, delta, g_session.CurClip.Shift),
                key.Parameter.Max);
        }


        void SetChan(int ch)
        {
            int first, last;
            g_session.CurClip.GetCurPatterns(out first, out last);

            ch += g_session.CurClip.CurChan;

            for (int p = first; p <= last; p++)
            { 
                var pat = g_session.CurClip.Patterns[p];

                if (ch >= g_nChans) ch = 0;
                if (ch < 0) ch = g_nChans - 1;

                if (g_move)
                {
                    var temp = pat.Channels[g_session.CurClip.CurChan];
                    pat.Channels[g_session.CurClip.CurChan] = pat.Channels[ch];
                    pat.Channels[ch]      = temp;

                    pat.Channels[g_session.CurClip.CurChan].UpdateNotes();
                    pat.Channels[ch]     .UpdateNotes();
                }
            }

            g_session.CurClip.CurChan = ch;

            if (g_session.CurClip.CurSrc > -1)
                g_session.CurClip.CurSrc = 0;

            //UpdateOctaveLabel();
            UpdateInstOff(g_session.CurClip.CurChan);
        }


        void SetShuffle(int ch, int sh)
        {
            sh += g_session.CurClip.CurrentPattern.Channels[ch].Shuffle;

            int first, last;
            g_session.CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            {
                var pat  = g_session.CurClip.Patterns[p];
                var chan = pat.Channels[ch];

                chan.Shuffle = MinMax(0, sh, g_session.TicksPerStep - 1);
            }
        }
    }
}
