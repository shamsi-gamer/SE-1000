using System;
using System.Collections.Generic;


namespace IngameScript
{
    partial class Program
    {
        //void CopyChan(Clip clip, int p, int ch)
        //{
        //    copyChan = new Channel(clip.Patterns[p].Channels[ch]);
        //}


        //static void PasteChan(Clip clip, int p, int ch)
        //{
        //    if (g_copyChans.Count == 0)
        //        return;

        //    int f, l;
        //    clip.GetPatterns(p, out f, out l);

        //    for (int i = f; i <= l; i++)
        //        clip.Patterns[i].Channels[ch] = new Channel(g_copyChan);

        //    UpdateInstOff(clip.CurChan);
        //}


        void Step(Clip clip, int ch)
        {
            if (OK(clip.EditPos))
            {
                if (  !clip.ChordMode 
                    || OK(clip.Chord))
                {
                    var chan = clip.CurPattern.Channels[ch];

                    int found;
                    while (OK(found = chan.Notes.FindIndex(n => 
                               clip.EditPat * g_patSteps + n.Step >= clip.EditPos 
                            && clip.EditPat * g_patSteps + n.Step <  clip.EditPos + 1)))
                        chan.Notes.RemoveAt(found);

                    lastNotes.Clear();
                }

                MoveEdit(clip, 1, True);

                lblStep.Mark();
            }
        }


        void Hold(Clip clip)
        {
            if (OK(clip.EditPos))
            {
                if (clip.EditNotes.Count > 0)
                {
                    EditedClip.Hold = !EditedClip.Hold;
                }
                else
                {
                    if (lastNotes.Count > 0)
                    {
                        for (int i = 0; i < lastNotes.Count; i++)
                        { 
                            var lastNote = lastNotes[i];

                            var pat = clip.Patterns.FindIndex(p => p.Channels.Contains(lastNote.Channel));
                            if (pat < 0) return;

                            lastNote.StepLength = Math.Min(
                                clip.EditPos - (pat * g_patSteps + lastNote.Step) + EditedClip.EditStep + ChordSpread(i),
                                10f * FPS / TicksPerStep);

                            TriggerNote(clip, lastNote.Number, lastNote.iChan, EditedClip.EditStep, ChordSpread(i));
                        }

                        MoveEdit(clip, 1);
                    }
                }
            }
            else
            {
                EditedClip.Hold = !EditedClip.Hold;

                //if (!EditClip.Hold)
                //    StopCurrentNotes(clip.CurChan);
            }
        }


        static void Interpolate(Clip clip)
        {
            if (!OK(clip.EditPos))
                return;

            if (   clip.CurSet < 0
                || !(   EditedClip.ParamKeys 
                     || EditedClip.ParamAuto))
                return;


            if (!OK(clip.Inter))
            {
                clip.StopEdit();

                clip.Inter = clip.CurChannel.Notes.Find(n =>
                       n.ClipStep >= clip.EditPos
                    && n.ClipStep <  clip.EditPos + EditedClip.EditStepIndex);
            }
            else
            {
                var note = clip.CurChannel.Notes.Find(n =>
                       n.ClipStep >= clip.EditPos
                    && n.ClipStep <  clip.EditPos + EditedClip.EditStepIndex);

                if (OK(note))
                {
                    var si = clip.Inter.ClipStep;
                    var sn = note      .ClipStep;

                    var path  = clip.Settings.Last().GetPath(clip.CurSrc);
                    var param = (Parameter)GetSettingFromPath(path);

                    var start = param.GetKeyValue(clip.Inter, clip.CurSrc);
                    var end   = param.GetKeyValue(note,       clip.CurSrc);

                    int f = (int)(si / g_patSteps);
                    int l = (int)(sn / g_patSteps);
                    int d = f < l ? 1 : -1;

                    for (int p = f; f < l ? (p <= l) : (p >= l); p += d)
                    {
                        foreach (var n in clip.Patterns[p].Channels[clip.CurChan].Notes)
                        {
                            if (   n.ClipStep <  Math.Min(si, sn)
                                || n.ClipStep >= Math.Max(si, sn))
                                continue;

                            var val = start + (end - start) * Math.Abs(n.ClipStep - si) / Math.Max(1, Math.Abs(sn - si));

                            var nParam = (Parameter)GetSettingFromPath(path);
                            var key = n.Keys.Find(k => k.Path == path);

                            if (OK(key))
                                SetKeyValue(key, val);
                            else
                            {
                                n.Keys.Add(new Key(clip.CurSrc, param, nParam.Value, n.ClipStep));
                                SetKeyValue(n.Keys.Last(), val);
                            }
                        }
                    }
                }

                clip.Inter = Note_null;
            }
        }


        static void EnableKeyMove(Clip clip)
        {
            if (OK(g_editKey))
                g_editKey = Key_null;

            else if (IsCurParam()
                  && OK(clip.EditPos)
                  && clip.ParamAuto)
            {
                var localPos = clip.EditPos % g_patSteps;

                var key = clip.SelChannel.AutoKeys.Find(k => 
                         k.Step >= localPos
                      && k.Step <  localPos + 1);

                g_editKey = key;
            }
        }


        static void SetKeyValue(Key key, float val)
        {
            switch (key.Path.Split('/').Last())
            {
                case strVol:  key.Value = MinMax(0,         val,    2); break;

                case strAtt:  key.Value = MinMax(0,         val, 1000); break;
                case strDec:  key.Value = MinMax(0,         val, 1000); break;
                case strSus:  key.Value = MinMax(0,         val,    1); break;
                case strRel:  key.Value = MinMax(0,         val, 1000); break;

                case strCnt:  key.Value = MinMax(0,         val, 1000); break;
                case strTime: key.Value = MinMax(0.000001f, val,   10); break;
                case strLvl:  key.Value = MinMax(0,         val,    1); break;
                case strPow:  key.Value = MinMax(0.000001f, val,   10); break;

                case strAmp:  key.Value = MinMax(0,         val,    1); break;
                case strFreq: key.Value = MinMax(0.000001f, val,   30); break;
                case strOff:  key.Value = MinMax(-100,      val,  100); break;
            }
        }


        void Edit()
        {
            if (OK(EditedClip.EditPos))
                EditedClip.LastEditPos = EditedClip.EditPos;

            EditedClip.EditPos =
                OK(EditedClip.EditPos)
                ? float.NaN
                : (OK(EditedClip.LastEditPos) ? EditedClip.LastEditPos : EditPat * g_patSteps);

            EditedClip.StopEdit();

            if (EditedClip.Hold)
                EditedClip.TrimCurrentNotes(CurChan);

            EditedClip.Hold = False;

            if (!OK(EditedClip.EditPos))
                EditedClip.Inter = Note_null;

            if (OK(EditedClip.EditPos)) 
                Recording = False;
        }


        void Record()
        {
            Recording = !Recording;
            if (Recording) EditedClip.EditPos = float_NaN;
        }


        void ToggleNote(Clip clip)
        {
            if (OK(clip.SelChan))
                return;

            if (OK(clip.EditPos))
            { 
                clip.Inter = Note_null;

                if (clip.EditNotes.Count > 0) clip.EditNotes.Clear();
                else                          clip.EditNotes.AddRange(GetEditNotes(clip));
            }
            else
            {
                if (clip.EditNotes.Count > 0)
                    clip.EditNotes.Clear();
                else 
                { 
                    clip.EditNotes.Clear();
                    clip.EditNotes.AddRange(GetChannelNotes(clip));
                }
            }
        }


        void CutNotes(Clip clip)
        {
            if (!OK(clip.SelChan))
                return;

            for (int p = 0; p <= clip.EditPat; p++)
            {
                var patStart =  clip.EditPat    *g_patSteps;
                var patEnd   = (clip.EditPat +1)*g_patSteps;

                var pat  = clip.Patterns[p];
                var chan = pat.Channels[clip.CurChan];

                var min  = (60 + chan.Transpose*12) * NoteScale;
                var max  = (84 + chan.Transpose*12) * NoteScale;

                foreach (var note in chan.Notes)
                {
                    if (   clip.EditPos > note.ClipStep
                        && clip.EditPos < note.ClipStep + note.StepLength)
                        note.StepLength = clip.EditPos - note.ClipStep + 1;
                }
            }

            g_labelsPressed.Add(lblCut);
        }


        static List<Note> GetEditNotes(Clip clip, bool onlyEdit = False)
        {
            var chan = clip.CurChannel;

            if (OK(clip.EditPos))
            { 
                var notes = new List<Note>();
                
                foreach (var note in chan.Notes)
                {
                    if (   note.ClipStep >= clip.EditPos
                        && note.ClipStep <  clip.EditPos + clip.EditStep)
                        notes.Add(note);
                }

                return notes;
            }
            else if (!onlyEdit)
                return new List<Note>(chan.Notes);
            else
                return new List<Note>();
        }


        List<Note> GetLongNotes(Clip clip)
        {
            if (OK(clip.EditPos))
            {
                var notes = new List<Note>();

                for (int p = 0; p <= clip.EditPat; p++)
                {
                    var patStart =  clip.EditPat   *g_patSteps;
                    var patEnd   = (clip.EditPat+1)*g_patSteps;

                    var pat  = clip.Patterns[p];
                    var chan = pat.Channels[clip.CurChan];

                    var min  = (60 + chan.Transpose*12) * NoteScale;
                    var max  = (84 + chan.Transpose*12) * NoteScale;

                    foreach (var note in chan.Notes)
                    {
                        if (   clip.EditPos > note.ClipStep
                            && clip.EditPos < note.ClipStep + note.StepLength - 1)
                            notes.Add(note);
                    }
                }

                return notes;
            }
            else
                return new List<Note>();
        }


        List<Note> GetChannelNotes(Clip clip)
        {
            var notes = new List<Note>();

            int first, last;
            clip.GetPatterns(clip.EditPat, out first, out last);

            for (int pat = first; pat <= last; pat++)
                notes.AddRange(clip.Patterns[pat].Channels[clip.CurChan].Notes);

            return notes;
        }


        void SetStepLength(int d)
        {
            var len = MinMax(4, TicksPerStep + d, 15);
            var newTime = g_time / len;

            TicksPerStep = len;

            foreach (var pat in EditedClip.Patterns)
            {
                foreach (var chan in pat.Channels)
                    chan.Shuffle = (int)Math.Min(chan.Shuffle, TicksPerStep - 1);
            }

            g_lcdPressed.Add(lcdInfo + (d > 0 ? 2 : 3));
        }


        void ChangeEditStep()
        {
            EditedClip.EditStepIndex++;

            if (EditedClip.EditStepIndex >= g_steps.Length - 1) // ignore the super long step
                EditedClip.EditStepIndex = 1;

            lblEditStep.Mark();
        }


        void ChangeEditLength()
        {
            EditedClip.EditLengthIndex++;

            if (EditedClip.EditLengthIndex >= g_steps.Length)
                EditedClip.EditLengthIndex = 0;

            lblEditLength.Mark();
        }


        void Left(Clip clip)
        {
            if (   OK(clip.EditPos)
                || IsCurSetting(typeof(Harmonics))) 
                 MoveEdit(clip, -1);
            else Shift(False);

            lblLeft.Mark();
        }


        void Right(Clip clip)
        {
            if (   OK(clip.EditPos)
                || IsCurSetting(typeof(Harmonics))) 
                 MoveEdit(clip, 1);
            else Shift(True);

            lblRight.Mark();
        }


        void MoveEdit(Clip clip, int move, bool create = False)
        {
            if (IsCurSetting(typeof(Harmonics)))
                clip.CurHarmonics.MoveEdit(move);

            else if (clip.EditNotes.Count > 0)
            {
                if (EditedClip.Hold) ResizeNotes(clip, move);
                else                 MoveNotes(clip, move);
            }

            else if (OK(g_editKey))
                MoveKey(clip, move);

            else if (OK(clip.EditPos))
                MoveEditPos(clip, move, create);
        }


        static void MoveEditPos(Clip clip, int move, bool create)
        {
            clip.EditPos += move * EditedClip.EditStep;

            if (clip.Follow)
            {
                if (clip.EditPos >= (clip.EditPat + 1) * g_patSteps) // TODO blocks
                {
                    if (clip.EditPos >= clip.StepLength)
                    {
                        if (create)
                        {
                            var pat = new Pattern(clip.CurPattern);
                            pat.Channels[clip.CurChan].Notes.Clear();

                            clip.Patterns.Insert(clip.EditPat + 1, pat);
                        }
                        else
                            clip.EditPos -= clip.StepLength;
                    }
                }
                else if (!OK(clip.EditPos))
                    clip.EditPos += clip.StepLength;
            }

            clip.LimitRecPosition();
        }


        static void MoveNotes(Clip clip, int move)
        {
            clip.EditPos += move * EditedClip.EditStep;
            clip.LimitRecPosition();

            var chan = clip.CurChannel;

            foreach (var n in clip.EditNotes)
            {
                n.Step += move * EditedClip.EditStep;

                if (   n.Step < 0
                    || n.Step >= g_patSteps)
                {
                    n.Channel.Notes.Remove(n);
                    n.Step -= move * g_patSteps;

                    chan.Notes.Add(n);
                    n.Channel = chan;
                }
            }
        }


        static void ResizeNotes(Clip clip, int move)
        {
            foreach (var n in clip.EditNotes)
            {
                var is05 = n.StepLength == 0.5f && EditedClip.EditStep >= 1;
                n.StepLength = MinMax(0.5f, n.StepLength + move * EditedClip.EditStepLength, 10f * FPS / TicksPerStep);
                if (is05) n.StepLength -= 0.5f;
            }
        }


        static void MoveKey(Clip clip, int move)
        {
            g_editKey.Step += move * EditedClip.EditStep;
            clip.EditPos   += move * EditedClip.EditStep;

            clip.LimitRecPosition();

            var chan = clip.CurChannel;

            if (   g_editKey.Step < 0
                || g_editKey.Step >= g_patSteps)
            { 
                g_editKey.Channel.AutoKeys.Remove(g_editKey);
                g_editKey.Step -= move * g_patSteps;

                chan.AutoKeys.Add(g_editKey);
                g_editKey.Channel = chan;
            }

            clip.UpdateAutoKeys();
        }


        static void Transpose(Clip clip, int ch, float tr)
        {
            if (clip.EditNotes.Count > 0)
            {
                foreach (var note in clip.EditNotes)
                    Transpose(clip, note, tr);
            }
            else
            {
                int first, last;
                clip.GetPatterns(clip.EditPat, out first, out last);

                for (int pat = first; pat <= last; pat++)
                    Transpose(clip, pat, ch, tr);
            }
        }


        static void Transpose(Clip clip, int pat, int ch, float tr)
        {
            var chan = clip.Patterns[pat].Channels[ch];

            foreach (var note in chan.Notes)
                Transpose(clip, note, tr);
        }


        static void Transpose(Clip clip, Note note, float tr)
        {
            note.Number = (int)Math.Round(note.Number + tr * (clip.Shift ? 12 : 1) * (clip.HalfSharp ? 1 : 2));
        }


        void SetTranspose(int d)
        {
            var tune = SelSource    ?.Tune
                    ?? SelInstrument?.Tune;

            if (EditedClip.Spread)
                EditedClip.ChordSpread = MinMax(0, EditedClip.ChordSpread + d, 16);
            
            else if (ShowPiano) SetTranspose(EditedClip, CurChan, d);
            else                SetShuffle(CurChan, d);

            if (d > 0) lblOctaveUp  .Mark();
            else       lblOctaveDown.Mark();
        }


        void SetTranspose(Clip clip, int ch, int tr)
        {
            tr += clip.CurPattern.Channels[ch].Transpose;

            int first, last;
            clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            {
                var pat  = clip.Patterns[p];
                var chan = pat.Channels[ch];

                chan.Transpose = MinMax(-3, tr, 4);
            }
        }


        void Spread()
        {
            EditedClip.Spread = !EditedClip.Spread;
        }
    }
}
