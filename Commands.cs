using System;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void New()
        {
            if (OK(EditedClip.CurSet))
                return;

            if (OK(EditedClip.CurSrc))
            {
                EditedClip.Settings.Clear();


                var inst = EditedClip.CurInstrument;

                if (inst.Sources.Count < maxDspSrc)
                { 
                    //for (int i = g_song.EditedClip.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    inst.Sources.Insert(EditedClip.CurSrc+1, new Source(inst));

                    EditedClip.CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.EditedClip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.EditedClip.CurSrc);
                UpdateSrcOff();
            }
            else if (OK(EditedClip.SelChan))
            {
                EditedClip.Settings.Clear();


                var inst = new Instrument();
                inst.Sources.Add(new Source(inst));

                inst.Name = GetNewName(inst.Name, str => Instruments.Exists(_s => _s.Name == str));

                Instruments.Insert(Instruments.IndexOf(EditedClip.CurInstrument) + 1, inst);
                SetCurInst(inst);

                //UpdateDspOffset(ref instOff, g_song.EditedClip.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(EditedClip.CurChan);

                UpdateInstName();
                g_inputValid = False;
            }


            lblNew.Mark();
        }


        void Duplicate()
        {
            if (OK(EditedClip.CurSet))
                return;

            if (OK(EditedClip.CurSrc))
            {
                EditedClip.Settings.Clear();


                var inst = EditedClip.CurInstrument;

                if (inst.Sources.Count < 8)
                {
                    //for (int i = g_song.EditedClip.EditedClip.CurSrc+1; i < inst.Sources.Count; i++)
                    //    inst.Sources[i].Index++;

                    var src = new Source(inst.Sources[EditedClip.CurSrc], inst);
                    inst.Sources.Insert(EditedClip.CurSrc+1, src);

                    EditedClip.CurSrc++;
                }

                //UpdateDspOffset(ref srcOff, g_song.EditedClip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.EditedClip.CurSrc);
                UpdateSrcOff();
            }
            else if (OK(EditedClip.SelChan))
            {
                EditedClip.Settings.Clear();


                var inst = new Instrument(EditedClip.CurInstrument);
                inst.Name = GetNewName(inst.Name, newName => Instruments.Exists(s => s.Name == newName));

                Instruments.Insert(Instruments.IndexOf(EditedClip.CurInstrument) + 1, inst);
                SetCurInst(inst);
                EditedClip.SrcOff = 0;

                SetInstName();

                //UpdateDspOffset(ref instOff, g_song.EditedClip.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(EditedClip.CurChan);
            }


            lblDup.Mark();
        }


        void Delete()
        {
            if (OK(EditedClip.CurSet))
                return;


            if (OK(EditedClip.CurSrc))
            {
                EditedClip.Settings.Clear();


                EditedClip.CurInstrument.Sources[EditedClip.CurSrc].Delete(EditedClip);
                EditedClip.CurInstrument.Sources.RemoveAt(EditedClip.CurSrc);

                //for (int i = g_song.EditedClip.CurSrc; i < inst.Sources.Count; i++)
                //    inst.Sources[i].Index--;

                if (EditedClip.CurInstrument.Sources.Count == 0)
                {
                    EditedClip.CurInstrument.Sources.Add(new Source(EditedClip.CurInstrument));
                    EditedClip.CurSrc = 0;
                }

                if (EditedClip.CurSrc >= EditedClip.CurInstrument.Sources.Count)
                    EditedClip.CurSrc =  EditedClip.CurInstrument.Sources.Count - 1;

                //UpdateDspOffset(ref srcOff, g_song.EditedClip.CurSrc, inst.Sources.Count, maxDspSrc, 0, 0);
                //UpdateSrcOff(CurrentInstrument(g_song), g_song.EditedClip.CurSrc);
                UpdateSrcOff();
            }
            else if (OK(EditedClip.SelChan))
            {
                EditedClip.Settings.Clear();


                var i = Instruments.IndexOf(EditedClip.CurInstrument);

                EditedClip.CurInstrument.Delete(EditedClip);
                Instruments.Remove(EditedClip.CurInstrument);

                if (Instruments.Count == 0)
                {
                    Instruments.Add(new Instrument());
                    Instruments[0].Sources.Add(new Source(Instruments[0]));
                }

                i = MinMax(0, i - 1, Instruments.Count - 1);

                foreach (var p in EditedClip.Patterns)
                    foreach (var c in p.Channels)
                        if (c.Instrument == EditedClip.CurInstrument) c.Instrument = Instruments[i];

                EditedClip.SrcOff = 0;

                //UpdateDspOffset(ref instOff, g_song.EditedClip.CurSrc, g_session.Instruments.Count, maxDspSrc, 0, 1);
                UpdateInstOff(EditedClip.CurChan);

                SetInstName();
            }

            lblDel.Mark();
        }


        void ToggleMove()
        {
            if (OK(EditedClip.CurSet)) 
                return;

            EditedClip.Move = !EditedClip.Move;
        }


        void MoveChan(int move)
        {
            if (OK(EditedClip.CurSet))
                return;


            if (EditedClip.SelChan < 0)
            {
                SetChan(-move);
            }
            else if (EditedClip.CurSrc < 0) // instrument
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
                else
                {
                    int first, last;
                    EditedClip.GetCurPatterns(out first, out last);

                    for (int p = first; p <= last; p++)
                        EditedClip.Patterns[p].Channels[EditedClip.CurChan].Instrument = Instruments[n];
                }


                SetInstName();
                g_inputValid = False;

                UpdateInstOff(EditedClip.CurChan);

                EditedClip.SrcOff = 0;
            }
            else // source
            {
                var inst = EditedClip.CurInstrument;
                var next = EditedClip.CurSrc + move;

                if (next >= inst.Sources.Count) next = 0;
                if (next < 0) next = inst.Sources.Count - 1;

                if (EditedClip.Move)
                {
                    var src = inst.Sources[EditedClip.CurSrc];
                    inst.Sources.RemoveAt(EditedClip.CurSrc);
                    inst.Sources.Insert(next, src);
                }

                EditedClip.CurSrc = next;

                dspMain.Panel.WriteText("");

                UpdateSrcOff();
            }


            if (move >= 0) lblNext.Mark();
            else           lblPrev.Mark();
        }


        void BackOut()
        {
            EditedClip.Move      = False;

            EditedClip.ParamKeys = False;
            EditedClip.ParamAuto = False;

            var _curSet = False;


            if (OK(EditedClip.CurSet))
            {
                if (IsCurParam())
                    EditedClip.CurSetting._IsCurrent = False;

                EditedClip.CurSet = -1;
                EditedClip.Settings.Clear();
                    
                _curSet = True;
            }


            if (   OK(EditedClip.CurSrc)
                && !_curSet)
            {
                EditedClip.CurSrc = -1;
                EditedClip.SrcOff =  0;

                EditedClip.Shift  = False;

                SetInstName(True);
                g_inputValid = False;
            }
            

            if (   OK(EditedClip.SelChan)
                && !_curSet)
            {
                EditedClip.SelChan = -1;

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

            if (OK(EditedClip.CurSet))
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

                    EditedClip.CurSet--;
                    EditedClip.Settings.RemoveAt(EditedClip.Settings.Count-1);
                }
            }
            else if (OK(EditedClip.CurSrc))
            {
                EditedClip.CurSrc = -1;
                EditedClip.SrcOff =  0;

                EditedClip.Shift = False;

                SetInstName(True);
                g_inputValid = False;
            }
            else if (OK(EditedClip.SelChan))
            {
                EditedClip.SelChan = -1;

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
            if (OK(EditedClip.CurSet))
                return;

            EditedClip.Move = False;

            if (EditedClip.SelChan < 0)
            {
                EditedClip.EditPos = float_NaN;
                EditedClip.SelChan = EditedClip.CurChan;

                UpdateInstOff(EditedClip.SelChan);

                SetInstName(True);
                g_inputValid = False;

                lblEnter.Mark(EditedClip.CurSrc < 0);
            }
            else if (EditedClip.CurSrc < 0)
            {
                EditedClip.CurSrc = 0;
                EditedClip.Shift  = False;

                SetInstName(False);

                lblEnter.Mark();
            }
        }


        void Command1()
        {
            if (OK(ModDestConnecting))
            {
                if (ModDestConnecting == EditedClip.CurSetting)
                    ResetModConnecting();

                else
                { 
                    ModDestConnecting.SrcSettings   .Add(OK(EditedClip.CurSet) ? EditedClip.CurSetting : Setting_null);
                    ModDestConnecting.SrcSources    .Add(EditedClip.SelSource);
                    ModDestConnecting.SrcInstruments.Add(EditedClip.SelInstrument);

                    SwitchToSetting(
                        ModDestClip,
                        ModDestChannel.Instrument,
                        ModDestSrcIndex, 
                        ModDestConnecting);

                    ResetModConnecting();
                }
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
                    ModCurChan        = EditedClip.CurChan;
                    ModSelChan        = EditedClip.SelChan;
                    ModCurPat         = EditedClip.CurPat;
                    ModDestSrcIndex   = EditedClip.CurSrc;
                    ModDestChannel    = EditedClip.SelChannel;
                    ModDestClip       = EditedClip;
                }

                lblCmd1.Mark();
            }
            else
            {
                if (OK(EditedClip.CurSrc))
                {
                    var src = EditedClip.SelSource;
                    src.On = !src.On;
                }
                else
                { 
                    if (!OK(EditedClip.SelChan))
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
            if (OK(EditedClip.CurSrc))
            {
                var src = EditedClip.SelSource;

                var newOsc = (int)src.Oscillator.Type + 1;
                if (newOsc > (int)OscType.Crunch) newOsc = 0;
                src.Oscillator = OscillatorFromType((OscType)newOsc);
            }


            lblCmd2.Mark();
        }


        void Command3()
        {
            if (IsCurParam())
            {
                var param = EditedClip.CurParam;
                var path  = EditedClip.Settings.Last().GetPath(EditedClip.CurSrc);

                
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
                    var chan = EditedClip.SelChannel;

                    var key = chan.AutoKeys.Find(k =>
                           k.Path == path
                        && k.Step >= (EditedClip.EditPos % g_patSteps)
                        && k.Step <  (EditedClip.EditPos % g_patSteps) + 1);

                    if (!OK(key)) // create
                    {
                        var val = Parameter.GetAutoValue(EditedClip, EditedClip.EditPos, EditedClip.CurPat, path);

                        var newKey = new Key(
                            EditedClip.CurSrc,
                            param,
                            OK(val) ? val : param.Value,
                            EditedClip.EditPos % g_patSteps,
                            EditedClip.SelChannel);

                        chan.AutoKeys.Add(newKey);
                        EditedClip.UpdateAutoKeys();
                    }
                    else // delete
                    {
                        chan.AutoKeys.Remove(key);
                        EditedClip.UpdateAutoKeys();
                    }
                }
                else if (OK(EditedClip.CurSet))
                {
                    if (EditedClip.CurSetting.CanDelete())
                        DeleteCurSetting(EditedClip);
                }

                lblCmd3.Mark();
            }
            else if (OK(EditedClip.CurSet))
            { 
                if (EditedClip.CurSetting.CanDelete())
                { 
                    DeleteCurSetting(EditedClip);
                    lblCmd3.Mark();
                }
            }
            else if (EditedClip.SelChan < 0)
            { 
                EditedClip.Transpose = !EditedClip.Transpose;
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
                var path = clip.Settings.Last().GetPath(clip.CurSrc);

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
                            var param = (Parameter)GetSettingFromPath(chan.Instrument, path);
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
                    Transpose(clip, clip.CurChan, delta);
            }


            (delta >= 0 ? lblUp : lblDown).Mark(!clip.Shift);
        }


        static void AdjustFromController(Clip clip, Setting setting, float delta)
        {
            if (IsParam(setting))
                AdjustParam(clip, (Parameter)setting, delta);
        }


        static void AdjustParam(Clip clip, Parameter param, float delta)
        {
            param.SetValue(
                MinMax(
                    param.Min, 
                    param.AdjustValue(param.Value, delta, clip.Shift),
                    param.Max),
                Note_null,
                EditedClip.CurSrc);
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

            ch += EditedClip.CurChan; 

            for (int p = first; p <= last; p++)
            { 
                var pat = EditedClip.Patterns[p];

                if (ch >= g_nChans) ch = 0;
                if (ch < 0) ch = g_nChans - 1;

                if (EditedClip.Move)
                {
                    var temp = pat.Channels[EditedClip.CurChan];
                    pat.Channels[EditedClip.CurChan] = pat.Channels[ch];
                    pat.Channels[ch]                 = temp;

                    pat.Channels[EditedClip.CurChan].UpdateNotes();
                    pat.Channels[ch]                .UpdateNotes();
                }
            }

            EditedClip.CurChan = ch;

            if (OK(EditedClip.CurSrc))
                EditedClip.CurSrc = 0;

            UpdateInstOff(EditedClip.CurChan);
        }


        public void SetShuffle(int ch, int sh)
        {
            sh += EditedClip.CurPattern.Channels[ch].Shuffle;

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
                var chan = pat.Channels[EditedClip.CurChan];

                g_copyChans.Add(new Channel(chan));
            }
        }


        static void Paste()
        {
            int first, last;
            EditedClip.GetCurPatterns(out first, out last);

            first += EditedClip.CurPat;
            last  += EditedClip.CurPat;

            for (int p = 0; p < Math.Min(last - first + 1, g_copyChans.Count); p++)
            {
                var srcChan = g_copyChans[p];
                var dstChan = EditedClip.Patterns[first + p].Channels[EditedClip.CurChan];

                if (EditedClip.RndInst)
                    dstChan.Instrument = srcChan.Instrument;

                foreach (var note in srcChan.Notes)
                    dstChan.Notes.Add(new Note(note, dstChan));
            }
        }
    }
}
