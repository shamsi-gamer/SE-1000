using System;
using System.Collections.Generic;
using System.Linq;


namespace IngameScript
{
    partial class Program
    {
        void High(int h)
        {
            var clip = CurClip;

            var tune = clip.SelectedSource    ?.Tune
                    ?? clip.SelectedInstrument?.Tune;

            if (h == 10)
            {
                if (   IsCurParam(strTune)
                    && (tune?.UseChord ?? false))
                { 
                    g_settings.RemoveLast();
                    clip.CurSet--;
                    clip.Piano = false;
                    //UpdateChordLabels();
                }
                else
                    clip.Piano = !clip.Piano;

                if (clip.Piano)
                    clip.Pick = false;

                //UpdateShuffleLabel();
                //UpdateOctaveLabel();
            }
            else if (IsCurParam(strTune)
                  && (tune?.UseChord ?? false)
                  && !(clip.ParamKeys || clip.ParamAuto))
            {
                var chord = tune.Chord;
                var note  = HighToNote(h);

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                tune.FinalChord = UpdateFinalTuneChord(tune.Chord, tune.AllOctaves);
            }
            else if (clip.ChordEdit
                  && clip.Chord > -1)
            { 
                var note  = HighToNote(h);
                var chord = clip.Chords[clip.Chord];

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                //UpdateChordLabels();

                if (chord.Count > 0)
                {
                    chord.Sort();

                    var oldIndex = clip.EditLengthIndex;
                    clip.EditLengthIndex = Math.Min(clip.EditLengthIndex, g_steps.Length-2);
                    PlayNote(clip, chord[0], chord, clip.CurChan);
                    clip.EditLengthIndex = oldIndex;
                }
            }
            else if (clip.Piano)
            {
                PlayNote(
                    clip,
                    HighToNote(h), 
                    clip.Chord > -1 && clip.ChordMode ? clip.Chords[clip.Chord] : null,
                    clip.CurChan);
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
                                   
                else if (h == 7) Flip(clip.CurChan, 4); 
                else if (h == 8) Flip(clip.CurChan, 8); 
                else if (h == 9) Flip(clip.CurChan, 16);

                if (   h != 2
                    && h != 3
                    && h != 4)
                    lblHigh[h].Mark();
            }
        }


        void Low(int l)
        {
            var tune = CurClip.SelectedSource    ?.Tune
                    ?? CurClip.SelectedInstrument?.Tune;

            if (   IsCurParam(strTune)
                && (tune?.UseChord ?? false))
            {
                var chord = tune.Chord;
                var note  = LowToNote(l);

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                tune.FinalChord = UpdateFinalTuneChord(tune.Chord, tune.AllOctaves);
            }
            else if (CurClip.ChordEdit
                  && CurClip.Chord > -1)
            {
                var note  = LowToNote(l);
                var chord = CurClip.Chords[CurClip.Chord];

                if (chord.Contains(note)) chord.Remove(note);
                else                      chord.Add   (note);

                if (chord.Count > 0)
                {
                    chord.Sort();

                    var oldIndex = CurClip.EditLengthIndex;
                    CurClip.EditLengthIndex = Math.Min(CurClip.EditLengthIndex, g_steps.Length-2);
                    PlayNote(CurClip, chord[0], chord, CurClip.CurChan);
                    CurClip.EditLengthIndex = oldIndex;
                }

                //UpdateChordLabels();
            }
            else if (CurClip.Piano)
            {
                if (l == 15)
                {
                    CurClip.HalfSharp = !CurClip.HalfSharp;
                    //UpdateLabel(lblLow[15], CurClip.HalfSharp);
                }
                else // l < 15
                {
                    PlayNote(
                        CurClip,
                        LowToNote(l),
                        CurClip.Chord > -1 && CurClip.ChordMode ? CurClip.Chords[CurClip.Chord] : null,
                        CurClip.CurChan);
                }
            }
            else
                Tick(CurClip.CurChan, l);
        }


        void Tick(int ch, int step)
        {
            int first, last;
            CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
                Tick(p, ch, step);
        }


        void Tick(int pat, int ch, int step)
        {
            var _chan = CurClip.Patterns[pat].Channels[ch];
            var  chan = CurClip.Patterns[pat].Channels[ch];

            var found = chan.Notes.Where(n => 
                   n.PatStep >= step
                && n.PatStep <  step+1).ToArray();

            if (found.Length == 0)
            {
                if (!CurClip.Pick)
                {
                    var notes = GetChordNotes(CurClip.CurNote);

                    for (int n = 0; n < notes.Count; n++)
                        chan.AddNote(new Note(chan, ch, 1, notes[n], step + ChordSpread(n), CurClip.EditStepLength));
                }
            }
            else if (CurClip.Pick)
            {
                CurClip.CurNote = found[0].Number;
                //g_showNote = CurClip.CurNote;

                CurClip.Pick = false;

                TriggerNote(
                    CurClip,
                    found[0].Number, 
                    ch, 
                    found[0].StepLength,
                    0);
            }
            else
            {
                CurClip.Pick = false;
                
                foreach (var n in found)
                    chan.Notes.Remove(n);
            }
        }


        void Shift(bool fwd)
        {
            if (CurClip.AllChan)
            {
                for (int i = 0; i < g_nChans; i++)
                    Shift(i, fwd);
            }
            else
                Shift(CurClip.CurChan, fwd);
        }


        void Shift(int ch, bool fwd)
        {
            var pats = CurClip.Patterns;

            var spill = new List<Note>();

            int first, last;
            CurClip.GetCurPatterns(out first, out last);

            if (fwd)
            {
                for (int p = last; p >= first; p--)
                {
                    var chan = pats[p].Channels[ch];

                    for (int n = chan.Notes.Count - 1; n >= 0; n--)
                    {
                        var note = chan.Notes[n];

                        note.PatStep += Math.Min(CurClip.EditStepIndex, 1);

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

                        note.PatStep -= Math.Min(CurClip.EditStepIndex, 1);

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
            CurClip.RndInst = !CurClip.RndInst;
            //UpdateHighLabels(CurClip.CurrentPattern, CurChannel);
        }


        void ToggleAllChannels()
        {
            CurClip.AllChan = !CurClip.AllChan;
            //UpdateHighLabels(CurClip.CurrentPattern, CurChannel);
        }


        void Flip(int ch, int frac)
        {
            int first, last;
            CurClip.GetCurPatterns(out first, out last);

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
            if (CurClip.AllChan)
            {
                for (int i = 0; i < g_nChans; i++)
                    ClearNotes(i);
            }
            else
                ClearNotes(CurClip.CurChan);
        }


        void ClearNotes(int ch)
        {
            int first, last;
            CurClip.GetCurPatterns(out first, out last);

            for (int p = first; p <= last; p++)
            { 
                var chan = CurClip.Patterns[p].Channels[ch];

                if (CurClip.ParamKeys)
                {
                    foreach (var note in chan.Notes)
                    { 
                        var param = GetCurrentParam(note.Instrument);

                        var index = 0;
                        while ((index = note.Keys.FindIndex(k => k.Path == param.GetPath(CurClip.CurSrc))) > -1)
                            note.Keys.RemoveAt(index);
                    }
                }
                else if (CurClip.ParamAuto)
                {
                    var param = GetCurrentParam(chan.Instrument);
                    var index = 0;
                        
                    while ((index = chan.AutoKeys.FindIndex(k => k.Path == param.GetPath(CurClip.CurSrc))) > -1)
                        chan.AutoKeys.RemoveAt(index);
                }
                else
                { 
                    chan.Notes.Clear();
                }
            }

            if (CurClip.ParamAuto)
                CurClip.UpdateAutoKeys();
        }


        void PickNote()
        {
            CurClip.Pick = !CurClip.Pick;
            //UpdateLabel(lblHigh[7], CurClip.Pick);
        }


        int HighToNote(int high)
        {
            var h = high * NoteScale + 1;

            if (high > 1) h++;
            if (high > 4) h++;
            if (high > 6) h++;

            return 
                  (60 + CurChannel.Transpose * 12 + h) * NoteScale 
                + (CurClip.HalfSharp ? 1 : 0);
        }


        int LowToNote(int low)
        {
            var l = low * NoteScale;

            if (low >  2) l--;
            if (low >  6) l--;
            if (low >  9) l--;
            if (low > 13) l--;

            return 
                  (60 + CurChannel.Transpose * 12 + l) * NoteScale
                + (CurClip.HalfSharp ? 1 : 0);
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
                + (CurClip.HalfSharp ? "‡" : "");
        }
    }
}
