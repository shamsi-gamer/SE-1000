using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void CopyChan(Clip clip, int p, int ch)
        {
            copyChan = new Channel(clip.Patterns[p].Channels[ch]);
        }


        void PasteChan(Clip clip, int p, int ch)
        {
            if (copyChan == null)
                return;

            int f, l;
            clip.GetPatterns(p, out f, out l);

            for (int i = f; i <= l; i++)
                clip.Patterns[i].Channels[ch] = new Channel(copyChan);

            UpdateInstOff(clip.CurChan);
        }


        void Step(Clip clip, int ch)
        {
            if (OK(clip.EditPos))
            {
                if (  !clip.ChordMode 
                    || clip.Chord > -1)
                {
                    var chan = g_session.CurClip.CurrentPattern.Channels[ch];

                    int found;
                    while ((found = chan.Notes.FindIndex(n => 
                               g_session.CurClip.CurPat * g_nSteps + n.PatStep >= clip.EditPos 
                            && g_session.CurClip.CurPat * g_nSteps + n.PatStep <  clip.EditPos + 1)) > -1)
                        chan.Notes.RemoveAt(found);

                    lastNotes.Clear();
                }

                MoveEdit(clip, 1, true);

                ////MarkLabel(lblStep);
            }
        }


        void Hold(Clip clip)
        {
            if (OK(clip.EditPos))
            {
                if (clip.EditNotes.Count > 0)
                {
                    g_session.CurClip.Hold = !g_session.CurClip.Hold;
                    //UpdateHoldLabel();
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
                                clip.EditPos - (pat * g_nSteps + lastNote.PatStep) + g_session.CurClip.EditStep + ChordSpread(i),
                                10f * FPS / g_session.TicksPerStep);

                            TriggerNote(clip, lastNote.Number, lastNote.iChan, g_session.CurClip.EditStep, ChordSpread(i));
                        }

                        MoveEdit(clip, 1);
                    }
                }
            }
            else
            {
                g_session.CurClip.Hold = !g_session.CurClip.Hold;
                //UpdateHoldLabel();

                //if (!g_session.CurClip.Hold)
                //    StopCurrentNotes(clip.CurChan);
            }
        }


        void Interpolate(Clip clip)
        {
            if (!OK(clip.EditPos))
                return;

            if (   clip.CurSet < 0
                || !(   g_session.CurClip.ParamKeys 
                     || g_session.CurClip.ParamAuto))
                return;


            if (clip.Inter == null)
            {
                clip.StopEdit();

                clip.Inter = clip.CurrentChannel.Notes.Find(n =>
                       n.SongStep >= clip.EditPos
                    && n.SongStep <  clip.EditPos + g_session.CurClip.EditStep);
            }
            else
            {
                var note = clip.CurrentChannel.Notes.Find(n =>
                       n.SongStep >= clip.EditPos
                    && n.SongStep <  clip.EditPos + g_session.CurClip.EditStep);

                if (note != null)
                {
                    var si = clip.Inter.SongStep;
                    var sn = note      .SongStep;

                    var path  = g_settings.Last().GetPath(g_session.CurClip.CurSrc);
                    var param = (Parameter)GetSettingFromPath(note.Instrument, path);

                    var start = param.GetKeyValue(clip.Inter, g_session.CurClip.CurSrc);
                    var end   = param.GetKeyValue(note,       g_session.CurClip.CurSrc);

                    int f = (int)(si / g_nSteps);
                    int l = (int)(sn / g_nSteps);
                    int d = f < l ? 1 : -1;

                    for (int p = f; f < l ? (p <= l) : (p >= l); p += d)
                    {
                        foreach (var n in clip.Patterns[p].Channels[clip.CurChan].Notes)
                        {
                            if (   n.SongStep <  Math.Min(si, sn)
                                || n.SongStep >= Math.Max(si, sn))
                                continue;

                            var val = start + (end - start) * Math.Abs(n.SongStep - si) / Math.Max(1, Math.Abs(sn - si));

                            var nParam = (Parameter)GetSettingFromPath(n.Instrument, path);
                            var key = n.Keys.Find(k => k.Path == path);

                            if (key != null)
                                SetKeyValue(key, val);
                            else
                            {
                                n.Keys.Add(new Key(g_session.CurClip.CurSrc, param, nParam.Value, n.SongStep));
                                SetKeyValue(n.Keys.Last(), val);
                            }
                        }
                    }
                }

                clip.Inter = null;
            }

            //UpdateLabel(lblCmd1, clip.Inter != null);
            //UpdateAdjustLabels(g_session.CurClip);
        }


        void EnableKeyMove(Clip clip)
        {
            if (   IsCurParam()
                && OK(clip.EditPos)
                && clip.ParamAuto)
            {
                var key = clip.SelectedChannel.AutoKeys.Find(k => 
                         k.StepTime >= (clip.EditPos % g_nSteps)
                      && k.StepTime <  (clip.EditPos % g_nSteps) + 1);

                g_editKey = g_editKey ?? key;

                //(lblCmd1, g_editKey != null);
            }
        }


        void SetKeyValue(Key key, float val)
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
            if (OK(g_session.CurClip.EditPos))
                g_session.CurClip.LastEditPos = g_session.CurClip.EditPos;

            g_session.CurClip.EditPos =
                OK(g_session.CurClip.EditPos)
                ? float.NaN
                : (OK(g_session.CurClip.LastEditPos) ? g_session.CurClip.LastEditPos : g_session.CurClip.CurPat * g_nSteps);

            g_session.CurClip.StopEdit();

            //UpdateAdjustLabels(g_session.CurClip);

            if (g_session.CurClip.Hold)
                g_session.CurClip.TrimCurrentNotes(g_session.CurClip.CurChan);

            g_session.CurClip.Hold = false;
            //UpdateLabel(lblHold, false);

            if (!OK(g_session.CurClip.EditPos))
            {
                g_session.CurClip.Inter = null;
                //UpdateLabel(lblCmd1, false);
            }

            //UpdateEditLabel(lblEdit, OK(g_session.CurClip.EditPos));
        }


        static Key PrevSongAutoKey(float pos, int p, int ch, string path)
        {
            var prevKeys = g_session.CurClip.ChannelAutoKeys[ch]
                .Where(k => 
                       (   path == ""
                        || k.Path == path)
                    && k.StepTime < pos)
                .ToList();
            
            return 
                prevKeys.Count > 0
                ? prevKeys.Last()
                : null;
        }


        static Key NextSongAutoKey(float pos, int p, int ch, string path)
        {
            var nextKeys = g_session.CurClip.ChannelAutoKeys[ch]
                .Where(k =>
                       (   path == ""
                        || k.Path == path)
                    && k.StepTime > pos)
                .ToList();

            return
                nextKeys.Count > 0
                ? nextKeys[0]
                : null;
        }


        void ToggleNote(Clip clip)
        {
            if (OK(clip.EditPos))
            { 
                clip.Inter = null;
                //UpdateLabel(lblCmd1, false);

                if (clip.EditNotes.Count > 0) clip.EditNotes.Clear();
                else                          clip.EditNotes.AddRange(GetEditNotes(clip));

                //UpdateEditLabel(lblEdit, OK(clip.EditPos));
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

            //UpdateHoldLabel();
            //UpdateAdjustLabels(g_session.CurClip);
        }


        void CutNotes(Clip clip)
        {
            for (int p = 0; p <= g_session.CurClip.CurPat; p++)
            {
                var patStart =  g_session.CurClip.CurPat   *g_nSteps;
                var patEnd   = (g_session.CurClip.CurPat +1)*g_nSteps;

                var pat  = clip.Patterns[p];
                var chan = pat.Channels[g_session.CurClip.CurChan];

                var min  = (60 + chan.Transpose*12) * NoteScale;
                var max  = (84 + chan.Transpose*12) * NoteScale;

                foreach (var note in chan.Notes)
                {
                    if (   clip.EditPos > note.SongStep
                        && clip.EditPos < note.SongStep + note.StepLength)
                        note.StepLength = clip.EditPos - note.SongStep + 1;
                }
            }

            g_mainPressed.Add(3);
        }


        List<Note> GetEditNotes(Clip clip, bool onlyEdit = false)
        {
            var chan = g_session.CurClip.CurrentChannel;

            if (OK(clip.EditPos))
            { 
                var notes = new List<Note>();
                
                foreach (var note in chan.Notes)
                {
                    if (   note.SongStep >= clip.EditPos
                        && note.SongStep <  clip.EditPos + g_session.CurClip.EditStep)
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

                for (int p = 0; p <= g_session.CurClip.CurPat; p++)
                {
                    var patStart =  clip.CurPat   *g_nSteps;
                    var patEnd   = (clip.CurPat+1)*g_nSteps;

                    var pat  = clip.Patterns[p];
                    var chan = pat.Channels[clip.CurChan];

                    var min  = (60 + chan.Transpose*12) * NoteScale;
                    var max  = (84 + chan.Transpose*12) * NoteScale;

                    foreach (var note in chan.Notes)
                    {
                        if (   clip.EditPos > note.SongStep
                            && clip.EditPos < note.SongStep + note.StepLength - 1)
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
            clip.GetPatterns(clip.CurPat, out first, out last);

            for (int pat = first; pat <= last; pat++)
                notes.AddRange(clip.Patterns[pat].Channels[clip.CurChan].Notes);

            return notes;
        }


        void SetStepLength(int d)
        {
            var len = MinMax(4, g_session.TicksPerStep + d, 15);
            var newTime = (float)g_time / len;

            g_session.TicksPerStep = len;

            foreach (var pat in g_session.CurClip.Patterns)
            {
                foreach (var chan in pat.Channels)
                    chan.Shuffle = Math.Min(chan.Shuffle, g_session.TicksPerStep - 1);
            }

            g_infoPressed.Add(d > 0 ? 2 : 3);
        }


        void ChangeEditStep()
        {
            g_session.CurClip.EditStep++;

            if (g_session.CurClip.EditStep >= g_steps.Length - 1) // ignore the super long step
                g_session.CurClip.EditStep = 1;

            ////MarkLabel(lblEditStep);
            //UpdateEditLabels();
        }


        void ChangeEditLength()
        {
            g_session.CurClip.EditLength++;

            if (g_session.CurClip.EditLength >= g_steps.Length)
                g_session.CurClip.EditLength = 0;

            ////MarkLabel(lblEditLength);
            //UpdateEditLabels();

            //g_sampleValid = F;
        }


        void Left(Clip clip)
        {
            MoveEdit(clip, -1);

            //if (OK(clip.EditPos))
            //    //MarkLabel(lblLeft);
        }


        void Right(Clip clip)
        {
            MoveEdit(clip, 1);

            //if (   OK(clip.EditPos)
            //    || IsCurSetting(typeof(Harmonics)))
                //MarkLabel(lblRight);
        }


        void MoveEdit(Clip clip, int move, bool create = false)
        {
            var chan = clip.SelectedChannel;

            if (IsCurSetting(typeof(Harmonics)))
            {
                CurHarmonics.MoveEdit(move);
            }
            else if (clip.EditNotes.Count > 0)
            {
                if (g_session.CurClip.Hold)
                {
                    foreach (var n in clip.EditNotes)
                    {
                        var is05 = n.StepLength == 0.5f && g_session.CurClip.EditLength >= 1;
                        n.StepLength = MinMax(0.5f, n.StepLength + move * g_session.CurClip.EditLength, 10f * FPS / g_session.TicksPerStep);
                        if (is05) n.StepLength -= 0.5f;
                    }
                }
                else
                {
                    var oldCur = clip.CurPat;

                    clip.EditPos += move * g_session.CurClip.EditStep;
                    clip.LimitRecPosition();

                    foreach (var n in clip.EditNotes)
                    {
                        n.PatStep += move * g_session.CurClip.EditStep;

                        if (   n.PatStep < 0
                            || n.PatStep >= g_nSteps)
                        {
                            n.Channel.Notes.Remove(n);
                            n.PatStep -= move * g_nSteps;

                            chan.Notes.Add(n);
                            n.Channel = chan;
                        }
                    }
                }
            }
            else if (g_editKey != null)
            {
                g_editKey.StepTime += move * g_session.CurClip.EditStep;
                clip.EditPos       += move * g_session.CurClip.EditStep;

                clip.LimitRecPosition();

                if (   g_editKey.StepTime < 0
                    || g_editKey.StepTime >= g_nSteps)
                { 
                    g_editKey.Channel.AutoKeys.Remove(g_editKey);
                    g_editKey.StepTime -= move * g_nSteps;

                    chan.AutoKeys.Add(g_editKey);
                    g_editKey.Channel = chan;
                }

                clip.UpdateAutoKeys();
            }
            else if (OK(clip.EditPos))
            {
                clip.EditPos += move * g_session.CurClip.EditStep;

                if (clip.Follow)
                {
                    if (clip.EditPos >= (clip.CurPat + 1) * g_nSteps) // TODO blocks
                    {
                        if (clip.EditPos >= clip.Patterns.Count * g_nSteps)
                        {
                            if (create)
                            {
                                var pat = new Pattern(clip.CurrentPattern);
                                pat.Channels[clip.CurChan].Notes.Clear();

                                clip.Patterns.Insert(clip.CurPat + 1, pat);
                            }
                            else
                                clip.EditPos -= clip.Patterns.Count * g_nSteps;
                        }
                    }
                    else if (!OK(clip.EditPos))
                        clip.EditPos += clip.Patterns.Count * g_nSteps;
                }

                clip.LimitRecPosition();
                //UpdateAdjustLabels(g_session.CurClip);
            }
        }


        void Transpose(Clip clip, int ch, float tr)
        {
            if (clip.EditNotes.Count > 0)
            {
                foreach (var note in clip.EditNotes)
                    Transpose(note, tr);
            }
            else
            {
                int first, last;
                clip.GetPatterns(clip.CurPat, out first, out last);

                for (int pat = first; pat <= last; pat++)
                    Transpose(clip, pat, ch, tr);
            }
        }


        void Transpose(Clip clip, int pat, int ch, float tr)
        {
            var chan = clip.Patterns[pat].Channels[ch];

            foreach (var note in chan.Notes)
                Transpose(note, tr);
        }


        void Transpose(Note note, float tr)
        {
            note.Number = (int)Math.Round(note.Number + tr * (g_session.CurClip.Shift ? 12 : 1) * (g_session.CurClip.HalfSharp ? 1 : 2));
        }


        void SetTranspose(Clip clip, int d)
        {
            var tune = clip.SelectedSource    ?.Tune
                    ?? clip.SelectedInstrument?.Tune;

            if (clip.Spread)
                clip.ChordSpread = MinMax(0, clip.ChordSpread + d, 16);
            
            else if (ShowPiano) SetTranspose(clip, clip.CurChan, d);
            else                SetShuffle(clip.CurChan, d);

            //MarkLabel(
                //d > 0
                //? lblTransposeUp
                //: lblTransposeDown);

            //UpdateOctaveLabel();
        }


        void SetTranspose(Clip clip, int ch, int tr)
        {
            tr += clip.CurrentPattern.Channels[ch].Transpose;

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
            g_session.CurClip.Spread = !g_session.CurClip.Spread;
            //UpdateLabel(lblSpread, g_session.CurClip.Spread);
            //UpdateOctaveLabel();
            //UpdateShuffleLabel();
        }
    }
}
