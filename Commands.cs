using System;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void New()
        {
            if (CurClip.CurSrc > -1)
            {
                CurClip.CurSet = -1;
                g_settings.Clear();


                var inst = CurClip.CurrentInstrument;

                if (inst.Sources.Count < maxDspSrc)
                { 
                    //for (int i = g_song.CurClip.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    inst.Sources.Insert(CurClip.CurSrc+1, new Source(inst));

                    CurClip.CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.CurClip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.CurClip.CurSrc);
                UpdateSrcOff();
            }
            else if (CurClip.SelChan > -1)
            {
                CurClip.CurSet = -1;
                g_settings.Clear();


                var inst = new Instrument();
                inst.Sources.Add(new Source(inst));

                inst.Name = GetNewName(inst.Name, str => g_session.Instruments.Exists(_s => _s.Name == str));

                g_session.Instruments.Insert(g_session.Instruments.IndexOf(CurClip.CurrentInstrument) + 1, inst);
                SetCurInst(inst);

                //UpdateOctaveLabel();
                //UpdateDspOffset(ref instOff, g_song.CurClip.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurClip.CurChan);

                UpdateInstName();
                g_inputValid = false;
            }


            lblNew.Mark();
        }


        void Duplicate()
        {
            if (CurClip.CurSrc > -1)
            {
                CurClip.CurSet = -1;
                g_settings.Clear();


                var inst = CurClip.CurrentInstrument;

                if (inst.Sources.Count < 8)
                {
                    //for (int i = g_song.CurClip.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    var src = new Source(inst.Sources[CurClip.CurSrc], inst);
                    inst.Sources.Insert(CurClip.CurSrc+1, src);

                    CurClip.CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.CurClip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.CurClip.CurSrc);
                UpdateSrcOff();
            }
            else if (CurClip.SelChan > -1)
            {
                CurClip.CurSet = -1;
                g_settings.Clear();


                var inst = new Instrument(CurClip.CurrentInstrument);
                inst.Name = GetNewName(inst.Name, newName => g_session.Instruments.Exists(s => s.Name == newName));

                g_session.Instruments.Insert(g_session.Instruments.IndexOf(CurClip.CurrentInstrument) + 1, inst);
                SetCurInst(inst);
                CurClip.SrcOff = 0;

                //UpdateOctaveLabel();
                UpdateInstName();

                //UpdateDspOffset(ref instOff, g_song.CurClip.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurClip.CurChan);
            }


            lblDup.Mark();
        }


        void Delete()
        {
            if (CurClip.CurSrc > -1)
            {
                CurClip.CurSet = -1;
                g_settings.Clear();


                var inst = CurClip.CurrentInstrument;

                inst.Sources[CurClip.CurSrc].Delete(CurClip);
                inst.Sources.RemoveAt(CurClip.CurSrc);

                //for (int i = g_song.CurClip.CurSrc; i < inst.Sources.Count; i++)
                //    inst.Sources[i].Index--;

                if (inst.Sources.Count == 0)
                {
                    inst.Sources.Add(new Source(inst));
                    CurClip.CurSrc = 0;
                }

                if (CurClip.CurSrc >= inst.Sources.Count)
                    CurClip.CurSrc = inst.Sources.Count - 1;

                //UpdateDspOffset(ref srcOff, g_song.CurClip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.CurClip.CurSrc);
                UpdateSrcOff();

                //g_sampleValid = F; 
            }
            else if (CurClip.SelChan > -1)
            {
                CurClip.CurSet = -1;
                g_settings.Clear();


                var i = g_session.Instruments.IndexOf(CurClip.CurrentInstrument);
                var inst = CurClip.CurrentInstrument;

                CurClip.CurrentInstrument.Delete(CurClip);
                g_session.Instruments.Remove(CurClip.CurrentInstrument);

                if (g_session.Instruments.Count == 0)
                {
                    g_session.Instruments.Add(new Instrument());
                    g_session.Instruments[0].Sources.Add(new Source(g_session.Instruments[0]));
                }

                i = MinMax(0, i - 1, g_session.Instruments.Count - 1);

                foreach (var p in CurClip.Patterns)
                    foreach (var c in p.Channels)
                        if (c.Instrument == inst) c.Instrument = g_session.Instruments[i];

                CurClip.SrcOff = 0;

                //UpdateDspOffset(ref instOff, g_song.CurClip.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurClip.CurChan);

                //UpdateOctaveLabel();
                UpdateInstName();

                //g_sampleValid = F; 
            }

            lblDelete.Mark();
        }


        public void ToggleMove()
        {
            if (CurClip.CurSet > -1) return;

            g_move = !g_move;
        }


        void Move(int move)
        {
            if (CurClip.CurSet > -1)
                return;


            if (CurClip.SelChan < 0)
            {
                SetChan(-move);
            }
            else if (CurClip.CurSrc < 0) // instrument
            {
                var i = g_session.Instruments.IndexOf(CurClip.CurrentInstrument);
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
                    CurClip.GetCurPatterns(out first, out last);

                    for (int p = first; p <= last; p++)
                        CurClip.Patterns[p].Channels[CurClip.CurChan].Instrument = g_session.Instruments[n];

                    //UpdateOctaveLabel();
                }


                UpdateInstName();
                g_inputValid = false;

                UpdateInstOff(CurClip.CurChan);

                CurClip.SrcOff = 0;
            }
            else // source
            {
                var inst = CurClip.CurrentInstrument;
                var next = CurClip.CurSrc + move;

                if (next >= inst.Sources.Count) next = 0;
                if (next < 0) next = inst.Sources.Count - 1;

                if (g_move)
                {
                    var src = inst.Sources[CurClip.CurSrc];
                    inst.Sources.RemoveAt(CurClip.CurSrc);
                    inst.Sources.Insert(next, src);
                }

                CurClip.CurSrc = next;

                dspMain.Panel.WriteText("", false);

                UpdateSrcOff();

                //UpdateLabel(lblCmd1, "On", 10, 10);
                //UpdateLabel(lblCmd1, inst.Sources[CurClip.CurSrc].On);
            }


            if (move >= 0) lblNext.Mark();
            else           lblPrev.Mark();
        }


        void BackOut()
        {
            g_move = false;

            CurClip.ParamKeys = false;
            CurClip.ParamAuto = false;

            var _curSet = false;


            if (CurClip.CurSet > -1)
            {
                bool ucl = false;

                if (IsCurParam())
                { 
                    CurSetting._IsCurrent = false;
                    if (IsCurParam(strTune)) ucl = true;
                }

                CurClip.CurSet = -1;
                g_settings.Clear();
                    
                if (ucl)
                { 
                    //UpdateKeyLabels();
                    //UpdateChordLabels();
                    //UpdateShuffleLabel();
                }

                _curSet = true;
            }


            if (     CurClip.CurSrc > -1
                && !_curSet)
            {
                CurClip.CurSrc = -1;
                CurClip.SrcOff =  0;

                CurClip.Shift  = false;

                UpdateInstName(true);
                g_inputValid = false;
            }
            

            if (     CurClip.SelChan > -1
                && !_curSet)
            {
                CurClip.SelChan = -1;

                CurClip.Shift = false;
                g_move  = false;

                UpdateInstName(false);
            }

            //UpdateAdjustLabels(CurClip);
            //UpdateNewLabels();
            
            lblOut.Mark();

            g_session.SaveInstruments();
        }


        void Back()
        {
            g_move = false;

            if (CurClip.CurSet > -1)
            {
                if (CurClip.ParamKeys)
                {
                    CurClip.ParamKeys = false;
                    CurClip.EditPos   = fN;
                    //UpdateEditLabel(lblEdit, false);
                }
                else if (CurClip.ParamAuto)
                {
                    CurClip.ParamAuto = false;
                    CurClip.EditPos   = fN;
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

                    CurClip.CurSet--;
                    g_settings.RemoveAt(g_settings.Count-1);
                    
                    if (ucl)
                    { 
                        //UpdateKeyLabels();
                        //UpdateChordLabels();
                        //UpdateShuffleLabel();
                    }
                }
            }
            else if (CurClip.CurSrc > -1)
            {
                CurClip.CurSrc = -1;
                CurClip.SrcOff =  0;

                CurClip.Shift = false;

                UpdateInstName(true);
                g_inputValid = false;
            }
            else if (CurClip.SelChan > -1)
            {
                CurClip.SelChan = -1;

                CurClip.Shift = false;
                g_move  = false;

                CurClip.EditPos = fN;
                //UpdateEditLabel(lblEdit, false);

                UpdateInstName(false);

                CurClip.ParamKeys = false;
                CurClip.ParamAuto = false;
            }

            lblBack.Mark();

            //UpdateNewLabels();
            //UpdateAdjustLabels(CurClip);

            g_session.SaveInstruments();
        }


        void Enter()
        {
            if (CurClip.CurSet > -1)
                return;

            g_move = false;

            if (CurClip.SelChan < 0)
            {
                CurClip.EditPos = fN;
                //UpdateEditLabel(lblEdit, false);

                CurClip.SelChan = CurClip.CurChan;

                //UpdateNewLabels();
                //UpdateAdjustLabels(CurClip);

                UpdateInstOff(CurClip.SelChan);

                UpdateInstName(true);
                g_inputValid = false;

                //MarkLabel(lblEnter, CurClip.CurSrc < 0);
                //UpdateLabel(lblCmd3, false);
            }
            else if (CurClip.CurSrc < 0)
            {
                CurClip.CurSrc = 0;

                CurClip.Shift = false;

                //UpdateNewLabels();
                //UpdateAdjustLabels(CurClip);

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
                    ModDestConnecting.SrcSettings   .Add(CurClip.CurSet > -1 ? CurSetting : null);
                    ModDestConnecting.SrcSources    .Add(CurClip.SelectedSource);
                    ModDestConnecting.SrcInstruments.Add(CurClip.SelectedInstrument);

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
                if (OK(CurClip.EditPos))
                {
                         if (CurClip.ParamKeys) Interpolate(CurClip);
                    else if (CurClip.ParamAuto) EnableKeyMove(CurClip);
                }
            }
            else if (IsCurSetting(typeof(Modulate)))
            {
                if (ModDestConnecting == null)
                {
                    ModDestConnecting = CurModulate;
                    ModCurChan        = CurClip.CurChan;
                    ModSelChan        = CurClip.SelChan;
                    ModCurPat         = CurClip.CurPat;
                    ModDestSrcIndex   = CurClip.CurSrc;
                    ModDestChannel    = CurClip.SelectedChannel;
                    ModDestClip       = CurClip;

                    //UpdateAdjustLabels(CurClip);
                }
            }
            else if (IsCurSetting(typeof(Arpeggio))
                  && OK(CurArpeggio.Clip.EditPos))
            {
                     if (CurClip.ParamKeys) Interpolate  (CurArpeggio.Clip);
                else if (CurClip.ParamAuto) EnableKeyMove(CurArpeggio.Clip);
            }
            else
            {
                if (CurClip.CurSrc > -1)
                {
                    var src = CurClip.SelectedSource;
                    src.On = !src.On;
                    //UpdateLabel(lblCmd1, src.On);
                }
                else
                { 
                    var inst = CurClip.CurrentInstrument;

                    var src =
                        CurClip.CurSrc > -1
                        ? inst.Sources[CurClip.CurSrc]
                        : null;

                    if (CurClip.SelChan < 0)
                    { 
                        CopyChan(CurClip, CurClip.CurPat, CurClip.CurChan);
                        //MarkLabel(lblCmd1);
                    }

                    //UpdatePasteLabel();
                }
            }
        }


        void Command2()
        {
            if (CurClip.CurSrc > -1)
            {
                var src = CurClip.SelectedSource;

                var newOsc = (int)src.Oscillator.Type + 1;
                if (newOsc > (int)OscType.Crunch) newOsc = 0;
                src.Oscillator = OscillatorFromType((OscType)newOsc);
                //MarkLabel(lblCmd2);
            }
            else
            { 
                var src =
                    CurClip.CurSrc > -1
                    ? CurClip.CurrentInstrument.Sources[CurClip.CurSrc]
                    : null;

                if (CurClip.SelChan < 0)
                {
                    int f, l;
                    CurClip.GetCurPatterns(out f, out l);

                    for (int p = f; p <= l; p++)
                        PasteChan(CurClip, p, CurClip.CurChan);

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
                var path  = g_settings.Last().GetPath(CurClip.CurSrc);

                
                if (   CurClip.ParamKeys
                    && OK(CurClip.EditPos))
                { 
                    var notes = GetEditNotes(CurClip);

                    foreach (var note in notes)
                    { 
                        var iKey = note.Keys.FindIndex(k => k.Path == path);
                        if (iKey > -1) note.Keys.RemoveAt(iKey);
                    }
                }
                else if (CurClip.ParamAuto
                      && OK(CurClip.EditPos))
                {
                    var chan = CurClip.SelectedChannel;

                    var key = chan.AutoKeys.Find(k =>
                           k.Path == path
                        && k.StepTime >= (CurClip.EditPos % g_nSteps)
                        && k.StepTime <  (CurClip.EditPos % g_nSteps) + 1);

                    if (key == null) // create
                    {
                        var val = Parameter.GetAutoValue(CurClip.EditPos, CurClip.CurPat, CurClip.SelChan, path);

                        var newKey = new Key(
                            CurClip.CurSrc,
                            param,
                            OK(val) ? val : param.Value,
                            CurClip.EditPos % g_nSteps,
                            CurClip.SelectedChannel);

                        chan.AutoKeys.Add(newKey);
                        CurClip.UpdateAutoKeys();
                    }
                    else // delete
                    {
                        chan.AutoKeys.Remove(key);
                        CurClip.UpdateAutoKeys();
                    }
                }

                //UpdateAdjustLabels(CurClip);
                //MarkLabel(lblCmd3);
            }
            else if (CurClip.CurSet > -1)
            {
                if (CurSetting.CanDelete())
                    RemoveSetting(CurSetting);

                //UpdateAdjustLabels(CurClip);
                //MarkLabel(lblCmd3);
            }
            else if (CurClip.SelChan < 0)
            { 
                CurClip.Transpose = !CurClip.Transpose;
                //UpdateAdjustLabels(CurClip);
            }
        }


        void Shift()
        {
            CurClip.Shift = !CurClip.Shift;
            //UpdateAdjustLabels(CurClip);
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
                var chan = CurClip.SelectedChannel;
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
                            note.Keys.Add(new Key(CurClip.CurSrc, param, param.Value, note.SongStep));
                            AdjustKey(note.Keys.Last(), delta);
                        }
                    }
                }
                else if (CurClip.ParamAuto)
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
                AdjustParam(CurClip, (Parameter)setting, delta);
                //MarkLabel(delta >= 0 ? lblUp : lblDown, !CurClip.Shift);
                //g_sampleValid = F;
            }
        }


        void AdjustParam(Clip clip, Parameter param, float delta)
        {
            param.SetValue(
                MinMax(
                    param.Min, 
                    param.AdjustValue(param.Value, delta, CurClip.Shift),
                    param.Max),
                null,
                CurClip.CurSrc);
        }


        void AdjustKey(Key key, float delta)
        {
            key.Value = MinMax(
                key.Parameter.Min,
                key.Parameter.AdjustValue(key.Value, delta, CurClip.Shift),
                key.Parameter.Max);
        }


        void SetChan(int ch)
        {
            int first, last;
            CurClip.GetCurPatterns(out first, out last);

            ch += CurClip.CurChan;

            for (int p = first; p <= last; p++)
            { 
                var pat = CurClip.Patterns[p];

                if (ch >= g_nChans) ch = 0;
                if (ch < 0) ch = g_nChans - 1;

                if (g_move)
                {
                    var temp = pat.Channels[CurClip.CurChan];
                    pat.Channels[CurClip.CurChan] = pat.Channels[ch];
                    pat.Channels[ch]      = temp;

                    pat.Channels[CurClip.CurChan].UpdateNotes();
                    pat.Channels[ch]     .UpdateNotes();
                }
            }

            CurClip.CurChan = ch;

            if (CurClip.CurSrc > -1)
                CurClip.CurSrc = 0;

            //UpdateOctaveLabel();
            UpdateInstOff(CurClip.CurChan);
        }


        void SetShuffle(int ch, int sh)
        {
            sh += CurClip.CurrentPattern.Channels[ch].Shuffle;

            int first, last;
            CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            {
                var pat  = CurClip.Patterns[p];
                var chan = pat.Channels[ch];

                chan.Shuffle = MinMax(0, sh, g_session.TicksPerStep - 1);
            }
        }
    }
}
