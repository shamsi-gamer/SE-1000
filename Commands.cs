using System;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void New()
        {
            if (CurSet > -1)
                return;

            if (CurSrc > -1)
            {
                g_settings.Clear();


                var inst = CurClip.CurInstrument;

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
            }
            else if (SelChan > -1)
            {
                g_settings.Clear();


                var inst = new Instrument();
                inst.Sources.Add(new Source(inst));

                inst.Name = GetNewName(inst.Name, str => g_session.Instruments.Exists(_s => _s.Name == str));

                g_session.Instruments.Insert(g_session.Instruments.IndexOf(CurClip.CurInstrument) + 1, inst);
                SetCurInst(inst);

                //UpdateOctaveLabel();
                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurChan);

                UpdateInstName();
                g_inputValid = F;
            }


            lblNew.Mark();
        }


        void Duplicate()
        {
            if (CurSet > -1)
                return;

            if (CurSrc > -1)
            {
                g_settings.Clear();


                var inst = CurClip.CurInstrument;

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
            }
            else if (SelChan > -1)
            {
                g_settings.Clear();


                var inst = new Instrument(CurClip.CurInstrument);
                inst.Name = GetNewName(inst.Name, newName => g_session.Instruments.Exists(s => s.Name == newName));

                g_session.Instruments.Insert(g_session.Instruments.IndexOf(CurClip.CurInstrument) + 1, inst);
                SetCurInst(inst);
                CurClip.SrcOff = 0;

                //UpdateOctaveLabel();
                UpdateInstName();

                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurChan);
            }


            lblDup.Mark();
        }


        void Delete()
        {
            if (CurSet > -1)
                return;


            if (CurSrc > -1)
            {
                g_settings.Clear();


                var inst = CurClip.CurInstrument;

                inst.Sources[CurSrc].Delete(CurClip);
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
            }
            else if (SelChan > -1)
            {
                g_settings.Clear();


                var i = g_session.Instruments.IndexOf(CurClip.CurInstrument);
                var inst = CurClip.CurInstrument;

                CurClip.CurInstrument.Delete(CurClip);
                g_session.Instruments.Remove(CurClip.CurInstrument);

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

                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurChan);

                UpdateInstName();
            }

            lblDel.Mark();
        }


        public void ToggleMove()
        {
            if (CurSet > -1) 
                return;

            g_move = !g_move;
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
                var i = g_session.Instruments.IndexOf(CurClip.CurInstrument);
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
                        CurClip.Patterns[p].Channels[CurChan].Instrument = g_session.Instruments[n];
                }


                UpdateInstName();
                g_inputValid = F;

                UpdateInstOff(CurChan);

                CurClip.SrcOff = 0;
            }
            else // source
            {
                var inst = CurClip.CurInstrument;
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

                dspMain.Panel.WriteText("", F);

                UpdateSrcOff();
            }


            if (move >= 0) lblNext.Mark();
            else           lblPrev.Mark();
        }


        void BackOut()
        {
            g_move = F;

            CurClip.ParamKeys = F;
            CurClip.ParamAuto = F;

            var _curSet = F;


            if (CurSet > -1)
            {
                bool ucl = F;

                if (IsCurParam())
                { 
                    CurSetting._IsCurrent = F;
                    if (IsCurParam(strTune)) ucl = T;
                }

                CurSet = -1;
                g_settings.Clear();
                    
                if (ucl)
                { 
                    //UpdateKeyLabels();
                    //UpdateChordLabels();
                    //UpdateShuffleLabel();
                }

                _curSet = T;
            }


            if (     CurSrc > -1
                && !_curSet)
            {
                CurSrc = -1;
                CurClip.SrcOff =  0;

                CurClip.Shift  = F;

                UpdateInstName(T);
                g_inputValid = F;
            }
            

            if (     SelChan > -1
                && !_curSet)
            {
                SelChan = -1;

                CurClip.Shift = F;
                g_move  = F;

                UpdateInstName(F);
            }

            lblOut.Mark();

            g_session.SaveInstruments();
        }


        void Back()
        {
            g_move = F;

            if (CurSet > -1)
            {
                if (CurClip.ParamKeys)
                {
                    CurClip.ParamKeys = F;
                    CurClip.EditPos   = fN;
                }
                else if (CurClip.ParamAuto)
                {
                    CurClip.ParamAuto = F;
                    CurClip.EditPos   = fN;
                }
                else
                {
                    if (IsCurSetting(typeof(Arpeggio)))
                        CurArpeggio.Clip.EditPos = -Math.Abs(CurArpeggio.Clip.EditPos); // turn off but keep value
                    else if (IsCurParam())
                        CurSetting._IsCurrent = F;

                    CurSet--;
                    g_settings.RemoveAt(g_settings.Count-1);
                }
            }
            else if (CurSrc > -1)
            {
                CurSrc = -1;
                CurClip.SrcOff =  0;

                CurClip.Shift = F;

                UpdateInstName(T);
                g_inputValid = F;
            }
            else if (SelChan > -1)
            {
                SelChan = -1;

                CurClip.Shift = F;
                g_move        = F;

                CurClip.EditPos = fN;

                UpdateInstName(F);

                CurClip.ParamKeys = F;
                CurClip.ParamAuto = F;
            }

            lblBack.Mark();

            g_session.SaveInstruments();
        }


        void Enter()
        {
            if (CurSet > -1)
                return;

            g_move = F;

            if (SelChan < 0)
            {
                CurClip.EditPos = fN;
                SelChan = CurChan;

                UpdateInstOff(SelChan);

                UpdateInstName(T);
                g_inputValid = F;

                lblEnter.Mark(CurSrc < 0);
            }
            else if (CurSrc < 0)
            {
                CurSrc = 0;
                CurClip.Shift  = F;

                UpdateInstName(F);

                lblEnter.Mark();
            }
        }


        void Command1()
        {
            if (OK(ModDestConnecting))
            {
                if (ModDestConnecting == CurSetting)
                    ResetModConnecting();

                else
                { 
                    ModDestConnecting.SrcSettings   .Add(CurSet > -1 ? CurSetting : null);
                    ModDestConnecting.SrcSources    .Add(SelSource);
                    ModDestConnecting.SrcInstruments.Add(SelInstrument);

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

                    lblCmd1.Mark();
                }
            }
            else if (IsCurSetting(typeof(Modulate)))
            {
                if (!OK(ModDestConnecting))
                {
                    ModDestConnecting = CurModulate;
                    ModCurChan        = CurChan;
                    ModSelChan        = SelChan;
                    ModCurPat         = CurPat;
                    ModDestSrcIndex   = CurSrc;
                    ModDestChannel    = SelChannel;
                    ModDestClip       = CurClip;
                }

                lblCmd1.Mark();
            }
            else if (IsCurSetting(typeof(Arpeggio))
                  && OK(CurArpeggio.Clip.EditPos))
            {
                     if (CurClip.ParamKeys) Interpolate  (CurArpeggio.Clip);
                else if (CurClip.ParamAuto) EnableKeyMove(CurArpeggio.Clip);

                lblCmd1.Mark();
            }
            else
            {
                if (CurSrc > -1)
                {
                    var src = SelSource;
                    src.On = !src.On;
                }
                else
                { 
                    if (SelChan < 0)
                    { 
                        if (g_lockView == 0)
                            g_lockView = ShowPiano ? 2 : 1;
                        else
                            g_lockView = 0;

                        //CopyChan(CurClip, CurPat, CurChan);
                    }
                }
            }
        }


        void Command2()
        {
            if (CurSrc > -1)
            {
                var src = SelSource;

                var newOsc = (int)src.Oscillator.Type + 1;
                if (newOsc > (int)OscType.Crunch) newOsc = 0;
                src.Oscillator = OscillatorFromType((OscType)newOsc);
            }
            else
            { 
                var src =
                    CurSrc > -1
                    ? CurClip.CurInstrument.Sources[CurSrc]
                    : null;

                if (SelChan < 0)
                {
                    int f, l;
                    CurClip.GetCurPatterns(out f, out l);

                    for (int p = f; p <= l; p++)
                        PasteChan(CurClip, p, CurChan);

                    copyChan = null;
                }
            }

            lblCmd2.Mark();
        }


        void Command3()
        {
            if (IsCurParam())
            {
                var param = CurParam;
                var path  = g_settings.Last().GetPath(CurSrc);

                
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
                    var chan = SelChannel;

                    var key = chan.AutoKeys.Find(k =>
                           k.Path == path
                        && k.StepTime >= (CurClip.EditPos % g_patSteps)
                        && k.StepTime <  (CurClip.EditPos % g_patSteps) + 1);

                    if (!OK(key)) // create
                    {
                        var val = Parameter.GetAutoValue(CurClip.EditPos, CurPat, path);

                        var newKey = new Key(
                            CurSrc,
                            param,
                            OK(val) ? val : param.Value,
                            CurClip.EditPos % g_patSteps,
                            SelChannel);

                        chan.AutoKeys.Add(newKey);
                        CurClip.UpdateAutoKeys();
                    }
                    else // delete
                    {
                        chan.AutoKeys.Remove(key);
                        CurClip.UpdateAutoKeys();
                    }
                }
                else if (CurSet > -1)
                {
                    if (CurSetting.CanDelete())
                        DeleteCurSetting();
                }

                lblCmd3.Mark();
            }
            else if (CurSet > -1)
            { 
                if (CurSetting.CanDelete())
                { 
                    DeleteCurSetting();
                    lblCmd3.Mark();
                }
            }
            else if (SelChan < 0)
            { 
                CurClip.Transpose = !CurClip.Transpose;
            }
        }


        void Shift()
        {
            CurClip.Shift = !CurClip.Shift;
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
                var chan = SelChannel;
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
                            note.Keys.Add(new Key(CurSrc, param, param.Value, note.SongStep));
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
                            && k.StepTime >= (clip.EditPos % g_patSteps) 
                            && k.StepTime <  (clip.EditPos % g_patSteps) + 1);

                        if (OK(key))
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
            }
            else if (clip.SelChan > -1)
            {
                if (IsParam(setting))
                    AdjustParam(clip, (Parameter)setting, delta);
            }  
            else
            {
                if (clip.Transpose) 
                    Transpose(clip, clip.CurChan, delta);
            }


            (delta >= 0 ? lblUp : lblDown).Mark(!clip.Shift);
        }


        void AdjustFromController(Clip clip, Setting setting, float delta)
        {
            if (IsParam(setting))
                AdjustParam(clip, (Parameter)setting, delta);
        }


        void AdjustParam(Clip clip, Parameter param, float delta)
        {
            param.SetValue(
                MinMax(
                    param.Min, 
                    param.AdjustValue(param.Value, delta, CurClip.Shift),
                    param.Max),
                null,
                CurSrc);
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

            ch += CurChan; 

            for (int p = first; p <= last; p++)
            { 
                var pat = CurClip.Patterns[p];

                if (ch >= g_nChans) ch = 0;
                if (ch < 0) ch = g_nChans - 1;

                if (g_move)
                {
                    var temp = pat.Channels[CurChan];
                    pat.Channels[CurChan] = pat.Channels[ch];
                    pat.Channels[ch]      = temp;

                    pat.Channels[CurChan].UpdateNotes();
                    pat.Channels[ch]     .UpdateNotes();
                }
            }

            CurChan = ch;

            if (CurSrc > -1)
                CurSrc = 0;

            UpdateInstOff(CurChan);
        }


        void SetShuffle(int ch, int sh)
        {
            sh += CurPattern.Channels[ch].Shuffle;

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
