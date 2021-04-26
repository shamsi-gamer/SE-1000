using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void CopyChan(Clip song, int p, int ch)
        {
            copyChan = new Channel(song.Patterns[p].Channels[ch]);
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
                    var chan = g_clip.CurrentPattern.Channels[ch];

                    int found;
                    while ((found = chan.Notes.FindIndex(n => 
                               g_clip.CurPat * g_nSteps + n.PatStep >= clip.EditPos 
                            && g_clip.CurPat * g_nSteps + n.PatStep <  clip.EditPos + 1)) > -1)
                        chan.Notes.RemoveAt(found);

                    lastNotes.Clear();
                }

                MoveEdit(clip, 1, true);

                MarkLight(lblStep);
            }
        }


        void Hold(Clip song)
        {
            if (OK(song.EditPos))
            {
                if (song.EditNotes.Count > 0)
                {
                    g_clip.Hold = !g_clip.Hold;
                    UpdateHoldLight();
                }
                else
                {
                    if (lastNotes.Count > 0)
                    {
                        for (int i = 0; i < lastNotes.Count; i++)
                        { 
                            var lastNote = lastNotes[i];

                            var pat = song.Patterns.FindIndex(p => p.Channels.Contains(lastNote.Channel));
                            if (pat < 0) return;

                            lastNote.StepLength = Math.Min(
                                song.EditPos - (pat * g_nSteps + lastNote.PatStep) + EditStep + ChordSpread(i),
                                10f * FPS / g_ticksPerStep);

                            TriggerNote(lastNote.Number, lastNote.iChan, EditStep, ChordSpread(i));
                        }

                        MoveEdit(song, 1);
                    }
                }
            }
            else
            {
                g_clip.Hold = !g_clip.Hold;
                UpdateHoldLight();

                //if (!g_clip.Hold)
                //    StopCurrentNotes(song.CurChan);
            }
        }


        void Interpolate(Clip clip)
        {
            if (!OK(clip.EditPos))
                return;

            if (   clip.CurSet < 0
                || !(   g_clip.ParamKeys 
                     || g_clip.ParamAuto))
                return;


            if (clip.Inter == null)
            {
                clip.StopEdit();

                clip.Inter = clip.CurrentChannel.Notes.Find(n =>
                       n.SongStep >= clip.EditPos
                    && n.SongStep <  clip.EditPos + EditStep);
            }
            else
            {
                var note = clip.CurrentChannel.Notes.Find(n =>
                       n.SongStep >= clip.EditPos
                    && n.SongStep <  clip.EditPos + EditStep);

                if (note != null)
                {
                    var si = clip.Inter.SongStep;
                    var sn = note      .SongStep;

                    var path  = g_settings.Last().GetPath(g_clip.CurSrc);
                    var param = (Parameter)GetSettingFromPath(note.Instrument, path);

                    var start = param.GetKeyValue(clip.Inter, g_clip.CurSrc);
                    var end   = param.GetKeyValue(note,       g_clip.CurSrc);

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
                                n.Keys.Add(new Key(g_clip.CurSrc, param, nParam.Value, n.SongStep));
                                SetKeyValue(n.Keys.Last(), val);
                            }
                        }
                    }
                }

                clip.Inter = null;
            }

            UpdateLight(lblCmd1, clip.Inter != null);
            UpdateAdjustLights(g_clip);
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

                UpdateLight(lblCmd1, g_editKey != null);
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
            if (OK(g_clip.EditPos))
                g_clip.LastEditPos = g_clip.EditPos;

            g_clip.EditPos =
                OK(g_clip.EditPos)
                ? float.NaN
                : (OK(g_clip.LastEditPos) ? g_clip.LastEditPos : g_clip.CurPat * g_nSteps);

            g_clip.StopEdit();

            UpdateAdjustLights(g_clip);

            if (g_clip.Hold)
                g_clip.TrimCurrentNotes(g_clip.CurChan);

            g_clip.Hold = false;
            UpdateLight(lblHold, false);

            if (!OK(g_clip.EditPos))
            {
                g_clip.Inter = null;
                UpdateLight(lblCmd1, false);
            }

            UpdateEditLight(lblEdit, OK(g_clip.EditPos));
        }


        static Key PrevSongAutoKey(float pos, int p, int ch, string path)
        {
            var prevKeys = g_clip.ChannelAutoKeys[ch]
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
            var nextKeys = g_clip.ChannelAutoKeys[ch]
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


        void ToggleNote(Clip song)
        {
            if (OK(song.EditPos))
            { 
                song.Inter = null;
                UpdateLight(lblCmd1, false);

                if (song.EditNotes.Count > 0) song.EditNotes.Clear();
                else                          song.EditNotes.AddRange(GetEditNotes(song));

                UpdateEditLight(lblEdit, OK(song.EditPos));
            }
            else
            {
                if (song.EditNotes.Count > 0)
                    song.EditNotes.Clear();
                else 
                { 
                    song.EditNotes.Clear();
                    song.EditNotes.AddRange(GetChannelNotes(song));
                }
            }

            UpdateHoldLight();
            UpdateAdjustLights(g_clip);
        }


        void CutNotes(Clip song)
        {
            for (int p = 0; p <= g_clip.CurPat; p++)
            {
                var patStart =  g_clip.CurPat   *g_nSteps;
                var patEnd   = (g_clip.CurPat +1)*g_nSteps;

                var pat  = song.Patterns[p];
                var chan = pat.Channels[g_clip.CurChan];

                var min  = (60 + chan.Transpose*12) * NoteScale;
                var max  = (84 + chan.Transpose*12) * NoteScale;

                foreach (var note in chan.Notes)
                {
                    if (   song.EditPos > note.SongStep
                        && song.EditPos < note.SongStep + note.StepLength)
                        note.StepLength = song.EditPos - note.SongStep + 1;
                }
            }

            g_mainPressed.Add(3);
        }


        List<Note> GetEditNotes(Clip song, bool onlyEdit = false)
        {
            var chan = g_clip.CurrentChannel;

            if (OK(song.EditPos))
            { 
                var notes = new List<Note>();
                
                foreach (var note in chan.Notes)
                {
                    if (   note.SongStep >= song.EditPos
                        && note.SongStep <  song.EditPos + EditStep)
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

                for (int p = 0; p <= g_clip.CurPat; p++)
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
            var len = MinMax(4, g_ticksPerStep + d, 15);
            var newTime = (float)g_time / len;

            g_ticksPerStep = len;

            foreach (var pat in g_clip.Patterns)
            {
                foreach (var chan in pat.Channels)
                    chan.Shuffle = Math.Min(chan.Shuffle, g_ticksPerStep - 1);
            }

            g_infoPressed.Add(d > 0 ? 2 : 3);
        }


        void ChangeEditStep()
        {
            g_clip.EditStep++;

            if (g_clip.EditStep >= g_steps.Length - 1) // ignore the super long step
                g_clip.EditStep = 1;

            MarkLight(lblEditStep);
            UpdateEditLights();
        }


        void ChangeEditLength()
        {
            g_clip.EditLength++;

            if (g_clip.EditLength >= g_steps.Length)
                g_clip.EditLength = 0;

            MarkLight(lblEditLength);
            UpdateEditLights();

            //g_sampleValid = F;
        }


        void Left(Clip song)
        {
            MoveEdit(song, -1);

            if (OK(song.EditPos))
                MarkLight(lblLeft);
        }


        void Right(Clip song)
        {
            MoveEdit(song, 1);

            if (   OK(song.EditPos)
                || IsCurSetting(typeof(Harmonics)))
                MarkLight(lblRight);
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
                if (g_clip.Hold)
                {
                    foreach (var n in clip.EditNotes)
                    {
                        var is05 = n.StepLength == 0.5f && EditStepLength >= 1;
                        n.StepLength = MinMax(0.5f, n.StepLength + move * EditStepLength, 10f * FPS / g_ticksPerStep);
                        if (is05) n.StepLength -= 0.5f;
                    }
                }
                else
                {
                    var oldCur = clip.CurPat;

                    clip.EditPos += move * EditStep;
                    clip.LimitRecPosition();

                    foreach (var n in clip.EditNotes)
                    {
                        n.PatStep += move * EditStep;

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
                g_editKey.StepTime += move * EditStep;
                clip.EditPos       += move * EditStep;

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
                clip.EditPos += move * EditStep;

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
                UpdateAdjustLights(g_clip);
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
            note.Number = (int)Math.Round(note.Number + tr * (g_clip.Shift ? 12 : 1) * (g_clip.HalfSharp ? 1 : 2));
        }


        void SetTranspose(Clip clip, int d)
        {
            var tune = clip.SelectedSource    ?.Tune
                    ?? clip.SelectedInstrument?.Tune;

            if (clip.Spread)
                clip.ChordSpread = MinMax(0, clip.ChordSpread + d, 16);
            
            else if (ShowPiano) SetTranspose(clip, clip.CurChan, d);
            else                SetShuffle(clip.CurChan, d);

            MarkLight(
                d > 0
                ? lblTransposeUp
                : lblTransposeDown);

            UpdateOctaveLight();
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
            g_clip.Spread = !g_clip.Spread;
            UpdateLight(lblSpread, g_clip.Spread);
            UpdateOctaveLight();
            UpdateShuffleLight();
        }
    }
}
