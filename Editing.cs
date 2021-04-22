using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void CopyChan(Song song, int p, int ch)
        {
            copyChan = new Channel(song.Patterns[p].Channels[ch]);
        }


        void PasteChan(Song song, int p, int ch)
        {
            if (copyChan == null)
                return;

            int f, l;
            GetPatterns(song, p, out f, out l);

            for (int i = f; i <= l; i++)
                song.Patterns[i].Channels[ch] = new Channel(copyChan);

            UpdateInstOff(CurChan);
        }


        void Step(Song song, int ch)
        {
            if (OK(song.EditPos))
            {
                if (   !g_chordMode 
                    || g_chord > -1)
                {
                    var chan = CurrentPattern.Channels[ch];

                    int found;
                    while ((found = chan.Notes.FindIndex(n => 
                               CurPat * g_nSteps + n.PatStep >= song.EditPos 
                            && CurPat * g_nSteps + n.PatStep <  song.EditPos + 1)) > -1)
                        chan.Notes.RemoveAt(found);

                    lastNotes.Clear();
                }

                MoveEdit(song, 1, true);

                MarkLight(lblStep);
            }
        }


        void Hold(Song song)
        {
            if (OK(song.EditPos))
            {
                if (song.EditNotes.Count > 0)
                {
                    g_hold = !g_hold;
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
                g_hold = !g_hold;
                UpdateHoldLight();

                //if (!g_hold)
                //    StopCurrentNotes(song.CurChan);
            }
        }


        void Interpolate(Song song)
        {
            if (!OK(song.EditPos))
                return;

            if (   CurSet < 0
                || !(   g_paramKeys 
                     || g_paramAuto))
                return;


            if (song.Inter == null)
            {
                StopEdit(song);

                song.Inter = CurrentChannel.Notes.Find(n =>
                       n.SongStep >= song.EditPos
                    && n.SongStep <  song.EditPos + EditStep);
            }
            else
            {
                var note = CurrentChannel.Notes.Find(n =>
                       n.SongStep >= song.EditPos
                    && n.SongStep <  song.EditPos + EditStep);

                if (note != null)
                {
                    var si = song.Inter.SongStep;
                    var sn = note      .SongStep;

                    var path  = g_settings.Last().GetPath(CurSrc);
                    var param = (Parameter)GetSettingFromPath(note.Instrument, path);

                    var start = param.GetKeyValue(song.Inter, CurSrc);
                    var end   = param.GetKeyValue(note,       CurSrc);

                    int f = (int)(si / g_nSteps);
                    int l = (int)(sn / g_nSteps);
                    int d = f < l ? 1 : -1;

                    for (int p = f; f < l ? (p <= l) : (p >= l); p += d)
                    {
                        foreach (var n in song.Patterns[p].Channels[CurChan].Notes)
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
                                n.Keys.Add(new Key(CurSrc, param, nParam.Value, n.SongStep));
                                SetKeyValue(n.Keys.Last(), val);
                            }
                        }
                    }
                }

                song.Inter = null;
            }

            UpdateLight(lblCmd1, song.Inter != null);
            UpdateAdjustLights(g_song);
        }


        void EnableKeyMove(Song song)
        {
            if (   IsCurParam()
                && OK(song.EditPos)
                && g_paramAuto)
            {
                var key = SelectedChannel.AutoKeys.Find(k => 
                         k.StepTime >= (song.EditPos % g_nSteps)
                      && k.StepTime <  (song.EditPos % g_nSteps) + 1);

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
            if (OK(g_song.EditPos))
                g_song.LastEditPos = g_song.EditPos;

            g_song.EditPos =
                OK(g_song.EditPos)
                ? float.NaN
                : (OK(g_song.LastEditPos) ? g_song.LastEditPos : CurPat * g_nSteps);

            StopEdit(g_song);

            UpdateAdjustLights(g_song);

            if (g_hold)
                g_song.TrimCurrentNotes(CurChan);

            g_hold = false;
            UpdateLight(lblHold, false);

            if (!OK(g_song.EditPos))
            {
                g_song.Inter = null;
                UpdateLight(lblCmd1, false);
            }

            UpdateEditLight(lblEdit, OK(g_song.EditPos));
        }


        static Key PrevSongAutoKey(float pos, int p, int ch, string path)
        {
            var prevKeys = g_song.ChannelAutoKeys[ch]
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
            var nextKeys = g_song.ChannelAutoKeys[ch]
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


        void ToggleNote(Song song)
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
            UpdateAdjustLights(g_song);
        }


        void CutNotes(Song song)
        {
            for (int p = 0; p <= CurPat; p++)
            {
                var patStart =  CurPat   *g_nSteps;
                var patEnd   = (CurPat+1)*g_nSteps;

                var pat  = song.Patterns[p];
                var chan = pat.Channels[CurChan];

                var min  = (60 + chan.Transpose*12) * NoteScale;
                var max  = (84 + chan.Transpose*12) * NoteScale;

                foreach (var note in chan.Notes)
                {
                    if (   song.EditPos > note.SongStep
                        && song.EditPos < note.SongStep + note.StepLength)
                        note.StepLength = song.EditPos - note.SongStep + 1;
                }
            }

            mainPressed.Add(3);
        }


        List<Note> GetEditNotes(Song song, bool onlyEdit = false)
        {
            var chan = CurrentChannel;

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


        List<Note> GetLongNotes(Song song)
        {
            if (OK(song.EditPos))
            {
                var notes = new List<Note>();

                for (int p = 0; p <= CurPat; p++)
                {
                    var patStart =  CurPat   *g_nSteps;
                    var patEnd   = (CurPat+1)*g_nSteps;

                    var pat  = song.Patterns[p];
                    var chan = pat.Channels[CurChan];

                    var min  = (60 + chan.Transpose*12) * NoteScale;
                    var max  = (84 + chan.Transpose*12) * NoteScale;

                    foreach (var note in chan.Notes)
                    {
                        if (   song.EditPos > note.SongStep
                            && song.EditPos < note.SongStep + note.StepLength - 1)
                            notes.Add(note);
                    }
                }

                return notes;
            }
            else
                return new List<Note>();
        }


        List<Note> GetChannelNotes(Song song)
        {
            var notes = new List<Note>();

            int first, last;
            GetPatterns(song, CurPat, out first, out last);

            for (int pat = first; pat <= last; pat++)
                notes.AddRange(song.Patterns[pat].Channels[CurChan].Notes);

            return notes;
        }


        void SetStepLength(int d)
        {
            var len = MinMax(4, g_ticksPerStep + d, 15);
            var newTime = (float)g_time / len;

            g_ticksPerStep = len;

            foreach (var pat in g_song.Patterns)
            {
                foreach (var chan in pat.Channels)
                    chan.Shuffle = Math.Min(chan.Shuffle, g_ticksPerStep - 1);
            }

            infoPressed.Add(d > 0 ? 2 : 3);
        }


        void ChangeEditStep()
        {
            g_editStep++;

            if (g_editStep >= g_steps.Length - 1) // ignore the super long step
                g_editStep = 1;

            MarkLight(lblEditStep);
            UpdateEditLights();
        }


        void ChangeEditLength()
        {
            g_editLength++;

            if (g_editLength >= g_steps.Length)
                g_editLength = 0;

            MarkLight(lblEditLength);
            UpdateEditLights();

            //g_sampleValid = F;
        }


        void Left(Song song)
        {
            MoveEdit(song, -1);

            if (OK(song.EditPos))
                MarkLight(lblLeft);
        }


        void Right(Song song)
        {
            MoveEdit(song, 1);

            if (   OK(song.EditPos)
                || IsCurSetting(typeof(Harmonics)))
                MarkLight(lblRight);
        }


        void MoveEdit(Song song, int move, bool create = false)
        {
            var chan = SelectedChannel;

            if (IsCurSetting(typeof(Harmonics)))
            {
                CurHarmonics.MoveEdit(move);
            }
            else if (song.EditNotes.Count > 0)
            {
                if (g_hold)
                {
                    foreach (var n in song.EditNotes)
                    {
                        var is05 = n.StepLength == 0.5f && EditStepLength >= 1;
                        n.StepLength = MinMax(0.5f, n.StepLength + move * EditStepLength, 10f * FPS / g_ticksPerStep);
                        if (is05) n.StepLength -= 0.5f;
                    }
                }
                else
                {
                    var oldCur = CurPat;

                    song.EditPos += move * EditStep;

                    LimitRecPosition(song);

                    foreach (var n in song.EditNotes)
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
                song.EditPos       += move * EditStep;

                LimitRecPosition(song);

                if (   g_editKey.StepTime < 0
                    || g_editKey.StepTime >= g_nSteps)
                { 
                    g_editKey.Channel.AutoKeys.Remove(g_editKey);
                    g_editKey.StepTime -= move * g_nSteps;

                    chan.AutoKeys.Add(g_editKey);
                    g_editKey.Channel = chan;
                }

                song.UpdateAutoKeys();
            }
            else if (OK(song.EditPos))
            {
                song.EditPos += move * EditStep;

                if (g_follow)
                {
                    if (song.EditPos >= (CurPat + 1) * g_nSteps) // TODO blocks
                    {
                        if (song.EditPos >= song.Patterns.Count * g_nSteps)
                        {
                            if (create)
                            {
                                var pat = new Pattern(CurrentPattern);
                                pat.Channels[CurChan].Notes.Clear();

                                song.Patterns.Insert(CurPat + 1, pat);
                            }
                            else
                                song.EditPos -= song.Patterns.Count * g_nSteps;
                        }
                    }
                    else if (!OK(song.EditPos))
                        song.EditPos += song.Patterns.Count * g_nSteps;
                }

                LimitRecPosition(song);
                UpdateAdjustLights(g_song);
            }
        }


        void Transpose(Song song, int ch, float tr)
        {
            if (song.EditNotes.Count > 0)
            {
                foreach (var note in song.EditNotes)
                    Transpose(note, tr);
            }
            else
            {
                int first, last;
                GetPatterns(song, CurPat, out first, out last);

                for (int pat = first; pat <= last; pat++)
                    Transpose(song, pat, ch, tr);
            }
        }


        void Transpose(Song song, int pat, int ch, float tr)
        {
            var chan = song.Patterns[pat].Channels[ch];

            foreach (var note in chan.Notes)
                Transpose(note, tr);
        }


        void Transpose(Note note, float tr)
        {
            note.Number = (int)Math.Round(note.Number + tr * (g_shift ? 12 : 1) * (g_halfSharp ? 1 : 2));
        }


        void SetTranspose(Song song, int d)
        {
            var tune = SelectedSource    ?.Tune
                    ?? SelectedInstrument?.Tune;

            if (g_spread)
                g_chordSpread = MinMax(0, g_chordSpread + d, 16);
            
            else if (ShowPiano) SetTranspose(song, CurChan, d);
            else                SetShuffle(CurChan, d);

            MarkLight(
                d > 0
                ? lblTransposeUp
                : lblTransposeDown);

            UpdateOctaveLight();
        }


        void SetTranspose(Song song, int ch, int tr)
        {
            tr += CurrentPattern.Channels[ch].Transpose;

            int first, last;
            GetPatterns(song, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
            {
                var pat  = song.Patterns[p];
                var chan = pat.Channels[ch];

                chan.Transpose = MinMax(-3, tr, 4);
            }
        }


        void Spread()
        {
            g_spread = !g_spread;
            UpdateLight(lblSpread, g_spread);
            UpdateOctaveLight();
            UpdateShuffleLight();
        }
    }
}
