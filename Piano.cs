using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    partial class Program
    {
        void High(int h)
        {
            var tune = SelectedSource    ?.Tune
                    ?? SelectedInstrument?.Tune;


            if (h == 10)
            {
                if (   IsCurParam("Tune")
                    && (tune?.UseChord ?? false))
                { 
                    g_settings.RemoveLast();
                    CurSet--;
                    g_piano = false;
                    UpdateChordLights();
                }
                else
                    g_piano = !g_piano;

                if (g_piano)
                    g_pick = false;

                UpdateShuffleLight();
                UpdateOctaveLight();
            }
            else if (IsCurParam("Tune")
                  && (tune?.UseChord ?? false)
                  && !(g_paramKeys || g_paramAuto))
            {
                var chord = tune.Chord;
                var note  = HighToNote(h);

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                tune.FinalChord = UpdateFinalTuneChord(tune.Chord, tune.AllOctaves);
            }
            else if (g_chordEdit
                  && g_chord > -1)
            { 
                var note  = HighToNote(h);
                var chord = g_chords[g_chord];

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                UpdateChordLights();

                if (chord.Count > 0)
                {
                    chord.Sort();

                    var oldLength = g_editLength;
                    g_editLength = Math.Min(g_editLength, g_steps.Length-2);
                    PlayNote(g_song, chord[0], chord, CurChan);
                    g_editLength = oldLength;
                }
            }
            else if (g_piano)
            {
                PlayNote(
                    CurSong,
                    HighToNote(h), 
                    g_chord > -1 && g_chordMode ? g_chords[g_chord] : null,
                    CurChan);
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
                                   
                else if (h == 7) Flip(CurPat, CurChan, 4); 
                else if (h == 8) Flip(CurPat, CurChan, 8); 
                else if (h == 9) Flip(CurPat, CurChan, 16);

                if (   h != 2
                    && h != 3
                    && h != 4)
                    MarkLight(lblHigh[h]);
            }
        }


        void Low(int l)
        {
            var tune = SelectedSource    ?.Tune
                    ?? SelectedInstrument?.Tune;

            if (   IsCurParam("Tune")
                && (tune?.UseChord ?? false))
            {
                var chord = tune.Chord;
                var note  = LowToNote(l);

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                tune.FinalChord = UpdateFinalTuneChord(tune.Chord, tune.AllOctaves);
            }
            else if (g_chordEdit
                  && g_chord > -1)
            {
                var note  = LowToNote(l);
                var chord = g_chords[g_chord];

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                if (chord.Count > 0)
                {
                    chord.Sort();

                    var oldLength = g_editLength;
                    g_editLength = Math.Min(g_editLength, g_steps.Length-2);
                    PlayNote(g_song, chord[0], chord, CurChan);
                    g_editLength = oldLength;
                }

                UpdateChordLights();
            }
            else if (g_piano)
            {
                if (l == 15)
                {
                    g_halfSharp = !g_halfSharp;
                    UpdateLight(lblLow[15], g_halfSharp);
                }
                else // l < 15
                {
                    PlayNote(
                        CurSong,
                        LowToNote(l),
                        g_chord > -1 && g_chordMode ? g_chords[g_chord] : null,
                        CurChan);
                }
            }
            else
                Tick(CurChan, l);
        }


        void Tick(int ch, int step)
        {
            int first, last;
            GetPatterns(CurSong, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
                Tick(p, ch, step);
        }


        void Tick(int pat, int ch, int step)
        {
            var _chan = g_song .Patterns[pat].Channels[ch];
            var  chan = CurSong.Patterns[pat].Channels[ch];

            var found = chan.Notes.Where(n => 
                   n.PatStep >= step
                && n.PatStep <  step+1).ToArray();

            if (found.Length == 0)
            {
                if (!g_pick)
                {
                    var notes = GetChordNotes(g_curNote);

                    foreach (var note in notes)
                        chan.AddNote(new Note(chan, ch, 1, note, step, EditLength));
                }
            }
            else if (g_pick)
            {
                g_curNote = found[0].Number;
                //g_showNote  = g_curNote;

                g_pick = false;

                TriggerNote(
                    found[0].Number, 
                    ch, 
                    found[0].StepLength,
                    0);
            }
            else
            {
                g_pick = false;
                
                foreach (var n in found)
                    chan.Notes.Remove(n);
            }
        }


        void Shift(bool fwd)
        {
            if (g_allChan)
            {
                for (int i = 0; i < nChans; i++)
                    Shift(i, fwd);
            }
            else
                Shift(CurChan, fwd);
        }


        void Shift(int ch, bool fwd)
        {
            var pats = CurSong.Patterns;

            var spill = new List<Note>();

            int first, last;
            GetPatterns(CurSong, CurPat, out first, out last);

            if (fwd)
            {
                for (int p = last; p >= first; p--)
                {
                    var chan = pats[p].Channels[ch];

                    for (int n = chan.Notes.Count - 1; n >= 0; n--)
                    {
                        var note = chan.Notes[n];

                        note.PatStep += Math.Min(EditStep, 1);

                        if (note.PatStep >= nSteps)
                        {
                            chan.Notes.RemoveAt(n);
                            note.PatStep -= nSteps;

                            if (p == last) spill.Add(note);
                            else pats[p + 1].Channels[ch].Notes.Add(note);
                        }
                    }
                }

                pats[first].Channels[ch].Notes.AddRange(spill);
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
                        {
                            chan.Notes.RemoveAt(n);
                            note.PatStep += nSteps;

                            if (p == first) spill.Add(note);
                            else pats[p-1].Channels[ch].Notes.Add(note);
                        }
                    }
                }

                pats[last].Channels[ch].Notes.AddRange(spill);
            }
        }


        void RandomInstrument()
        {
            g_rndInst = !g_rndInst;
            UpdateHighLights(CurrentPattern, CurrentChannel);
        }


        void ToggleAllChannels()
        {
            g_allChan = !g_allChan;
            UpdateHighLights(CurrentPattern, CurrentChannel);
        }


        void Flip(int pat, int ch, int frac)
        {
            for (int step = 0; step < nSteps; step += nSteps / frac)
                Tick(pat, ch, step);
        }


        void ClearNotes()
        {
            if (g_allChan)
            {
                for (int i = 0; i < nChans; i++)
                    ClearNotes(i);
            }
            else
                ClearNotes(CurChan);
        }


        void ClearNotes(int ch)
        {
            int first, last;
            GetPatterns(CurSong, CurPat, out first, out last);

            for (int p = first; p <= last; p++)
            { 
                var chan = CurSong.Patterns[p].Channels[ch];

                if (g_paramKeys)
                {
                    foreach (var note in chan.Notes)
                    { 
                        var param = GetCurrentParam(note.Instrument);

                        var index = 0;
                        while ((index = note.Keys.FindIndex(k => k.Path == param.GetPath(CurSrc))) > -1)
                            note.Keys.RemoveAt(index);
                    }
                }
                else if (g_paramAuto)
                {
                    var param = GetCurrentParam(chan.Instrument);
                    var index = 0;
                        
                    while ((index = chan.AutoKeys.FindIndex(k => k.Path == param.GetPath(CurSrc))) > -1)
                        chan.AutoKeys.RemoveAt(index);
                }
                else
                { 
                    chan.Notes.Clear();
                }
            }

            if (g_paramAuto)
                CurSong.UpdateAutoKeys();
        }


        void PickNote()
        {
            g_pick = !g_pick;
            UpdateLight(lblHigh[7], g_pick);
        }


        int HighToNote(int high)
        {
            var h = high * NoteScale + 1;

            if (high > 1) h++;
            if (high > 4) h++;
            if (high > 6) h++;

            return 
                  (60 + CurrentChannel.Transpose * 12 + h) * NoteScale 
                + (g_halfSharp ? 1 : 0);
        }


        int LowToNote(int low)
        {
            var l = low * NoteScale;

            if (low >  2) l--;
            if (low >  6) l--;
            if (low >  9) l--;
            if (low > 13) l--;

            return 
                  (60 + CurrentChannel.Transpose * 12 + l) * NoteScale
                + (g_halfSharp ? 1 : 0);
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
                + (g_halfSharp ? "‡" : "");
        }
    }
}
