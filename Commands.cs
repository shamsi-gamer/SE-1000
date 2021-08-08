using System;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void New()
        {
            if (OK(CurSet))
                return;

            if (OK(CurSrc))
            {
                EditedClip.Settings.Clear();


                var inst = EditedClip.CurInstrument;

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
            else if (OK(SelChan))
            {
                EditedClip.Settings.Clear();


                var inst = new Instrument(this);
                inst.Sources.Add(new Source(inst));

                inst.Name = GetNewName(inst.Name, str => Instruments.Exists(i => i.Name == str));
                
                Instruments.Insert(Instruments.IndexOf(EditedClip.CurInstrument) + 1, inst);
                SetCurInst(inst);

                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurChan);

                SetInstName();
                //UpdateInstName();
                //g_inputValid = False;
            }


            lblNew.Mark();
        }



        void Duplicate()
        {
            if (OK(CurSet))
                return;

            if (OK(CurSrc))
            {
                EditedClip.Settings.Clear();


                var inst = EditedClip.CurInstrument;

                if (inst.Sources.Count < 8)
                {
                    //for (int i = g_song.EditedClip.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    var src = new Source(inst.Sources[CurSrc], inst);
                    inst.Sources.Insert(CurSrc+1, src);

                    CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.CurSrc);
                UpdateSrcOff();
            }
            else if (OK(SelChan))
            {
                EditedClip.Settings.Clear();


                var inst = new Instrument(EditedClip.CurInstrument);
                inst.Name = GetNewName(inst.Name, newName => Instruments.Exists(s => s.Name == newName));

                Instruments.Insert(Instruments.IndexOf(EditedClip.CurInstrument) + 1, inst);
                SetCurInst(inst);
                EditedClip.SrcOff = 0;

                SetInstName();

                //UpdateDspOffset(ref instOff, g_song.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(CurChan);
            }


            lblDup.Mark();
        }



        void Delete()
        {
            if (OK(CurSet))
                return;


            if (OK(CurSrc))
            {
                EditedClip.Settings.Clear();


                EditedClip.CurInstrument.Sources[CurSrc].Delete();
                EditedClip.CurInstrument.Sources.RemoveAt(CurSrc);

                //for (int i = g_song.CurSrc; i < inst.Sources.Count; i++)
                //    inst.Sources[i].Index--;

                if (EditedClip.CurInstrument.Sources.Count == 0)
                {
                    EditedClip.CurInstrument.Sources.Add(new Source(EditedClip.CurInstrument));
                    CurSrc = 0;
                }

                if (CurSrc >= EditedClip.CurInstrument.Sources.Count)
                    CurSrc =  EditedClip.CurInstrument.Sources.Count - 1;

                //UpdateDspOffset(ref srcOff, g_song.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.CurSrc);
                UpdateSrcOff();
            }
            else if (OK(SelChan))
            {
                EditedClip.Settings.Clear();


                var i = Instruments.IndexOf(EditedClip.CurInstrument);

                EditedClip.CurInstrument.Delete();
                Instruments.Remove(EditedClip.CurInstrument);

                if (Instruments.Count == 0)
                {
                    Instruments.Add(new Instrument(this));
                    Instruments[0].Sources.Add(new Source(Instruments[0]));
                }

                i = MinMax(0, i - 1, Instruments.Count - 1);

                foreach (var p in EditedClip.Patterns)
                    foreach (var c in p.Channels)
                        if (c.Instrument == EditedClip.CurInstrument) c.Instrument = Instruments[i];

                EditedClip.SrcOff = 0;

                UpdateInstOff(CurChan);

                SetInstName();
            }

            lblDel.Mark();
        }



        void ToggleShowMixer()
        {
            ShowMixer++;
            
            if (ShowMixer > 2)
                ShowMixer = 0;
        }



        void ToggleShowClip()
        {
            ShowClip = !ShowClip;
            
            if (!ShowClip) 
                HideClip = true;

            dspInfo.Panel.WriteText(ShowClip ? EditedClip.Name : SessionName);
        }



        void ToggleCueClip()
        {
            CueClip++;
            
            if (CueClip > 2)
                CueClip = 0;


            // just in case some stuff was cued,
            // start playing it if turning off cue
            if (CueClip == 0)
            {
                foreach (var track in Tracks)
                {
                    if (    OK(track.NextClip)
                        && !OK(track.PlayClip))
                        track.NextClip = -1;
                }
            }
        }



        void ToggleMove()
        {
            if (OK(CurSet)) 
                return;

            EditedClip.Move = !EditedClip.Move;
        }



        void MoveChan(int move)
        {
            if (OK(CurSet))
                return;


            if (SelChan < 0)
            {
                SetChan(-move);
            }
            else if (CurSrc < 0) // instrument
            {
                var i = Instruments.IndexOf(EditedClip.CurInstrument);
                var n = i + move;

                if (n >= Instruments.Count) n = 0;
                if (n < 0)                  n = Instruments.Count - 1;


                if (EditedClip.Move)
                {
                    var inst = Instruments[i];

                    Instruments.RemoveAt(i);
                    Instruments.Insert(n, inst);
                }
                else // change instrument
                    SetCurInst(Instruments[n]);


                SetInstName();
                g_inputValid = False;

                UpdateInstOff(CurChan);

                EditedClip.SrcOff = 0;
            }
            else // source
            {
                var inst = EditedClip.CurInstrument;
                var next = CurSrc + move;

                if (next >= inst.Sources.Count) next = 0;
                if (next < 0) next = inst.Sources.Count - 1;

                if (EditedClip.Move)
                {
                    var src = inst.Sources[CurSrc];
                    inst.Sources.RemoveAt(CurSrc);
                    inst.Sources.Insert(next, src);
                }

                CurSrc = next;

                dspMain.Panel.WriteText("");

                UpdateSrcOff();
            }


            if (move >= 0) lblNext.Mark();
            else           lblPrev.Mark();
        }



        static void SetInstrument(Channel chan, Instrument oldInst, Instrument newInst)
        {
            chan.Instrument = newInst;

                        
            // update keys

            foreach (var note in chan.Notes)
            {
                foreach (var key in note.Keys)
                {
                    if (key.Parameter.Instrument == oldInst)
                        key.Parameter.Instrument = newInst;
                }
            }

            foreach (var key in chan.AutoKeys)
            {
                if (key.Parameter.Instrument == oldInst)
                    key.Parameter.Instrument = newInst;
            }
        }



        static void BackOut()
        {
            EditedClip.Move      = False;

            EditedClip.ParamKeys = False;
            EditedClip.ParamAuto = False;


            if (OK(CurSet))
            {
                if (IsCurParam())
                    EditedClip.CurSetting._IsCurrent = False;

                CurSet = -1;
                EditedClip.Settings.Clear();
            }


            if (OK(CurSrc))
            {
                CurSrc = -1;
                EditedClip.SrcOff =  0;

                EditedClip.Shift  = False;

                SetInstName(True);
                g_inputValid = False;
            }
            

            if (OK(SelChan))
            {
                SelChan = -1;

                EditedClip.Shift = False;
                EditedClip.Move  = False;

                SetInstName(False);
            }

            lblOut.Mark();

            //SaveInstruments();
        }



        void Back()
        {
            EditedClip.Move = False;

            if (OK(CurSet))
            {
                if (EditedClip.ParamKeys)
                {
                    EditedClip.ParamKeys = False;
                    EditedClip.EditPos   = float_NaN;
                }
                else if (EditedClip.ParamAuto)
                {
                    EditedClip.ParamAuto = False;
                    EditedClip.EditPos   = float_NaN;
                }
                else
                {
                    if (IsCurParam())
                        EditedClip.CurSetting._IsCurrent = False;

                    CurSet--;

                    EditedClip.Settings.RemoveAt(EditedClip.Settings.Count-1);
                }
            }
            else if (OK(CurSrc))
            {
                CurSrc = -1;
                EditedClip.SrcOff =  0;

                EditedClip.Shift = False;

                SetInstName(True);
                g_inputValid = False;
            }
            else if (OK(SelChan))
            {
                SelChan = -1;

                EditedClip.Shift = False;
                EditedClip.Move  = False;

                EditedClip.EditPos = float_NaN;

                SetInstName(False);

                EditedClip.ParamKeys = False;
                EditedClip.ParamAuto = False;
            }

            lblBack.Mark();

            //SaveInstruments();
        }



        void Enter()
        {
            if (OK(CurSet))
                return;

            EditedClip.Move = False;

            if (SelChan < 0)
            {
                EditedClip.EditPos = float_NaN;
                SelChan = CurChan;

                UpdateInstOff(SelChan);

                SetInstName(True);
                g_inputValid = False;

                lblEnter.Mark(CurSrc < 0);
            }
            else if (CurSrc < 0)
            {
                CurSrc = 0;
                EditedClip.Shift  = False;

                SetInstName(False);

                lblEnter.Mark();
            }
        }



        void Command1()
        {
            if (OK(ModDestConnecting))
            {
                if (ModDestConnecting != EditedClip.CurSetting)
                { 
                    ModDestConnecting.ModSettings   .Add(OK(CurSet) ? EditedClip.CurSetting : Setting_null);
                    ModDestConnecting.ModSources    .Add(SelSource);
                    ModDestConnecting.ModInstruments.Add(SelInstrument);

                    SwitchToSetting(
                        ModDestClip,
                        ModDestConnecting);
                }
             
                ResetModConnecting();
            }
            else if (IsCurParam())
            {
                if (OK(EditedClip.EditPos))
                {
                    if (EditedClip.ParamKeys)
                    {
                        Interpolate(EditedClip);
                        lblCmd1.Mark();
                    }
                    else if (EditedClip.ParamAuto) 
                        EnableKeyMove(EditedClip);
                }
            }
            else if (IsCurSetting(typeof(Modulate)))
            {
                if (!OK(ModDestConnecting))
                {
                    ModDestConnecting = EditedClip.CurModulate;
                    ModCurChan        = CurChan;
                    ModSelChan        = SelChan;
                    ModCurPat         = EditPat;
                    ModDestSrcIndex   = CurSrc;
                    ModDestChannel    = SelChannel;
                    ModDestClip       = EditedClip;
                }

                lblCmd1.Mark();
            }
            else
            {
                if (OK(CurSrc))
                {
                    var src = SelSource;
                    src.On = !src.On;
                }
                else
                { 
                    if (!OK(SelChan))
                    { 
                        if (LockView == 0)
                            LockView = ShowPiano ? 2 : 1;
                        else
                            LockView = 0;
                    }
                }
            }
        }



        void Command2()
        {
            if (OK(CurSrc))
            {
                var src = SelSource;

                var newOsc = (int)src.Oscillator.Type + 1;
                if (newOsc > (int)OscType.Crunch) newOsc = 0;
                src.Oscillator = OscillatorFromType((OscType)newOsc);
            }
            else if (!OK(SelChan)
                   && EditedClip.RndInst
                   && OK(Array.Find(EditPattern.Channels, c => c.Instrument == CurChannel.Instrument)))
                CollapseChannels();


            lblCmd2.Mark();
        }



        void Command3()
        {
            if (IsCurParam())
            {
                var param = EditedClip.CurParam;
                var path  = param.Path;
                //var path  = EditedClip.Settings.Last().Path(CurSrc);

                
                if (   EditedClip.ParamKeys
                    && OK(EditedClip.EditPos))
                { 
                    var notes = GetEditNotes(EditedClip);

                    foreach (var note in notes)
                    { 
                        var iKey = note.Keys.FindIndex(k => k.Path == path);
                        if (OK(iKey)) note.Keys.RemoveAt(iKey);
                    }
                }
                else if (EditedClip.ParamAuto
                      && OK(EditedClip.EditPos))
                {
                    AddDeleteAutoKey(param, path);
                }
                else if (OK(CurSet))
                {
                    if (EditedClip.CurSetting.CanDelete())
                        DeleteCurSetting(EditedClip);
                }

                lblCmd3.Mark();
            }
            else if (OK(CurSet))
            { 
                if (   IsCurSetting(typeof(Modulate))
                    && ((Modulate)CurSetting).ModSettings.Count > 0)
                {
                    ((Modulate)CurSetting).ModSettings   .RemoveLast();
                    ((Modulate)CurSetting).ModSources    .RemoveLast();
                    ((Modulate)CurSetting).ModInstruments.RemoveLast();
                }
                else if (EditedClip.CurSetting.CanDelete())
                { 
                    DeleteCurSetting(EditedClip);
                    lblCmd3.Mark();
                }
            }
            else if (SelChan < 0)
            { 
                EditedClip.Transpose = !EditedClip.Transpose;
            }
        }



        public void AddDeleteAutoKey(Parameter param, string path)
        {
            var key = SelChannel.AutoKeys.Find(k =>
                   k.Path == path
                && k.Step >= (EditedClip.EditPos % g_patSteps)
                && k.Step <  (EditedClip.EditPos % g_patSteps) + 1);

            if (OK(key)) // delete
            {
                SelChannel.AutoKeys.Remove(key);
                EditedClip.UpdateAutoKeys();
            }

            if (  !OK(key)
                && OK(EditedClip.EditPos)) // create
            {
                var val = Parameter.GetAutoValue(EditedClip, EditedClip.EditPos, EditPat, path);

                var newKey = new Key(
                    CurSrc,
                    param,
                    OK(val) ? val : param.Value,
                    EditedClip.EditPos % g_patSteps,
                    SelChannel);

                SelChannel.AutoKeys.Add(newKey);
                EditedClip.UpdateAutoKeys();
            }
        }



        static void RecordAutoKey(Parameter param, string path)
        {
            var playStep = (float)Math.Round(EditedClip.Track.PlayStep);
            var playChan = EditedClip.Patterns[EditedClip.Track.PlayPat].Channels[CurChan];

            var val      = Parameter.GetAutoValue(EditedClip, playStep, EditedClip.Track.PlayPat, path);


            if (   OK(playChan.Notes.Find(n => 
                          n.Step >= (playStep % g_patSteps)
                       && n.Step <  (playStep % g_patSteps) + 1))
                && param.Value != val)
            { 
                var key = playChan.AutoKeys.Find(k =>
                       k.Path == path
                    && k.Step >= (playStep % g_patSteps)
                    && k.Step <  (playStep % g_patSteps) + 1);
                            
                if (OK(key)) // delete
                    playChan.AutoKeys.Remove(key);
                            
                var newKey = new Key(
                    CurSrc,
                    param,
                    param.Value,
                    playStep % g_patSteps,
                    playChan);

                playChan.AutoKeys.Add(newKey);
                EditedClip.UpdateAutoKeys();
            }
        }



        public void ToggleShift()
        {
            EditedClip.Shift = !EditedClip.Shift;
        }



        public void Adjust(Clip clip, Setting setting, float delta)
        {
            if (IsSettingType(setting, typeof(Harmonics)))
            {
                clip.CurHarmonics.Adjust(delta);
            }
            else if (   IsParam(setting)
                        && (   clip.ParamKeys 
                            || clip.ParamAuto))
            {
                var chan = clip.SelChannel;
                var path = clip.CurSetting.Path;

                if (clip.ParamKeys)
                { 
                    var notes = GetEditNotes(clip);

                    foreach (var note in notes)
                    { 
                        var iKey = note.Keys.FindIndex(k => k.Path == path);

                        if (OK(iKey))
                            AdjustKey(note.Keys[iKey], delta);
                        else
                        {
                            var param = (Parameter)GetSettingFromPath(path);
                            note.Keys.Add(new Key(clip.CurSrc, param, param.Value, note.ClipStep));
                            AdjustKey(note.Keys.Last(), delta);
                        }
                    }
                }
                else if (clip.ParamAuto)
                {
                    if (OK(clip.EditPos))
                    { 
                        var key = chan.AutoKeys.Find(
                               k => k.Path == path
                            && k.Step >= (clip.EditPos % g_patSteps) 
                            && k.Step <  (clip.EditPos % g_patSteps) + 1);

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
            else if (OK(clip.SelChan))
            {
                if (IsParam(setting))
                    AdjustParam(clip, (Parameter)setting, delta);
            }  
            else
            {
                if (clip.Transpose) 
                    Transpose(clip, delta);
            }


            (delta >= 0 ? lblUp : lblDown).Mark(!clip.Shift);
        }



        static void AdjustFromController(Clip clip, Setting setting, float delta)
        {
            if (!IsParam(setting)) return;
            var param = (Parameter)setting;

            AdjustParam(clip, param, delta);

            if (   Recording
                && clip.Track.PlayTime % TicksPerStep == 0) // only once per tick, at the start of the tick
                RecordAutoKey(param, param.Path);
        }



        static void AdjustParam(Clip clip, Parameter param, float delta)
        {
            param.SetValue(
                MinMax(
                    param.Min, 
                    param.AdjustValue(param.Value, delta, clip.Shift),
                    param.Max),
                Note_null);
        }



        void AdjustKey(Key key, float delta)
        {
            key.Value = MinMax(
                key.Parameter.Min,
                key.Parameter.AdjustValue(key.Value, delta, EditedClip.Shift),
                key.Parameter.Max);
        }



        void SetChan(int ch)
        {
            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            ch += CurChan; 

            for (int p = first; p <= last; p++)
            { 
                var pat = EditedClip.Patterns[p];

                if (ch >= g_nChans) ch = 0;
                if (ch < 0) ch = g_nChans - 1;

                if (EditedClip.Move)
                {
                    var temp = pat.Channels[CurChan];
                    pat.Channels[CurChan] = pat.Channels[ch];
                    pat.Channels[ch]                 = temp;

                    pat.Channels[CurChan].UpdateNotes();
                    pat.Channels[ch]                .UpdateNotes();
                }
            }

            CurChan = ch;

            if (OK(CurSrc))
                CurSrc = 0;

            UpdateInstOff(CurChan);
        }



        static void SetShuffle(int ch, int sh)
        {
            sh += EditPattern.Channels[ch].Shuffle;

            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            {
                var pat  = EditedClip.Patterns[p];
                var chan = pat.Channels[ch];

                chan.Shuffle = MinMax(0, sh, TicksPerStep - 1);
            }
        }



        static void Copy()
        {
            g_copyChans.Clear();

            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            {
                var pat  = EditedClip.Patterns[p];
                var chan = pat.Channels[CurChan];

                g_copyChans.Add(new Channel(chan));
            }
        }
        


        static void Paste()
        {
            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            if (   EditedClip.AllPats
                || EditedClip.Block)
            {
                for (int p = 0; p < Math.Min(last-first+1, EditedClip.Patterns.Count); p++)
                    PasteChan(first + p, first + p);
            }
            else
            {
                for (int p = 0; p < Math.Min(last-first+1, g_copyChans.Count); p++)
                    PasteChan(p, first + p);
            }
        }



        static void PasteChan(int srcPat, int dstPat)
        { 
            PasteChan(
                g_copyChans[srcPat % g_copyChans.Count],
                EditedClip.Patterns[dstPat].Channels[CurChan]);
        }



        static void PasteChan(Channel srcChan, Channel dstChan)
        { 
            if (EditedClip.RndInst)
                dstChan.Instrument = srcChan.Instrument;

            foreach (var note in srcChan.Notes)
            { 
                if (!OK(dstChan.Notes.Find(n => 
                       n.Step   == note.Step 
                    && n.Number == note.Number)))
                    dstChan.Notes.Add(new Note(note, dstChan, dstChan.Index));
            }

            foreach (var key in srcChan.AutoKeys)
                dstChan.AutoKeys.Add(new Key(key));
        }
    }
}
