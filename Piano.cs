using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void High(int h)
        {
            var tune = g_clip.SelectedSource    ?.Tune
                    ?? g_clip.SelectedInstrument?.Tune;


            if (h == 10)
            {
                if (   IsCurParam(strTune)
                    && (tune?.UseChord ?? false))
                { 
                    g_settings.RemoveLast();
                    g_clip.CurSet--;
                    g_clip.Piano = false;
                    UpdateChordLights();
                }
                else
                    g_clip.Piano = !g_clip.Piano;

                if (g_clip.Piano)
                    g_clip.Pick = false;

                UpdateShuffleLight();
                UpdateOctaveLight();
            }
            else if (IsCurParam(strTune)
                  && (tune?.UseChord ?? false)
                  && !(g_clip.ParamKeys || g_clip.ParamAuto))
            {
                var chord = tune.Chord;
                var note  = HighToNote(h);

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                tune.FinalChord = UpdateFinalTuneChord(tune.Chord, tune.AllOctaves);
            }
            else if (g_clip.ChordEdit
                  && g_clip.Chord > -1)
            { 
                var note  = HighToNote(h);
                var chord = g_clip.Chords[g_clip.Chord];

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                UpdateChordLights();

                if (chord.Count > 0)
                {
                    chord.Sort();

                    var oldLength = g_clip.EditLength;
                    g_clip.EditLength = Math.Min(g_clip.EditLength, g_steps.Length-2);
                    PlayNote(g_clip, chord[0], chord, g_clip.CurChan);
                    g_clip.EditLength = oldLength;
                }
            }
            else if (g_clip.Piano)
            {
                PlayNote(
                    g_clip,
                    HighToNote(h), 
                    g_clip.Chord > -1 && g_clip.ChordMode ? g_clip.Chords[g_clip.Chord] : null,
                    g_clip.CurChan);
            }
            else
            {
                     if (h == 0) Shift(false);
                else if (h == 1) Shift(true); 

                else if (h == 2) PickNote();  
                else if (h == 3) ToggleAllChannels();
                else if (h == 4) RandomInstrument();

                else if (h == 5) RandomChannelNotes();
                else if (h == 6) ClearNotes();
                                   
                else if (h == 7) Flip(g_clip.CurChan, 4); 
                else if (h == 8) Flip(g_clip.CurChan, 8); 
                else if (h == 9) Flip(g_clip.CurChan, 16);

                if (   h != 2
                    && h != 3
                    && h != 4)
                    MarkLight(lblHigh[h]);
            }
        }


        void Low(int l)
        {
            var tune = g_clip.SelectedSource    ?.Tune
                    ?? g_clip.SelectedInstrument?.Tune;

            if (   IsCurParam(strTune)
                && (tune?.UseChord ?? false))
            {
                var chord = tune.Chord;
                var note  = LowToNote(l);

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                tune.FinalChord = UpdateFinalTuneChord(tune.Chord, tune.AllOctaves);
            }
            else if (g_clip.ChordEdit
                  && g_clip.Chord > -1)
            {
                var note  = LowToNote(l);
                var chord = g_clip.Chords[g_clip.Chord];

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                if (chord.Count > 0)
                {
                    chord.Sort();

                    var oldLength = g_clip.EditLength;
                    g_clip.EditLength = Math.Min(g_clip.EditLength, g_steps.Length-2);
                    PlayNote(g_clip, chord[0], chord, g_clip.CurChan);
                    g_clip.EditLength = oldLength;
                }

                UpdateChordLights();
            }
            else if (g_clip.Piano)
            {
                if (l == 15)
                {
                    g_clip.HalfSharp = !g_clip.HalfSharp;
                    UpdateLight(lblLow[15], g_clip.HalfSharp);
                }
                else // l < 15
                {
                    PlayNote(
                        g_clip,
                        LowToNote(l),
                        g_clip.Chord > -1 && g_clip.ChordMode ? g_clip.Chords[g_clip.Chord] : null,
                        g_clip.CurChan);
                }
            }
            else
                Tick(g_clip.CurChan, l);
        }


        void Tick(int ch, int step)
        {
            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                Tick(p, ch, step);
        }


        void Tick(int pat, int ch, int step)
        {
            var _chan = g_clip.Patterns[pat].Channels[ch];
            var  chan = g_clip.Patterns[pat].Channels[ch];

            var found = chan.Notes.Where(n => 
                   n.PatStep >= step
                && n.PatStep <  step+1).ToArray();

            if (found.Length == 0)
            {
                if (!g_clip.Pick)
                {
                    var notes = GetChordNotes(g_clip.CurNote);

                    for (int n = 0; n < notes.Count; n++)
                        chan.AddNote(new Note(chan, ch, 1, notes[n], step + ChordSpread(n), EditStepLength));
                }
            }
            else if (g_clip.Pick)
            {
                g_clip.CurNote = found[0].Number;
                //g_showNote = g_clip.CurNote;

                g_clip.Pick = false;

                TriggerNote(
                    found[0].Number, 
                    ch, 
                    found[0].StepLength,
                    0);
            }
            else
            {
                g_clip.Pick = false;
                
                foreach (var n in found)
                    chan.Notes.Remove(n);
            }
        }


        void Shift(bool fwd)
        {
            if (g_clip.AllChan)
            {
                for (int i = 0; i < g_nChans; i++)
                    Shift(i, fwd);
            }
            else
                Shift(g_clip.CurChan, fwd);
        }


        void Shift(int ch, bool fwd)
        {
            var pats = g_clip.Patterns;

            var spill = new List<Note>();

            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            if (fwd)
            {
                for (int p = last; p >= first; p--)
                {
                    var chan = pats[p].Channels[ch];

                    for (int n = chan.Notes.Count - 1; n >= 0; n--)
                    {
                        var note = chan.Notes[n];

                        note.PatStep += Math.Min(EditStep, 1);

                        if (note.PatStep >= g_nSteps)
                            spill.Add(note);
                    }
                }

                foreach (var n in spill)
                {
                    var spillPat  = n.PatIndex == last ? first : n.PatIndex+1;
                    var spillChan = pats[spillPat].Channels[ch];

                    MoveSpillNotes(n, spillChan, -g_nSteps);
                }
            }
            else
            {
                for (int p = first; p <= last; p++)
                {
                    var chan = pats[p].Channels[ch];

                    for (int n = chan.Notes.Count - 1; n >= 0; n--)
                    {
                        var note = chan.Notes[n];

                        note.PatStep -= Math.Min(EditStep, 1);

                        if (note.PatStep < 0)
                            spill.Add(note);
                    }
                }

                foreach (var n in spill)
                {
                    var spillPat  = n.PatIndex == first ? last : n.PatIndex-1;
                    var spillChan = pats[spillPat].Channels[ch];

                    MoveSpillNotes(n, spillChan, g_nSteps);
                }
            }
        }


        void MoveSpillNotes(Note note, Channel spillChan, float dSteps)
        {
            note.Channel.Notes.Remove(note);
            spillChan.Notes.Add(note);

            note.Channel  = spillChan;
            note.PatStep += dSteps;
        }


        void RandomInstrument()
        {
            g_clip.RndInst = !g_clip.RndInst;
            UpdateHighLights(g_clip.CurrentPattern, g_clip.CurrentChannel);
        }


        void ToggleAllChannels()
        {
            g_clip.AllChan = !g_clip.AllChan;
            UpdateHighLights(g_clip.CurrentPattern, g_clip.CurrentChannel);
        }


        void Flip(int ch, int frac)
        {
            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                for (int step = 0; step < g_nSteps; step += g_nSteps / frac)
                    Tick(p, ch, step);
            }
        }


        void Flip(int pat, int ch, int frac)
        {
            for (int step = 0; step < g_nSteps; step += g_nSteps / frac)
                Tick(pat, ch, step);
        }


        void ClearNotes()
        {
            if (g_clip.AllChan)
            {
                for (int i = 0; i < g_nChans; i++)
                    ClearNotes(i);
            }
            else
                ClearNotes(g_clip.CurChan);
        }


        void ClearNotes(int ch)
        {
            int first, last;
            g_clip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                var chan = g_clip.Patterns[p].Channels[ch];

                if (g_clip.ParamKeys)
                {
                    foreach (var note in chan.Notes)
                    { 
                        var param = GetCurrentParam(note.Instrument);

                        var index = 0;
                        while ((index = note.Keys.FindIndex(k => k.Path == param.GetPath(g_clip.CurSrc))) > -1)
                            note.Keys.RemoveAt(index);
                    }
                }
                else if (g_clip.ParamAuto)
                {
                    var param = GetCurrentParam(chan.Instrument);
                    var index = 0;
                        
                    while ((index = chan.AutoKeys.FindIndex(k => k.Path == param.GetPath(g_clip.CurSrc))) > -1)
                        chan.AutoKeys.RemoveAt(index);
                }
                else
                { 
                    chan.Notes.Clear();
                }
            }

            if (g_clip.ParamAuto)
                g_clip.UpdateAutoKeys();
        }


        void PickNote()
        {
            g_clip.Pick = !g_clip.Pick;
            UpdateLight(lblHigh[7], g_clip.Pick);
        }


        int HighToNote(int high)
        {
            var h = high * NoteScale + 1;

            if (high > 1) h++;
            if (high > 4) h++;
            if (high > 6) h++;

            return 
                  (60 + g_clip.CurrentChannel.Transpose * 12 + h) * NoteScale 
                + (g_clip.HalfSharp ? 1 : 0);
        }


        int LowToNote(int low)
        {
            var l = low * NoteScale;

            if (low >  2) l--;
            if (low >  6) l--;
            if (low >  9) l--;
            if (low > 13) l--;

            return 
                  (60 + g_clip.CurrentChannel.Transpose * 12 + l) * NoteScale
                + (g_clip.HalfSharp ? 1 : 0);
        }


        string HighNoteName(int high, bool halfSharp)
        {
            var h = high;
            if (high > 1) h++;
            if (high > 4) h++;
            if (high > 6) h++;

            if (halfSharp) 
            {
                var c = (char)(65 + (h+3) % 7);
                return c + "Ъ";
            }
            else
            { 
                var c = (char)(65 + (h+2) % 7);
                return c + "#";
            }
        }


        string LowNoteName(int low, bool halfSharp)
        {
            return
                  S((char)(65 + (low+2) % 7)) 
                + (g_clip.HalfSharp ? "‡" : "");
        }
    }
}
